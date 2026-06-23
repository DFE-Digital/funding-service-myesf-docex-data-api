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
    /// The class implementing the methods to filter elements by applying multiple date filters.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public class DateRangeFilterService<TElement> : IDateRangeFilterService<TElement>
    {
        /// <inheritdoc/>
        public Task<FilterResult<TElement>> FilterAndGetUpdatedFilters(
            IEnumerable<TElement> elements,
            IEnumerable<IDateRangeFilter<TElement>> filters,
            IEnumerable<DateRangeFilterOption> filterOptions)
        {
            It.IsEmpty(filters)
                .AsGuard<ArgumentNullException>();

            if (It.IsNull(filterOptions))
            {
                filterOptions = Collection.Empty<DateRangeFilterOption>();
            }

            filterOptions.Any(option =>
                !filters.Any(filter => filter.FilterKey.IsEqualToIgnoreCase(option.Key)))
                    .AsGuard<KeyNotFoundException>();

            var filteredItems = It.IsEmpty(elements)
                ? Collection.Empty<TElement>()
                : Filter(elements, filters, filterOptions);

            var updatedFilters = GetUpdatedFilters(filters, filterOptions);

            var result = new FilterResult<TElement>
            {
                Items = filteredItems,
                Filters = updatedFilters
            };

            return Task.FromResult(result);
        }

        private IEnumerable<TElement> Filter(
            IEnumerable<TElement> elements,
            IEnumerable<IDateRangeFilter<TElement>> filters,
            IEnumerable<DateRangeFilterOption> filterOptions)
        {
            if (It.IsEmpty(filterOptions) || filterOptions.All(option => !option.From.HasValue && !option.To.HasValue))
            {
                return elements;
            }

            var elementsByFilterOption = filterOptions.Select(option =>
            {
                var filter = filters.Single(filter => filter.FilterKey.IsEqualToIgnoreCase(option.Key));
                return elements.Where(element => filter.IsElementInsideRange(element, option.From.Value, option.To.Value));
            })
            .Where(category => category.Any());

            return elementsByFilterOption.Any()
                ? elementsByFilterOption.Aggregate((x, y) => x.Intersect(y))
                : Collection.Empty<TElement>();
        }

        private IEnumerable<DateRangeFilter> GetUpdatedFilters(
            IEnumerable<IDateRangeFilter<TElement>> filters,
            IEnumerable<DateRangeFilterOption> filterOptions)
        {
            var updatedFilters = Collection.Empty<DateRangeFilter>();

            foreach (var currentFilter in filters)
            {
                var filter = new DateRangeFilter
                {
                    Title = currentFilter.FilterTitle,
                    Key = currentFilter.FilterKey
                };

                var filterOption = filterOptions.FirstOrDefault(option =>
                    option.Key.IsEqualToIgnoreCase(currentFilter.FilterKey));

                if (It.Has(filterOption))
                {
                    filter.From = filterOption.From;
                    filter.To = filterOption.To;
                }

                updatedFilters.Add(filter);
            }

            return updatedFilters;
        }
    }
}