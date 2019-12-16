using Microsoft.Extensions.Options;

namespace Dazinator.Extensions.Options
{
    /// <summary>
    /// Implementation of <see cref="IOptions{TOptions}"/> and <see cref="IOptionsSnapshot{TOptions}"/> that utilises the same
    /// <see cref="IOptionsMonitorCache{TOptions}"/> as <see cref="OptionsMonitor{TOptions}"/> to avoid inconsistencies during requests.
    /// </summary>
    /// <typeparam name="TOptions">Options type.</typeparam>
    public class OptionsManager<TOptions> : IOptions<TOptions>, IOptionsSnapshot<TOptions>
        where TOptions : class, new()
    {
        private readonly IOptionsFactory<TOptions> _factory;
        private readonly IOptionsMonitorCache<TOptions> _cache; //= new OptionsCache<TOptions>(); // Note: this is a private cache

        /// <summary>
        /// Initializes a new instance with the specified options configurations.
        /// </summary>
        /// <param name="factory">The factory to use to create options.</param>
        public OptionsManager(IOptionsFactory<TOptions> factory, IOptionsMonitorCache<TOptions> cache)
        {
            _cache = cache;
            _factory = factory;
        }

        /// <summary>
        /// The default configured <typeparamref name="TOptions"/> instance, equivalent to Get(Options.DefaultName).
        /// </summary>
        public TOptions Value
        {
            get
            {
                return Get(Microsoft.Extensions.Options.Options.DefaultName);
            }
        }

        /// <summary>
        /// Returns a configured <typeparamref name="TOptions"/> instance with the given <paramref name="name"/>.
        /// </summary>
        public virtual TOptions Get(string name)
        {
            name = name ?? Microsoft.Extensions.Options.Options.DefaultName;

            // Store the options in our instance cache
            return _cache.GetOrAdd(name, () => _factory.Create(name));
        }
    }

}
