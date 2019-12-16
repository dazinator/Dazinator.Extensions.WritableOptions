using Dazinator.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.Options
{
    /// <summary>
    /// Extension methods for adding options services to the DI container.
    /// </summary>
    public static class DazinatorOptionsServiceCollectionExtensions
    {

        /// <summary>
        /// Removes default <see cref="IOptionsSnapshot{TOptions}"/> and <see cref="IOptions{TOptions}"/> registrations, and registers
        /// an implementation that will use the same <see cref="IOptionsMonitorCache{TOptions}"/> as <see cref="OptionsMonitor{TOptions}"/>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddOptionsManagerBackedByMonitorCache(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.RemoveAll(typeof(IOptionsSnapshot<>));
            services.RemoveAll(typeof(IOptions<>));

            services.TryAdd(ServiceDescriptor.Singleton(typeof(IOptions<>), typeof(Dazinator.Extensions.Options.OptionsManager<>)));
            services.TryAdd(ServiceDescriptor.Scoped(typeof(IOptionsSnapshot<>), typeof(Dazinator.Extensions.Options.OptionsManager<>)));
            return services;
        }

        /// <summary>
        /// Removes default <see cref="IOptionsSnapshot{TOptions}"/> and <see cref="IOptions{TOptions}"/> registrations, and registers
        /// an implementation that will use the same <see cref="IOptionsMonitorCache{TOptions}"/> as <see cref="OptionsMonitor{TOptions}"/>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddOptionsManagerBackedByMonitorCache<TOptions>(this IServiceCollection services)
            where TOptions : class, new()
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.RemoveAll(typeof(IOptionsSnapshot<TOptions>));
            services.RemoveAll(typeof(IOptions<TOptions>));

            services.TryAdd(ServiceDescriptor.Singleton(typeof(IOptions<TOptions>), typeof(Dazinator.Extensions.Options.OptionsManager<TOptions>)));
            services.TryAdd(ServiceDescriptor.Scoped(typeof(IOptionsSnapshot<TOptions>), typeof(Dazinator.Extensions.Options.OptionsManager<TOptions>)));
            return services;
        }



        public static OptionsBuilder<TOptions> UseMonitorCache<TOptions>(this OptionsBuilder<TOptions> builder)
            where TOptions : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            //services.AddOptions();
            builder.Services.AddOptionsManagerBackedByMonitorCache();
            return builder;
        }

    }
}




