using Pds.Core.Caching.Interfaces;
using Pds.Core.Caching.Models;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.FDS;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Lookup
{
    /// <summary>
    /// The organisations lookup.
    /// </summary>
    public class OrganisationsLookup : IOrganisationsLookup
    {
        private readonly ILoggerAdapter<OrganisationsLookup> _logger;
        private readonly IOrganisationService _organisationService;

        private readonly ICacheManager _cacheManager;

        private ICacheService Cache => _cacheManager.CacheService;

        private ICacheKeyBuilder CacheKeyBuilder => _cacheManager.CacheKeyBuilder;

        private CacheOptions AllOrganisationsCachingOptions => _cacheManager.CacheOptionsProvider.AllOrganisations;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationsLookup"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="organisationService">The organisation service.</param>
        /// <param name="cacheManager">The cache manager.</param>
        public OrganisationsLookup(
             IOrganisationService organisationService,
             ILoggerAdapter<OrganisationsLookup> logger,
             ICacheManager cacheManager)
        {
            _organisationService = organisationService;
            _logger = logger;
            _cacheManager = cacheManager;
        }

        /// <inheritdoc/>
        public Task<bool> Exists(OrganisationIdentifier identifier)
        {
            try
            {
                ValidateIdentifier(identifier);

                return ExistsAsync(identifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(Exists)} for identifier: {JsonSerializer.Serialize(identifier)}");
                throw;
            }
        }

        /// <inheritdoc/>
        public Task<Organisation> Get(OrganisationIdentifier identifier)
        {
            try
            {
                ValidateIdentifier(identifier);

                return GetAsync(identifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(Get)} for identifier: {JsonSerializer.Serialize(identifier)}");
                throw;
            }
        }

        /// <inheritdoc/>
        public Task<IDictionary<string, Organisation>> GetAllOrganisations()
        {
            try
            {
                return GetAllOrganisationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(GetAllOrganisations)}");
                throw;
            }
        }

        /// <inheritdoc/>
        public Task<IEnumerable<OrganisationIdentifier>> GetSelfAndChildUkprns(
            OrganisationIdentifier identifier)
        {
            try
            {
                ValidateIdentifier(identifier);

                return GetSelfAndChildUkprnsAsync(identifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(GetSelfAndChildUkprns)} for identifier: {JsonSerializer.Serialize(identifier)}");
                throw;
            }
        }

        private static void ValidateIdentifier(OrganisationIdentifier identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }
        }

        private async Task<bool> ExistsAsync(OrganisationIdentifier identifier)
        {
            return !(await GetAsync(identifier) is UnknownOrganisation);
        }

        private async Task<Organisation> GetAsync(OrganisationIdentifier identifier)
        {
            try
            {
                var organisation = identifier.Type == OrganisationIdentifierType.Ukprn ? await _organisationService.GetOrganisation(identifier.Value) : null;

                return organisation ?? new UnknownOrganisation(identifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(GetAsync)} for identifier: {identifier}");
                throw;
            }
        }

        private async Task<IEnumerable<OrganisationIdentifier>> GetSelfAndChildUkprnsAsync(
            OrganisationIdentifier identifier)
        {
            var organisation = await GetAsync(identifier);

            var identifiers = organisation.Identifiers.ToList();

            if (organisation.ChildOrganisations?.Any() == true)
            {
                identifiers.AddRange(organisation.ChildOrganisations.SelectMany(org => org.Identifiers));
            }

            return identifiers.Where(id => id.Type == OrganisationIdentifierType.Ukprn).ToList();
        }

        private async Task<IDictionary<string, Organisation>> GetAllOrganisationsAsync()
        {
            try
            {
                var organisations = await Cache.Get(
                CacheKeyBuilder.BuildAllOrganisationsKey(),
                () => _organisationService.GetAllOrganisations(),
                AllOrganisationsCachingOptions);

                return organisations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(GetAllOrganisationsAsync)}");
                throw;
            }
        }
    }
}