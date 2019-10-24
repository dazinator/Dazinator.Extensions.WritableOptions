using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dazinator.Extensions.WritableOptions.Tests
{
    public partial class Utf8JsonReaderExtensionsTests
    {
        public class JsonFileOptionsStore<TOptions>
            where TOptions : class, new()
        {
            private IOptionsSnapshot<TOptions> _options;
            private readonly IFileProvider _fileProvider;
            private readonly string _fileSubpath;
            private readonly string _sectionName;

            public JsonFileOptionsStore(IOptionsSnapshot<TOptions> options, IFileProvider fileProvider, string fileSubpath, string sectionName)
            {
                _options = options;
                _fileProvider = fileProvider;
                _fileSubpath = fileSubpath;
                _sectionName = sectionName;
            }

            public async Task Update(Action<TOptions> makeChanges)
            {

                var fileInfo = _fileProvider.GetFileInfo(_fileSubpath);
                if (fileInfo.Exists)
                {
                    using var readStream = fileInfo.CreateReadStream();
                    using var doc = await JsonDocument.ParseAsync(readStream);

                    var writer = new Utf8JsonWriter(GetWriteStream());

                    JsonElement currentElement = doc.RootElement;
                    var sectionSegments = _sectionName?.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    int navigatedDepth = 0;

                    foreach (var item in currentElement.EnumerateObject())
                    {
                        var lookingForSegment = sectionSegments.Length > 0 ? sectionSegments[navigatedDepth] : "";
                        if (!item.NameEquals(lookingForSegment))
                        {
                            //foreach (var item in currentElement.EnumerateObject())
                            //{
                            //}
                            //    element = currentElement;
                            //return false;
                        }
                        item.WriteTo(writer);
                    }
                    foreach (var item in sectionSegments)
                    {

                    }

                    // write all existing content to new write stream )overwriting any existing file content
                    // but when we get to our section, we need to write the serialized object instead.
                    if (doc.TryGetPropertyAtPath(_sectionName, out JsonElement element))
                    {

                        JsonSerializer.Deserialize<TestOptions>(element.GetRawText());
                    }

                   // doc.RootElement.EnumerateObject()
                    // var content = fileInfo.ReadAllContent();
                    //var reader = new Utf8JsonStreamReader(readStream, 32 * 1024);



                }


                //   Utf8JsonWriter writer = new Utf8JsonWriter()
                //   var file = fileProvider.
                //            using var releasesResponse = await JsonDocument.ParseAsync(await httpClient.GetStreamAsync(
                //"https://raw.githubusercontent.com/dotnet/core/master/release-notes/releases-index.json"));


                // reader.

                // System.Text.Json.JsonDocument.

            }

            private Stream GetWriteStream()
            {
                throw new NotImplementedException();
            }

            private static char[] _sectionDelimiterChars = new char[] { ':' };

        }
    }
}
