using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.FDS
{
    /// <summary>
    /// Class representing a fds api provider response.
    /// </summary>
    public class FdsApiResponse
    {
        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the provider data.
        /// </summary>
        public IEnumerable<Provider> Data { get; set; }
    }
}