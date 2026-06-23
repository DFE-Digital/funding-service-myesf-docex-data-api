using Newtonsoft.Json;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    ///  Information about a specific user.
    /// </summary>
    public class FileMetadataUser
    {
        /// <summary>
        /// Gets or sets user principal.
        /// </summary>
        /// <remarks>Spelled as 'Principle' to support old data.</remarks>
        [JsonProperty(PropertyName = "Principle")]
        public string Principal { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the user data is encrypted.
        /// </summary>
        public bool IsEncrypted { get; set; }
    }
}