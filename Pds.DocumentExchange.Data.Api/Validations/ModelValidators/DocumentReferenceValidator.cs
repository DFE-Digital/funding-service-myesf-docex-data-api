using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The document reference validator.
    /// </summary>
    public class DocumentReferenceValidator : AbstractValidator<DocumentReference>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentReferenceValidator"/> class.
        /// </summary>
        public DocumentReferenceValidator()
        {
            RuleFor(documentReference => documentReference.BatchIdentifier)
                .NotEmpty()
                .WithMessage("The document reference batch identifier cannot be empty.");

            RuleFor(documentReference => documentReference.ParentBatchIdentifier)
                .NotEmpty()
                .WithMessage("The document reference parent batch identifier cannot be empty.");

            RuleFor(documentReference => documentReference.FileName)
                .NotEmpty()
                .WithMessage("The document reference file name cannot be empty.");
        }
    }
}