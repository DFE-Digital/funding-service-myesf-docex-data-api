using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Extensions;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Service to select and filter documents.
    /// </summary>
    public class ExchangeDocumentSelector : IExchangeDocumentSelector
    {
        private readonly IAgencyExchangeDocumentsService _agencyExchangeDocumentsService;
        private readonly IOrganisationExchangeDocumentsService _organisationExchangeDocumentsService;
        private readonly ITeamsLookup _teamsLookup;
        private readonly IOrganisationsLookup _organisationsLookup;
        private readonly IFiltersExecutionManager<ExchangeDocument> _filtersManager;
        private readonly IFilterToListResultConverter<ExchangeDocument> _filterToListResultConverter;

        private readonly ILoggerAdapter<ExchangeDocumentSelector> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeDocumentSelector"/> class.
        /// </summary>
        /// <param name="agencyExchangeDocumentsService">The agency exchange documents service.</param>
        /// <param name="organisationExchangeDocumentsService">The organisation exchange documents service.</param>
        /// <param name="teamsLookup">The teams lookup.</param>
        /// <param name="organisationsLookup">The organisations lookup.</param>
        /// <param name="filtersManager">The filters manager.</param>
        /// <param name="filterToListResultConverter">The filter to list result converter.</param>
        /// <param name="logger">The logger.</param>
        public ExchangeDocumentSelector(
            IAgencyExchangeDocumentsService agencyExchangeDocumentsService,
            IOrganisationExchangeDocumentsService organisationExchangeDocumentsService,
            ITeamsLookup teamsLookup,
            IOrganisationsLookup organisationsLookup,
            IFiltersExecutionManager<ExchangeDocument> filtersManager,
            IFilterToListResultConverter<ExchangeDocument> filterToListResultConverter,
            ILoggerAdapter<ExchangeDocumentSelector> logger)
        {
            _agencyExchangeDocumentsService = agencyExchangeDocumentsService;
            _organisationExchangeDocumentsService = organisationExchangeDocumentsService;
            _teamsLookup = teamsLookup;
            _organisationsLookup = organisationsLookup;
            _filtersManager = filtersManager;
            _filterToListResultConverter = filterToListResultConverter;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ListResult<ExchangeDocument>> GetAgencyTeamFiles(
            IEnumerable<string> teams,
            ExchangeListDocumentOptions options)
        {
            It.IsEmpty(teams)
                .AsGuard<ArgumentNullException>(nameof(teams));

            It.IsNull(options)
                .AsGuard<ArgumentNullException>(nameof(options));

            _logger.LogInformation($"Getting agency's documents - Direction {options.DocumentStatusOption}");

            foreach (var team in teams)
            {
                if (!await _teamsLookup.Exists(team))
                {
                    _logger.LogError($"Passed in team '{team}' not within permitted teams");

                    return new ListResult<ExchangeDocument>
                    {
                        Items = Collection.Empty<ExchangeDocument>(),
                        Filters = Collection.Empty<ListFilter>()
                    };
                }
            }

            _logger.LogInformation($"Getting documents for {(teams.Count() > 1 ? "teams" : "team")} ({string.Join(",", teams)}) - Direction {options.DocumentStatusOption}");

            var teamsExchangeDocuments = await _agencyExchangeDocumentsService.GetExchangeDocuments(teams, options.DocumentStatusOption);

            var agencyExchangeDocumentsByDocRefs = FilterByDocumentReferences(teamsExchangeDocuments, options.DocumentReferences);

            var result = await FilterAndCreateResultForAgency(teams, agencyExchangeDocumentsByDocRefs, options);

            _logger.LogInformation($"Retrieved documents for {(teams.Count() > 1 ? "teams" : "team")} ({string.Join(",", teams)}) - Direction {options.DocumentStatusOption} TotalItems Count {result.TotalItems}");
            return result;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ExchangeDocument>> GetAgencyTeamExchangeDocuments(IEnumerable<string> teams, IEnumerable<DocumentReference> documentReferences)
        {
            It.IsEmpty(teams)
                .AsGuard<ArgumentNullException>(nameof(teams));

            _logger.LogInformation($"Getting agency's documents - Direction {ExchangeDocumentDirection.SentByOrganisation}");

            foreach (var team in teams)
            {
                if (!await _teamsLookup.Exists(team))
                {
                    _logger.LogError($"Passed in team '{team}' not within permitted teams");

                    return new List<ExchangeDocument>();
                }
            }

            _logger.LogInformation($"Getting documents for {(teams.Count() > 1 ? "teams" : "team")} ({string.Join(",", teams)}) - Direction {ExchangeDocumentDirection.SentByOrganisation}");

            var teamsExchangeDocuments = await _agencyExchangeDocumentsService.GetExchangeDocuments(teams, ExchangeDocumentDirection.SentByOrganisation);
            var agencyExchangeDocumentsByDocRefs = FilterByDocumentReferences(teamsExchangeDocuments, documentReferences);

            return agencyExchangeDocumentsByDocRefs;
        }

        /// <inheritdoc/>
        public async Task<ListResult<ExchangeDocument>> GetOrganisationFiles(ExchangeListOrganisationDocumentOptions options)
        {
            (It.IsNull(options) || It.IsNull(options.OrganisationIdentifier))
                .AsGuard<ArgumentNullException>(nameof(options.OrganisationIdentifier));

            It.IsEmpty(options.OrganisationIdentifier.Value)
                .AsGuard<ArgumentNullException>(nameof(options.OrganisationIdentifier.Value));

            _logger.LogInformation($"Getting documents for organisation {options.OrganisationIdentifier.Value} - Direction {options.DocumentStatusOption}");

            var organisationExchangeDocuments = await _organisationExchangeDocumentsService.GetExchangeDocuments(options.OrganisationIdentifier, options.DocumentStatusOption);
            var organisationExchangeDocumentsByDocRefs = FilterByDocumentReferences(organisationExchangeDocuments, options.DocumentReferences);

            var result = await FilterAndCreateResultForOrganisation(organisationExchangeDocumentsByDocRefs, options);

            return result;
        }

        private IEnumerable<ExchangeDocument> FilterByDocumentReferences(
            IEnumerable<ExchangeDocument> exchangeDocuments,
            IEnumerable<DocumentReference> documentReferences)
        {
            if (It.IsEmpty(exchangeDocuments) || It.IsEmpty(documentReferences))
            {
                return exchangeDocuments;
            }

            return exchangeDocuments
                .Where(exchangeDoc => IsContainedInDocumentReferences(exchangeDoc.DocumentReference, documentReferences))
                .ToList();

            bool IsContainedInDocumentReferences(DocumentReference docReference, IEnumerable<DocumentReference> docReferences)
                => docReferences.Any(doc =>
                    doc.BatchIdentifier.IsEqualToIgnoreCase(docReference.BatchIdentifier)
                    && doc.ParentBatchIdentifier.IsEqualToIgnoreCase(docReference.ParentBatchIdentifier)
                    && doc.FileName.IsEqualToIgnoreCase(docReference.FileName));
        }

        private async Task<ListResult<ExchangeDocument>> FilterAndCreateResultForAgency(
            IEnumerable<string> teams,
            IEnumerable<ExchangeDocument> exchangeDocuments,
            ExchangeListDocumentOptions options)
        {
            var filterKeys = GetFilterKeysForAgency(teams);

            var filterResult = await _filtersManager.Filter(exchangeDocuments, options.FilterOptions, filterKeys);
            filterResult.Items = SortExchangeDocumentsByUploadedDate(filterResult.Items);

            return _filterToListResultConverter.Convert(filterResult, options.PageSize, options.PageNumber);
        }

        private IEnumerable<FilterKey> GetFilterKeysForAgency(IEnumerable<string> teams)
        {
            var filterKeys = new List<FilterKey>
            {
                FilterKey.Status,
                FilterKey.Ukprn,
                FilterKey.ProductIdList,
                FilterKey.UploadDate,
                FilterKey.ProviderType
            };

            if (teams.Count() > 1)
            {
                filterKeys.Insert(0, FilterKey.Team);
            }

            return filterKeys;
        }

        private async Task<ListResult<ExchangeDocument>> FilterAndCreateResultForOrganisation(
            IEnumerable<ExchangeDocument> exchangeDocuments,
            ExchangeListOrganisationDocumentOptions options)
        {
            var filterKeys = await GetFilterKeysForOrganisation(options.OrganisationIdentifier, options.DocumentStatusOption);

            var filterResult = await _filtersManager.Filter(exchangeDocuments, options.FilterOptions, filterKeys);
            filterResult.Items = SortExchangeDocumentsByUploadedDate(filterResult.Items);

            return _filterToListResultConverter.Convert(filterResult, options.PageSize, options.PageNumber);
        }

        private async Task<IEnumerable<FilterKey>> GetFilterKeysForOrganisation(OrganisationIdentifier organisationIdentifier, ExchangeDocumentDirection direction)
        {
            var organisation = await _organisationsLookup.Get(organisationIdentifier);

            return It.HasValues(organisation.ChildOrganisations)
                ? GetFilterKeysForParentOrganisation(direction)
                : new[] { FilterKey.ProductIdList, FilterKey.AcademicYear };
        }

        private IEnumerable<FilterKey> GetFilterKeysForParentOrganisation(ExchangeDocumentDirection direction)
            => direction switch
            {
                ExchangeDocumentDirection.PublishedByAgency
                    => new[] { FilterKey.Status, FilterKey.Organisation, FilterKey.ProductIdList },

                ExchangeDocumentDirection.SentByOrganisation
                    => new[] { FilterKey.Organisation, FilterKey.ProductIdList, FilterKey.AcademicYear },

                _ => throw new NotImplementedException()
            };

        private IEnumerable<ExchangeDocument> SortExchangeDocumentsByUploadedDate(IEnumerable<ExchangeDocument> exchangeDocuments)
            => exchangeDocuments.OrderByDescending(GetUploadedDate);

        private DateTime GetUploadedDate(ExchangeDocument exchangeDocument)
        {
            var uploadedEvent = exchangeDocument.EventHistory.FirstOrDefault(h => h.EventType == ExchangeDocumentEventType.PublishedByAgency || h.EventType == ExchangeDocumentEventType.SentByOrganisation);

            return It.IsNull(uploadedEvent)
                ? default
                : uploadedEvent.EventDateTime;
        }
    }
}