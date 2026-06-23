using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models.Filters;
using Pds.DocumentExchange.Data.Services.Enums;
using System;
using System.Linq;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The text-box filter option validator.
    /// </summary>
    public class TextBoxFilterOptionValidator : FilterOptionValidator<TextBoxFilterOption>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextBoxFilterOptionValidator"/> class.
        /// </summary>
        public TextBoxFilterOptionValidator()
            : base()
        {
            var validFilterKeys = new[] { FilterKey.Ukprn.ToString() };

            RuleFor(filterOption => filterOption.Key)
                .Must(key => validFilterKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"The text-box filter key can only be: {string.Join(", ", validFilterKeys)}.");
        }
    }
}