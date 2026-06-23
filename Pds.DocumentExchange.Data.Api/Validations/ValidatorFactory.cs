using FluentValidation;
using System;

namespace Pds.DocumentExchange.Data.Api.Validations
{
    /// <summary>
    /// Service to get a validator for a given type.
    /// </summary>
    public class ValidatorFactory : IValidatorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public ValidatorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public IValidator<T> GetValidator<T>()
            => _serviceProvider.GetService(typeof(IValidator<T>)) as IValidator<T>;

        /// <inheritdoc/>
        public IValidator GetValidator(Type type)
        {
            var genericType = typeof(IValidator<>).MakeGenericType(type);
            return _serviceProvider.GetService(genericType) as IValidator;
        }
    }
}