using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Xunit;
using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.FileInfo;
using Microsoft.Extensions.Options;

namespace Dazinator.Extensions.WritableOptions.Tests
{

    public class JsonUpdatableOptionsTests
    {

        [Theory]
        [InlineData(@"")]
        public void Can_Update_Options(string sectionPath)
        {

            var inMemoryFileProvider = new InMemoryFileProvider();
            var readBytes = Encoding.UTF8.GetBytes("{}");
            var readStream = new MemoryStream(readBytes);
            readStream.Position = 0;
            inMemoryFileProvider.Directory.AddFile("/", new MemoryStreamFileInfo(readStream, Encoding.UTF8, "appsettings.json"));


            var services = new ServiceCollection();
            services.AddOptions();

            var configBuilder = new ConfigurationBuilder();
            // Add some default settings
            var configSettings = new Dictionary<string, string>();

            //configSettings.Add($":Enabled".TrimStart(':'), "True");
            //configSettings.Add($":SomeInt".TrimStart(':'), "73");
            configSettings.Add($"{sectionPath}:Enabled".TrimStart(':'), "True");
            configSettings.Add($"{sectionPath}:SomeInt".TrimStart(':'), "73");
            configBuilder.AddInMemoryCollection(configSettings);


            // Add the json file (in memory) - this is the file that gets modified.
            configBuilder.AddJsonFile(inMemoryFileProvider, "appsettings.json", false, true);
            var config = configBuilder.Build();

            var writeStream = new MemoryStream();
            services.ConfigureJsonUpdatableOptions<TestOptions>(config, sectionPath, () => readStream, () =>
            {
                var newFile = new MemoryStreamFileInfo(writeStream, Encoding.UTF8, "appsettings.json");
                inMemoryFileProvider.Directory.AddOrUpdateFile("/", newFile);
                return writeStream;
            }, leaveOpen: true);

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            var writableOptions = scope.ServiceProvider.GetRequiredService<IUpdatableOptions<TestOptions>>();
            writableOptions.Update((options) =>
            {

                Assert.Null(options.SomeDecimal);
                Assert.Equal(73, options.SomeInt);
                Assert.True(options.Enabled);

                options.Enabled = true;
                options.SomeDecimal = 8.2m;
                options.SomeInt = 99;
            });

            var modifiedFile = inMemoryFileProvider.Directory.GetFile("/appsettings.json");
            var modifiedContents = modifiedFile.FileInfo.ReadAllContent();
            Console.WriteLine(modifiedContents);

            var newOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>();
            Assert.True(newOptions.Value.Enabled);
            Assert.Equal(8.2m, newOptions.Value.SomeDecimal);
            Assert.Equal(99, newOptions.Value.SomeInt);

        }

    }
}
