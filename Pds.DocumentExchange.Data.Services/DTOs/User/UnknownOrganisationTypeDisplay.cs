using Pds.Core.Common.Organisation.Models;

namespace Pds.DocumentExchange.Data.Services.DTOs.User
{
    /// <summary>
    /// The unknown organisation type display.
    /// </summary>
    public class UnknownOrganisationTypeDisplay : DisplayValues
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownOrganisationTypeDisplay"/> class.
        /// </summary>
        public UnknownOrganisationTypeDisplay()
        {
            Singular = "[Unknown organisation type]";
            Plural = "[Unknown organisation types]";
        }
    }
}