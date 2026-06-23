using System;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Filters
{
    /// <summary>
    /// The date range filter.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    public interface IDateRangeFilter<TElement> : IRangeFilter<TElement, DateTime>
    {
    }
}