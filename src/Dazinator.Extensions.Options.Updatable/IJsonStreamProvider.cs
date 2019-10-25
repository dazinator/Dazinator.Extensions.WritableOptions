using System.IO;

namespace Dazinator.Extensions.WritableOptions
{
    public interface IJsonStreamProvider<TOptions>
         where TOptions : class, new()
    {
        Stream OpenReadStream();
        Stream OpenWriteStream();

    }
}
