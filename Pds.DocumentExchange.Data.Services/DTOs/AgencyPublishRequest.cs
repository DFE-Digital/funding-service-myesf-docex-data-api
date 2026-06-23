using Pds.DocumentExchange.Data.Services.DTOs.User;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Request object representing an agency team user's request to publish a product.
    /// </summary>
    public class AgencyPublishRequest
    {
        /// <summary>
        /// Gets or sets the product ID to publish.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets information about the user who is publishing.
        /// </summary>
        public UserInfo UserInfo { get; set; }
    }
}