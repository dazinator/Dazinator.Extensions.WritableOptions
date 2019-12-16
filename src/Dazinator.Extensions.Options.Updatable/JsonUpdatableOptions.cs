using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;

namespace Dazinator.Extensions.Options.Updatable
{
    public class JsonUpdatableOptions<TOptions> : IOptionsUpdater<TOptions>
        where TOptions : class, new()
    {
        private readonly IOptionsMonitor<TOptions> _monitor;

        // private IOptionsSnapshot<TOptions> _options;
        private readonly IJsonStreamProvider<TOptions> _jsonFileStreamProvider;
        private readonly IOptionsMonitorCache<TOptions> _cache;
        private readonly string _sectionName;
        private readonly bool _leaveOpen;
        private readonly static JsonSerializerOptions _defaultSerializerOptions = new JsonSerializerOptions() { IgnoreNullValues = true, WriteIndented = true };

        // public TOptions Value => _options.Value;

        public JsonUpdatableOptions(
            IOptionsMonitor<TOptions> monitor,
            IJsonStreamProvider<TOptions> jsonFileStreamProvider,
            IOptionsMonitorCache<TOptions> cache,
            string sectionName,
            bool leaveOpen = false)
        {
            _monitor = monitor;
            _jsonFileStreamProvider = jsonFileStreamProvider;
            _cache = cache;
            _sectionName = sectionName;
            _leaveOpen = leaveOpen;
        }

        public void Update(Action<TOptions> makeChanges, string namedOption = null)
        {

            using (var memStream = new MemoryStream())
            {
                var optionValue =   string.IsNullOrWhiteSpace(namedOption) ? _monitor.CurrentValue : _monitor.Get(namedOption);
                makeChanges(optionValue);

                using (var writer = new Utf8JsonWriter(memStream))
                {
                    using (var readStream = _jsonFileStreamProvider.OpenReadStream())
                    {
                        var reader = new Utf8JsonStreamReader(readStream, 1024);
                        writer.WriteJsonWithModifiedSection<TOptions>(reader, _sectionName, optionValue, _defaultSerializerOptions);
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


                var name = namedOption ?? Microsoft.Extensions.Options.Options.DefaultName;
                _cache.TryRemove(name);
                _cache.TryAdd(name, optionValue);

            }
        }      
    }
}

