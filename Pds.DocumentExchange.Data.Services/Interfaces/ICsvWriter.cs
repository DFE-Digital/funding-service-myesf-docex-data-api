using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// The CSV writer interface.
    /// </summary>
    public interface ICsvWriter
    {
        /// <summary>
        /// Writes a collection to a CSV file.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entities">The collection of entities.</param>
        /// <returns>The CSV file as a byte array.</returns>
        byte[] WriteCsvFile<TEntity>(IEnumerable<TEntity> entities);
    }
}