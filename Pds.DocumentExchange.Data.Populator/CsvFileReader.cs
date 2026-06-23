using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator
{
    /// <summary>
    /// The CSV file reader.
    /// </summary>
    public class CsvFileReader
    {
        /// <summary>
        /// Reads a file.
        /// </summary>
        /// <typeparam name="TRecord">The record type.</typeparam>
        /// <param name="file">The file.</param>
        /// <returns>An awaitable <see cref="Task"/> returning the records collection.</returns>
        public async Task<IEnumerable<TRecord>> Read<TRecord>(IFormFile file)
        {
            var fileContent = await ReadFileContent(file);
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null
            };

            using var stringReader = new StringReader(fileContent);
            using var csvReader = new CsvReader(stringReader, csvConfig);

            return csvReader.GetRecords<TRecord>().ToList();
        }

        /// <summary>
        /// Reads the file content.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>An awaitable <see cref="Task"/> returning the file content.</returns>
        private async Task<string> ReadFileContent(IFormFile file)
        {
            using var fileContentStream = new MemoryStream();

            await file.CopyToAsync(fileContentStream);
            return Encoding.UTF8.GetString(fileContentStream.ToArray());
        }
    }
}