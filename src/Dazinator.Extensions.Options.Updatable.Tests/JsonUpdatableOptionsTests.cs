using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Dazinator.AspNet.Extensions.FileProviders;
using Dazinator.AspNet.Extensions.FileProviders.FileInfo;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;
using System.Threading.Tasks;
using System.Text.Json;

namespace Dazinator.Extensions.Options.Updatable.Tests
{

    public class JsonUpdatableOptionsTests
    {

        [Theory]
        [InlineData(@"")]
        [InlineData(@"TestPath:Foo")]
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

            // var existingOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>();
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
            var modifiedContents = Dazinator.AspNet.Extensions.FileProviders.IFileProviderExtensions.ReadAllContent(modifiedFile.FileInfo);
            Console.WriteLine(modifiedContents);

            var newOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>();
            Assert.True(newOptions.Value.Enabled);
            Assert.Equal(8.2m, newOptions.Value.SomeDecimal);
            Assert.Equal(99, newOptions.Value.SomeInt);

        }

        [Theory]
        [InlineData(@"")]
        [InlineData(@"TestPath:Foo")]
        public void Can_Update_Options_Custom_SerialisationOptions(string sectionPath)
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
          
            configSettings.Add($"{sectionPath}:Enabled".TrimStart(':'), "True");
            configSettings.Add($"{sectionPath}:SomeInt".TrimStart(':'), "73");
            configBuilder.AddInMemoryCollection(configSettings);

            // Add the json file (in memory) - this is the file that gets modified.
            configBuilder.AddJsonFile(inMemoryFileProvider, "appsettings.json", false, true);
            var config = configBuilder.Build();

            var customSerialisationOptions = new JsonSerializerOptions() { WriteIndented = true, IgnoreNullValues = false };
            var writeStream = new MemoryStream();
            services.ConfigureJsonUpdatableOptions<TestOptions>(config, sectionPath, () => readStream, () =>
            {
                var newFile = new MemoryStreamFileInfo(writeStream, Encoding.UTF8, "appsettings.json");
                inMemoryFileProvider.Directory.AddOrUpdateFile("/", newFile);
                return writeStream;
            }, customSerialisationOptions, leaveOpen: true);

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            // var existingOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>();
            var writableOptions = scope.ServiceProvider.GetRequiredService<IUpdatableOptions<TestOptions>>();
            writableOptions.Update((options) =>
            {
                Assert.Null(options.SomeDecimal);
                Assert.Equal(73, options.SomeInt);
                Assert.True(options.Enabled);

                options.SomeDecimal = null;
            });

            var modifiedFile = inMemoryFileProvider.Directory.GetFile("/appsettings.json");
            var modifiedContents = Dazinator.AspNet.Extensions.FileProviders.IFileProviderExtensions.ReadAllContent(modifiedFile.FileInfo);
            Console.WriteLine(modifiedContents);

            var newOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>();
            Assert.Null(newOptions.Value.SomeDecimal);

            var expectedJson = "{\r\n  \"Enabled\": true,\r\n  \"SomeInt\": 73,\r\n  \"SomeDecimal\": null\r\n}";
            Assert.Equal(expectedJson, modifiedContents);

        }

        [Fact()]
        public void Updating_Existing_Options_Truncates_File_Correctly()
        {

            var directory = Environment.CurrentDirectory;
            var fileName = $"{Guid.NewGuid()}.json";
            var filePath = Path.Combine(directory, fileName);
            var originalJson = "{ \"SetupComplete\":false,\"SetupStatus\":3,\"Database\":{ \"ConnectionString\":\"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\"Provider\":\"System.Data.SqlClient\"},\"Smtp\":{ \"SmtpHost\":\"foo.bar.com\",\"SmtpPort\":444,\"FromName\":\"Foo\",\"FromEmailAddress\":\"foo@bar.io\",\"Username\":\"foo@bar.io\",\"Password\":\"FAKE\",\"RequiresAuthentication\":true},\"Tenant\":{\"Id\":0,\"Email\":\"foo@bar.io\",\"IsCurrent\":false}}";
            var expectedJson = "{\r\n  \"SetupComplete\": true,\r\n  \"SetupStatus\": 4,\r\n  \"Database\": {\r\n    \"ConnectionString\": \"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\r\n    \"Provider\": \"System.Data.SqlClient\"\r\n  },\r\n  \"Smtp\": {\r\n    \"SmtpHost\": \"foo.bar.com\",\r\n    \"SmtpPort\": 444,\r\n    \"FromName\": \"Foo\",\r\n    \"FromEmailAddress\": \"foo@bar.io\",\r\n    \"Username\": \"foo@bar.io\",\r\n    \"Password\": \"FAKE\",\r\n    \"RequiresAuthentication\": true\r\n  },\r\n  \"Tenant\": {\r\n    \"Id\": 0,\r\n    \"Email\": \"foo@bar.io\",\r\n    \"IsCurrent\": false\r\n  }\r\n}";

            var originalByteLength = Encoding.UTF8.GetBytes(originalJson);
            var expectedByteLength = Encoding.UTF8.GetBytes(expectedJson);

            File.WriteAllText(filePath, originalJson);

            var services = new ServiceCollection();
            services.AddOptions();

            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(directory);
            // Add the json file (in memory) - this is the file that gets modified.
            var fileProvider = new PhysicalFileProvider(directory);
            configBuilder.AddJsonFile(fileProvider, fileName, false, true);
            var config = configBuilder.Build();

            services.ConfigureJsonUpdatableOptions<PlatformSetupOptionsDto>(config, new FileJsonStreamProvider<PlatformSetupOptionsDto>(directory, fileName));

            //var writeStream = new MemoryStream();
            //services.ConfigureJsonUpdatableOptions<PlatformSetupOptionsDto>(config, sectionPath,
            //}, leaveOpen: true);

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            //var existingOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            var writableOptions = scope.ServiceProvider.GetRequiredService<IUpdatableOptions<PlatformSetupOptionsDto>>();
            writableOptions.Update((options) =>
            {
                Assert.False(options.SetupComplete);
                Assert.Equal(PlatformSetupStatus.AwaitingTenantAdminConfirmation, options.SetupStatus);

                options.SetupComplete = true;
                options.SetupStatus = PlatformSetupStatus.SetupComplete;
            });

            var modifiedFile = fileProvider.GetFileInfo(fileName);
            var modifiedContents = Dazinator.AspNet.Extensions.FileProviders.IFileProviderExtensions.ReadAllContent(modifiedFile);
            Console.WriteLine(modifiedContents);

            Assert.Equal(expectedJson, modifiedContents);

            var newOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            Assert.True(newOptions.Value.SetupComplete);
        }

        [Fact()]
        public void Updating_Existing_Options_Roundtrips_EscapeSequence()
        {

            var directory = Environment.CurrentDirectory;
            var fileName = $"{Guid.NewGuid()}.json";
            var filePath = Path.Combine(directory, fileName);

            //var original = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=hub2;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            //var expected = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=hub2;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            //var testObject = new TestEscapeSequenceOptions() { ConnectionString = expected };
            //var options = new JsonSerializerOptions() { IgnoreNullValues = true };
            //var expectedJson = System.Text.Json.JsonSerializer.Serialize<TestEscapeSequenceOptions>(testObject, options);


            var originalJson = "{ \"SetupComplete\":false,\"SetupStatus\":3,\"Database\":{ \"ConnectionString\":\"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\"Provider\":\"System.Data.SqlClient\"},\"Smtp\":{ \"SmtpHost\":\"foo.bar.com\",\"SmtpPort\":444,\"FromName\":\"Foo\",\"FromEmailAddress\":\"foo@bar.io\",\"Username\":\"foo@bar.io\",\"Password\":\"FAKE\",\"RequiresAuthentication\":true},\"Tenant\":{\"Id\":0,\"Email\":\"foo@bar.io\",\"IsCurrent\":false}}";
            var expectedJson = "{\r\n  \"SetupComplete\": true,\r\n  \"SetupStatus\": 4,\r\n  \"Database\": {\r\n    \"ConnectionString\": \"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\r\n    \"Provider\": \"System.Data.SqlClient\"\r\n  },\r\n  \"Smtp\": {\r\n    \"SmtpHost\": \"foo.bar.com\",\r\n    \"SmtpPort\": 444,\r\n    \"FromName\": \"Foo\",\r\n    \"FromEmailAddress\": \"foo@bar.io\",\r\n    \"Username\": \"foo@bar.io\",\r\n    \"Password\": \"FAKE\",\r\n    \"RequiresAuthentication\": true\r\n  },\r\n  \"Tenant\": {\r\n    \"Id\": 0,\r\n    \"Email\": \"foo@bar.io\",\r\n    \"IsCurrent\": false\r\n  }\r\n}";

            var originalByteLength = Encoding.UTF8.GetBytes(originalJson);
            var expectedByteLength = Encoding.UTF8.GetBytes(expectedJson);

            File.WriteAllText(filePath, originalJson);

            var services = new ServiceCollection();
            services.AddOptions();

            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(directory);
            // Add the json file (in memory) - this is the file that gets modified.
            var fileProvider = new PhysicalFileProvider(directory);
            configBuilder.AddJsonFile(fileProvider, fileName, false, true);
            var config = configBuilder.Build();

            services.ConfigureJsonUpdatableOptions<PlatformSetupOptionsDto>(config, new FileJsonStreamProvider<PlatformSetupOptionsDto>(directory, fileName));

            //var writeStream = new MemoryStream();
            //services.ConfigureJsonUpdatableOptions<PlatformSetupOptionsDto>(config, sectionPath,
            //}, leaveOpen: true);

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            //var existingOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            var writableOptions = scope.ServiceProvider.GetRequiredService<IUpdatableOptions<PlatformSetupOptionsDto>>();
            writableOptions.Update((options) =>
            {
                Assert.False(options.SetupComplete);
                Assert.Equal(PlatformSetupStatus.AwaitingTenantAdminConfirmation, options.SetupStatus);

                options.SetupComplete = true;
                options.SetupStatus = PlatformSetupStatus.SetupComplete;
            });

            var modifiedFile = fileProvider.GetFileInfo(fileName);
            var modifiedContents = Dazinator.AspNet.Extensions.FileProviders.IFileProviderExtensions.ReadAllContent(modifiedFile);
            Console.WriteLine(modifiedContents);

            Assert.Equal(expectedJson, modifiedContents);

            var newOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            Assert.True(newOptions.Value.SetupComplete);
        }

        [Fact()]
        public void Updating_Existing_Options_Roundtrips_EscapeSequence_PhysicalFileSystem()
        {

            var directory = Environment.CurrentDirectory;
            var fileName = $"{Guid.NewGuid()}.json";
            var filePath = Path.Combine(directory, fileName);

            var expectedConnString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            var originalJson = "{ \"SetupComplete\":false,\"SetupStatus\":3,\"Database\":{ \"ConnectionString\":\"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\"Provider\":\"System.Data.SqlClient\"},\"Smtp\":{ \"SmtpHost\":\"foo.bar.com\",\"SmtpPort\":444,\"FromName\":\"Foo\",\"FromEmailAddress\":\"foo@bar.io\",\"Username\":\"foo@bar.io\",\"Password\":\"FAKE\",\"RequiresAuthentication\":true},\"Tenant\":{\"Id\":0,\"Email\":\"foo@bar.io\",\"IsCurrent\":false}}";
            var expectedJson = "{\r\n  \"SetupComplete\": true,\r\n  \"SetupStatus\": 4,\r\n  \"Database\": {\r\n    \"ConnectionString\": \"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\r\n    \"Provider\": \"System.Data.SqlClient\"\r\n  },\r\n  \"Smtp\": {\r\n    \"SmtpHost\": \"foo.bar.com\",\r\n    \"SmtpPort\": 444,\r\n    \"FromName\": \"Foo\",\r\n    \"FromEmailAddress\": \"foo@bar.io\",\r\n    \"Username\": \"foo@bar.io\",\r\n    \"Password\": \"FAKE\",\r\n    \"RequiresAuthentication\": true\r\n  },\r\n  \"Tenant\": {\r\n    \"Id\": 0,\r\n    \"Email\": \"foo@bar.io\",\r\n    \"IsCurrent\": false\r\n  }\r\n}";
            var originalByteLength = Encoding.UTF8.GetBytes(originalJson);
            var expectedByteLength = Encoding.UTF8.GetBytes(expectedJson);

            File.WriteAllText(filePath, originalJson);

            var services = new ServiceCollection();
            services.AddOptions();

            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(directory);
            // Add the json file (in memory) - this is the file that gets modified.
            var fileProvider = new PhysicalFileProvider(directory);
            configBuilder.AddJsonFile(fileProvider, fileName, false, true);
            var config = configBuilder.Build();

            services.ConfigureJsonUpdatableOptions<PlatformSetupOptionsDto>(config, new FileJsonStreamProvider<PlatformSetupOptionsDto>(directory, fileName));

            //var writeStream = new MemoryStream();
            //services.ConfigureJsonUpdatableOptions<PlatformSetupOptionsDto>(config, sectionPath,
            //}, leaveOpen: true);

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            //var existingOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            var writableOptions = scope.ServiceProvider.GetRequiredService<IUpdatableOptions<PlatformSetupOptionsDto>>();
            writableOptions.Update((options) =>
            {
                Assert.False(options.SetupComplete);
                Assert.Equal(PlatformSetupStatus.AwaitingTenantAdminConfirmation, options.SetupStatus);

                options.SetupComplete = true;
                options.SetupStatus = PlatformSetupStatus.SetupComplete;
            });

            var modifiedFile = fileProvider.GetFileInfo(fileName);
            var modifiedContents = IFileProviderExtensions.ReadAllContent(modifiedFile);
            Console.WriteLine(modifiedContents);
            Assert.Equal(expectedJson, modifiedContents);

            var newOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            Assert.True(newOptions.Value.SetupComplete);
            Assert.Equal(expectedConnString, newOptions.Value.Database.ConnectionString);

            // udpate again

            writableOptions.Update((options) =>
            {
                Assert.Equal(expectedConnString, options.Database.ConnectionString);
            });

            modifiedFile = fileProvider.GetFileInfo(fileName);
            modifiedContents = IFileProviderExtensions.ReadAllContent(modifiedFile);
            Console.WriteLine(modifiedContents);
            Assert.Equal(expectedJson, modifiedContents);

            newOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            Assert.Equal(expectedConnString, newOptions.Value.Database.ConnectionString);






        }

        [Fact()]
        public void Updating_Existing_Options_Roundtrips_EscapeSequence_WithSectionAndPhysicalFileSystem()
        {

            var directory = Environment.CurrentDirectory;
            var fileName = $"{Guid.NewGuid()}.json";
            var filePath = Path.Combine(directory, fileName);

            var expectedConnString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            var originalJson = "{\"Foo\":\"bar\\bar\",\"Setup\":{\"SetupComplete\":false,\"SetupStatus\":3,\"Database\":{\"ConnectionString\":\"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\"Provider\":\"System.Data.SqlClient\"},\"Smtp\":{\"SmtpHost\":\"foo.bar.com\",\"SmtpPort\":444,\"FromName\":\"Foo\",\"FromEmailAddress\":\"foo@bar.io\",\"Username\":\"foo@bar.io\",\"Password\":\"FAKE\",\"RequiresAuthentication\":true},\"Tenant\":{\"Id\":0,\"Email\":\"foo@bar.io\",\"IsCurrent\":false}}}";
            var expectedJson = "{\r\n  \"Foo\": \"bar\\bar\",\r\n  \"Setup\": {\r\n    \"SetupComplete\": true,\r\n    \"SetupStatus\": 4,\r\n    \"Database\": {\r\n      \"ConnectionString\": \"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\r\n      \"Provider\": \"System.Data.SqlClient\"\r\n    },\r\n    \"Smtp\": {\r\n      \"SmtpHost\": \"foo.bar.com\",\r\n      \"SmtpPort\": 444,\r\n      \"FromName\": \"Foo\",\r\n      \"FromEmailAddress\": \"foo@bar.io\",\r\n      \"Username\": \"foo@bar.io\",\r\n      \"Password\": \"FAKE\",\r\n      \"RequiresAuthentication\": true\r\n    },\r\n    \"Tenant\": {\r\n      \"Id\": 0,\r\n      \"Email\": \"foo@bar.io\",\r\n      \"IsCurrent\": false\r\n    }\r\n  }\r\n}";

            var originalByteLength = Encoding.UTF8.GetBytes(originalJson);
            var expectedByteLength = Encoding.UTF8.GetBytes(expectedJson);

            File.WriteAllText(filePath, originalJson);

            var services = new ServiceCollection();
            services.AddOptions();

            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(directory);
            // Add the json file (in memory) - this is the file that gets modified.
            var fileProvider = new PhysicalFileProvider(directory);
            configBuilder.AddJsonFile(fileProvider, fileName, false, true);
            var config = configBuilder.Build();
            var section = config.GetSection("Setup");

            services.ConfigureJsonUpdatableOptions<PlatformSetupOptionsDto>(section, new FileJsonStreamProvider<PlatformSetupOptionsDto>(directory, fileName));

            //var writeStream = new MemoryStream();
            //services.ConfigureJsonUpdatableOptions<PlatformSetupOptionsDto>(config, sectionPath,
            //}, leaveOpen: true);

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            //var existingOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            var writableOptions = scope.ServiceProvider.GetRequiredService<IUpdatableOptions<PlatformSetupOptionsDto>>();
            writableOptions.Update((options) =>
            {
                Assert.False(options.SetupComplete);
                Assert.Equal(PlatformSetupStatus.AwaitingTenantAdminConfirmation, options.SetupStatus);

                options.SetupComplete = true;
                options.SetupStatus = PlatformSetupStatus.SetupComplete;
            });

            var modifiedFile = fileProvider.GetFileInfo(fileName);
            var modifiedContents = IFileProviderExtensions.ReadAllContent(modifiedFile);
            Console.WriteLine(modifiedContents);
            Assert.Equal(expectedJson, modifiedContents);

            var newOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            Assert.True(newOptions.Value.SetupComplete);
            Assert.Equal(expectedConnString, newOptions.Value.Database.ConnectionString);

            // udpate again

            writableOptions.Update((options) =>
            {
                Assert.Equal(expectedConnString, options.Database.ConnectionString);
            });

            modifiedFile = fileProvider.GetFileInfo(fileName);
            modifiedContents = IFileProviderExtensions.ReadAllContent(modifiedFile);
            Console.WriteLine(modifiedContents);
            Assert.Equal(expectedJson, modifiedContents);

            newOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<PlatformSetupOptionsDto>>();
            Assert.Equal(expectedConnString, newOptions.Value.Database.ConnectionString);

        }


    }
}
