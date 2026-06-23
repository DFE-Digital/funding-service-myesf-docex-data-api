using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The property type filter.
    /// </summary>
    public class ProviderTypeFilter : ListFilterBase<ExchangeDocument>
    {
        private readonly IOrganisationSubtypesLookup _organisationTypesLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderTypeFilter"/> class.
        /// </summary>
        /// <param name="elements">The collection of elements to be filtered.</param>
        /// <param name="organisationTypesLookup">The organisation types lookup.</param>
        public ProviderTypeFilter(
            IEnumerable<ExchangeDocument> elements,
            IOrganisationSubtypesLookup organisationTypesLookup)
            : base(elements)
        {
            _organisationTypesLookup = organisationTypesLookup;
        }

        /// <inheritdoc/>
        public override string FilterTitle
            => "Filter by provider type";

        /// <inheritdoc/>
        public override string FilterKey
            => Enums.FilterKey.ProviderType.ToString();

        /// <inheritdoc/>
        public override Func<ExchangeDocument, Task<string>> GetFilterValueFromElement
            => (exchangeDocument) => GetOrganisationSubTypeFromValue(exchangeDocument.Organisation);

        /// <inheritdoc/>
        public override Func<string, Task<string>> GetFilterTitleFromValue
            => value => GetOrganisationSubTypePlural(value);

        /// <inheritdoc/>
        public override Func<string, Task<string>> GetFilterCategoryFromValue
            => (value) => GetOrganisationTypePlural(value);

        /// <inheritdoc/>
        public override ListFilterType ListFilterType => ListFilterType.Group;

        private async Task<string> GetOrganisationSubTypeFromValue(Organisation organisation)
        {
            await _organisationTypesLookup.LoadOrganisationDisplays(organisation);

            return organisation.OrganisationSubType;
        }

        private async Task<string> GetOrganisationTypePlural(string value)
        {
            var organisationTypeDisplay = await _organisationTypesLookup.GetOrganisationTypeDisplay(value);
            return organisationTypeDisplay.Plural;
        }

        private async Task<string> GetOrganisationSubTypePlural(string value)
        {
            var organisationSubtypeDisplay = await _organisationTypesLookup.Get(value);
            return organisationSubtypeDisplay.Plural;
        }
    }
}