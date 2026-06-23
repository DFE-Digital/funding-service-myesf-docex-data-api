using Pds.DocumentExchange.Data.Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// An interface to provide the option of publishing documents.
    /// </summary>
    public interface IDocumentPublisher
    {
        /// <summary>
        /// Publishes a list of documents.
        /// </summary>
        /// <param name="team">The name of agency team.</param>
        /// <param name="agencyPublishRequest">The agency publish request.</param>
        /// <returns>An awaitable task.</returns>
        Task<KeyValuePair<Product, int>> PublishDocuments(
            string team,
            AgencyPublishRequest agencyPublishRequest);
    }
}