using Microsoft.Extensions.Configuration;

namespace Pds.DocumentExchange.Data.Api.Extensions
{
    /// <summary>
    /// Extension methods for IConfiguration.
    /// </summary>
    public static class IConfigurationExtensions
    {
        /// <summary>
        ///  Attempts to load the configuration section into a new object of class T.
        /// </summary>
        /// <typeparam name="T">The configuration class type.</typeparam>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="sectionName">The section name.</param>
        /// <returns>The configuration object of class T.</returns>
        public static T LoadSection<T>(this IConfiguration configuration, string sectionName)
            where T : new()
        {
            var result = new T();
            configuration.Bind(sectionName, result);

            return result;
        }
    }
}