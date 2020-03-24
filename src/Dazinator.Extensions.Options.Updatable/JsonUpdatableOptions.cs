using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;

namespace Dazinator.Extensions.Options.Updatable
{
    public class JsonUpdatableOptions<TOptions> : IUpdatableOptions<TOptions>
        where TOptions : class, new()
    {
        // ##############################################################################################################################
        // Properties
        // ##############################################################################################################################

        #region Properties

        // ##########################################################################################
        // Public Properties
        // ##########################################################################################

        public TOptions Value => _Monitor.CurrentValue;

        public TOptions Get(string name) => _Monitor.Get(name);

        // ##########################################################################################
        // Private Properties
        // ##########################################################################################

        private readonly IOptionsMonitor<TOptions> _Monitor;

        private readonly IJsonStreamProvider<TOptions> _JsonFileStreamProvider;
        private readonly string _SectionName;
        private readonly JsonSerializerOptions _JsonSerializerOptions;

        #endregion

        // ##############################################################################################################################
        // Constructor
        // ##############################################################################################################################

        #region Constructor

        public JsonUpdatableOptions(
            IOptionsMonitor<TOptions> monitor,
            IJsonStreamProvider<TOptions> jsonFileStreamProvider,
            string sectionName,
            JsonSerializerOptions jsonSerializerOptions)
        {
            _Monitor = monitor;
            _JsonFileStreamProvider = jsonFileStreamProvider;
            _SectionName = sectionName;
            _JsonSerializerOptions = jsonSerializerOptions;
        }

        #endregion

        // ##############################################################################################################################
        // public methods
        // ##############################################################################################################################

        #region public methods

        public void Update(Action<TOptions> makeChanges, string namedOption = null)
        {
            using (var memStream = new MemoryStream())
            {
                var optionValue =   string.IsNullOrWhiteSpace(namedOption) ? _Monitor.CurrentValue : _Monitor.Get(namedOption);
                makeChanges(optionValue);

                using (var writer = new Utf8JsonWriter(memStream, _JsonSerializerOptions.ToJsonWriterOptions()))
                {
                    using (var readStream = _JsonFileStreamProvider.OpenReadStream())
                    {
                        var reader = new Utf8JsonStreamReader(readStream, 1024);
                        writer.WriteJsonWithModifiedSection(reader, _SectionName, optionValue, _JsonSerializerOptions);
                    }
                }
                memStream.Position = 0;
                var writeStream = _JsonFileStreamProvider.OpenWriteStream();
                using (writeStream)
                {
                    memStream.CopyTo(writeStream);
                }
            }
        }  

        #endregion
    }
}

