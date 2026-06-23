using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pds.DocumentExchange.Data.Services.Enums;
using System;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    ///  Information about a specific users history.
    /// </summary>
    public class FileMetadataHistory
    {
        /// <summary>
        /// Gets or sets the type of action being recorded.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public FileAction Action { get; set; }

        /// <summary>
        /// Gets or sets the user the history relates to (if applicable).
        /// </summary>
        public FileMetadataUser User { get; set; }

        /// <summary>
        /// Gets or sets when the event was raised.
        /// </summary>
        public DateTime ActionDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets extra detail about the event being recorded.
        /// </summary>
        public string Message { get; set; }
    }
}