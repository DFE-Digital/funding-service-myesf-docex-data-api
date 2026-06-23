using System;

namespace Pds.DocumentExchange.Data.Api.Models.SupportTools
{
    /// <summary>
    /// The SPI refresh log response.
    /// </summary>
    public class SpiRefreshLogResponse
    {
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
