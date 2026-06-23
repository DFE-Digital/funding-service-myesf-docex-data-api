using System;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>
    /// The parent organisation UKPRs database record.
    /// </summary>
    public class ParentOrganisationUkprns
    {
        /// <summary>
        /// Gets or sets the parent UKPRNs.
        /// </summary>
        public IEnumerable<int> ParentUkprns { get; set; }

        /// <summary>
        /// Gets or sets the load date and time in UTC format.
        /// </summary>
        public DateTime LoadDateTimeUtc { get; set; }
    }
}