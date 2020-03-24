using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IConfigurationExtensions
    {
        public static string GetFullSectionName(this IConfigurationSection configSection)
        {
            var fullSectionName = $"{configSection.Path}".TrimStart(':');
            return fullSectionName;
        }
    }
}