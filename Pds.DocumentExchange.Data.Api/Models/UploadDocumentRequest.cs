namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>A model containing the details of the file being uploaded.</summary>
    public class UploadDocumentRequest
    {
        /// <summary>Gets or sets the name of the file.</summary>
        public string FileName { get; set; }

        /// <summary>Gets or sets the 'bytes' - that is the actual content of the file.</summary>
        public byte[] Bytes { get; set; }

        /// <summary>Gets or sets the user information.</summary>
        public UserInfo UserInfo { get; set; }

        /// <summary>Gets or sets the product identifier.</summary>
        public int ProductIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the organisation on behalf of which the document is being uploaded.
        /// </summary>
        public OrganisationIdentifier FromOrganisation { get; set; }
    }
}