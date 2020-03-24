using System;
using System.IO;

namespace Dazinator.Extensions.Options.Updatable
{
    public class FileJsonStreamProvider<TOptions> : IJsonStreamProvider<TOptions>
       where TOptions : class, new()
    {
        private readonly string _FullFilePath;

        [Obsolete("Do not use this constructor anymore")]
        public FileJsonStreamProvider(string baseDirectory, string filePath)
        {
            _FullFilePath = System.IO.Path.Combine(baseDirectory, filePath);
        }

        public FileJsonStreamProvider(string fullFilePath)
        {
            _FullFilePath = fullFilePath;
        }

        public Stream OpenReadStream()
        {
            if (File.Exists(_FullFilePath))
            {
                return File.OpenRead(_FullFilePath);
            }
            else
            {
                var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{}")); // empty json object.
                return memoryStream;
            }
        }

        public Stream OpenWriteStream()
        {
            if (!File.Exists(_FullFilePath))
            {
                return File.Create(_FullFilePath);
            }
            else
            {
                return File.Open(_FullFilePath, FileMode.Truncate, FileAccess.Write);
            }      
        }
    }
}
