using System;

namespace Dazinator.Extensions.Options.Updatable
{
    public interface IUpdatableOptions<TOptions>
    {
        void Update(Action<TOptions> makeChanges, string namedOption = null);
        TOptions Value { get; }

        TOptions Get(string name);
    }
}

