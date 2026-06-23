using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;

namespace Pds.DocumentExchange.Data.Services.Implementations.Caching
{
    /// <inheritdoc cref="ICacheKeyBuilder"/>
    public class CacheKeyBuilder : ICacheKeyBuilder
    {
        private const string CacheKeyPrefix = "DocEx";

        /// <inheritdoc/>
        public string BuildAgencyTeamFileShareKey(string team)
        {
            return $"{CacheKeyPrefix}:AgencyTeam:{team}:FileShareItems";
        }

        /// <inheritdoc/>
        public string BuildAgencyTeamExchangedDocumentsKey(string team, ExchangeDocumentDirection direction)
        {
            return $"{CacheKeyPrefix}:AgencyTeam:{team}:ExchangedDocuments:{direction}";
        }

        /// <inheritdoc/>
        public string BuildAgencyDocumentValidationKey(string team, string fileName)
        {
            return $"{CacheKeyPrefix}:AgencyTeam:{team}:FileShareItems:{fileName}:ValidationResult";
        }

        /// <inheritdoc/>
        public string BuildOrganisationExchangedDocumentsKey(OrganisationIdentifier organisationIdentifier, ExchangeDocumentDirection direction)
        {
            return $"{CacheKeyPrefix}:Organisation:{organisationIdentifier.Type}_{organisationIdentifier.Value}:ExchangedDocuments:{direction}";
        }

        /// <inheritdoc/>
        public string BuildConfigurationKey(ConfigurationSection configurationSection)
        {
            return $"{CacheKeyPrefix}:Configuration:{configurationSection}";
        }

        /// <inheritdoc/>
        public string BuildListConfigurationKey(ConfigurationSection configurationSection)
        {
            return $"{CacheKeyPrefix}:ListConfiguration:{configurationSection}";
        }

        /// <inheritdoc/>
        public string BuildEmailResourceLockKey(string parentBatchId)
        {
            return $"{CacheKeyPrefix}:EmailLock:{parentBatchId}";
        }

        /// <inheritdoc/>
        public string BuildOrganisationKey(OrganisationIdentifier organisationIdentifier)
        {
            return $"Organisation:{organisationIdentifier.Type}_{organisationIdentifier.Value}";
        }

        /// <inheritdoc/>
        public string BuildOrganisationSubtypeDisplayKey(string organisationSubtype)
        {
            return $"{CacheKeyPrefix}:OrganisationSubtype:{organisationSubtype}";
        }

        /// <inheritdoc/>
        public string BuildOrganisationTypeDisplayBySubtypeKey(string organisationSubtype)
        {
            return $"{CacheKeyPrefix}:OrganisationType:{organisationSubtype}";
        }

        /// <inheritdoc/>
        public string BuildAllOrganisationsKey()
        {
            return $"{CacheKeyPrefix}:AllOrgsDict";
        }
    }
}