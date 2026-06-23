namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>The Organisation Info.</summary>
    public class OrganisationInfo
    {
        /// <summary>
        /// Gets or sets the organisation identifier.
        /// </summary>
        public OrganisationIdentifier OrganisationIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the organisation name.
        /// </summary>
        public string Name { get; set; }
    }
}