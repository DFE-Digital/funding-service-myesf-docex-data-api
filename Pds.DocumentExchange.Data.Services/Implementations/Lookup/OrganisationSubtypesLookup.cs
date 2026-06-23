using Pds.Core.Caching.Interfaces;
using Pds.Core.Caching.Models;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using System;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Lookup
{
    /// <summary>
    /// The organisation subtypes lookup.
    /// </summary>
    public class OrganisationSubtypesLookup : IOrganisationSubtypesLookup
    {
        private readonly ICacheManager _cacheManager;

        private ICacheService Cache => _cacheManager.CacheService;

        private ICacheKeyBuilder CacheKeyBuilder => _cacheManager.CacheKeyBuilder;

        private CacheOptions OrganisationTypesCachingOptions => _cacheManager.CacheOptionsProvider.OrganisationTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationSubtypesLookup"/> class.
        /// </summary>
        /// <param name="cacheManager">The cache manager.</param>
        public OrganisationSubtypesLookup(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        /// <inheritdoc/>
        public async Task<bool> Exists(string identifier)
            => !(await Get(identifier) is UnknownOrganisationTypeDisplay);

        /// <inheritdoc/>
        public async Task<DisplayValues> Get(string identifier)
        {
            It.IsEmpty(identifier)
               .AsGuard<ArgumentNullException>(nameof(identifier));

            return await Cache.Get(
                CacheKeyBuilder.BuildOrganisationSubtypeDisplayKey(identifier),
                () => Task.FromResult((DisplayValues)new UnknownOrganisationTypeDisplay()),
                OrganisationTypesCachingOptions);
        }

        /// <inheritdoc/>
        public async Task LoadOrganisationDisplays(Organisation organisation)
        {
            It.IsNull(organisation)
                .AsGuard<ArgumentNullException>();

            if (It.Has(organisation.OrganisationSubType))
            {
                await SetCache(
                    CacheKeyBuilder.BuildOrganisationSubtypeDisplayKey(organisation.OrganisationSubType),
                    organisation.OrganisationSubTypeDisplay);

                if (It.Has(organisation.OrganisationType))
                {
                    await SetCache(
                        CacheKeyBuilder.BuildOrganisationTypeDisplayBySubtypeKey(organisation.OrganisationSubType),
                        organisation.OrganisationTypeDisplay);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<DisplayValues> GetOrganisationTypeDisplay(string organisationSubtypeIdentifier)
        {
            It.IsEmpty(organisationSubtypeIdentifier)
               .AsGuard<ArgumentNullException>(nameof(organisationSubtypeIdentifier));

            return await Cache.Get(
                CacheKeyBuilder.BuildOrganisationTypeDisplayBySubtypeKey(organisationSubtypeIdentifier),
                () => Task.FromResult((DisplayValues)new UnknownOrganisationTypeDisplay()),
                OrganisationTypesCachingOptions);
        }

        private Task SetCache(string key, DisplayValues organisationTypeDisplay)
            => Cache.Set(
                key,
                () => Task.FromResult(organisationTypeDisplay),
                OrganisationTypesCachingOptions);
    }
}