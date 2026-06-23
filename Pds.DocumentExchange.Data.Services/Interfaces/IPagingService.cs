using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Interface providing methods to page collections.
    /// </summary>
    public interface IPagingService
    {
        /// <summary>
        /// Returns a list that contains the paged sublists in this instance according to the specified page size.
        /// </summary>
        /// <typeparam name="T">The type of items contained in the list.</typeparam>
        /// <param name="list">The IEnumerable instance.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>An IEnumerable whose elements contain the sublists in this instance according to the specified page size.</returns>
        /// /// <exception cref="ArgumentException">
        ///     If the IEnumerable instance is null.
        ///     If the page size is smaller than one.
        /// </exception>
        IEnumerable<IEnumerable<T>> Paginate<T>(IEnumerable<T> list, int pageSize);
    }
}