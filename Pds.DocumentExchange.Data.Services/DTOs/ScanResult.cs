using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Represents the result of a virus scan.
    /// </summary>
    public class ScanResult
    {
        /// <summary>
        /// Gets or sets the name of the service that performed the current scan.
        /// </summary>
        public string ScanServiceName { get; set; }

        /// <summary>
        /// Gets or sets the file safety, indicating whether a virus was detected.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public FileSafety FileSafety { get; set; }

        /// <summary>
        /// Gets or sets the time taken to complete the virus scan in milliseconds.
        /// </summary>
        public long Took { get; set; }

        /// <summary>
        /// Gets or sets the results of virus scans performed as sub-operations of the current result.
        /// </summary>
        public IEnumerable<ScanResult> InnerResults { get; set; }
    }
}