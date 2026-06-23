namespace Pds.DocumentExchange.Data.Api.Models.SupportTools
{
    /// <summary>
    /// The SPI refresh log request.
    /// </summary>
    public class SpiRefreshLogRequest
    {
        /// <summary>
        /// Gets or sets the UKPRN.
        /// </summary>
        public int Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the action.
        /// </summary>
        public string Email { get; set; }
    }
}
