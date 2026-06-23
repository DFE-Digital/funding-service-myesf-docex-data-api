using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.Services.Common.Helpers;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The exchange list document options validator.
    /// </summary>
    public class ExchangeListDocumentOptionsValidator : AbstractValidator<ExchangeListDocumentOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeListDocumentOptionsValidator"/> class.
        /// </summary>
        public ExchangeListDocumentOptionsValidator()
        {
            RuleFor(options => options)
                .SetValidator(new ListOptionsValidator());

            RuleForEach(options => options.DocumentReferences)
                .SetValidator(new DocumentReferenceValidator())
                .When(options => It.HasValues(options.DocumentReferences));
        }
    }
}