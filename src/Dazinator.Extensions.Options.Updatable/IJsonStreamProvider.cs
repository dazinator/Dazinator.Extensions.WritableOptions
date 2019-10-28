using System.IO;

namespace Dazinator.Extensions.Options.Updatable
{
    public interface IJsonStreamProvider<TOptions>
         where TOptions : class, new()
    {
        Stream OpenReadStream();
        Stream OpenWriteStream();

    }
}
