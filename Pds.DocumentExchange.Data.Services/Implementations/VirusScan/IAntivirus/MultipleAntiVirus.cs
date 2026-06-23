using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Methods to scan a file using multiple antiviruses.
    /// </summary>
    public class MultipleAntivirus : IAntivirus
    {
        private readonly List<IAntivirus> _antiVirusList;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleAntivirus"/> class.
        /// </summary>
        /// <param name="antiViruses">The additional antiviruses.</param>
        public MultipleAntivirus(IEnumerable<IAntivirus> antiViruses)
        {
            _antiVirusList = new List<IAntivirus>(antiViruses);
        }

        /// <inheritdoc/>
        public async Task<ScanResult> Scan(string fileName, Stream stream)
        {
            var stopwatch = Stopwatch.StartNew();

            var scanTasks = _antiVirusList.Select(antivirus => ScanFile(antivirus, fileName, stream));
            var innerScanResults = await Task.WhenAll(scanTasks);
            var fileSafety = AggregateFileSafety(innerScanResults);

            stopwatch.Stop();

            return new ScanResult
            {
                ScanServiceName = nameof(MultipleAntivirus),
                FileSafety = fileSafety,
                Took = stopwatch.ElapsedMilliseconds,
                InnerResults = innerScanResults
            };
        }

        private async Task<ScanResult> ScanFile(IAntivirus antivirus, string fileName, Stream stream)
        {
            Stream streamCopy = new MemoryStream();

            streamCopy.Position = 0;
            await stream.CopyToAsync(streamCopy);

            streamCopy.Position = 0;
            return await antivirus.Scan(fileName, streamCopy);
        }

        private FileSafety AggregateFileSafety(IEnumerable<ScanResult> scanResults)
        {
            var result = FileSafety.Okay;

            var fileSafeties = scanResults.Select(scanResult => scanResult.FileSafety).ToList();

            bool virusFound = fileSafeties.Contains(FileSafety.Virus);
            if (virusFound)
            {
                result = FileSafety.Virus;
            }
            else
            {
                bool errorOccurred = fileSafeties.Contains(FileSafety.InternalError);
                if (errorOccurred)
                {
                    result = FileSafety.InternalError;
                }
            }

            return result;
        }
    }
}