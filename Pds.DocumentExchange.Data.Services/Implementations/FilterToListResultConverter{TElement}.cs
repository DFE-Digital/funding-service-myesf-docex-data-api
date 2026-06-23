using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Linq;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// A class that transforms FilterResult into ListResult objects.
    /// </summary>
    /// <typeparam name="TElement">The contained items type.</typeparam>
    public class FilterToListResultConverter<TElement> : IFilterToListResultConverter<TElement>
    {
        private readonly IPagingService _pagingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterToListResultConverter{TElement}"/> class.
        /// </summary>
        /// <param name="pagingService">The paging service.</param>
        public FilterToListResultConverter(IPagingService pagingService)
        {
            _pagingService = pagingService;
        }

        /// <inheritdoc/>
        public ListResult<TElement> Convert(FilterResult<TElement> filterResult, int pageSize, int pageNumber)
        {
            It.IsNull(filterResult)
                .AsGuard<ArgumentNullException>(nameof(filterResult));

            var pagedFilteredFiles = _pagingService.Paginate(filterResult.Items, pageSize);
            var selectedPageIndex = pageNumber - 1;

            var items = pagedFilteredFiles.Count() > selectedPageIndex && selectedPageIndex >= 0
                ? pagedFilteredFiles.ElementAt(selectedPageIndex)
                : Enumerable.Empty<TElement>();

            return new ListResult<TElement>
            {
                Items = items.ToList(),
                TotalItems = filterResult.Items.Count(),
                TotalPages = pagedFilteredFiles.Count(),
                Filters = filterResult.Filters
            };
        }
    }
}