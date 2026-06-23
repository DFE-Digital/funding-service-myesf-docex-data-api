using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Validations
{
    /// <summary>
    /// Interface providing methods to validate models.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validates the specified model and updates the model state dictionary accordingly.
        /// </summary>
        /// <typeparam name="TModel">The model type.</typeparam>
        /// <param name="model">The model.</param>
        /// <param name="addToModelState">The model state dictionary.</param>
        /// <returns>True if the model is valid / False otherwise.</returns>
        //bool Validate<TModel>(TModel model, ModelStateDictionary modelStateDictionary)
        bool Validate<TModel>(TModel model, Action<ValidationResult> addToModelState);

        /// <summary>
        /// Validates the specified team and updates the model state dictionary accordingly.
        /// </summary>
        /// <param name="teamIdentifier">The team identifier.</param>
        /// <param name="addModelError">The model error action.</param>
        /// <returns>True if the team identifier is valid / False otherwise.</returns>
        Task<bool> ValidateTeam(string teamIdentifier, Action<string, string> addModelError);

        /// <summary>
        /// Validates the specified teams and updates the model state dictionary accordingly.
        /// </summary>
        /// <param name="teamIdentifiers">The team identifiers.</param>
        /// <param name="addModelError">The model error action.</param>
        /// <returns>True if the team identifiers are all valid / False otherwise.</returns>
        Task<bool> ValidateTeams(IEnumerable<string> teamIdentifiers, Action<string, string> addModelError);

        /// <summary>
        /// Validates the specified product identifier and updates the model state dictionary accordingly.
        /// </summary>
        /// <param name="productIdentifier">The product identifier.</param>
        /// <param name="addModelError">The model error action.</param>
        /// <returns>True if the product identifier is valid / False otherwise.</returns>
        Task<bool> ValidateProductIdentifier(int productIdentifier, Action<string, string> addModelError);
    }
}