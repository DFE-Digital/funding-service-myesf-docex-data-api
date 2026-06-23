using FluentValidation;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Models.Filters;
using System;
using System.Linq;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The filter option validator.
    /// </summary>
    /// <typeparam name="TFilterOption">The filter option type.</typeparam>
    public class FilterOptionValidator<TFilterOption> : AbstractValidator<TFilterOption>
        where TFilterOption : IFilterOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterOptionValidator{TFilterOption}"/> class.
        /// </summary>
        public FilterOptionValidator()
        {
            var validFilterOptionTypes = Enum.GetValues(typeof(FilterOptionType))
                .Cast<FilterOptionType>()
                .Select(filterOptionType => filterOptionType.ToString());

            RuleFor(filterOption => filterOption.Type)
                .Must(type => validFilterOptionTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"The filter type can only be: {string.Join(", ", validFilterOptionTypes)}.");
        }
    }
}