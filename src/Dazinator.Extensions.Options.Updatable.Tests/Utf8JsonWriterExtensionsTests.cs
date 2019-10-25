using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Dazinator.Extensions.WritableOptions.Tests
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
