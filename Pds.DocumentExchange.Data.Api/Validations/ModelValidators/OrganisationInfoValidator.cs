using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The organisation info validation.
    /// </summary>
    public class OrganisationInfoValidator : AbstractValidator<OrganisationInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationInfoValidator"/> class.
        /// </summary>
        public OrganisationInfoValidator()
        {
            RuleFor(organisationInfo => organisationInfo.OrganisationIdentifier)
                .SetValidator(new OrganisationIdentifierValidator())
                .DependentRules(() =>
                {
                    RuleFor(organisationInfo => organisationInfo.OrganisationIdentifier)
                    .NotNull()
                    .WithMessage("The organisation identifier cannot be null.");
                });
        }
    }
}