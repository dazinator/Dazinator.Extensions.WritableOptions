using System;

namespace Dazinator.Extensions.WritableOptions
{
    public interface IUpdatableOptions<TOptions>
    {
        void Update(Action<TOptions> makeChanges, string namedOption = null);
        TOptions Value { get; }

        TOptions Get(string name);
    }
}

