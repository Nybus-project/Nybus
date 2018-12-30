using Microsoft.Extensions.Configuration;

namespace Nybus.Utils
{
    public static class ConfigurationExtensions
    {
        public static bool TryGetValue(this IConfiguration configuration, string key, out string value)
        {
            value = configuration[key];
            return value != null;
        }

        public static bool TryGetSection(this IConfiguration configuration, string key, out IConfigurationSection section)
        {
            section = configuration.GetSection(key);
            return section != null;
        }
    }
}