using System;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Filters
{
    /// <summary>
    /// The range filter interface.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <typeparam name="TRange">The range type.</typeparam>
    public interface IRangeFilter<TElement, TRange> : IFilter<TElement>
        where TRange : IComparable<TRange>
    {
        /// <summary>
        /// Gets whether the element is inside of a given range.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="start">The range start.</param>
        /// <param name="end">The range end.</param>
        /// <returns>True if the element is inside the range / False otherwise.</returns>
        bool IsElementInsideRange(TElement element, TRange start, TRange end);
    }
}