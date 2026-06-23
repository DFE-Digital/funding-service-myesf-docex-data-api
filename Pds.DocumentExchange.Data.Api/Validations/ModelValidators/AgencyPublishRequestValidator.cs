using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The agency publish request validator.
    /// </summary>
    public class AgencyPublishRequestValidator : AbstractValidator<AgencyPublishRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgencyPublishRequestValidator"/> class.
        /// </summary>
        public AgencyPublishRequestValidator()
        {
            RuleFor(agencyPublishRequest => agencyPublishRequest.UserInfo)
                .NotEmpty()
                .WithMessage("The user info cannot be null.");

            RuleFor(agencyPublishRequest => agencyPublishRequest.UserInfo.Principal)
                .NotEmpty()
                .WithMessage("The user principal cannot be empty.")
                .When(agencyPublishRequest => agencyPublishRequest.UserInfo != null);
        }
    }
}