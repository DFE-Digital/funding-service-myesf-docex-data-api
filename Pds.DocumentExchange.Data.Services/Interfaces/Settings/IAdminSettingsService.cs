using Pds.DocumentExchange.Data.Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Settings
{
    /// <summary>
    /// Interface exposing methods to query and update the Document Exchange configuration data.
    /// </summary>
    public interface IAdminSettingsService : ISettingsReader, ISettingsWriter
    {
        /// <summary>
        /// Gets the products that the organisation can upload.
        /// </summary>
        /// <returns>The action result containing the list of supported products if successful.</returns>
        Task<IEnumerable<Product>> GetProductsThatOrganisationsCanUpload();

        /// <summary>
        /// Gets the product by its identifier.
        /// </summary>
        /// <param name="identifier">The product identifier.</param>
        /// <returns>The product.</returns>
        Task<Product> GetProduct(int identifier);

        /// <summary>
        /// Gets the cache warm-up reject invalid data from SPI setting value.
        /// </summary>
        /// <returns>The reject invalid data setting value.</returns>
        Task<bool> GetCacheWarmUpRejectSpiInvalidData();
    }
}