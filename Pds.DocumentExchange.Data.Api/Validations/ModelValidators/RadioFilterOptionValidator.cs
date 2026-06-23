using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models.Filters;
using Pds.DocumentExchange.Data.Services.Enums;
using System;
using System.Linq;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The radio filter option validator.
    /// </summary>
    public class RadioFilterOptionValidator : FilterOptionValidator<RadioFilterOption>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RadioFilterOptionValidator"/> class.
        /// </summary>
        public RadioFilterOptionValidator()
            : base()
        {
            var validFilterKeys = new[]
            {
                FilterKey.Team.ToString(),
                FilterKey.ProductIdRadio.ToString()
            };

            RuleFor(filterOption => filterOption.Key)
                .Must(key => validFilterKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"The radio filter key can only be: {string.Join(", ", validFilterKeys)}.");
        }
    }
}