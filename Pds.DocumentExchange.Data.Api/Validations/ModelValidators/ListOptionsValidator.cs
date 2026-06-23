using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.Services.Common.Helpers;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The list options validator.
    /// </summary>
    public class ListOptionsValidator : AbstractValidator<ListOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListOptionsValidator"/> class.
        /// </summary>
        public ListOptionsValidator()
        {
            RuleForEach(options => options.FilterOptions)
                   .SetInheritanceValidator(validator =>
                   {
                       validator.Add(new DateRangeFilterOptionValidator());
                       validator.Add(new ListFilterOptionValidator());
                       validator.Add(new RadioFilterOptionValidator());
                       validator.Add(new TextBoxFilterOptionValidator());
                   })
                   .When(options => It.HasValues(options.FilterOptions));

            RuleFor(options => options.PageNumber)
                    .GreaterThanOrEqualTo(1)
                    .WithMessage("The page number should be at least one.");

            RuleFor(options => options.PageSize)
                    .GreaterThanOrEqualTo(1)
                    .WithMessage("The page size should be at least one.");
        }
    }
}