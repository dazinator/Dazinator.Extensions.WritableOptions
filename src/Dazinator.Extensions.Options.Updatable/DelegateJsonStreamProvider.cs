using System;
using System.IO;

namespace Dazinator.Extensions.WritableOptions
{

    public class DelegateJsonStreamProvider<TOptions> : IJsonStreamProvider<TOptions>
         where TOptions : class, new()
    {
        private readonly Func<Stream> _getReadStream;
        private readonly Func<Stream> _getWriteStream;

        public DelegateJsonStreamProvider(Func<Stream> getReadStream, Func<Stream> getWriteStream)
        {
            _getReadStream = getReadStream;
            _getWriteStream = getWriteStream;
        }

        public Stream OpenReadStream()
        {
            return _getReadStream();
        }

        public Stream OpenWriteStream()
        {
            return _getWriteStream();
        }
    }
}
