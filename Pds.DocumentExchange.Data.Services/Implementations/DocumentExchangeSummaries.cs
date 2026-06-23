using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Methods to retrieve the document exchange summaries.
    /// </summary>
    public class DocumentExchangeSummaries : IDocumentExchangeSummaries
    {
        private readonly IAgencyExchangeDocumentsService _agencyExchangeDocumentsService;
        private readonly IOrganisationExchangeDocumentsService _organisationExchangeDocumentsService;
        private readonly ITeamsLookup _teamsLookup;
        private readonly IConfigurationDataService _configurationDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentExchangeSummaries"/> class.
        /// </summary>
        /// <param name="agencyExchangeDocumentsService">The agency exchange documents service.</param>
        /// <param name="organisationExchangeDocumentsService">The organisation exchange documents service.</param>
        /// <param name="teamsLookup">The teams lookup.</param>
        /// <param name="configurationDataService">The configuration data service.</param>
        public DocumentExchangeSummaries(
            IAgencyExchangeDocumentsService agencyExchangeDocumentsService,
            IOrganisationExchangeDocumentsService organisationExchangeDocumentsService,
            ITeamsLookup teamsLookup,
            IConfigurationDataService configurationDataService)
        {
            _agencyExchangeDocumentsService = agencyExchangeDocumentsService;
            _organisationExchangeDocumentsService = organisationExchangeDocumentsService;
            _teamsLookup = teamsLookup;
            _configurationDataService = configurationDataService;
        }

        /// <inheritdoc/>
        public async Task<Summary> GetTeamsSummary(IEnumerable<string> teams)
        {
            It.IsEmpty(teams)
                .AsGuard<ArgumentNullException>(nameof(teams));

            foreach (var team in teams)
            {
                if (!await _teamsLookup.Exists(team))
                {
                    throw new ArgumentException($"{team} is not a valid team.", nameof(teams));
                }
            }

            var documents = await _agencyExchangeDocumentsService.GetReceivedFilesForAgencyWithSeenHistory(teams);

            var groupedDocuments = GroupExchangeDocumentsByVersion(documents);

            return new Summary
            {
                CountOfNewDocuments = groupedDocuments.Count(doc => !doc.Viewed),
                DocumentExchangeEnabled = await _configurationDataService.IsDocumentExchangeEnabled()
            };
        }

        /// <inheritdoc/>
        public async Task<Summary> GetOrganisationUserSummary(UserInfo userInfo)
        {
            It.IsNull(userInfo)
                .AsGuard<ArgumentNullException>(nameof(userInfo));

            It.IsNull(userInfo.OrganisationInfo)
                .AsGuard<ArgumentNullException>(nameof(userInfo.OrganisationInfo));

            It.IsNull(userInfo.OrganisationInfo.OrganisationIdentifier)
                .AsGuard<ArgumentNullException>(nameof(userInfo.OrganisationInfo.OrganisationIdentifier));

            It.IsEmpty(userInfo.Principal)
                .AsGuard<ArgumentNullException>(nameof(userInfo.Principal));

            var organisationExchangeDocuments = await _organisationExchangeDocumentsService.GetExchangeDocuments(userInfo.OrganisationInfo.OrganisationIdentifier, ExchangeDocumentDirection.PublishedByAgency);
            return await CreateSummary(organisationExchangeDocuments);
        }

        private async Task<Summary> CreateSummary(IEnumerable<ExchangeDocument> exchangeDocuments)
        {
            var count = exchangeDocuments?.Count(doc => IsFileNew(doc)) ?? 0;
            bool enabled = await _configurationDataService.IsDocumentExchangeEnabled();

            return new Summary
            {
                CountOfNewDocuments = count,
                DocumentExchangeEnabled = enabled
            };
        }

        private bool IsFileNew(ExchangeDocument exchangeDocument)
            => exchangeDocument?.EventHistory?.Any(history => history?.EventType == ExchangeDocumentEventType.DownloadedByReceiver) != true;

        private IEnumerable<DocumentWithViewedStatus> GroupExchangeDocumentsByVersion(IEnumerable<DocumentWithViewedStatus> exchangeDocuments)
        {
            var documentsGroups = exchangeDocuments.GroupBy(document =>
            new
            {
                document.FromUKPRN,
                document.FileType,
                document.Year
            });

            return documentsGroups.Select(documentsGroup =>
            {
                var orderedExchangeDocuments = documentsGroup.OrderByDescending(doc => doc.Version);
                var latestVersionExchangeDocument = orderedExchangeDocuments.First();

                return latestVersionExchangeDocument;
            });
        }
    }
}