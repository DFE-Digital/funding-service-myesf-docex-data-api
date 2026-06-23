using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Pds.DocumentExchange.Data.Api.Validations
{
    /// <summary>
    /// Extensions class for <see cref="IServiceCollection"/> for registering the validations.
    /// </summary>
    public static class ValidationsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the validations.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection AddValidations(
           this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<Startup>();

            services.AddSingleton<IValidatorFactory, ValidatorFactory>();
            services.AddSingleton<IValidationService, ValidationService>();

            return services;
        }
    }
}