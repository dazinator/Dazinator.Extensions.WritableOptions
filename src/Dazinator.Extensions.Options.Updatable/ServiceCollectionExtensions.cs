using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using Dazinator.Extensions.WritableOptions;
using System.IO;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureJsonUpdatableOptions<TOptions>(this IServiceCollection services, IConfigurationSection configSection, IJsonStreamProvider<TOptions> jsonStreamProvider, bool leaveOpen = false)
              where TOptions : class, new()
        {
            var fullSectionName = $"{configSection.Path}:{configSection.Key}".TrimStart(':');
            services.Configure<TOptions>(configSection);
            return AddJsonUpdatableOptions(services, jsonStreamProvider, leaveOpen, fullSectionName);
        }

        private static IServiceCollection AddJsonUpdatableOptions<TOptions>(IServiceCollection services, IJsonStreamProvider<TOptions> jsonStreamProvider, bool leaveOpen, string fullSectionName) where TOptions : class, new()
        {
            return services.AddScoped<IUpdatableOptions<TOptions>, JsonUpdatableOptions<TOptions>>(sp =>
            {
                var optionsSnapshot = sp.GetRequiredService<IOptionsSnapshot<TOptions>>();
                return new JsonUpdatableOptions<TOptions>(optionsSnapshot, jsonStreamProvider, fullSectionName, leaveOpen);
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
            IConfiguration section = null;
            if (!string.IsNullOrWhiteSpace(sectionName))
            {
                section = configuration.GetSection(sectionName);
            }
            else
            {
                section = configuration;
            }
            services.ConfigureJsonUpdatableOptions<TOptions>(section, getReadStream, getWriteStream, leaveOpen);
            return services;
        }

        public static IServiceCollection AddJsonUpdatableOptions<TOptions>(this IServiceCollection services, string sectionPath, Func<Stream> getReadStream, Func<Stream> getWriteStream, bool leaveOpen = false)
             where TOptions : class, new()
        {
            var streamProvider = new DelegateJsonStreamProvider<TOptions>(getReadStream, getWriteStream);
            return services.ConfigureJsonUpdatableOptions(sectionPath, streamProvider, leaveOpen);
        }

        public static IServiceCollection ConfigureJsonUpdatableOptions<TOptions>(this IServiceCollection services, string sectionPath, IJsonStreamProvider<TOptions> jsonStreamProvider, bool leaveOpen = false)
            where TOptions : class, new()
        {
            return AddJsonUpdatableOptions(services, jsonStreamProvider, leaveOpen, sectionPath);
       
        }
    }
}
