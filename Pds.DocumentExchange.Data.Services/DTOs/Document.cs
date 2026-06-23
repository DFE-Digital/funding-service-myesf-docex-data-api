using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs.User;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Abstract class representing common properties of any document in the system.
    /// </summary>
    public abstract class Document
    {
        /// <summary>
        /// Gets or sets the product.
        /// </summary>
        public Product Product { get; set; }

        /// <summary>
        /// Gets or sets the organisation information.
        /// </summary>
        public OrganisationInfo OrganisationInfo { get; set; }

        /// <summary>
        ///  Gets or sets the organisation.
        /// </summary>
        public Organisation Organisation { get; set; }

        /// <summary>
        /// Gets or sets the document year, e.g. 201920.
        /// </summary>
        public int Year { get; set; }
    }
}