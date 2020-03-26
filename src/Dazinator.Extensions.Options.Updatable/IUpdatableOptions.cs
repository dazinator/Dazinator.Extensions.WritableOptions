using System;
using System.Text.Json;

namespace Dazinator.Extensions.Options.Updatable
{
    
    public interface IUpdatableOptions<TOptions>
    {
        void Update(Action<TOptions> makeChanges, string namedOption = null, JsonSerializerOptions serialiserOptions = null);
        TOptions Value { get; }

        TOptions Get(string name);
    }
}

