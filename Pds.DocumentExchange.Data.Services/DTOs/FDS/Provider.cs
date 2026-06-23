using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.FDS
{
    /// <summary>
    /// Class representing a provider.
    /// </summary>
    public class Provider
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the urn.
        /// </summary>
        public int? Urn { get; set; }

        /// <summary>
        /// Gets or sets the ukprn.
        /// </summary>
        public int? Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the sub type.
        /// </summary>
        public string SubType { get; set; }

        /// <summary>
        /// Gets or sets the company house number.
        /// </summary>
        public string CompanyHouseNumber { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the group id number.
        /// </summary>
        public string GroupIdNumber { get; set; }

        /// <summary>
        /// Gets or sets the trust code.
        /// </summary>
        public string TrustCode { get; set; }

        /// <summary>
        /// Gets or sets the lacode.
        /// </summary>
        public string LaCode { get; set; }

        /// <summary>
        /// Gets or sets the management group.
        /// </summary>
        public Provider? ManagementGroup { get; set; }

        /// <summary>
        /// Gets or sets the child providers.
        /// </summary>
        public IEnumerable<Provider>? Child { get; set; }
    }
}
