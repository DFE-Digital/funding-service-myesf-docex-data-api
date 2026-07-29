using MapsterMapper;
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
    /// The class implementing the methods to filter elements by applying multiple radio filters.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public class RadioFilterService<TElement> : MatchingValueFilterService<TElement>, IRadioFilterService<TElement>
    {
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="RadioFilterService{TElement}"/> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        public RadioFilterService(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<FilterResult<TElement>> FilterAndGetUpdatedFilters(
            IEnumerable<TElement> elements,
            IEnumerable<IMatchingValueSelectionFilter<TElement>> filters,
            IEnumerable<RadioFilterOption> filterOptions)
        {
            It.IsEmpty(filters)
                .AsGuard<ArgumentNullException>();

            if (It.IsNull(filterOptions))
            {
                filterOptions = Collection.Empty<RadioFilterOption>();
            }

            filterOptions.Any(option =>
                !filters.Any(filter => filter.FilterKey.IsEqualToIgnoreCase(option.Key)))
                    .AsGuard<KeyNotFoundException>();

            var updatedFilters = await GetUpdatedFilters(filters, filterOptions);
            var updatedFilterOptions = _mapper.Map<IEnumerable<RadioFilter>, IEnumerable<RadioFilterOption>>(updatedFilters);

            var filteredItems = It.IsEmpty(elements)
                ? Collection.Empty<TElement>()
                : await Filter(elements, filters, updatedFilterOptions);

            return new FilterResult<TElement>
            {
                Items = filteredItems,
                Filters = updatedFilters
            };
        }

        private async Task<IEnumerable<RadioFilter>> GetUpdatedFilters(
            IEnumerable<IMatchingValueSelectionFilter<TElement>> filters,
            IEnumerable<RadioFilterOption> filterOptions)
        {
            var updatedFilters = Collection.Empty<RadioFilter>();

            foreach (var currentFilter in filters)
            {
                var titleValuePairs = await currentFilter.GetAllTitleValuePairs();

                var filterOption = filterOptions.FirstOrDefault(option =>
                    option.Key.IsEqualToIgnoreCase(currentFilter.FilterKey));

                var filter = new RadioFilter
                {
                    Title = currentFilter.FilterTitle,
                    Key = currentFilter.FilterKey,
                    Values = titleValuePairs.Select(pair => new RadioFilterValue
                    {
                        Title = pair.Title,
                        Value = pair.Value,
                        Selected = filterOption != null && pair.Value.IsEqualToIgnoreCase(filterOption.Value)
                    })
                    .ToList()
                };

                if (filter.Values.All(value => !value.Selected))
                {
                    var firstValue = filter.Values.First();
                    firstValue.Selected = true;
                }

                updatedFilters.Add(filter);
            }

            return updatedFilters;
        }
    }
}