using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Lookup
{
    /// <summary>
    /// Interface providing common lookup methods.
    /// </summary>
    /// <typeparam name="TElement">The element type to lookup.</typeparam>
    /// <typeparam name="TIdentifier">The element identifier type.</typeparam>
    public interface ILookup<TElement, TIdentifier>
    {
        /// <summary>
        /// Determines whether an element exists or not by the specified identifier.
        /// </summary>
        /// <param name="identifier">The element identifier.</param>
        /// <returns>True if the element exists / False otherwise.</returns>
        Task<bool> Exists(TIdentifier identifier);

        /// <summary>
        /// Gets the element by the specified identifier.
        /// </summary>
        /// <param name="identifier">The element identifier.</param>
        /// <returns>The element with the specified identifier / Default element if it doesn't exist.</returns>
        Task<TElement> Get(TIdentifier identifier);
    }
}