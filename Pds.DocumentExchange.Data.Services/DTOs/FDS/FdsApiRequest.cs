using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.FDS
{
    /// <summary>
    /// Class representing a request to FDS service.
    /// </summary>
    public class FdsApiRequest
    {
        /// <summary>
        /// Gets or sets the search criteria.
        /// </summary>
        public List<List<SearchCriteria>> SearchCriteria { get; set; } = new List<List<SearchCriteria>>();

        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the sort column.
        /// </summary>
        public string SortColumn { get; set; } = "ukprn";

        /// <summary>
        /// Gets or sets a value indicating whether the response data is on sort order descending.
        /// </summary>
        public bool SortDescending { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the response data to skip paging.
        /// </summary>
        public bool SkipPaging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the response data is spi data format.
        /// </summary>
        public bool SpiData { get; set; } = true;
    }
}