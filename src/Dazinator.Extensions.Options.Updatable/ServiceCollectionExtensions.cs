using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Dazinator.Extensions.Options.Updatable;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Default <see cref="JsonSerializerOptions"/> which are used for the internal <see cref="JsonSerializer"/>
        /// </summary>
        public static readonly JsonSerializerOptions DefaultSerializerOptions = new JsonSerializerOptions
        { 
            IgnoreNullValues = true,
            WriteIndented = true
        };
        
        // ##############################################################################################################################
        // ConfigureJsonUpdateableOptions
        // ##############################################################################################################################

        #region ConfigureJsonUpdateableOptions

        public static IServiceCollection ConfigureJsonUpdateableOptions<TOptions>(this IServiceCollection services, string filePath, params IConfigurationSection[] configurationSections) where TOptions : class, new()
        {
            return services.ConfigureJsonUpdateableOptions<TOptions>(filePath, DefaultSerializerOptions, configurationSections);
        }

        public static IServiceCollection ConfigureJsonUpdateableOptions<TOptions>(this IServiceCollection services, string filePath, JsonSerializerOptions jsonSerializerOptions, params IConfigurationSection[] configurationSections) where TOptions : class, new()
        {
            foreach (var configSection in configurationSections)
            {
                services.Configure<TOptions>(configSection);
                return _AddJsonUpdateableOptions(services, new FileJsonStreamProvider<TOptions>(filePath), configSection.GetFullSectionName(), jsonSerializerOptions);
            }

            return services;
        }

        public static IServiceCollection ConfigureJsonUpdateableOptions<TOptions>(this IServiceCollection services, string filePath, IConfiguration configSection) where TOptions : class, new()
        {
            return services.ConfigureJsonUpdateableOptions<TOptions>(filePath, DefaultSerializerOptions, configSection);
        }

        public static IServiceCollection ConfigureJsonUpdateableOptions<TOptions>(this IServiceCollection services, string filePath, JsonSerializerOptions options, IConfiguration configSection) where TOptions : class, new()
        {
            services.Configure<TOptions>(configSection);
            return _AddJsonUpdateableOptions(services, new FileJsonStreamProvider<TOptions>(filePath), string.Empty, options);
        }

        #endregion
        
        // ##############################################################################################################################
        // AddJsonUpdateableOptions
        // ##############################################################################################################################

        #region AddJsonUpdateableOptions

        public static IServiceCollection AddJsonUpdateableOptions<TOptions>(this IServiceCollection services, string filePath, params IConfigurationSection[] configurationSections) where TOptions : class, new()
        {
            return services.AddJsonUpdateableOptions<TOptions>(filePath, DefaultSerializerOptions, configurationSections);
        }

        public static IServiceCollection AddJsonUpdateableOptions<TOptions>(this IServiceCollection services, string filePath, JsonSerializerOptions jsonSerializerOptions, params IConfigurationSection[] configurationSections) where TOptions : class, new()
        {
            foreach (var configSection in configurationSections)
            {
                return _AddJsonUpdateableOptions(services, new FileJsonStreamProvider<TOptions>(filePath), configSection.GetFullSectionName(), jsonSerializerOptions);
            }

            return services;
        }

        #endregion
        
        // ##############################################################################################################################
        // private methods
        // ##############################################################################################################################

        #region private methods

        private static IServiceCollection _AddJsonUpdateableOptions<TOptions>(IServiceCollection services, IJsonStreamProvider<TOptions> jsonStreamProvider, string fullSectionName, JsonSerializerOptions jsonSerializerOptions) where TOptions : class, new()
        {
            services.AddOptionsManagerBackedByMonitorCache<TOptions>();
            return services.AddScoped<IUpdatableOptions<TOptions>, JsonUpdatableOptions<TOptions>>(sp =>
            {
                var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<TOptions>>();

                return new JsonUpdatableOptions<TOptions>(optionsMonitor, jsonStreamProvider, fullSectionName, jsonSerializerOptions);
            });
        }

        #endregion
    }
    
    //        public static IServiceCollection ConfigureJsonUpdateableOptions<TOptions>(this IServiceCollection services, IConfigurationSection configSection, IJsonStreamProvider<TOptions> jsonStreamProvider, bool leaveOpen = false)
//              where TOptions : class, new()
//        {
//            return services.ConfigureJsonUpdateableOptions(configSection, jsonStreamProvider, DefaultSerializerOptions, leaveOpen);
//        }

//        public static IServiceCollection ConfigureJsonUpdateableOptions<TOptions>(this IServiceCollection services, IConfigurationSection configSection, Func<Stream> getReadStream, Func<Stream> getWriteStream, JsonSerializerOptions options, bool leaveOpen = false) where TOptions : class, new()
//        {
//            var streamProvider = new DelegateJsonStreamProvider<TOptions>(getReadStream, getWriteStream);
//            return services.ConfigureJsonUpdateableOptions(configSection, streamProvider, options, leaveOpen);
//        }

//        public static IServiceCollection ConfigureJsonUpdateableOptions<TOptions>(this IServiceCollection services, IConfigurationSection configSection, Func<Stream> getReadStream, Func<Stream> getWriteStream, bool leaveOpen = false) where TOptions : class, new()
//        {
//            var streamProvider = new DelegateJsonStreamProvider<TOptions>(getReadStream, getWriteStream);
//            return services.ConfigureJsonUpdateableOptions(configSection, streamProvider, DefaultSerializerOptions, leaveOpen);
//        }

//        public static IServiceCollection ConfigureJsonUpdateableOptions<TOptions>(this IServiceCollection services, IConfiguration configSection, IJsonStreamProvider<TOptions> jsonStreamProvider, JsonSerializerOptions options, bool leaveOpen = false)
//where TOptions : class, new()
//        {
//            services.Configure<TOptions>(configSection);
//            return _AddJsonUpdatableOptions(services, jsonStreamProvider, leaveOpen, string.Empty, options);
//        }

//        public static IServiceCollection ConfigureJsonUpdateableOptions<TOptions>(this IServiceCollection services, IConfiguration configSection, IJsonStreamProvider<TOptions> jsonStreamProvider, bool leaveOpen = false)
//            where TOptions : class, new()
//        {
//            return services.ConfigureJsonUpdateableOptions(configSection, jsonStreamProvider, DefaultSerializerOptions, leaveOpen);
//        }

//        public static IServiceCollection ConfigureJsonUpdateableOptions<TOptions>(this IServiceCollection services, IConfiguration configuration, Func<Stream> getReadStream, Func<Stream> getWriteStream, JsonSerializerOptions options, bool leaveOpen = false) where TOptions : class, new()
//        {
//            services.Configure<TOptions>(configuration);

//            var streamProvider = new DelegateJsonStreamProvider<TOptions>(getReadStream, getWriteStream);
//            return _AddJsonUpdatableOptions(services, streamProvider, leaveOpen, string.Empty, options);
//        }

//        public static IServiceCollection ConfigureJsonUpdateableOptions<TOptions>(this IServiceCollection services, IConfiguration configuration, Func<Stream> getReadStream, Func<Stream> getWriteStream, bool leaveOpen = false) where TOptions : class, new()
//        {
//            services.Configure<TOptions>(configuration);

//            var streamProvider = new DelegateJsonStreamProvider<TOptions>(getReadStream, getWriteStream);
//            return _AddJsonUpdatableOptions(services, streamProvider, leaveOpen, string.Empty, DefaultSerializerOptions);
//        }

//        public static IServiceCollection ConfigureJsonUpdateableOptions<TOptions>(this IServiceCollection services, IConfiguration configuration, string sectionName, Func<Stream> getReadStream, Func<Stream> getWriteStream, JsonSerializerOptions options, bool leaveOpen = false) where TOptions : class, new()
//        {
//            if (!string.IsNullOrWhiteSpace(sectionName))
//            {
//                var section = configuration.GetSection(sectionName);
//                services.ConfigureJsonUpdateableOptions<TOptions>(section, getReadStream, getWriteStream, options, leaveOpen);
//            }
//            else
//            {
//                services.ConfigureJsonUpdateableOptions<TOptions>(configuration, getReadStream, getWriteStream, options, leaveOpen);

//            }
//            return services;
//        }

//        public static IServiceCollection ConfigureJsonUpdateableOptions<TOptions>(this IServiceCollection services, IConfiguration configuration, string sectionName, Func<Stream> getReadStream, Func<Stream> getWriteStream, bool leaveOpen = false) where TOptions : class, new()
//        {
//            if (!string.IsNullOrWhiteSpace(sectionName))
//            {
//                var section = configuration.GetSection(sectionName);
//                services.ConfigureJsonUpdateableOptions<TOptions>(section, getReadStream, getWriteStream, DefaultSerializerOptions, leaveOpen);
//            }
//            else
//            {
//                services.ConfigureJsonUpdateableOptions<TOptions>(configuration, getReadStream, getWriteStream, DefaultSerializerOptions, leaveOpen);

//            }
//            return services;
//        }
}