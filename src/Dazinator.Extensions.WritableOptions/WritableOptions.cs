using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Dazinator.Extensions.WritableOptions
{

    public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
    {
        private readonly IFileProvider _fileProvider;
        private readonly IOptionsMonitor<T> _options;
        private readonly string _section;
        private readonly string _file;

        public WritableOptions(
            IFileProvider fileProvider,
            Action<string, string> writeFile,
            IOptionsMonitor<T> options,
            string section,
            string file)
        {
            _fileProvider = fileProvider;
            WriteFile = writeFile;
            _options = options;
           // _configuration = configuration;
            _section = section;
            _file = file;
        }

        public T Value => _options.CurrentValue;

        public Action<string, string> WriteFile { get; }

        public T Get(string name) => _options.Get(name);

        public void Update(Action<T> applyChanges)
        {
            var fileProvider = _fileProvider;
            var fileInfo = fileProvider.GetFileInfo(_file);
            T sectionObject = null;
            JObject jObject = null;

            if (fileInfo.Exists)
            {
                var content = ReadAllContent(fileInfo);
                //     var physicalPath = fileInfo.PhysicalPath;     
                // TODO: Remove newtonsoft.json in favour of System.Text.Json;

                jObject = JsonConvert.DeserializeObject<JObject>(content);
                sectionObject = jObject.TryGetValue(_section, out JToken section) ?
                    JsonConvert.DeserializeObject<T>(section.ToString()) : (Value ?? new T());
            }
            else
            {
                sectionObject = new T();
                jObject = new JObject();
            }


            applyChanges(sectionObject);

            jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));
            var newJsonContent = JsonConvert.SerializeObject(jObject, Formatting.Indented);

            WriteFile(_file, newJsonContent);
           // _configuration.Reload();

        }

        public static string ReadAllContent(IFileInfo fileInfo)
        {
            using (var reader = new StreamReader(fileInfo.CreateReadStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
