using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces.Converters;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The agency document filters factory.
    /// </summary>
    public class AgencyDocumentFiltersFactory : IFiltersFactory<AgencyDocument>
    {
        private readonly IProductsLookup _productsLookup;
        private readonly IConfigurationDataService _configurationDataService;
        private readonly IConvertAgencyDocumentErrorTypes _converter;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgencyDocumentFiltersFactory"/> class.
        /// </summary>
        /// <param name="productsLookup">The products lookup.</param>
        /// <param name="configurationDataService">The configuration data service.</param>
        /// <param name="converter">The error type to string converter.</param>
        public AgencyDocumentFiltersFactory(
            IProductsLookup productsLookup,
            IConfigurationDataService configurationDataService,
            IConvertAgencyDocumentErrorTypes converter)
        {
            _productsLookup = productsLookup;
            _configurationDataService = configurationDataService;
            _converter = converter;
        }

        /// <inheritdoc/>
        public IEnumerable<IFilter<AgencyDocument>> GetFilters(
            IEnumerable<AgencyDocument> elements,
            IEnumerable<FilterKey> filterKeys)
        {
            var allFilters = Collection.Empty<IFilter<AgencyDocument>>();

            filterKeys.ForEach(filterKey
                => allFilters.Add(GetFilter(elements, filterKey)));

            return allFilters;
        }

        private IFilter<AgencyDocument> GetFilter(IEnumerable<AgencyDocument> elements, FilterKey filterKey)
        {
            return filterKey switch
            {
                FilterKey.ProductIdList => new DocumentTypeListFilter<AgencyDocument>(elements, _productsLookup),
                FilterKey.ProductIdRadio => new DocumentTypeRadioFilter<AgencyDocument>(_configurationDataService, elements),
                FilterKey.DocumentNameError => new DocumentErrorTypeFilter(elements, _converter),
                FilterKey.Team => new AgencyDocumentTeamFilter(_configurationDataService),
                _ => throw new ArgumentException("The specified filter key is not valid for the factory.")
            };
        }
    }
}