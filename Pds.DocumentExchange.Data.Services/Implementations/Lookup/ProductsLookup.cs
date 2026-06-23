using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Lookup
{
    /// <summary>
    /// The products lookup.
    /// </summary>
    public class ProductsLookup : IProductsLookup
    {
        private readonly IConfigurationDataService _configurationDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductsLookup"/> class.
        /// </summary>
        /// <param name="configurationDataService">The configuration data service.</param>
        public ProductsLookup(IConfigurationDataService configurationDataService)
        {
            _configurationDataService = configurationDataService;
        }

        /// <inheritdoc/>
        public async Task<bool> Exists(int identifier)
            => !(await Get(identifier) is UnknownProduct);

        /// <inheritdoc/>
        public async Task<bool> Exists(string identifier)
            => !(await Get(identifier) is UnknownProduct);

        /// <inheritdoc/>
        public async Task<Product> Get(int identifier)
        {
            var allProducts = await _configurationDataService.GetProducts();
            return allProducts.FirstOrDefault(p => p.Identifier == identifier) ?? new UnknownProduct();
        }

        /// <inheritdoc/>
        public async Task<Product> Get(string identifier)
        {
            if (It.IsEmpty(identifier) || !int.TryParse(identifier, out int identifierAsInteger))
            {
                return new UnknownProduct();
            }

            return await Get(identifierAsInteger);
        }
    }
}