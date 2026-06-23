using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The class that filters a document lists based on the specified filter keys and
    /// selected options.
    /// </summary>
    /// <typeparam name="TDocument">The document type.</typeparam>
    public class FiltersExecutionManager<TDocument> : IFiltersExecutionManager<TDocument>
    {
        private readonly IFiltersFactory<TDocument> _filtersFactory;
        private readonly IDateRangeFilterService<TDocument> _dateRangeFilterService;
        private readonly IListFilterService<TDocument> _listFilterService;
        private readonly IRadioFilterService<TDocument> _radioFilterService;
        private readonly ITextBoxFilterService<TDocument> _textBoxFilterService;

        private readonly Dictionary<FilterKey, FilterType> _filterKeysTypes = new Dictionary<FilterKey, FilterType>
        {
            [FilterKey.Status] = FilterType.ListFilter,
            [FilterKey.ProductIdList] = FilterType.ListFilter,
            [FilterKey.ProductIdRadio] = FilterType.RadioFilter,
            [FilterKey.DocumentNameError] = FilterType.ListFilter,
            [FilterKey.AcademicYear] = FilterType.ListFilter,
            [FilterKey.Organisation] = FilterType.ListFilter,
            [FilterKey.ProviderType] = FilterType.ListFilter,
            [FilterKey.Team] = FilterType.RadioFilter,
            [FilterKey.UploadDate] = FilterType.DateRangeFilter,
            [FilterKey.Ukprn] = FilterType.TextBoxFilter
        };

        private readonly Dictionary<FilterType, int> _filterTypesExecutionOrder = new Dictionary<FilterType, int>
        {
            [FilterType.ListFilter] = 0,
            [FilterType.DateRangeFilter] = 1,
            [FilterType.RadioFilter] = 2,
            [FilterType.TextBoxFilter] = 3
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FiltersExecutionManager{TDocument}"/> class.
        /// </summary>
        /// <param name="filtersFactory">The filters factory.</param>
        /// <param name="dateRangeFilterService">The date range filters service.</param>
        /// <param name="listFilterService">The list filter service.</param>
        /// <param name="radioFilterService">The radio filter service.</param>
        /// <param name="textBoxFilterService">The text-box filter service.</param>
        public FiltersExecutionManager(
            IFiltersFactory<TDocument> filtersFactory,
            IDateRangeFilterService<TDocument> dateRangeFilterService,
            IListFilterService<TDocument> listFilterService,
            IRadioFilterService<TDocument> radioFilterService,
            ITextBoxFilterService<TDocument> textBoxFilterService)
        {
            _filtersFactory = filtersFactory;
            _dateRangeFilterService = dateRangeFilterService;
            _listFilterService = listFilterService;
            _radioFilterService = radioFilterService;
            _textBoxFilterService = textBoxFilterService;
        }

        /// <inheritdoc/>
        public async Task<FilterResult<TDocument>> Filter(
            IEnumerable<TDocument> documents,
            IEnumerable<IFilterOption> filterOptions,
            IEnumerable<FilterKey> filterKeys)
        {
            var filterKeysByType = filterKeys
                                    .GroupBy(key => _filterKeysTypes[key])
                                    .OrderBy(group => _filterTypesExecutionOrder[group.Key]);

            var documentsResult = documents;
            var filtersResult = new List<IFilter>();

            foreach (var currentKeyGroup in filterKeysByType)
            {
                var filters = _filtersFactory.GetFilters(documentsResult, currentKeyGroup);
                var filterResult = await GetFilterResultByFilterType(currentKeyGroup.Key, documentsResult, filters, filterOptions);

                documentsResult = filterResult.Items;
                filtersResult.AddRange(filterResult.Filters);
            }

            return new FilterResult<TDocument>
            {
                Items = documentsResult.ToList(),
                Filters = SortFiltersByKeysOrder(filtersResult, filterKeys)
            };
        }

        private async Task<FilterResult<TDocument>> GetFilterResultByFilterType(
            FilterType filterType,
            IEnumerable<TDocument> documents,
            IEnumerable<IFilter<TDocument>> filters,
            IEnumerable<IFilterOption> filterOptions)
        {
            return filterType switch
            {
                FilterType.DateRangeFilter => await _dateRangeFilterService.FilterAndGetUpdatedFilters(
                                                        documents,
                                                        filters.OfType<IDateRangeFilter<TDocument>>(),
                                                        filterOptions.OfType<DateRangeFilterOption>()),

                FilterType.ListFilter => await _listFilterService.FilterAndGetUpdatedFilters(
                                                    documents,
                                                    filters.OfType<IListFilter<TDocument>>(),
                                                    filterOptions.OfType<ListFilterOption>()),

                FilterType.RadioFilter => await _radioFilterService.FilterAndGetUpdatedFilters(
                                                    documents,
                                                    filters.OfType<IMatchingValueSelectionFilter<TDocument>>(),
                                                    filterOptions.OfType<RadioFilterOption>()),

                FilterType.TextBoxFilter => await _textBoxFilterService.FilterAndGetUpdatedFilters(
                                                    documents,
                                                    filters.OfType<IMatchingTextBoxValueFilter<TDocument>>(),
                                                    filterOptions.OfType<TextBoxFilterOption>()),

                _ => throw new ArgumentException("The specified filter type is not valid.")
            };
        }

        private IEnumerable<IFilter> SortFiltersByKeysOrder(
            IEnumerable<IFilter> filters,
            IEnumerable<FilterKey> filterKeys)
        {
            var filterKeysList = filterKeys.Select(key => key.ToString()).ToList();
            return filters.OrderBy(filter => filterKeysList.IndexOf(filter.Key)).ToList();
        }
    }
}