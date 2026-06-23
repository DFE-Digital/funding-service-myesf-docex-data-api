using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The document type radio filter.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public class DocumentTypeRadioFilter<TElement> : IMatchingValueSelectionFilter<TElement>
        where TElement : Document
    {
        private readonly IConfigurationDataService _configurationDataService;
        private readonly IEnumerable<TElement> _elements;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTypeRadioFilter{TElement}"/> class.
        /// </summary>
        /// <param name="configurationDataService"><see cref="IConfigurationDataService"/>.</param>
        /// <param name="elements">The collection of elements to be filtered.</param>
        public DocumentTypeRadioFilter(
            IConfigurationDataService configurationDataService,
            IEnumerable<TElement> elements)
        {
            _configurationDataService = configurationDataService;
            _elements = elements;
        }

        /// <inheritdoc/>
        public string FilterTitle
            => "Select a document type";

        /// <inheritdoc/>
        public string FilterKey
            => Enums.FilterKey.ProductIdRadio.ToString();

        /// <inheritdoc/>
        public FilterType FilterType
            => FilterType.RadioFilter;

        /// <inheritdoc/>
        public Task<bool> DoesElementMatchValue(TElement element, string value)
        {
            var productId = (element.Product ?? new UnknownProduct()).Identifier;

            return Task.FromResult(productId.ToString() == value);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<(string Title, string Value)>> GetAllTitleValuePairs()
        {
            var products = await _configurationDataService.GetProducts();
            var unknownProduct = new UnknownProduct();

            var productIds = _elements
                .Select(e => (e.Product ?? unknownProduct).Identifier)
                .Distinct()
                .ToHashSet();

            return products
                .Append(unknownProduct)
                .Where(p => productIds.Contains(p.Identifier))
                .Select(product => (Title: product.PluralName, Value: product.Identifier.ToString()));
        }
    }
}