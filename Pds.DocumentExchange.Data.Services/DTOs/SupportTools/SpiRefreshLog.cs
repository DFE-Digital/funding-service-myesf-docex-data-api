using Newtonsoft.Json;
using Pds.DocumentExchange.Data.Repository.DTOs;
using System;

namespace Pds.DocumentExchange.Data.Services.DTOs.SupportTools
{
    /// <summary>
    /// The SPI refresh log Cosmos Db document.
    /// </summary>
    public class SpiRefreshLog : CosmosDbDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpiRefreshLog"/> class.
        /// </summary>
        public SpiRefreshLog()
        {
            Id = Guid.NewGuid().ToString();
            ActionDateTimeUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the cosmos document type.
        /// </summary>
        [JsonProperty(PropertyName = "documentType")]
        public string DocumentType { get; set; } = nameof(SpiRefreshLog);

        /// <summary>
        /// Gets or sets the UKPRN.
        /// </summary>
        public int Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the action.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the action was performed.
        /// </summary>
        public DateTime ActionDateTimeUtc { get; set; }
    }
}