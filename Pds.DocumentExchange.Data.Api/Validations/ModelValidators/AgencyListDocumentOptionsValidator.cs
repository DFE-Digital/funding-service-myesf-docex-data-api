using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The agency list document options validator.
    /// </summary>
    public class AgencyListDocumentOptionsValidator : AbstractValidator<AgencyListDocumentOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgencyListDocumentOptionsValidator"/> class.
        /// </summary>
        public AgencyListDocumentOptionsValidator()
        {
            RuleFor(options => options)
                .SetValidator(new ListOptionsValidator());
        }
    }
}