using Dazinator.Extensions.WriteableOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureWritable<T>(
            this IServiceCollection services,
            IConfigurationSection section,
            IConfigurationRoot configurationRoot,
            IFileProvider fileProvider,
            Action<string, string> writeFile,
            string file = "appsettings.json") where T : class, new()
        {
            services.Configure<T>(section);
            services.AddScoped<IWritableOptions<T>>(provider =>
            {
                var options = provider.GetService<IOptionsMonitor<T>>();
                return new WritableOptions<T>(fileProvider, writeFile, options, configurationRoot, section.Key, file);
            });
        }
    }
}
