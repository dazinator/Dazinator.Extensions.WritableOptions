using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;

namespace Dazinator.Extensions.WritableOptions.Tests
{
    public partial class Utf8JsonReaderExtensionsTests
    {

        public interface IJsonFileStreamProvider
        {
            Stream OpenReadStream();
            Stream OpenWriteStream();

        }

        public class JsonFileOptionsStore<TOptions>
            where TOptions : class, new()
        {
            private IOptionsSnapshot<TOptions> _options;
            private readonly IJsonFileStreamProvider _jsonFileStreamProvider;
            private readonly string _sectionName;
            private readonly static JsonSerializerOptions _defaultSerializerOptions = new JsonSerializerOptions() { IgnoreNullValues = true, WriteIndented = true };

            public JsonFileOptionsStore(
                IOptionsSnapshot<TOptions> options,
                IJsonFileStreamProvider jsonFileStreamProvider,
                string sectionName)
            {
                _options = options;
                _jsonFileStreamProvider = jsonFileStreamProvider;
                _sectionName = sectionName;
            }

            public void Update(Action<TOptions> makeChanges, JsonSerializerOptions serialiserOptions = null)
            {
                var reader = new Utf8JsonStreamReader(_jsonFileStreamProvider.OpenReadStream(), 1024);
                using (var memStream = new MemoryStream())
                {
                    if (serialiserOptions == null)
                    {
                        serialiserOptions = _defaultSerializerOptions;
                    }

                    makeChanges(_options.Value);

                    using (var writer = new Utf8JsonWriter(memStream))
                    {
                        writer.WriteJsonWithModifiedSection<TOptions>(reader, _sectionName, _options.Value, serialiserOptions);
                    }

                    memStream.Position = 0;
                    using (var writeStream = _jsonFileStreamProvider.OpenWriteStream())
                    {
                        memStream.CopyTo(writeStream);
                    }
                }
            }
        }
    }
}
