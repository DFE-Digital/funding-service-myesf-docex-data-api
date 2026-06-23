using FluentValidation;
using FluentValidation.Results;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Validations
{
    /// <summary>
    /// Service providing methods to validate controller models.
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly IValidatorFactory _validatorFactory;
        private readonly ITeamsLookup _teamsLookup;
        private readonly IProductsLookup _productsLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationService"/> class.
        /// </summary>
        /// <param name="validatorFactory">The validator factory.</param>
        /// <param name="teamsLookup">The teams lookup.</param>
        /// <param name="productsLookup">The products lookup.</param>
        public ValidationService(
            IValidatorFactory validatorFactory,
            ITeamsLookup teamsLookup,
            IProductsLookup productsLookup)
        {
            _validatorFactory = validatorFactory;
            _teamsLookup = teamsLookup;
            _productsLookup = productsLookup;
        }

        /// <inheritdoc/>
        public bool Validate<TModel>(TModel model, Action<ValidationResult> addToModelState)
        {
            var validator = _validatorFactory.GetValidator<TModel>();

            if (validator == null)
            {
                throw new ArgumentException($"There is no validator for the type {nameof(TModel)}.", nameof(model));
            }

            var results = validator.Validate(new ValidationContext<TModel>(model));

            if (!results.IsValid)
            {
                addToModelState(results);
            }

            return results.IsValid;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateTeam(string teamIdentifier, Action<string, string> addModelError)
        {
            if (string.IsNullOrWhiteSpace(teamIdentifier))
            {
                addModelError(nameof(teamIdentifier), "The team name cannot be empty.");
                return false;
            }

            if (!await _teamsLookup.Exists(teamIdentifier))
            {
                addModelError(nameof(teamIdentifier), $"{teamIdentifier} is not a valid team identifier.");
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateTeams(IEnumerable<string> teamIdentifiers, Action<string, string> addModelError)
        {
            if (teamIdentifiers?.Any() != true)
            {
                addModelError(nameof(teamIdentifiers), "The team identifiers list cannot be empty.");
                return false;
            }

            foreach (var teamIdentifier in teamIdentifiers)
            {
                if (!await ValidateTeam(teamIdentifier, addModelError))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateProductIdentifier(int productIdentifier, Action<string, string> addModelError)
        {
            if (!await _productsLookup.Exists(productIdentifier))
            {
                addModelError(nameof(productIdentifier), "The provided product identifier does not exist.");
                return false;
            }

            return true;
        }
    }
}