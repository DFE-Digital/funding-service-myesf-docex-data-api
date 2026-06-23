using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Base class for an antivirus scanner.
    /// </summary>
    public abstract class BaseAntivirus : IAntivirus
    {
        /// <inheritdoc/>
        public async Task<ScanResult> Scan(string fileName, Stream stream)
        {
            var stopwatch = Stopwatch.StartNew();

            var fileSafety = await GetFileSafety(fileName, stream);

            stopwatch.Stop();

            return new ScanResult
            {
                ScanServiceName = AntivirusName,
                FileSafety = fileSafety,
                Took = stopwatch.ElapsedMilliseconds
            };
        }

        /// <summary>
        /// Gets the name of the Antivirus.
        /// </summary>
        protected abstract string AntivirusName { get; }

        /// <summary>
        /// Gets the safety of the given stream.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="stream">The file stream.</param>
        /// <returns>The file safety result.</returns>
        protected abstract Task<FileSafety> GetFileSafety(string fileName, Stream stream);
    }
}