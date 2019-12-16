using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using Dazinator.Extensions.Options.Updatable;
using System.IO;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureJsonUpdatableOptions<TOptions>(this IServiceCollection services, IConfigurationSection configSection, IJsonStreamProvider<TOptions> jsonStreamProvider, bool leaveOpen = false)
              where TOptions : class, new()
        {
            var fullSectionName = $"{configSection.Path}".TrimStart(':');
            services.Configure<TOptions>(configSection);
            return AddJsonUpdatableOptions(services, jsonStreamProvider, leaveOpen, fullSectionName);
        }

        public static IServiceCollection ConfigureJsonUpdatableOptions<TOptions>(this IServiceCollection services, IConfiguration configSection, IJsonStreamProvider<TOptions> jsonStreamProvider, bool leaveOpen = false)
where TOptions : class, new()
        {
            services.Configure<TOptions>(configSection);
            return AddJsonUpdatableOptions(services, jsonStreamProvider, leaveOpen, string.Empty);
        }

        private static IServiceCollection AddJsonUpdatableOptions<TOptions>(IServiceCollection services, IJsonStreamProvider<TOptions> jsonStreamProvider, bool leaveOpen, string fullSectionName) where TOptions : class, new()
        {
            services.AddOptionsManagerBackedByMonitorCache<TOptions>();
            return services.AddScoped<IOptionsUpdater<TOptions>, JsonUpdatableOptions<TOptions>>(sp =>
            {
                // var optionsSnapshot = sp.GetRequiredService<IOptionsSnapshot<TOptions>>();
                var optionsCache = sp.GetRequiredService<IOptionsMonitorCache<TOptions>>();
                var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();

                return new JsonUpdatableOptions<TOptions>(optionsMonitor, jsonStreamProvider, optionsCache, fullSectionName, leaveOpen);
            });
        }

        public static IServiceCollection ConfigureJsonUpdatableOptions<TOptions>(this IServiceCollection services, IConfigurationSection configSection, Func<Stream> getReadStream, Func<Stream> getWriteStream, bool leaveOpen = false)
      where TOptions : class, new()
        {
            var streamProvider = new DelegateJsonStreamProvider<TOptions>(getReadStream, getWriteStream);
            return services.ConfigureJsonUpdatableOptions(configSection, streamProvider, leaveOpen);
        }

        public static IServiceCollection ConfigureJsonUpdatableOptions<TOptions>(this IServiceCollection services, IConfiguration configuration, Func<Stream> getReadStream, Func<Stream> getWriteStream, bool leaveOpen = false)
where TOptions : class, new()
        {
            services.Configure<TOptions>(configuration);

            var streamProvider = new DelegateJsonStreamProvider<TOptions>(getReadStream, getWriteStream);
            return AddJsonUpdatableOptions(services, streamProvider, leaveOpen, string.Empty);

        }

        public static IServiceCollection ConfigureJsonUpdatableOptions<TOptions>(this IServiceCollection services, IConfiguration configuration, string sectionName, Func<Stream> getReadStream, Func<Stream> getWriteStream, bool leaveOpen = false)
where TOptions : class, new()
        {
            if (!string.IsNullOrWhiteSpace(sectionName))
            {
                var section = configuration.GetSection(sectionName);
                services.ConfigureJsonUpdatableOptions<TOptions>(section, getReadStream, getWriteStream, leaveOpen);
            }
            else
            {
                services.ConfigureJsonUpdatableOptions<TOptions>(configuration, getReadStream, getWriteStream, leaveOpen);

            }
            return services;
        }

        public static IServiceCollection AddJsonUpdatableOptions<TOptions>(this IServiceCollection services, string sectionPath, Func<Stream> getReadStream, Func<Stream> getWriteStream, bool leaveOpen = false)
             where TOptions : class, new()
        {
            var streamProvider = new DelegateJsonStreamProvider<TOptions>(getReadStream, getWriteStream);
            return services.AddJsonUpdatableOptions(sectionPath, streamProvider, leaveOpen);
        }

        public static IServiceCollection AddJsonUpdatableOptions<TOptions>(this IServiceCollection services, string sectionPath, IJsonStreamProvider<TOptions> jsonStreamProvider, bool leaveOpen = false)
            where TOptions : class, new()
        {
            return AddJsonUpdatableOptions(services, jsonStreamProvider, leaveOpen, sectionPath);

        }
    }
}
