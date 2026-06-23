using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The exchange list organisation document options validator.
    /// </summary>
    public class ExchangeListOrganisationDocumentOptionsValidator : AbstractValidator<ExchangeListOrganisationDocumentOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeListOrganisationDocumentOptionsValidator"/> class.
        /// </summary>
        public ExchangeListOrganisationDocumentOptionsValidator()
        {
            RuleFor(options => options.OrganisationIdentifier)
                .SetValidator(new OrganisationIdentifierValidator())
                .DependentRules(() =>
                {
                    RuleFor(options => options.OrganisationIdentifier)
                    .NotNull()
                    .WithMessage("The organisation identifier cannot be null.");
                });

            RuleFor(options => options)
                .SetValidator(new ExchangeListDocumentOptionsValidator());
        }
    }
}