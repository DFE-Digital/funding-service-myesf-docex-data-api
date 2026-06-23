using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The user info validator.
    /// </summary>
    public class UserInfoValidator : AbstractValidator<UserInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfoValidator"/> class.
        /// </summary>
        public UserInfoValidator()
        {
            RuleFor(userInfo => userInfo.Principal)
                .NotEmpty()
                .WithMessage("The user principal cannot be empty.");

            RuleFor(userInfo => userInfo.OrganisationInfo)
                .SetValidator(new OrganisationInfoValidator())
                .DependentRules(() =>
                {
                    RuleFor(x => x.OrganisationInfo)
                    .NotNull()
                    .WithMessage("The organisation info cannot be null.");
                });
        }
    }
}