using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models.Filters;
using Pds.DocumentExchange.Data.Services.Enums;
using System;
using System.Linq;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The date range filter option validator.
    /// </summary>
    public class DateRangeFilterOptionValidator : FilterOptionValidator<DateRangeFilterOption>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateRangeFilterOptionValidator"/> class.
        /// </summary>
        public DateRangeFilterOptionValidator()
            : base()
        {
            var validFilterKeys = new[] { FilterKey.UploadDate.ToString() };

            RuleFor(filterOption => filterOption.Key)
                .Must(key => validFilterKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"The date range filter key can only be: {string.Join(", ", validFilterKeys)}.");

            RuleFor(filterOption => filterOption.From)
                .NotNull()
                .When(filterOption => filterOption.To.HasValue);

            RuleFor(filterOption => filterOption.To)
                .NotNull()
                .When(filterOption => filterOption.From.HasValue);

            When(
                filterOption => filterOption.From.HasValue && filterOption.To.HasValue,
                () => RuleFor(filterOption => filterOption.To)
                        .GreaterThanOrEqualTo(filterOption => filterOption.From));
        }
    }
}