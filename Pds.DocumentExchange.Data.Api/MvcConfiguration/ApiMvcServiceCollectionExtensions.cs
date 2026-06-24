using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pds.DocumentExchange.Data.Api.Extensions;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Interfaces;

namespace Pds.DocumentExchange.Data.Api.MvcConfiguration
{
    /// <summary>
    /// Extension methods for setting up API MVC services in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ApiMvcServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services for API controllers to the specified <see cref="IServiceCollection"/>,
        /// using custom conventions for routing.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The services collection.</returns>
        public static IServiceCollection AddApiControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
            });

            return services;
        }

        /// <summary>
        /// Add document exchange services configuration.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration (section provider).</param>
        /// <returns>The services collection.</returns>
        public static IServiceCollection AddDocumentExchangeServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DocumentExchangeServicesConfiguration>(options =>
            {
                configuration
                    .GetSection("DocumentExchangeServices")
                    .Bind(options);
            });

            return services;
        }

        /// <summary>
        /// Adds the cache configuration.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration provider.</param>
        /// <returns>The services collection.</returns>
        public static IServiceCollection AddCacheConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.LoadSection<CacheConfiguration>("Cache");

            services.AddSingleton<ICacheConfiguration>(config);

            return services;
        }
    }
}