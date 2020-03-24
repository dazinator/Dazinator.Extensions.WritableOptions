using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.FileInfo;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Dazinator.Extensions.Options.Updatable.Tests
{
    public class JsonUpdatableOptionsTests : IDisposable
    {
        protected JToken Original;
        protected JToken Expected;
        protected readonly string FilePath;
        protected readonly string FileName;
        protected readonly string Directory;

        protected JToken Current => JToken.Parse(File.ReadAllText(FilePath));

        protected readonly IServiceScope Scope;

        public JsonUpdatableOptionsTests()
        {
            var originalJsonString = "{ \"SetupComplete\":false,\"SetupStatus\":3,\"Database\":{ \"ConnectionString\":\"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\"Provider\":\"System.Data.SqlClient\"},\"Smtp\":{ \"SmtpHost\":\"foo.bar.com\",\"SmtpPort\":444,\"FromName\":\"Foo\",\"FromEmailAddress\":\"foo@bar.io\",\"Username\":\"foo@bar.io\",\"Password\":\"FAKE\",\"RequiresAuthentication\":true},\"Tenant\":{\"Id\":0,\"Email\":\"foo@bar.io\",\"IsCurrent\":false}}";
            var expectedJsonString = "{\"SetupComplete\":true,\"SetupStatus\":4,\"Database\":{\"ConnectionString\":\"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\"Provider\":\"System.Data.SqlClient\"},\"Smtp\":{\"SmtpHost\":\"foo.bar.com\",\"SmtpPort\":444,\"FromName\":\"Foo\",\"FromEmailAddress\":\"foo@bar.io\",\"Username\":\"foo@bar.io\",\"Password\":\"FAKE\",\"RequiresAuthentication\":true},\"Tenant\":{\"Id\":0,\"Email\":\"foo@bar.io\",\"IsCurrent\":false}}";

            Original = JToken.Parse(originalJsonString);
            Expected = JToken.Parse(expectedJsonString);

            Directory = Environment.CurrentDirectory;
            FileName = $"{Guid.NewGuid()}.json";
            FilePath = Path.Combine(Directory, FileName);

            File.WriteAllText(FilePath, originalJsonString);

            var services = new ServiceCollection();
            services.AddOptions();

            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(Environment.CurrentDirectory);
            // Add the json file (in memory) - this is the file that gets modified.
            var fileProvider = new PhysicalFileProvider(Environment.CurrentDirectory);
            configBuilder.AddJsonFile(fileProvider, FileName, false, true);
            var config = configBuilder.Build();

            services.ConfigureJsonUpdateableOptions<PlatformSetupOptionsDto>(FilePath, config);

            var sp = services.BuildServiceProvider();
            Scope = sp.CreateScope();
        }

        [Theory]
        [InlineData(@"")]
        [InlineData(@"TestPath:Foo")]
        public void Can_Update_Options(string sectionPath)
        {
            // needs further investigation
            // MemoryStreamFileInfo currently not supported anymore
            return;
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
            
            //var writeStream = new MemoryStream();
            //services.ConfigureJsonUpdatableOptions<TestOptions>(config, sectionPath, () => readStream, () =>
            //{
            //    var newFile = new MemoryStreamFileInfo(writeStream, Encoding.UTF8, "appsettings.json");
            //    inMemoryFileProvider.Directory.AddOrUpdateFile("/", newFile);
            //    return writeStream;
            //}, leaveOpen: true);
            services.ConfigureJsonUpdateableOptions<TestOptions>("", config);
            
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            // var existingOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>();
            var writableOptions = scope.ServiceProvider.GetRequiredService<IUpdatableOptions<TestOptions>>();
            writableOptions.Update(options =>
            {
                options.SomeDecimal.Should().BeNull();
                options.SomeInt.Should().Be(73);
                options.Enabled.Should().BeTrue();

                options.Enabled = true;
                options.SomeDecimal = 8.2m;
                options.SomeInt = 99;
            });

            var modifiedFile = inMemoryFileProvider.Directory.GetFile("/appsettings.json");
            var modifiedContents = IFileProviderExtensions.ReadAllContent(modifiedFile.FileInfo);
            Console.WriteLine(modifiedContents);

            var newOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>();
            Assert.True(newOptions.Value.Enabled);
            Assert.Equal(8.2m, newOptions.Value.SomeDecimal);
            Assert.Equal(99, newOptions.Value.SomeInt);

        }

        [Fact]
        public void Updating_Existing_Options_Roundtrips_EscapeSequence()
        {
           var writableOptions = Scope.ServiceProvider.GetRequiredService<IUpdatableOptions<PlatformSetupOptionsDto>>();
            writableOptions.Update(options =>
            {
                options.SetupComplete.Should().BeFalse();
                options.SetupStatus.Should().Be(options.SetupStatus);

                options.SetupComplete = true;
                options.SetupStatus = PlatformSetupStatus.SetupComplete;
            });

            Expected.Should().BeEquivalentTo(Current);
            
            var newOptions = Scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            newOptions.Value.SetupComplete.Should().BeTrue();
        }

        [Fact]
        public void Updating_Existing_Options_Roundtrips_EscapeSequence_PhysicalFileSystem()
        {
            var expectedConnString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            var writableOptions = Scope.ServiceProvider.GetRequiredService<IUpdatableOptions<PlatformSetupOptionsDto>>();
            writableOptions.Update(options =>
            {
                options.SetupComplete.Should().BeFalse();
                options.SetupStatus.Should().Be(options.SetupStatus);

                options.SetupComplete = true;
                options.SetupStatus = PlatformSetupStatus.SetupComplete;
            });

            Expected.Should().BeEquivalentTo(Current);
            
            var newOptions = Scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            newOptions.Value.SetupComplete.Should().BeTrue();

            newOptions.Value.Database.ConnectionString.Should().Be(expectedConnString);

            // update again
            writableOptions.Update(options =>
            {
                options.Database.ConnectionString.Should().Be(expectedConnString);
            });
            
            Expected.Should().BeEquivalentTo(Current);
            
            newOptions = Scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            newOptions.Value.Database.ConnectionString.Should().Be(expectedConnString);
        }

        [Fact]
        public void Updating_Existing_Options_Roundtrips_EscapeSequence_WithSectionAndPhysicalFileSystem()
        {
           var expectedConnString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            // override default json data
            var originalJson = "{\"Foo\":\"bar\\bar\",\"Setup\":{\"SetupComplete\":false,\"SetupStatus\":3,\"Database\":{\"ConnectionString\":\"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\"Provider\":\"System.Data.SqlClient\"},\"Smtp\":{\"SmtpHost\":\"foo.bar.com\",\"SmtpPort\":444,\"FromName\":\"Foo\",\"FromEmailAddress\":\"foo@bar.io\",\"Username\":\"foo@bar.io\",\"Password\":\"FAKE\",\"RequiresAuthentication\":true},\"Tenant\":{\"Id\":0,\"Email\":\"foo@bar.io\",\"IsCurrent\":false}}}";
            var expectedJson = "{\"Foo\":\"bar\\bar\",\"Setup\":{\"SetupComplete\":true,\"SetupStatus\":4,\"Database\":{\"ConnectionString\":\"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\"Provider\":\"System.Data.SqlClient\"},\"Smtp\":{\"SmtpHost\":\"foo.bar.com\",\"SmtpPort\":444,\"FromName\":\"Foo\",\"FromEmailAddress\":\"foo@bar.io\",\"Username\":\"foo@bar.io\",\"Password\":\"FAKE\",\"RequiresAuthentication\":true},\"Tenant\":{\"Id\":0,\"Email\":\"foo@bar.io\",\"IsCurrent\":false}}}";
            Original = JToken.Parse(originalJson);
            Expected = JToken.Parse(expectedJson);
            
            File.WriteAllText(FilePath, originalJson);

            var services = new ServiceCollection();
            services.AddOptions();

            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(Directory);
            // Add the json file (in memory) - this is the file that gets modified.
            var fileProvider = new PhysicalFileProvider(Directory);
            configBuilder.AddJsonFile(fileProvider, FileName, false, true);
            var config = configBuilder.Build();
            var section = config.GetSection("Setup");

            services.ConfigureJsonUpdateableOptions<PlatformSetupOptionsDto>(FilePath, section);

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            var writableOptions = scope.ServiceProvider.GetRequiredService<IUpdatableOptions<PlatformSetupOptionsDto>>();
            writableOptions.Update(options =>
            {
                options.SetupComplete.Should().BeFalse();
                options.SetupStatus.Should().Be(options.SetupStatus);

                options.SetupComplete = true;
                options.SetupStatus = PlatformSetupStatus.SetupComplete;
            });

            Expected.Should().BeEquivalentTo(Current);

            var newOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            newOptions.Value.SetupComplete.Should().BeTrue();
            newOptions.Value.Database.ConnectionString.Should().Be(expectedConnString);

            // update again
            writableOptions.Update(options =>
            {
                expectedConnString.Should().Be(options.Database.ConnectionString);
            });
            Expected.Should().BeEquivalentTo(Current);

            newOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            newOptions.Value.Database.ConnectionString.Should().Be(expectedConnString);
        }

        /// <summary>
        /// Cleanup test
        /// </summary>
        public void Dispose()
        {
            Scope?.Dispose();
            File.Delete(FilePath);
        }
    }
}
