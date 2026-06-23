using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.Extensions;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The class implementing the methods to filter elements by applying multiple text-box filters.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public class TextBoxFilterService<TElement> : MatchingValueFilterService<TElement>, ITextBoxFilterService<TElement>
    {
        /// <inheritdoc/>
        public async Task<FilterResult<TElement>> FilterAndGetUpdatedFilters(
            IEnumerable<TElement> elements,
            IEnumerable<IMatchingTextBoxValueFilter<TElement>> filters,
            IEnumerable<TextBoxFilterOption> filterOptions)
        {
            It.IsEmpty(filters)
                .AsGuard<ArgumentNullException>();

            if (It.IsNull(filterOptions))
            {
                filterOptions = Collection.Empty<TextBoxFilterOption>();
            }

            filterOptions.Any(option =>
                !filters.Any(filter => filter.FilterKey.IsEqualToIgnoreCase(option.Key)))
                    .AsGuard<KeyNotFoundException>();

            var filteredItems = It.IsEmpty(elements)
                ? Collection.Empty<TElement>()
                : await Filter(elements, filters, filterOptions);

            var updatedFilters = GetUpdatedFilters(filters, filterOptions);

            return new FilterResult<TElement>
            {
                Items = filteredItems,
                Filters = updatedFilters
            };
        }

        private IEnumerable<TextBoxFilter> GetUpdatedFilters(
            IEnumerable<IMatchingTextBoxValueFilter<TElement>> filters,
            IEnumerable<TextBoxFilterOption> filterOptions)
        {
            var updatedFilters = Collection.Empty<TextBoxFilter>();

            foreach (var currentFilter in filters)
            {
                var filterOption = filterOptions.FirstOrDefault(option =>
                    option.Key.IsEqualToIgnoreCase(currentFilter.FilterKey));

                var filter = new TextBoxFilter
                {
                    Title = currentFilter.FilterTitle,
                    Key = currentFilter.FilterKey,
                    Hint = currentFilter.TextBoxHint,
                    Regex = currentFilter.Regex,
                    ValidationErrorMessage = currentFilter.ValidationErrorMessage,
                    Value = filterOption != null ? filterOption.Value : string.Empty
                };

                updatedFilters.Add(filter);
            }

            return updatedFilters;
        }
    }
}