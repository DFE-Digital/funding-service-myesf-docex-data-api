using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.Enums;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Caching
{
    /// <summary>
    /// Builds cache keys.
    /// </summary>
    public interface ICacheKeyBuilder
    {
        /// <summary>
        /// Builds a cache key for an agency team's file share.
        /// </summary>
        /// <param name="team">The agency team.</param>
        /// <returns>Cache key as a string.</returns>
        string BuildAgencyTeamFileShareKey(string team);

        /// <summary>
        /// Builds a cache key for an agency team's exchanged documents.
        /// </summary>
        /// <param name="team">The agency team.</param>
        /// <param name="direction">The direction of exchanged documents.</param>
        /// <returns>Cache key as a string.</returns>
        string BuildAgencyTeamExchangedDocumentsKey(string team, ExchangeDocumentDirection direction);

        /// <summary>
        /// Builds a cache key for the validation result of a given file.
        /// </summary>
        /// <param name="team">The agency team.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>Cache key as a string.</returns>
        string BuildAgencyDocumentValidationKey(string team, string fileName);

        /// <summary>
        /// Builds a cache key for an organisation's exchanged documents.
        /// </summary>
        /// <param name="organisationIdentifier">The organisation identifier.</param>
        /// <param name="direction">The direction of exchanged documents.</param>
        /// <returns>Cache key as a string.</returns>
        string BuildOrganisationExchangedDocumentsKey(OrganisationIdentifier organisationIdentifier, ExchangeDocumentDirection direction);

        /// <summary>
        /// Builds a cache key for a configuration section.
        /// </summary>
        /// <param name="configurationSection">The configuration section.</param>
        /// <returns>Cache key as a string.</returns>
        string BuildConfigurationKey(ConfigurationSection configurationSection);

        /// <summary>
        /// Builds a cache key for a list configuration section.
        /// </summary>
        /// <param name="configurationSection">The configuration section.</param>
        /// <returns>Cache key as a string.</returns>
        string BuildListConfigurationKey(ConfigurationSection configurationSection);

        /// <summary>
        /// Builds a cache key for an email resource lock.
        /// </summary>
        /// <param name="parentBatchId">The parent batch id.</param>
        /// <returns>Cache key as a string.</returns>
        string BuildEmailResourceLockKey(string parentBatchId);

        /// <summary>
        /// Builds a cache key for an organisation.
        /// </summary>
        /// <param name="organisationIdentifier">The organisation identifier.</param>
        /// <returns>Cache key as a string.</returns>
        string BuildOrganisationKey(OrganisationIdentifier organisationIdentifier);

        /// <summary>
        /// Builds a cache key for an organisation subtype display.
        /// </summary>
        /// <param name="organisationSubtype">The organisation subtype.</param>
        /// <returns>Cache key as a string.</returns>
        string BuildOrganisationSubtypeDisplayKey(string organisationSubtype);

        /// <summary>
        /// Builds a cache key for an organisation type display based on the subtype.
        /// </summary>
        /// <param name="organisationSubtype">The organisation subtype.</param>
        /// <returns>Cache key as a string.</returns>
        string BuildOrganisationTypeDisplayBySubtypeKey(string organisationSubtype);

        /// <summary>
        /// Builds a cache key for all organisations.
        /// </summary>
        /// <returns>Cache key as a string.</returns>
        string BuildAllOrganisationsKey();
    }
}