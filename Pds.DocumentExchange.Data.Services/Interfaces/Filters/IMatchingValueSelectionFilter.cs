using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Filters
{
    /// <summary>
    /// The matching value from selection filter.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    public interface IMatchingValueSelectionFilter<TElement> : IMatchingValueFilter<TElement>
    {
        /// <summary>
        /// Gets all filter titles along with their respective values.
        /// </summary>
        /// <returns>A collection of all title and value pairs.</returns>
        Task<IEnumerable<(string Title, string Value)>> GetAllTitleValuePairs();
    }
}