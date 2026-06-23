using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Pds.DocumentExchange.Data.Services.Extensions;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Api.Controllers
{
    /// <summary>
    /// Base class for an API controller.
    /// </summary>
    [Route("api/[controller]/[action]", Name = "[controller]_[action]")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Add's to the model state..
        /// </summary>
        /// <param name="results">The validation results.</param>
        [NonAction]
        public void AddToModelState(ValidationResult results)
        {
            results.AddToModelState(ModelState, null);
        }

        /// <summary>
        /// Gets an enumerable collection of parameter values from a csv string.
        /// </summary>
        /// <param name="parameters">The parameters as a csv string.</param>
        /// <returns>The collection of parameter values extracted from the csv input.</returns>
        protected static IEnumerable<string> GetParameterValuesFromCsvString(string parameters)
        {
            return parameters.SplitAndTrim(',');
        }
    }
}