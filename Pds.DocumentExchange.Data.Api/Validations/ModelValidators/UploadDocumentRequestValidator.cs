using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The upload document request validator.
    /// </summary>
    public class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UploadDocumentRequestValidator"/> class.
        /// </summary>
        public UploadDocumentRequestValidator()
        {
            RuleFor(uploadDocumentRequest => uploadDocumentRequest.FileName)
                .NotEmpty()
                .WithMessage("The file name cannot be empty.");

            RuleFor(uploadDocumentRequest => uploadDocumentRequest.Bytes)
                .NotEmpty()
                .WithMessage("The file content cannot be empty.");

            RuleFor(uploadDocumentRequest => uploadDocumentRequest.UserInfo)
                .SetValidator(new UserInfoValidator())
                .DependentRules(() =>
                {
                    RuleFor(uploadDocumentRequest => uploadDocumentRequest.UserInfo)
                    .NotNull()
                    .WithMessage("The user information cannot be null.");
                });

            RuleFor(uploadDocumentRequest => uploadDocumentRequest.FromOrganisation)
                .SetValidator(new OrganisationIdentifierValidator())
                .DependentRules(() =>
                {
                    RuleFor(uploadDocumentRequest => uploadDocumentRequest.FromOrganisation)
                    .NotNull()
                    .WithMessage("The organisation identifier cannot be null.");
                });
        }
    }
}