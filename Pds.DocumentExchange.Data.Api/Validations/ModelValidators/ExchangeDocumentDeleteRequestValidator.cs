using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.Services.Common.Helpers;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The exchange document delete request validator.
    /// </summary>
    public class ExchangeDocumentDeleteRequestValidator : AbstractValidator<ExchangeDocumentDeleteRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeDocumentDeleteRequestValidator"/> class.
        /// </summary>
        public ExchangeDocumentDeleteRequestValidator()
        {
            RuleFor(agencyPublishRequest => agencyPublishRequest.UserInfo)
                .NotEmpty()
                .WithMessage("The user info cannot be null.");

            RuleFor(agencyPublishRequest => agencyPublishRequest.UserInfo.Principal)
                .NotEmpty()
                .WithMessage("The user principal cannot be empty.")
                .When(agencyPublishRequest => agencyPublishRequest.UserInfo != null);

            RuleForEach(options => options.DocumentReferences)
                .SetValidator(new DocumentReferenceValidator())
                .When(options => It.HasValues(options.DocumentReferences));
        }
    }
}