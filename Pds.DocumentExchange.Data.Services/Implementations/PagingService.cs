using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Methods to page collections.
    /// </summary>
    public class PagingService : IPagingService
    {
        /// <inheritdoc/>
        public IEnumerable<IEnumerable<T>> Paginate<T>(IEnumerable<T> list, int pageSize)
        {
            if (list == null)
            {
                throw new ArgumentException("The list cannot be null.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentException("The page size should be equal or greater than one.");
            }

            return list.Select((item, index) => new { PageIndex = index / pageSize, Item = item })
                        .GroupBy(item => item.PageIndex, item => item.Item).ToList();
        }
    }
}