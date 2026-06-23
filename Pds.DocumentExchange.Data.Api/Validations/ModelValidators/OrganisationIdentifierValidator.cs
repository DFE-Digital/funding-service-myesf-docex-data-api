using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The organisation identifier validator.
    /// </summary>
    public class OrganisationIdentifierValidator : AbstractValidator<OrganisationIdentifier>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationIdentifierValidator"/> class.
        /// </summary>
        public OrganisationIdentifierValidator()
        {
            RuleFor(organisationId => organisationId.Value)
                .NotEmpty()
                .WithMessage("The organisation identifier value cannot be empty.");
        }
    }
}