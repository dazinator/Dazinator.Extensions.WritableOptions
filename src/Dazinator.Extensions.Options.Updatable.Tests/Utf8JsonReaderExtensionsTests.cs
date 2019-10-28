using System.Text;
using System.Text.Json;
using Xunit;

namespace Dazinator.Extensions.Options.Updatable.Tests
{

    public partial class Utf8JsonReaderExtensionsTests
    {

        [Theory]
        [InlineData(@"{ ""foo"":""bar""}", "")]
        [InlineData(@"{ ""one"": { ""foo"":""bar"" } }", "one")]
        [InlineData(@"{ ""one"": { ""two"": { ""foo"":""bar"" } } }", "one:two")]
        [InlineData(@"{ ""OnE"": { ""foo"":""bar"" } }", "oNe")]
        [InlineData(@"{ ""rubbish"": { ""SkipThis"" : true }, ""one"": { ""foo"":""bar"" } }", "one")]

        public void Can_Navigate_Utf8JsonReader_ToSection(string json, string sectionPath)
        {

            byte[] data = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(data, isFinalBlock: true, state: default);
            Assert.True(reader.NavigateToSection(sectionPath));



            //var services = new ServiceCollection();
            //services.AddOptions();

            //var configBuilder = new ConfigurationBuilder();
            //var configSettings = new Dictionary<string, string>();
            //configSettings.Add($"{sectionPath}:Enabled".TrimStart(':'), "True");
            //configBuilder.AddInMemoryCollection(configSettings);
            //var config = configBuilder.Build();

            //var section = config.GetSection(sectionPath);

            //  services.ConfigureWritable<TestOptions>(section,)



        }
        
    }
}
