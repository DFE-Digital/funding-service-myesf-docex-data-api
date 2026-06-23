using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Interface to provide methods to validate agency documents.
    /// </summary>
    public interface IAgencyDocumentValidator
    {
        /// <summary>
        /// Validates a document.
        /// </summary>
        /// <param name="team">The team name.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="organisations">The organisations.</param>
        /// <returns>The agency document error type.</returns>
        Task<AgencyDocumentErrorType> ValidateDocument(string team, string fileName, IDictionary<string, Organisation> organisations);
    }
}