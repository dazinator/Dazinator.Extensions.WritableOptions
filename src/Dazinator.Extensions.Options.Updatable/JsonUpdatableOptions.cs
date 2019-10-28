using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;

namespace Dazinator.Extensions.Options.Updatable
{
    public class JsonUpdatableOptions<TOptions> : IUpdatableOptions<TOptions>
        where TOptions : class, new()
    {
        private IOptionsSnapshot<TOptions> _options;
        private readonly IJsonStreamProvider<TOptions> _jsonFileStreamProvider;
        private readonly string _sectionName;
        private readonly bool _leaveOpen;
        private readonly static JsonSerializerOptions _defaultSerializerOptions = new JsonSerializerOptions() { IgnoreNullValues = true, WriteIndented = true };

        public TOptions Value => _options.Value;

        public JsonUpdatableOptions(
            IOptionsSnapshot<TOptions> options,
            IJsonStreamProvider<TOptions> jsonFileStreamProvider,
            string sectionName,
            bool leaveOpen = false)
        {
            _options = options;
            _jsonFileStreamProvider = jsonFileStreamProvider;
            _sectionName = sectionName;
            _leaveOpen = leaveOpen;
        }

        public void Update(Action<TOptions> makeChanges, string namedOption = null)
        {
            var reader = new Utf8JsonStreamReader(_jsonFileStreamProvider.OpenReadStream(), 1024);
            using (var memStream = new MemoryStream())
            {
                var optionValue = string.IsNullOrWhiteSpace(namedOption) ? _options.Value : _options.Get(namedOption);
                makeChanges(optionValue);

                using (var writer = new Utf8JsonWriter(memStream))
                {
                    writer.WriteJsonWithModifiedSection<TOptions>(reader, _sectionName, optionValue, _defaultSerializerOptions);
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

            }
        }

        public TOptions Get(string name)
        {
            return _options.Get(name);
        }
    }
}

