using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Extensions;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The class implementing the methods to filter elements by applying multiple list filters.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public class ListFilterService<TElement> : IListFilterService<TElement>
    {
        private readonly IEqualityComparer<TElement> _elementEqualityComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListFilterService{TElement}"/> class.
        /// </summary>
        public ListFilterService()
            : this(EqualityComparer<TElement>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListFilterService{TElement}"/> class.
        /// </summary>
        /// <param name="elementEqualityComparer">The element equality comparer.</param>
        public ListFilterService(IEqualityComparer<TElement> elementEqualityComparer)
        {
            _elementEqualityComparer = elementEqualityComparer;
        }

        /// <inheritdoc/>
        public async Task<FilterResult<TElement>> FilterAndGetUpdatedFilters(
            IEnumerable<TElement> elements,
            IEnumerable<IListFilter<TElement>> filters,
            IEnumerable<ListFilterOption> filterOptions)
        {
            It.IsEmpty(filters)
                .AsGuard<ArgumentNullException>();

            if (It.IsNull(filterOptions))
            {
                filterOptions = Collection.Empty<ListFilterOption>();
            }

            var updatedFilterOptions = filterOptions.ToList();
            updatedFilterOptions.RemoveAll(option =>
             !filters.Any(filter => filter.FilterKey.IsEqualToIgnoreCase(option.Key)));

            var filteredItems = It.IsEmpty(elements)
                ? Collection.Empty<TElement>()
                : await Filter(elements, filters, updatedFilterOptions);

            var updatedFilters = await GetUpdatedFilters(filters, updatedFilterOptions);

            return new FilterResult<TElement>
            {
                Items = filteredItems,
                Filters = updatedFilters
            };
        }

        private async Task<IEnumerable<TElement>> Filter(
            IEnumerable<TElement> elements,
            IEnumerable<IListFilter<TElement>> filters,
            IEnumerable<ListFilterOption> filterOptions)
        {
            if (It.IsEmpty(filterOptions) || filterOptions.All(f => It.IsEmpty(f.Values)))
            {
                return elements;
            }

            var elementsByValueTasks = filterOptions.Select(options => GetFilterElementsByValue(options, filters));

            var elementsByCategories = await Task.WhenAll(elementsByValueTasks);
            elementsByCategories = elementsByCategories.Where(category => category.Any()).ToArray();

            return IntersectCollections(elementsByCategories);
        }

        private async Task<IEnumerable<ListFilter>> GetUpdatedFilters(
            IEnumerable<IListFilter<TElement>> filters,
            IEnumerable<ListFilterOption> filterOptions)
        {
            var updatedFilters = Collection.Empty<ListFilter>();

            foreach (var currentFilter in filters)
            {
                var filter = new ListFilter
                {
                    Title = currentFilter.FilterTitle,
                    Key = currentFilter.FilterKey,
                    Values = Collection.Empty<FilterValue>(),
                    Groups = Collection.Empty<FilterGroup>()
                };

                var filterValues = Collection.Empty<FilterValue>();

                var stringFilterValues = await currentFilter.GetFilterValues();

                foreach (var currenstring in stringFilterValues)
                {
                    var filterValue = await CreateFilterValue(currentFilter, currenstring, filters, filterOptions);
                    filterValues.Add(filterValue);
                }

                PopulateFilterValues(filter, currentFilter.ListFilterType, filterValues);
                updatedFilters.Add(filter);
            }

            return updatedFilters;
        }

        private async Task<FilterValue> CreateFilterValue(
            IListFilter<TElement> filter,
            string filterValue,
            IEnumerable<IListFilter<TElement>> filters,
            IEnumerable<ListFilterOption> filterOptions)
        {
            return new FilterValue
            {
                Title = await filter.GetFilterTitleFromValue(filterValue),
                Value = filterValue,
                Selected = IsFilterValueSelected(filter, filterValue, filterOptions),
                Count = await GetFilterValueCount(filter.FilterKey, filterValue, filters, filterOptions),
                Category = await filter.GetFilterCategoryFromValue(filterValue)
            };
        }

        private void PopulateFilterValues(ListFilter filter, ListFilterType filterType, IEnumerable<FilterValue> filterValues)
        {
            var orderedFilterValues = filterValues.OrderBy(filterValue => filterValue.Title);

            if (filterType == ListFilterType.Group)
            {
                filter.Groups = orderedFilterValues
                                .GroupBy(filterValue => filterValue.Category)
                                .Select(group => new FilterGroup
                                {
                                    Title = group.Key,
                                    Values = group.AsEnumerable()
                                })
                                .OrderBy(group => group.Title)
                                .ToList();
            }
            else
            {
                filter.Values = orderedFilterValues.ToList();
            }
        }

        private async Task<IEnumerable<TElement>> GetFilterElementsByValue(
            ListFilterOption filterOption,
            IEnumerable<IListFilter<TElement>> filters)
        {
            var filter = filters.Single(filter => filter.FilterKey.IsEqualToIgnoreCase(filterOption.Key));
            return await filter.GetElementsByFilterValue(filterOption.Values);
        }

        private bool IsFilterValueSelected(
            IListFilter<TElement> filter,
            string filterValue,
            IEnumerable<ListFilterOption> filterOptions)
                => filterOptions.Any(option =>
                    option.Key.IsEqualToIgnoreCase(filter.FilterKey)
                    && option.Values.Contains(filterValue));

        private async Task<int> GetFilterValueCount(
            string filterKey,
            string filterValue,
            IEnumerable<IListFilter<TElement>> filters,
            IEnumerable<ListFilterOption> filterOptions)
        {
            var filter = filters.Single(filter => filter.FilterKey.IsEqualToIgnoreCase(filterKey));
            var filterElements = await filter.GetElementsByFilterValue(filterValue);

            var elementsByOtherCategoriesTasks = filterOptions
                                                    .Where(option => !option.Key.IsEqualToIgnoreCase(filterKey))
                                                    .Select(option => GetFilterElementsByValue(option, filters));

            var elementsByOtherCategories = await Task.WhenAll(elementsByOtherCategoriesTasks);
            elementsByOtherCategories = elementsByOtherCategories.Where(category => category.Any()).ToArray();

            var intersectedElements = elementsByOtherCategories.Any()
                ? IntersectCollections(elementsByOtherCategories, filterElements)
                : filterElements;

            return intersectedElements.Count();
        }

        private IEnumerable<TElement> IntersectCollections(
            IEnumerable<IEnumerable<TElement>> elements,
            params IEnumerable<TElement>[] extraElements)
        {
            var allElements = elements.Concat(extraElements);

            return allElements.Any()
                ? allElements.Aggregate((x, y) => x.Intersect(y, _elementEqualityComparer))
                : Collection.Empty<TElement>();
        }
    }
}