using FluentValidation;
using Pds.DocumentExchange.Data.Api.Models.Filters;
using Pds.DocumentExchange.Data.Services.Enums;
using System;
using System.Linq;

namespace Pds.DocumentExchange.Data.Api.Validations.ModelValidators
{
    /// <summary>
    /// The list filter option validator.
    /// </summary>
    public class ListFilterOptionValidator : FilterOptionValidator<ListFilterOption>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListFilterOptionValidator"/> class.
        /// </summary>
        public ListFilterOptionValidator()
            : base()
        {
            var validFilterKeys = new[]
            {
                FilterKey.Status,
                FilterKey.ProductIdList,
                FilterKey.DocumentNameError,
                FilterKey.AcademicYear,
                FilterKey.Organisation,
                FilterKey.ProviderType
            }
            .Select(key => key.ToString());

            RuleFor(filterOption => filterOption.Key)
                .Must(key => validFilterKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"The list filter key can only be: {string.Join(", ", validFilterKeys)}.");
        }
    }
}