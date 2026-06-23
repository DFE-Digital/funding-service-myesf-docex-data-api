using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The document type list filter.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public class DocumentTypeListFilter<TElement> : ListFilterBase<TElement>
        where TElement : Document
    {
        private readonly IProductsLookup _productsLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTypeListFilter{TElement}"/> class.
        /// </summary>
        /// <param name="elements">The collection of elements to be filtered.</param>
        /// <param name="productsLookup">The products lookup.</param>
        public DocumentTypeListFilter(IEnumerable<TElement> elements, IProductsLookup productsLookup)
            : base(elements)
        {
            _productsLookup = productsLookup;
        }

        /// <inheritdoc/>
        public override string FilterTitle
            => "Filter by document type";

        /// <inheritdoc/>
        public override string FilterKey
            => Enums.FilterKey.ProductIdList.ToString();

        /// <inheritdoc/>
        public override Func<TElement, Task<string>> GetFilterValueFromElement
            => (element) =>
            {
                var productId = element.Product?.Identifier ?? -1;

                return Task.FromResult(productId.ToString());
            };

        /// <inheritdoc/>
        public override Func<string, Task<string>> GetFilterTitleFromValue
            => GetProductPluralNameById;

        private async Task<string> GetProductPluralNameById(string value)
        {
            var product = await _productsLookup.Get(value);

            return product.PluralName;
        }
    }
}