using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models;
using System.Linq;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The exchange document download request validator.
    /// </summary>
    public class ExchangeDocumentDownloadRequestValidator : AbstractValidator<ExchangeDocumentDownloadRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeDocumentDownloadRequestValidator"/> class.
        /// </summary>
        public ExchangeDocumentDownloadRequestValidator()
        {
            RuleFor(agencyPublishRequest => agencyPublishRequest.UserInfo)
                .NotEmpty()
                .WithMessage("The user info cannot be null.");

            RuleFor(agencyPublishRequest => agencyPublishRequest.UserInfo.Principal)
                .NotEmpty()
                .WithMessage("The user principal cannot be empty.")
                .When(agencyPublishRequest => agencyPublishRequest.UserInfo != null);

            RuleForEach(options => options.ListOptions.DocumentReferences)
                .SetValidator(new DocumentReferenceValidator())
                .When(options => options.ListOptions?.DocumentReferences?.Any() == true);
        }
    }
}