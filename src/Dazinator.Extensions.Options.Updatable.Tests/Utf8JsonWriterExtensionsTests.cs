using System.IO;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Dazinator.Extensions.Options.Updatable.Tests
{

    public class Utf8JsonWriterExtensionsTests
    {

        [Theory]
        [InlineData(@"", "", @"{""Enabled"":true}")]
        [InlineData(@"", "one", @"{""one"":{""Enabled"":true}}")]
        [InlineData(@"", "one:two", @"{""one"":{""two"":{""Enabled"":true}}}")]
        [InlineData(@"{}", "", @"{""Enabled"":true}")]
        [InlineData(@"{}", "one", @"{""one"":{""Enabled"":true}}")]
        [InlineData(@"{""foo"":""bar""}", "", @"{""Enabled"":true}")]
        [InlineData(@"{ ""one"": { ""foo"":""bar"" } }", "one", @"{""one"":{""Enabled"":true}}")]
        [InlineData(@"{ ""rubbish"": { ""SkipThis"" : true }, ""one"": { ""foo"":""bar"" } }", "one", @"{""rubbish"":{""SkipThis"":true},""one"":{""Enabled"":true}}")]
        [InlineData(@"{ ""rubbish"": { ""SkipThis"" : true }, ""one"": { ""foo"":""bar"", ""two"":{ ""old"":true } } }", "one:two", @"{""rubbish"":{""SkipThis"":true},""one"":{""foo"":""bar"",""two"":{""Enabled"":true}}}")]
        [InlineData(@"{ ""rubbish"": { ""SkipThis"" : true }, ""one"": { ""foo"":""bar"" } }", "one:two", @"{""rubbish"":{""SkipThis"":true},""one"":{""foo"":""bar"",""two"":{""Enabled"":true}}}")]
        public void Can_Write_Modified_Json_Section(string json, string sectionPath, string expected)
        {
            byte[] data = Encoding.UTF8.GetBytes(json);
            var testObject = new TestOptions() { Enabled = true };

            var reader = new Utf8JsonReader(data, isFinalBlock: true, state: default);
            var memStream = new MemoryStream();
            var options = new JsonSerializerOptions() { IgnoreNullValues = true };
            using (var writer = new Utf8JsonWriter(memStream))
            {
                writer.WriteJsonWithModifiedSection<TestOptions>(reader, sectionPath, testObject, options);
            }
            memStream.Position = 0;
            var written = Encoding.UTF8.GetString(memStream.ToArray());
            Assert.Equal(expected, written);
            // Console.WriteLine(written);
            memStream.Position = 0;
            var doc = JsonDocument.Parse(memStream);
            var result = doc.TryGetPropertyAtPath(sectionPath, out JsonElement element);
            Assert.True(result);

            var deserialised = JsonSerializer.Deserialize<TestOptions>(element.GetRawText());
            Assert.NotNull(deserialised);
            Assert.True(deserialised.Enabled);

        }


        [Fact]
        public void Can_RoundTrip_Escape_Sequences()
        {

            var expected = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=hub2;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            var testObject = new TestEscapeSequenceOptions() { ConnectionString = expected };
            var options = new JsonSerializerOptions() { IgnoreNullValues = true };
            var expectedJson = System.Text.Json.JsonSerializer.Serialize<TestEscapeSequenceOptions>(testObject, options);
                
            byte[] data = Encoding.UTF8.GetBytes(expectedJson);
            var reader = new Utf8JsonReader(data, isFinalBlock: true, state: default);
            var sectionPath = "";

            var memStream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(memStream))
            {
                writer.WriteJsonWithModifiedSection<TestEscapeSequenceOptions>(reader, sectionPath, testObject, options);
            }
            memStream.Position = 0;
            var written = Encoding.UTF8.GetString(memStream.ToArray());
            Assert.Equal(expectedJson, written);
            // Console.WriteLine(written);
            memStream.Position = 0;
            var doc = JsonDocument.Parse(memStream);
            var result = doc.TryGetPropertyAtPath(sectionPath, out JsonElement element);
            Assert.True(result);

            var deserialised = JsonSerializer.Deserialize<TestEscapeSequenceOptions>(element.GetRawText());
            Assert.NotNull(deserialised);

            Assert.Equal(expected, deserialised.ConnectionString);

        }


        [Fact]
        public void Can_Update_Root_Regression()
        {
            var json = "{ \"SetupComplete\":true,\"SetupStatus\":4,\"Database\":{ \"ConnectionString\":\"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\"Provider\":\"System.Data.SqlClient\"},\"Smtp\":{ \"SmtpHost\":\"foo.bar.com\",\"SmtpPort\":444,\"FromName\":\"Foo\",\"FromEmailAddress\":\"foo@bar.io\",\"Username\":\"foo@bar.io\",\"Password\":\"FAKE\",\"RequiresAuthentication\":true},\"Tenant\":{\"Id\":0,\"Email\":\"foo@bar.io\",\"IsCurrent\":false}}";
            var expectedJson = "{\"SetupComplete\":true,\"SetupStatus\":4,\"Database\":{\"ConnectionString\":\"Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=foo;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False\",\"Provider\":\"System.Data.SqlClient\"},\"Smtp\":{\"SmtpHost\":\"foo.bar.com\",\"SmtpPort\":444,\"FromName\":\"Foo\",\"FromEmailAddress\":\"foo@bar.io\",\"Username\":\"foo@bar.io\",\"Password\":\"FAKE\",\"RequiresAuthentication\":true},\"Tenant\":{\"Id\":0,\"Email\":\"foo@bar.io\",\"IsCurrent\":false}}";

            byte[] data = Encoding.UTF8.GetBytes(json);

            PlatformSetupOptionsDto existing = JsonSerializer.Deserialize<PlatformSetupOptionsDto>(json);
            existing.SetupComplete = true;
            existing.SetupStatus = PlatformSetupStatus.SetupComplete;


            var reader = new Utf8JsonReader(data, isFinalBlock: true, state: default);
            var memStream = new MemoryStream();
            var options = new JsonSerializerOptions() { IgnoreNullValues = true };
            using (var writer = new Utf8JsonWriter(memStream))
            {
                writer.WriteJsonWithModifiedSection<PlatformSetupOptionsDto>(reader, "", existing, options);
            }
            memStream.Position = 0;
            var written = Encoding.UTF8.GetString(memStream.ToArray());
            Assert.Equal(expectedJson, written);
            // Console.WriteLine(written);
            memStream.Position = 0;
            var doc = JsonDocument.Parse(memStream);
            var result = doc.TryGetPropertyAtPath("", out JsonElement element);
            Assert.True(result);

            var deserialised = JsonSerializer.Deserialize<PlatformSetupOptionsDto>(element.GetRawText());
            Assert.NotNull(deserialised);
            Assert.True(deserialised.SetupComplete);

        }



        [Fact]
        public void Can_Write_Different_Types()
        {
            byte[] data = Encoding.UTF8.GetBytes("");
            var testObject = new TestOptions() { Enabled = true, SomeDecimal = 1.6m, SomeInt = 55 };

            var reader = new Utf8JsonReader(data, isFinalBlock: true, state: default);
            var memStream = new MemoryStream();
            var options = new JsonSerializerOptions() { IgnoreNullValues = true };
            using (var writer = new Utf8JsonWriter(memStream))
            {
                writer.WriteJsonWithModifiedSection<TestOptions>(reader, "", testObject, options);
            }
            memStream.Position = 0;
            //var written = Encoding.UTF8.GetString(memStream.ToArray());
            //Assert.Equal(expected, written);
            // Console.WriteLine(written);
            // memStream.Position = 0;
            var doc = JsonDocument.Parse(memStream);
            var result = doc.TryGetPropertyAtPath("", out JsonElement element);
            Assert.True(result);

            var deserialised = JsonSerializer.Deserialize<TestOptions>(element.GetRawText());
            Assert.NotNull(deserialised);
            Assert.True(deserialised.Enabled);
            Assert.Equal(testObject.SomeInt, deserialised.SomeInt);
            Assert.Equal(testObject.SomeDecimal, deserialised.SomeDecimal);

        }
    }

 
}
