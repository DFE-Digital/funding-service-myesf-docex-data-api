namespace Pds.DocumentExchange.Data.Services.DTOs.User
{
    /// <summary>
    /// User info.
    /// </summary>
    public class UserInfo
    {
        /// <inheritdoc/>
        public string Principal { get; set; }

        /// <inheritdoc/>
        public string FullName { get; set; }

        /// <inheritdoc/>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the organisation info.
        /// </summary>
        public OrganisationInfo OrganisationInfo { get; set; }

        /// <inheritdoc/>
        public bool IsViewAsProvider { get; set; }
    }
}