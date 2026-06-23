namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>
    /// Information about the user performing an action.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// Gets or sets the principal.
        /// </summary>
        public string Principal { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the organisation information.
        /// </summary>
        public OrganisationInfo OrganisationInfo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is accessing as View As Provider.
        /// </summary>
        public bool IsViewAsProvider { get; set; }
    }
}