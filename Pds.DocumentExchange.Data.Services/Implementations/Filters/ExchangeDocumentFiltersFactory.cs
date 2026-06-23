using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The exchange document filters factory.
    /// </summary>
    public class ExchangeDocumentFiltersFactory : IFiltersFactory<ExchangeDocument>
    {
        private readonly IProductsLookup _productsLookup;
        private readonly IConfigurationDataService _configurationDataService;
        private readonly IOrganisationSubtypesLookup _organisationSubtypesLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeDocumentFiltersFactory"/> class.
        /// </summary>
        /// <param name="productsLookup">The products lookup.</param>
        /// <param name="configurationDataService">The configuration data service.</param>
        /// <param name="organisationSubtypesLookup">The organisation subtypes lookup.</param>
        public ExchangeDocumentFiltersFactory(
            IProductsLookup productsLookup,
            IConfigurationDataService configurationDataService,
            IOrganisationSubtypesLookup organisationSubtypesLookup)
        {
            _productsLookup = productsLookup;
            _configurationDataService = configurationDataService;
            _organisationSubtypesLookup = organisationSubtypesLookup;
        }

        /// <inheritdoc/>
        public IEnumerable<IFilter<ExchangeDocument>> GetFilters(
            IEnumerable<ExchangeDocument> elements,
            IEnumerable<FilterKey> filterKeys)
        {
            var allFilters = Collection.Empty<IFilter<ExchangeDocument>>();

            filterKeys.ForEach(filterKey
                => allFilters.Add(GetFilter(elements, filterKey)));

            return allFilters;
        }

        private IFilter<ExchangeDocument> GetFilter(IEnumerable<ExchangeDocument> elements, FilterKey filterKey)
        {
            return filterKey switch
            {
                FilterKey.Status => new StatusTypeFilter(elements),
                FilterKey.Organisation => new OrganisationFilter(elements),
                FilterKey.ProductIdList => new DocumentTypeListFilter<ExchangeDocument>(elements, _productsLookup),
                FilterKey.AcademicYear => new AcademicYearFilter(elements),
                FilterKey.Team => new ExchangeDocumentTeamFilter(_configurationDataService),
                FilterKey.ProviderType => new ProviderTypeFilter(elements, _organisationSubtypesLookup),
                FilterKey.UploadDate => new UploadedDateRangeFilter(),
                FilterKey.Ukprn => new UkprnFilter(),
                _ => throw new ArgumentException("The specified filter key is not valid for the factory.")
            };
        }
    }
}