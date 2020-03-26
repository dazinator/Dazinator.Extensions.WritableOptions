using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;

namespace Dazinator.Extensions.Options.Updatable
{
    public class JsonUpdatableOptions<TOptions> : IUpdatableOptions<TOptions>
        where TOptions : class, new()
    {
        private readonly IOptionsMonitor<TOptions> _monitor;

        private readonly IJsonStreamProvider<TOptions> _jsonFileStreamProvider;
        private readonly string _sectionName;
        private readonly bool _leaveOpen;
        private readonly static JsonSerializerOptions _defaultSerializerOptions = new JsonSerializerOptions() { IgnoreNullValues = true, WriteIndented = true };
        private readonly JsonSerializerOptions _serializerOptions;

        public TOptions Value => _monitor.CurrentValue;

        public TOptions Get(string name) => _monitor.Get(name);

        public JsonUpdatableOptions(
            IOptionsMonitor<TOptions> monitor,
            IJsonStreamProvider<TOptions> jsonFileStreamProvider,
            string sectionName,
            JsonSerializerOptions serializerOptions = null,
            bool leaveOpen = false)
        {
            _monitor = monitor;
            _jsonFileStreamProvider = jsonFileStreamProvider;
            _sectionName = sectionName;
            _serializerOptions = serializerOptions ?? _defaultSerializerOptions;
            _leaveOpen = leaveOpen;
        }

        public void Update(Action<TOptions> makeChanges, string namedOption = null, JsonSerializerOptions serialiserOptions = null)
        {
            using (var memStream = new MemoryStream())
            {
                var optionValue = string.IsNullOrWhiteSpace(namedOption) ? _monitor.CurrentValue : _monitor.Get(namedOption);
                makeChanges(optionValue);

                var jsonWriterOptions = new JsonWriterOptions();
                var serialserOptions = serialiserOptions ?? _serializerOptions;
                jsonWriterOptions.Indented = serialserOptions.WriteIndented;

                using (var writer = new Utf8JsonWriter(memStream, jsonWriterOptions))
                {
                    using (var readStream = _jsonFileStreamProvider.OpenReadStream())
                    {
                        var reader = new Utf8JsonStreamReader(readStream, 1024);
                        writer.WriteJsonWithModifiedSection<TOptions>(reader, _sectionName, optionValue, serialserOptions);
                    }
                }
                memStream.Position = 0;
                var writeStream = _jsonFileStreamProvider.OpenWriteStream();
                if (_leaveOpen)
                {
                    memStream.CopyTo(writeStream);
                }
                else
                {
                    using (writeStream)
                    {
                        memStream.CopyTo(writeStream);
                    }
                }


                //var name = namedOption ?? Microsoft.Extensions.Options.Options.DefaultName;
                //_cache.TryRemove(name);
                //_cache.TryAdd(name, optionValue);

            }
        }
    }
}

