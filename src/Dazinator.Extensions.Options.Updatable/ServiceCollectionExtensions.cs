using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using Dazinator.Extensions.Options.Updatable;
using System.IO;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection
{    

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureJsonUpdatableOptions<TOptions>(this IServiceCollection services, 
            IConfigurationSection configSection, 
            IJsonStreamProvider<TOptions> jsonStreamProvider, 
            JsonSerializerOptions serializerOptions = default,
            bool leaveOpen = false)
              where TOptions : class, new()
        {
            var fullSectionName = $"{configSection.Path}".TrimStart(':');
            services.Configure<TOptions>(configSection);
            return AddJsonUpdatableOptions(services, jsonStreamProvider, leaveOpen, fullSectionName, serializerOptions);
        }

        public static IServiceCollection ConfigureJsonUpdatableOptions<TOptions>(this IServiceCollection services, 
            IConfiguration configSection,
            IJsonStreamProvider<TOptions> jsonStreamProvider,
            JsonSerializerOptions serializerOptions = default,
            bool leaveOpen = false)
where TOptions : class, new()
        {
            services.Configure<TOptions>(configSection);
            return AddJsonUpdatableOptions(services, jsonStreamProvider, leaveOpen, string.Empty, serializerOptions);
        }

        private static IServiceCollection AddJsonUpdatableOptions<TOptions>(IServiceCollection services, 
            IJsonStreamProvider<TOptions> jsonStreamProvider, 
            bool leaveOpen, 
            string fullSectionName,
            JsonSerializerOptions serializerOptions = default) where TOptions : class, new()
        {
            services.AddOptionsManagerBackedByMonitorCache<TOptions>();
            return services.AddScoped<IUpdatableOptions<TOptions>, JsonUpdatableOptions<TOptions>>(sp =>
            {
                // var optionsSnapshot = sp.GetRequiredService<IOptionsSnapshot<TOptions>>();
               // var optionsCache = sp.GetRequiredService<IOptionsMonitorCache<TOptions>>();
                var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();

                return new JsonUpdatableOptions<TOptions>(optionsMonitor, jsonStreamProvider, fullSectionName, serializerOptions, leaveOpen);
            });
        }

        public static IServiceCollection ConfigureJsonUpdatableOptions<TOptions>(this IServiceCollection services, 
            IConfigurationSection configSection, 
            Func<Stream> getReadStream, 
            Func<Stream> getWriteStream,
            JsonSerializerOptions serializerOptions = default,
            bool leaveOpen = false)
      where TOptions : class, new()
        {
            var streamProvider = new DelegateJsonStreamProvider<TOptions>(getReadStream, getWriteStream);
            return services.ConfigureJsonUpdatableOptions(configSection, streamProvider, serializerOptions, leaveOpen);
        }

        public static IServiceCollection ConfigureJsonUpdatableOptions<TOptions>(this IServiceCollection services, 
            IConfiguration configuration, 
            Func<Stream> getReadStream, 
            Func<Stream> getWriteStream,
            JsonSerializerOptions serializerOptions = default,
            bool leaveOpen = false)
where TOptions : class, new()
        {
            services.Configure<TOptions>(configuration);

            var streamProvider = new DelegateJsonStreamProvider<TOptions>(getReadStream, getWriteStream);
            return AddJsonUpdatableOptions(services, streamProvider, leaveOpen, string.Empty, serializerOptions);

        }

        public static IServiceCollection ConfigureJsonUpdatableOptions<TOptions>(this IServiceCollection services,
            IConfiguration configuration, 
            string sectionName, 
            Func<Stream> getReadStream, 
            Func<Stream> getWriteStream,
            JsonSerializerOptions serializerOptions = default,
            bool leaveOpen = false)
where TOptions : class, new()
        {
            if (!string.IsNullOrWhiteSpace(sectionName))
            {
                var section = configuration.GetSection(sectionName);
                services.ConfigureJsonUpdatableOptions<TOptions>(section, getReadStream, getWriteStream, serializerOptions, leaveOpen);
            }
            else
            {
                services.ConfigureJsonUpdatableOptions<TOptions>(configuration, getReadStream, getWriteStream, serializerOptions, leaveOpen);

            }
            return services;
        }

        public static IServiceCollection AddJsonUpdatableOptions<TOptions>(this IServiceCollection services, 
            string sectionPath, Func<Stream> getReadStream,
            Func<Stream> getWriteStream,
            JsonSerializerOptions serializerOptions = default,
            bool leaveOpen = false)
             where TOptions : class, new()
        {
            var streamProvider = new DelegateJsonStreamProvider<TOptions>(getReadStream, getWriteStream);
            return services.AddJsonUpdatableOptions(sectionPath, streamProvider, serializerOptions, leaveOpen);
        }

        public static IServiceCollection AddJsonUpdatableOptions<TOptions>(this IServiceCollection services, 
            string sectionPath, 
            IJsonStreamProvider<TOptions> jsonStreamProvider,
            JsonSerializerOptions serializerOptions = default,
            bool leaveOpen = false)
            where TOptions : class, new()
        {
            return AddJsonUpdatableOptions(services, jsonStreamProvider, leaveOpen, sectionPath, serializerOptions);

        }
    }
}
