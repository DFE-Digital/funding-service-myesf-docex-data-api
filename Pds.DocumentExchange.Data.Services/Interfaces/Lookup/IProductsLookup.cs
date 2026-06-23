using Pds.DocumentExchange.Data.Services.DTOs;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Lookup
{
    /// <summary>
    /// The products lookup interface.
    /// </summary>
    public interface IProductsLookup : ILookup<Product, int>, ILookup<Product, string>
    {
    }
}