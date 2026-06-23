using Pds.Core.Caching.Interfaces;
using Pds.Core.Caching.Models;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.Converters;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <inheritdoc />
    public class AgencyService : IAgencyService
    {
        private readonly IDirectoriesManager _directoriesManager;
        private readonly IOrganisationsLookup _organisationsLookup;
        private readonly IProductsLookup _productsLookup;
        private readonly IFileNameProvider _fileNameProvider;
        private readonly IFiltersExecutionManager<AgencyDocument> _filtersManager;
        private readonly ITeamsLookup _teamsLookup;
        private readonly IAgencyDocumentValidator _agencyDocumentValidator;
        private readonly IConvertAgencyDocumentErrorTypes _converter;
        private readonly ICacheManager _cacheManager;
        private readonly IFilterToListResultConverter<AgencyDocument> _filterToListResultConverter;
        private readonly ILoggerAdapter<AgencyService> _logger;

        private ICacheService Cache => _cacheManager.CacheService;

        private ICacheKeyBuilder CacheKeyBuilder => _cacheManager.CacheKeyBuilder;

        private CacheOptions DocumentListCachingOptions => _cacheManager.CacheOptionsProvider.DocumentList;

        private CacheOptions DocumentValidationCachingOptions => _cacheManager.CacheOptionsProvider.DocumentValidation;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgencyService"/> class.
        /// </summary>
        /// <param name="directoriesManager">The virus scan result processor.</param>
        /// <param name="organisationsLookup">The organisation API client.</param>
        /// <param name="productsLookup">The service that gets Document exchange configuration data.</param>
        /// <param name="fileNameProvider">The file helper class.</param>
        /// <param name="filtersManager">The filters manager.</param>
        /// <param name="teamsLookup">The teams lookup.</param>
        /// <param name="agencyDocumentValidator">The agency document validator.</param>
        /// <param name="converter">The error type to string converter.</param>
        /// <param name="cacheManager">The cache manager.</param>
        /// <param name="filterToListResultConverter">The filter to list result converter.</param>
        /// <param name="logger">The logger.</param>
        public AgencyService(
            IDirectoriesManager directoriesManager,
            IOrganisationsLookup organisationsLookup,
            IProductsLookup productsLookup,
            IFileNameProvider fileNameProvider,
            IFiltersExecutionManager<AgencyDocument> filtersManager,
            ITeamsLookup teamsLookup,
            IAgencyDocumentValidator agencyDocumentValidator,
            IConvertAgencyDocumentErrorTypes converter,
            ICacheManager cacheManager,
            IFilterToListResultConverter<AgencyDocument> filterToListResultConverter,
            ILoggerAdapter<AgencyService> logger)
        {
            _directoriesManager = directoriesManager;
            _organisationsLookup = organisationsLookup;
            _productsLookup = productsLookup;
            _fileNameProvider = fileNameProvider;
            _filtersManager = filtersManager;
            _teamsLookup = teamsLookup;
            _agencyDocumentValidator = agencyDocumentValidator;
            _converter = converter;
            _cacheManager = cacheManager;
            _filterToListResultConverter = filterToListResultConverter;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<FileShareSummary> Summary(IEnumerable<string> teams)
        {
            try
            {
                _logger.LogInformation("Getting File share summary.");

                var summary = new FileShareSummary();

                if (!await ValidateTeams(teams))
                {
                    return summary;
                }

                var getFileValidationTasks = new List<Task<AgencyDocumentErrorType>>();

                var filesByTeam = await GetFilesByTeamsList(teams);

                var organisations = await _organisationsLookup.GetAllOrganisations();
                _logger.LogInformation($"Received all organisations : {organisations.Count()}");

                foreach (var currentFilesByTeam in filesByTeam)
                {
                    foreach (var currentFile in currentFilesByTeam.Files)
                    {
                        getFileValidationTasks.Add(GetFileValidation(currentFilesByTeam.Team, currentFile.FileName, organisations));
                    }
                }

                var getFileValidationResults = await Task.WhenAll(getFileValidationTasks);

                _logger.LogInformation("FileValidation completed");

                foreach (var currentValidationResult in getFileValidationResults)
                {
                    if (currentValidationResult == AgencyDocumentErrorType.NoError)
                    {
                        summary.ValidCount++;
                    }
                    else
                    {
                        summary.InvalidCount++;
                    }
                }

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(Summary)} for teams: {string.Join(",", teams)}");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<ListResult<AgencyDocument>> GetDocuments(IEnumerable<string> teams, AgencyListDocumentOptions options)
        {
            try
            {
                var documentsResult = new ListResult<AgencyDocument>
                {
                    Items = Collection.Empty<AgencyDocument>()
                };

                if (!await ValidateTeams(teams))
                {
                    return documentsResult;
                }

                var getAgencyDocumentTasks = new List<Task<AgencyDocument>>();

                var filesByTeam = await GetFilesByTeamsList(teams);
                var organisations = await _organisationsLookup.GetAllOrganisations();
                foreach (var currentFilesByTeam in filesByTeam)
                {
                    var teamFiles = It.HasValues(options.DocumentNames)
                        ? FilterFilesByDocumentNames(currentFilesByTeam.Files, options.DocumentNames)
                        : currentFilesByTeam.Files;

                    foreach (var currentFile in teamFiles)
                    {
                        getAgencyDocumentTasks.Add(GetAgencyDocument(currentFilesByTeam.Team, currentFile.FileName, options.Validity, organisations));
                    }
                }

                var agencyDocuments = await Task.WhenAll(getAgencyDocumentTasks);
                var agencyDocumentsToReturn = agencyDocuments.Where(doc => doc != null).ToList();

                if (agencyDocumentsToReturn.Any())
                {
                    documentsResult = await FilterAndCreateResult(teams, agencyDocumentsToReturn, options);
                }

                return documentsResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(GetDocuments)} for teams: {string.Join(",", teams)} and options:{JsonSerializer.Serialize(options)}");
                throw;
            }
        }

        private async Task<bool> ValidateTeams(IEnumerable<string> teams)
        {
            foreach (var team in teams)
            {
                if (await _teamsLookup.Exists(team))
                {
                    continue;
                }

                _logger.LogError($"Passed in team '{team}' not within permitted teams");
                return false;
            }

            return true;
        }

        private async Task<IEnumerable<(string Team, List<FileReferenceInfo> Files)>> GetFilesByTeamsList(IEnumerable<string> teams)
        {
            var filesByTeamTasks = teams.Select(async team =>
            {
                var files = await GetFilesByTeam(team);
                return new { Team = team, Files = files };
            });

            var filesByTeam = await Task.WhenAll(filesByTeamTasks);
            return filesByTeam.Select(f => (f.Team, f.Files)).ToList();
        }

        private Task<List<FileReferenceInfo>> GetFilesByTeam(string team)
            => Cache.Get(
                CacheKeyBuilder.BuildAgencyTeamFileShareKey(team),
                async () =>
                {
                    var directory = await _directoriesManager.GetDirectory(team);
                    return await directory.GetFiles().ToListAsync();
                },
                DocumentListCachingOptions);

        private Task<AgencyDocumentErrorType> GetFileValidation(string team, string fileName, IDictionary<string, Organisation> organisations)
            => Cache.Get(
                CacheKeyBuilder.BuildAgencyDocumentValidationKey(team, fileName),
                () => _agencyDocumentValidator.ValidateDocument(team, fileName, organisations),
                DocumentValidationCachingOptions);

        private async Task<AgencyDocument> GetAgencyDocument(string team, string fileName, AgencyDocumentValidity validity, IDictionary<string, Organisation> organisations)
        {
            AgencyDocument agencyDocument = null;

            var fileValidationResult = await GetFileValidation(team, fileName, organisations);

            var showValidDocuments = validity == AgencyDocumentValidity.Valid;
            var isDocumentValid = fileValidationResult == AgencyDocumentErrorType.NoError;

            if (showValidDocuments && isDocumentValid)
            {
                agencyDocument = await CreateValidAgencyDocument(team, fileName, organisations);
            }

            if (!showValidDocuments && !isDocumentValid)
            {
                agencyDocument = await CreateInvalidAgencyDocument(team, fileName, fileValidationResult);
            }

            return agencyDocument;
        }

        private IEnumerable<FileReferenceInfo> FilterFilesByDocumentNames(
           IEnumerable<FileReferenceInfo> files,
           IEnumerable<string> documentNames)
        {
            foreach (var currentFile in files)
            {
                if (documentNames.Contains(currentFile.FileName, StringComparer.OrdinalIgnoreCase))
                {
                    yield return currentFile;
                }
            }
        }

        private async Task<AgencyDocument> CreateValidAgencyDocument(string team, string fileName, IDictionary<string, Organisation> organisations)
        {
            var components = _fileNameProvider.GetComponents(fileName);

            var organisationInfo = GetOrganisationInfo(components.OrganisationIdentifier, organisations);
            var product = await _productsLookup.Get(components.ProductIdentifier);

            return new AgencyDocument
            {
                FileName = fileName,
                ErrorType = AgencyDocumentErrorType.NoError,
                IsValid = true,
                OrganisationInfo = organisationInfo,
                Product = product,
                Year = components.AcademicYear.Value,
                Team = team
            };
        }

        private OrganisationInfo GetOrganisationInfo(int organisationIdentifierValue, IDictionary<string, Organisation> organisations)
        {
            var organisationIdentifier = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = organisationIdentifierValue.ToString()
            };

            var organisation = organisations.ContainsKey(organisationIdentifierValue.ToString())
                                                ? organisations[organisationIdentifierValue.ToString()]
                                                : new UnknownOrganisation(organisationIdentifier);

            return new OrganisationInfo
            {
                OrganisationIdentifier = organisationIdentifier,
                Name = organisation.Name
            };
        }

        private async Task<AgencyDocument> CreateInvalidAgencyDocument(string team, string fileName, AgencyDocumentErrorType errorType)
        {
            var invalidAgencyDocument = new AgencyDocument
            {
                FileName = fileName,
                ErrorType = errorType,
                ErrorDescription = _converter.Convert(errorType),
                IsValid = false,
                Team = team
            };

            if (errorType == AgencyDocumentErrorType.ProductIdentifierInvalidFormat)
            {
                invalidAgencyDocument.Product = new UnknownProduct();
            }
            else
            {
                var components = _fileNameProvider.GetComponents(fileName, false);
                var fileProduct = await _productsLookup.Get(components.ProductIdentifier);

                invalidAgencyDocument.Product = fileProduct;
            }

            return invalidAgencyDocument;
        }

        private async Task<ListResult<AgencyDocument>> FilterAndCreateResult(
            IEnumerable<string> teams,
            IEnumerable<AgencyDocument> agencyDocuments,
            AgencyListDocumentOptions options)
        {
            var filterResult = await FilterDocuments(teams, agencyDocuments, options);
            return _filterToListResultConverter.Convert(filterResult, options.PageSize, options.PageNumber);
        }

        private Task<FilterResult<AgencyDocument>> FilterDocuments(
            IEnumerable<string> teams,
            IEnumerable<AgencyDocument> agencyDocuments,
            AgencyListDocumentOptions options)
        {
            var filterKeys = options.Validity == AgencyDocumentValidity.Valid
                ? new List<FilterKey> { FilterKey.ProductIdRadio }
                : new List<FilterKey> { FilterKey.DocumentNameError, FilterKey.ProductIdList };

            if (teams.Count() > 1)
            {
                filterKeys.Insert(0, FilterKey.Team);
            }

            return _filtersManager.Filter(agencyDocuments, options.FilterOptions, filterKeys);
        }
    }
}