using System.IO;

namespace Dazinator.Extensions.Options.Updatable
{
    public class FileJsonStreamProvider<TOptions> : IJsonStreamProvider<TOptions>
       where TOptions : class, new()
    {
        private readonly string _baseDirectory;
        private readonly string _filePath;
        private readonly string _fullFilePath;

        public FileJsonStreamProvider(string baseDirectory, string filePath)
        {
            _baseDirectory = baseDirectory;
            _filePath = filePath;
            _fullFilePath = System.IO.Path.Combine(_baseDirectory, _filePath);
        }

        public Stream OpenReadStream()
        {
            if (File.Exists(_fullFilePath))
            {
                return File.OpenRead(_fullFilePath);
            }
            else
            {
                var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("{}")); // empty json object.
                return memoryStream;
            }
        }

        public Stream OpenWriteStream()
        {
            if (!File.Exists(_fullFilePath))
            {
                return File.Create(_fullFilePath);
            }
            else
            {
                return File.Open(_fullFilePath, FileMode.Truncate, FileAccess.Write);
            }      
        }
    }
}
