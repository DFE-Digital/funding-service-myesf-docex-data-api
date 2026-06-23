using Pds.DocumentExchange.Data.Services.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelperWriter = CsvHelper.CsvWriter;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// The CSV writer implementation.
    /// </summary>
    public class CsvWriter : ICsvWriter
    {
        /// <inheritdoc/>
        public byte[] WriteCsvFile<TEntity>(IEnumerable<TEntity> entities)
        {
            byte[] result;

            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using (var csvWriter = new CsvHelperWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(entities);
                streamWriter.Flush();
                result = memoryStream.ToArray();
            }

            return result;
        }
    }
}