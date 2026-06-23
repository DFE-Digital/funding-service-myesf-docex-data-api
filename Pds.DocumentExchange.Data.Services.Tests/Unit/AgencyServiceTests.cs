using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.Converters;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using Pds.DocumentExchange.Data.Services.Tests.Unit.Caching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class AgencyServiceTests : BaseCacheTests
    {
        private readonly Mock<IDirectoriesManager> _directoriesManager = new Mock<IDirectoriesManager>(MockBehavior.Strict);
        private readonly Mock<IDirectory> _directory = new Mock<IDirectory>(MockBehavior.Strict);
        private readonly Mock<IOrganisationsLookup> _organisationsLookup = new Mock<IOrganisationsLookup>(MockBehavior.Strict);
        private readonly Mock<IProductsLookup> _productsLookup = new Mock<IProductsLookup>(MockBehavior.Strict);
        private readonly Mock<IFileNameProvider> _fileNameProvider = new Mock<IFileNameProvider>(MockBehavior.Strict);
        private readonly Mock<IFiltersExecutionManager<AgencyDocument>> _filtersManager = new Mock<IFiltersExecutionManager<AgencyDocument>>(MockBehavior.Strict);
        private readonly Mock<ITeamsLookup> _teamsLookup = new Mock<ITeamsLookup>(MockBehavior.Strict);
        private readonly Mock<IAgencyDocumentValidator> _agencyDocumentValidator = new Mock<IAgencyDocumentValidator>(MockBehavior.Strict);
        private readonly Mock<IConvertAgencyDocumentErrorTypes> _converter = new Mock<IConvertAgencyDocumentErrorTypes>(MockBehavior.Strict);
        private readonly Mock<IFilterToListResultConverter<AgencyDocument>> _filterToListResultConverter = new Mock<IFilterToListResultConverter<AgencyDocument>>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<AgencyService>> _logger = new Mock<ILoggerAdapter<AgencyService>>();

        private readonly AgencyService _agencyService;

        public AgencyServiceTests()
        {
            _agencyService = new AgencyService(
                _directoriesManager.Object,
                _organisationsLookup.Object,
                _productsLookup.Object,
                _fileNameProvider.Object,
                _filtersManager.Object,
                _teamsLookup.Object,
                _agencyDocumentValidator.Object,
                _converter.Object,
                CacheManager,
                _filterToListResultConverter.Object,
                _logger.Object);
        }

        #region Summary

        [TestMethod, TestCategory("Unit")]
        public async Task Summary_ReturnsFileShareSummaryContainingValidCounts()
        {
            // Arrange
            var team = "valid-team-name";

            var files = new Dictionary<string, AgencyDocumentErrorType>
            {
                ["10079319_10084_201920.pdf"] = AgencyDocumentErrorType.NoError,
                ["10013279_10084_201920.pdf"] = AgencyDocumentErrorType.NoError,
                ["10006689_10086_201920_#@.pdf"] = AgencyDocumentErrorType.DocumentNameContainsInvalidCharacters,
                ["10071664_10086_201920.pdf"] = AgencyDocumentErrorType.NoError,
                ["10071664_XXXXX_201920.pdf"] = AgencyDocumentErrorType.ProductIdentifierInvalidFormat
            };

            var filesReferenceInfo = files.Keys.Select(fileName => new FileReferenceInfo { FileName = fileName });

            _teamsLookup
               .Setup(c => c.Exists(It.IsAny<string>()))
               .ReturnsAsync(true);

            _directory
                .Setup(d => d.GetFiles())
                .Returns(CreateIAsyncEnumerable(filesReferenceInfo));

            _directoriesManager
                .Setup(d => d.GetDirectory(It.IsAny<string>()))
                .ReturnsAsync(_directory.Object);

            SetupLoadOrganisations();

            _agencyDocumentValidator
                .Setup(v => v.ValidateDocument(team, It.IsAny<string>(), It.IsAny<IDictionary<string, Organisation>>()))
                .ReturnsAsync((string team, string fileName, IDictionary<string, Organisation> organisations) =>
                {
                    return files[fileName];
                });

            SetupCacheGetMocks<List<FileReferenceInfo>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team));

            SetupCacheGetMocks<AgencyDocumentErrorType>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentValidation,
                files.Keys
                    .Select((Func<string, Expression<Func<ICacheKeyBuilder, string>>>)(fileName => cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team, fileName)))
                    .ToArray());

            _organisationsLookup
                 .Setup(lookup => lookup.GetAllOrganisations())
                 .ReturnsAsync(new Dictionary<string, Organisation>());

            // Act
            var response = await _agencyService.Summary(new List<string> { team });

            // Assert
            response.InvalidCount.Should().Be(2);
            response.ValidCount.Should().Be(3);
            VerifyCacheMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Summary_ForMultipleTeams_ReturnsFileShareSummaryContainingValidCounts()
        {
            // Arrange
            var team1 = "valid-team-name1";
            var team2 = "valid-team-name2";

            var files = new Dictionary<string, AgencyDocumentErrorType>
            {
                ["10079319_10084_201920.pdf"] = AgencyDocumentErrorType.NoError,
                ["10013279_10084_201920.pdf"] = AgencyDocumentErrorType.NoError,
                ["10006689_10086_201920_#@.pdf"] = AgencyDocumentErrorType.DocumentNameContainsInvalidCharacters,
                ["10071664_10086_201920.pdf"] = AgencyDocumentErrorType.NoError,
                ["10071664_XXXXX_201920.pdf"] = AgencyDocumentErrorType.ProductIdentifierInvalidFormat
            };

            var filesReferenceInfo = files.Keys.Select(fileName => new FileReferenceInfo { FileName = fileName });

            _teamsLookup
               .Setup(c => c.Exists(It.IsAny<string>()))
               .ReturnsAsync(true);

            _directory
                .Setup(d => d.GetFiles())
                .Returns(CreateIAsyncEnumerable(filesReferenceInfo));

            _directoriesManager
                .Setup(d => d.GetDirectory(It.IsAny<string>()))
                .ReturnsAsync(_directory.Object);

            SetupLoadOrganisations();

            _agencyDocumentValidator
                .Setup(v => v.ValidateDocument(team1, It.IsAny<string>(), It.IsAny<IDictionary<string, Organisation>>()))
                .ReturnsAsync((string team, string fileName, IDictionary<string, Organisation> organisations) =>
                {
                    return files[fileName];
                });

            _agencyDocumentValidator
                .Setup(v => v.ValidateDocument(team2, It.IsAny<string>(), It.IsAny<IDictionary<string, Organisation>>()))
                .ReturnsAsync((string team, string fileName, IDictionary<string, Organisation> organisations) =>
                {
                    return files[fileName];
                });

            SetupCacheGetMocks<List<FileReferenceInfo>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team1),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team2));

            SetupCacheGetMocks<AgencyDocumentErrorType>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentValidation,
                files.Keys
                    .Select((Func<string, Expression<Func<ICacheKeyBuilder, string>>>)(fileName => cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team1, fileName)))
                .Union(files.Keys
                    .Select((Func<string, Expression<Func<ICacheKeyBuilder, string>>>)(fileName => cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team2, fileName))))
                .ToArray());

            _organisationsLookup
                .Setup(lookup => lookup.GetAllOrganisations())
                .ReturnsAsync(new Dictionary<string, Organisation>());

            // Act
            var response = await _agencyService.Summary(new List<string> { team1, team2 });

            // Assert
            response.InvalidCount.Should().Be(2 * 2);
            response.ValidCount.Should().Be(3 * 2);
            VerifyCacheMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Summary_ReturnsEmptyFileShare_WhenTeamIsNotPermitted()
        {
            // Arrange
            _teamsLookup
                .Setup(c => c.Exists(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var response = await _agencyService.Summary(new List<string> { "invalid-team" });

            // Assert
            response.InvalidCount.Should().Be(0);
            response.ValidCount.Should().Be(0);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Summary_ReturnsZeroValidAndInvalid_WhenNoFilesInFileStore()
        {
            // Arrange
            var team = "valid-team-name";

            _teamsLookup
                .Setup(c => c.Exists(team))
                .ReturnsAsync(true);

            _directory
                .Setup(d => d.GetFiles())
                .Returns(GetEmptyIAsyncEnumerable<FileReferenceInfo>());

            _directoriesManager
                .Setup(d => d.GetDirectory(It.IsAny<string>()))
                .ReturnsAsync(_directory.Object);

            SetupCacheGetMocks<List<FileReferenceInfo>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team));

            _organisationsLookup
                .Setup(lookup => lookup.GetAllOrganisations())
                .ReturnsAsync(new Dictionary<string, Organisation>());

            // Act
            var response = await _agencyService.Summary(new List<string> { team });

            // Assert
            response.InvalidCount.Should().Be(0);
            response.ValidCount.Should().Be(0);
            VerifyCacheMocks();
        }

        #endregion


        #region GetDocuments

        [TestMethod, TestCategory("Unit")]
        public async Task GetDocuments_WhenTeamIsNotPermitted_ReturnsEmptyFileShare()
        {
            // Arrange
            var team = "invalid-team";
            var teams = new[] { team };

            SetupTeamsLookup(team, false);

            // Act
            var response = await _agencyService.GetDocuments(
                teams,
                It.IsAny<AgencyListDocumentOptions>());

            // Assert
            response.Items.Should().BeNullOrEmpty();
            response.Filters.Should().BeNullOrEmpty();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetDocuments_ForSingleTeam_WhenAgencyDocumentValidityIsValid_ReturnsValidDocuments()
        {
            // Arrange
            var team = "valid-team";
            var teams = new[] { team };

            var documentOptions = new AgencyListDocumentOptions
            {
                Validity = AgencyDocumentValidity.Valid,
                PageSize = 10
            };

            var fileAgencyDocPair1 = CreateAndSetupValidDocument(12345678, "10002", 201819, ".pdf", team);
            var fileAgencyDocPair2 = CreateAndSetupValidDocument(12345678, "10002", 202021, ".pdf", team);
            var fileAgencyDocPair3 = CreateAndSetupValidDocument(12345678, "10002", 202122, ".pdf", team);

            var fileShareFiles = new[] { fileAgencyDocPair1.file, fileAgencyDocPair2.file, fileAgencyDocPair3.file };
            var expectedAgencyDocs = new[] { fileAgencyDocPair1.agencyDocument, fileAgencyDocPair2.agencyDocument, fileAgencyDocPair3.agencyDocument };

            var expectedFilterResult = new FilterResult<AgencyDocument>
            {
                Items = expectedAgencyDocs,
                Filters = Enumerable.Empty<RadioFilter>()
            };

            var expectedResult = new ListResult<AgencyDocument>
            {
                Items = expectedAgencyDocs,
                Filters = expectedFilterResult.Filters,
                TotalPages = 1,
                TotalItems = 3
            };

            SetupTeamsLookup(team, true);
            SetupDirectory(team, fileShareFiles);
            SetupLoadOrganisations();

            Expression<Func<IEnumerable<AgencyDocument>, bool>> agencyDocExpression =
                documents => documents.All(doc => expectedFilterResult.Items.Any(res => doc.FileName == res.FileName && doc.IsValid == res.IsValid));

            _filtersManager
                .Setup(manager => manager.Filter(
                    It.Is(agencyDocExpression),
                    documentOptions.FilterOptions,
                    It.Is<IEnumerable<FilterKey>>(keys => keys.Contains(FilterKey.ProductIdRadio))))
                .ReturnsAsync(expectedFilterResult);

            _filterToListResultConverter
                 .Setup(c => c.Convert(expectedFilterResult, documentOptions.PageSize, documentOptions.PageNumber))
                 .Returns(expectedResult);

            SetupCacheGetMocks<List<FileReferenceInfo>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team));

            SetupCacheGetMocks<AgencyDocumentErrorType>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentValidation,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team, fileAgencyDocPair1.file.FileName),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team, fileAgencyDocPair2.file.FileName),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team, fileAgencyDocPair3.file.FileName));

            // Act
            var result = await _agencyService.GetDocuments(
                teams,
                documentOptions);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            VerifyCacheMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetDocuments_ForMultipleTeams_WhenAgencyDocumentValidityIsValid_ReturnsValidDocuments()
        {
            // Arrange
            var team1 = "valid-team-01";
            var team2 = "valid-team-02";
            var team3 = "valid-team-03";

            var teams = new[] { team1, team2, team3 };

            var documentOptions = new AgencyListDocumentOptions
            {
                Validity = AgencyDocumentValidity.Valid,
                PageSize = 10
            };

            var fileAgencyDocPair1 = CreateAndSetupValidDocument(12345678, "10002", 201819, ".pdf", team1);
            var fileAgencyDocPair2 = CreateAndSetupValidDocument(12345678, "10002", 202021, ".pdf", team1);
            var fileAgencyDocPair3 = CreateAndSetupValidDocument(12345678, "10002", 202122, ".pdf", team2);
            var fileAgencyDocPair4 = CreateAndSetupValidDocument(12345678, "99999", 199091, ".docx", team3);
            var fileAgencyDocPair5 = CreateAndSetupValidDocument(12345678, "10987", 215051, ".xlsx", team3);

            var expectedAgencyDocs = new[]
            {
                fileAgencyDocPair1.agencyDocument,
                fileAgencyDocPair2.agencyDocument,
                fileAgencyDocPair3.agencyDocument,
                fileAgencyDocPair4.agencyDocument,
                fileAgencyDocPair5.agencyDocument
            };

            var expectedFilterResult = new FilterResult<AgencyDocument>
            {
                Items = expectedAgencyDocs,
                Filters = Enumerable.Empty<ListFilter>()
            };

            var expectedResult = new ListResult<AgencyDocument>
            {
                Items = expectedAgencyDocs,
                Filters = expectedFilterResult.Filters,
                TotalPages = 1,
                TotalItems = 5
            };

            SetupTeamsLookup(team1, true);
            SetupTeamsLookup(team2, true);
            SetupTeamsLookup(team3, true);

            SetupDirectory(team1, new[] { fileAgencyDocPair1.file, fileAgencyDocPair2.file });
            SetupDirectory(team2, new[] { fileAgencyDocPair3.file });
            SetupDirectory(team3, new[] { fileAgencyDocPair4.file, fileAgencyDocPair5.file });

            SetupLoadOrganisations();

            Expression<Func<IEnumerable<AgencyDocument>, bool>> agencyDocExpression =
                documents => documents.All(doc => expectedFilterResult.Items.Any(res => doc.FileName == res.FileName && doc.IsValid == res.IsValid));

            var filterKeys = new[] { FilterKey.ProductIdRadio };

            _filtersManager
                .Setup(manager => manager.Filter(
                    It.Is(agencyDocExpression),
                    documentOptions.FilterOptions,
                    It.Is<IEnumerable<FilterKey>>(keys => filterKeys.All(filterKey => keys.Contains(filterKey)))))
                .ReturnsAsync(expectedFilterResult);

            _filterToListResultConverter
                 .Setup(c => c.Convert(expectedFilterResult, documentOptions.PageSize, documentOptions.PageNumber))
                 .Returns(expectedResult);

            SetupCacheGetMocks<List<FileReferenceInfo>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team1),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team2),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team3));

            SetupCacheGetMocks<AgencyDocumentErrorType>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentValidation,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team1, fileAgencyDocPair1.file.FileName),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team1, fileAgencyDocPair2.file.FileName),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team2, fileAgencyDocPair3.file.FileName),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team3, fileAgencyDocPair4.file.FileName),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team3, fileAgencyDocPair5.file.FileName));

            // Act
            var result = await _agencyService.GetDocuments(
                teams,
                documentOptions);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            VerifyCacheMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetDocuments_ForSingleTeam_WhenAgencyDocumentValidityIsInvalid_ReturnsInvalidDocuments()
        {
            // Arrange
            var team = "valid-team";
            var teams = new[] { team };

            var documentOptions = new AgencyListDocumentOptions
            {
                Validity = AgencyDocumentValidity.Invalid,
                PageSize = 10
            };

            var fileAgencyDocPair = CreateAndSetupInvalidDocument(12345678, "10002", 201819, ".pdf", team, AgencyDocumentErrorType.ProductIdentifierInvalidFormat);

            var fileShareFiles = new[] { fileAgencyDocPair.file };
            var expectedAgencyDocs = new[] { fileAgencyDocPair.agencyDocument };

            var expectedFilterResult = new FilterResult<AgencyDocument>
            {
                Items = expectedAgencyDocs,
                Filters = Enumerable.Empty<ListFilter>()
            };

            var expectedResult = new ListResult<AgencyDocument>
            {
                Items = expectedAgencyDocs,
                Filters = expectedFilterResult.Filters,
                TotalPages = 1,
                TotalItems = 1
            };

            SetupTeamsLookup(team, true);
            SetupDirectory(team, fileShareFiles);
            SetupLoadOrganisations();

            Expression<Func<IEnumerable<AgencyDocument>, bool>> agencyDocExpression =
                documents => documents.All(doc => expectedFilterResult.Items.Any(res => doc.FileName == res.FileName && doc.IsValid == res.IsValid));

            var filterKeys = new[] { FilterKey.DocumentNameError, FilterKey.ProductIdList };

            _filtersManager
                .Setup(manager => manager.Filter(
                    It.Is(agencyDocExpression),
                    documentOptions.FilterOptions,
                    It.Is<IEnumerable<FilterKey>>(keys => keys.All(key => filterKeys.Contains(key)))))
                .ReturnsAsync(expectedFilterResult);

            _filterToListResultConverter
                 .Setup(c => c.Convert(expectedFilterResult, documentOptions.PageSize, documentOptions.PageNumber))
                 .Returns(expectedResult);

            SetupCacheGetMocks<List<FileReferenceInfo>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team));

            SetupCacheGetMocks<AgencyDocumentErrorType>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentValidation,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team, fileAgencyDocPair.file.FileName));

            // Act
            var result = await _agencyService.GetDocuments(
                teams,
                documentOptions);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            VerifyCacheMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetDocuments_ForMultipleTeams_WhenAgencyDocumentValidityIsInvalid_ReturnsInvalidDocumentsWithProductInfo()
        {
            // Arrange
            var team1 = "valid-team-01";
            var team2 = "valid-team-02";
            var team3 = "valid-team-03";

            var teams = new[] { team1, team2, team3 };

            var documentOptions = new AgencyListDocumentOptions
            {
                Validity = AgencyDocumentValidity.Invalid,
                PageSize = 10
            };

            var fileAgencyDocPair1 = CreateAndSetupInvalidDocument(12345678, "10002", 201819, ".pdf", team1, AgencyDocumentErrorType.OrganisationIdentifierNotRecognised);
            var fileAgencyDocPair2 = CreateAndSetupInvalidDocument(12345678, "10002", 202021, ".pdf", team1, AgencyDocumentErrorType.OrganisationIdentifierNotRecognised);
            var fileAgencyDocPair3 = CreateAndSetupInvalidDocument(12345678, "10002", 202122, ".pdf", team2, AgencyDocumentErrorType.ProductIdentifierInvalidFormat);
            var fileAgencyDocPair4 = CreateAndSetupInvalidDocument(12345678, "99999", 199091, ".docx", team3, AgencyDocumentErrorType.ProductIdentifierInvalidFormat);
            var fileAgencyDocPair5 = CreateAndSetupInvalidDocument(12345678, "10987", 215051, ".xlsx", team3, AgencyDocumentErrorType.OrganisationIdentifierNotRecognised);

            var expectedAgencyDocs = new[]
            {
                fileAgencyDocPair1.agencyDocument,
                fileAgencyDocPair2.agencyDocument,
                fileAgencyDocPair3.agencyDocument,
                fileAgencyDocPair4.agencyDocument,
                fileAgencyDocPair5.agencyDocument
            };

            var expectedFilterResult = new FilterResult<AgencyDocument>
            {
                Items = expectedAgencyDocs,
                Filters = Enumerable.Empty<ListFilter>()
            };

            var expectedResult = new ListResult<AgencyDocument>
            {
                Items = expectedAgencyDocs,
                Filters = expectedFilterResult.Filters,
                TotalPages = 1,
                TotalItems = 5
            };

            SetupTeamsLookup(team1, true);
            SetupTeamsLookup(team2, true);
            SetupTeamsLookup(team3, true);

            SetupDirectory(team1, new[] { fileAgencyDocPair1.file, fileAgencyDocPair2.file });
            SetupDirectory(team2, new[] { fileAgencyDocPair3.file });
            SetupDirectory(team3, new[] { fileAgencyDocPair4.file, fileAgencyDocPair5.file });

            SetupLoadOrganisations();

            Expression<Func<IEnumerable<AgencyDocument>, bool>> agencyDocExpression =
                documents => documents.All(doc => expectedFilterResult.Items.Any(res => doc.FileName == res.FileName && doc.IsValid == res.IsValid));

            var filterKeys = new[] { FilterKey.Team, FilterKey.DocumentNameError, FilterKey.ProductIdList };

            _filtersManager
                .Setup(manager => manager.Filter(
                    It.Is(agencyDocExpression),
                    documentOptions.FilterOptions,
                    It.Is<IEnumerable<FilterKey>>(keys => filterKeys.All(filterKey => keys.Contains(filterKey)))))
                .ReturnsAsync(expectedFilterResult);

            _filterToListResultConverter
                 .Setup(c => c.Convert(expectedFilterResult, documentOptions.PageSize, documentOptions.PageNumber))
                 .Returns(expectedResult);

            SetupCacheGetMocks<List<FileReferenceInfo>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team1),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team2),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team3));

            SetupCacheGetMocks<AgencyDocumentErrorType>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentValidation,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team1, fileAgencyDocPair1.file.FileName),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team1, fileAgencyDocPair2.file.FileName),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team2, fileAgencyDocPair3.file.FileName),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team3, fileAgencyDocPair4.file.FileName),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team3, fileAgencyDocPair5.file.FileName));

            // Act
            var result = await _agencyService.GetDocuments(
                teams,
                documentOptions);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            VerifyCacheMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetDocuments_WhenAgencyDocumentNamesSpecified_ReturnsMatchingDocuments()
        {
            // Arrange
            var team = "valid-team";
            var teams = new[] { team };

            var fileAgencyDocPair1 = CreateAndSetupValidDocument(12345678, "10002", 201819, ".pdf", team);
            var fileAgencyDocPair2 = CreateAndSetupValidDocument(12345678, "10002", 202021, ".pdf", team);
            var fileAgencyDocPair3 = CreateAndSetupValidDocument(12345678, "10002", 202122, ".pdf", team);

            var documentOptions = new AgencyListDocumentOptions
            {
                Validity = AgencyDocumentValidity.Valid,
                PageSize = 10,
                DocumentNames = new[] { fileAgencyDocPair1.file.FileName }
            };

            var fileShareFiles = new[] { fileAgencyDocPair1.file, fileAgencyDocPair2.file, fileAgencyDocPair3.file };
            var expectedAgencyDocs = new[] { fileAgencyDocPair1.agencyDocument };

            var expectedFilterResult = new FilterResult<AgencyDocument>
            {
                Items = expectedAgencyDocs,
                Filters = Enumerable.Empty<RadioFilter>()
            };

            var expectedResult = new ListResult<AgencyDocument>
            {
                Items = expectedAgencyDocs,
                Filters = expectedFilterResult.Filters,
                TotalPages = 1,
                TotalItems = 1
            };

            SetupTeamsLookup(team, true);
            SetupDirectory(team, fileShareFiles);
            SetupLoadOrganisations();

            Expression<Func<IEnumerable<AgencyDocument>, bool>> agencyDocExpression =
                documents => documents.All(doc => expectedFilterResult.Items.Any(res => doc.FileName == res.FileName && doc.IsValid == res.IsValid));

            _filtersManager
                .Setup(manager => manager.Filter(
                    It.Is(agencyDocExpression),
                    documentOptions.FilterOptions,
                    It.Is<IEnumerable<FilterKey>>(keys => keys.Contains(FilterKey.ProductIdRadio))))
                .ReturnsAsync(expectedFilterResult);

            _filterToListResultConverter
                 .Setup(c => c.Convert(expectedFilterResult, documentOptions.PageSize, documentOptions.PageNumber))
                 .Returns(expectedResult);

            SetupCacheGetMocks<List<FileReferenceInfo>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team));

            SetupCacheGetMocks<AgencyDocumentErrorType>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentValidation,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyDocumentValidationKey(team, fileAgencyDocPair1.file.FileName));

            // Act
            var result = await _agencyService.GetDocuments(
                teams,
                documentOptions);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            VerifyCacheMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetDocuments_WhenNoFilesInFileStore_ReturnsEmptyResult()
        {
            // Arrange
            var team = "valid-team";
            var teams = new[] { team };

            var documentOptions = new AgencyListDocumentOptions
            {
                Validity = AgencyDocumentValidity.Invalid,
                PageSize = 10
            };

            var expectedResult = new ListResult<AgencyDocument>
            {
                Items = Enumerable.Empty<AgencyDocument>(),
                TotalItems = 0,
                TotalPages = 0
            };

            SetupTeamsLookup(team, true);

            _directory
                .Setup(d => d.GetFiles())
                .Returns(GetEmptyIAsyncEnumerable<FileReferenceInfo>());

            _directoriesManager
                .Setup(d => d.GetDirectory(team))
                .ReturnsAsync(_directory.Object);

            SetupCacheGetMocks<List<FileReferenceInfo>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamFileShareKey(team));

            _organisationsLookup
                  .Setup(lookup => lookup.GetAllOrganisations())
                  .ReturnsAsync(new Dictionary<string, Organisation>()
                    {
                        {
                          "12345678",
                          new Organisation
                          {
                              Name = $"test-organisation-12345678",
                              Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = "12345678" } }
                          }
                        },
                        {
                          "11223344",
                          new Organisation
                          {
                              Name = $"test-organisation-11223344",
                              Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = "11223344" } }
                          }
                        }
                    });

            // Act
            var result = await _agencyService.GetDocuments(
                teams,
                documentOptions);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            VerifyCacheMocks();
        }

        #endregion

        private void SetupTeamsLookup(string team, bool isValid)
            => _teamsLookup
               .Setup(c => c.Exists(team))
               .ReturnsAsync(isValid);

        private (FileReferenceInfo file, AgencyDocument agencyDocument) CreateAndSetupValidDocument(
            int organisationId,
            string productId,
            int year,
            string extension,
            string team)
        {
            var fileReferenceInfo = new FileReferenceInfo
            {
                FileName = $"{organisationId}_{productId}_{year}{extension}"
            };

            _fileNameProvider
                .Setup(f => f.GetComponents(fileReferenceInfo.FileName, true))
                .Returns(new FileNameComponents
                {
                    OriginalFileName = fileReferenceInfo.FileName,
                    OrganisationIdentifier = organisationId,
                    ProductIdentifier = productId,
                    AcademicYear = year,
                    Extension = extension
                });

            _agencyDocumentValidator
                .Setup(v => v.ValidateDocument(team, fileReferenceInfo.FileName, It.IsAny<IDictionary<string, Organisation>>()))
                .ReturnsAsync(AgencyDocumentErrorType.NoError);

            var product = CreateAndSetupProduct(productId, team);
            var organisationInfo = CreateAndSetupOrganisation(organisationId);

            var agencyDocument = new AgencyDocument
            {
                FileName = fileReferenceInfo.FileName,
                IsValid = true,
                ErrorType = AgencyDocumentErrorType.NoError,
                Product = product,
                OrganisationInfo = organisationInfo,
                Team = team
            };

            return (fileReferenceInfo, agencyDocument);
        }

        private (FileReferenceInfo file, AgencyDocument agencyDocument) CreateAndSetupInvalidDocument(
            int organisationId,
            string productId,
            int year,
            string extension,
            string team,
            AgencyDocumentErrorType errorType)
        {
            var fileReferenceInfo = new FileReferenceInfo
            {
                FileName = $"{organisationId}_{productId}_{year}{extension}"
            };

            _agencyDocumentValidator
                .Setup(v => v.ValidateDocument(team, fileReferenceInfo.FileName, It.IsAny<IDictionary<string, Organisation>>()))
                .ReturnsAsync(errorType);

            Product product = null;

            if (errorType != AgencyDocumentErrorType.ProductIdentifierInvalidFormat)
            {
                _fileNameProvider
                    .Setup(f => f.GetComponents(fileReferenceInfo.FileName, false))
                    .Returns(new FileNameComponents
                    {
                        OriginalFileName = fileReferenceInfo.FileName,
                        OrganisationIdentifier = organisationId,
                        ProductIdentifier = productId,
                        AcademicYear = year,
                        Extension = extension
                    });

                product = new Product
                {
                    AgencyTeams = new[] { team },
                    Identifier = int.Parse(productId),
                    Name = "product-name",
                    PluralName = "product-plural-name"
                };

                _productsLookup
                    .Setup(lookup => lookup.Get(productId))
                    .ReturnsAsync(product);
            }

            var errorDescription = $"The error description for {errorType}";

            _converter
                .Setup(converter => converter.Convert(errorType))
                .Returns(errorDescription);

            var organisationInfo = CreateAndSetupOrganisation(organisationId);

            var agencyDocument = new AgencyDocument
            {
                FileName = fileReferenceInfo.FileName,
                IsValid = false,
                ErrorType = errorType,
                ErrorDescription = errorDescription,
                Product = product,
                OrganisationInfo = organisationInfo,
                Team = team
            };

            return (fileReferenceInfo, agencyDocument);
        }

        private Product CreateAndSetupProduct(string productId, string team)
        {
            var product = new Product
            {
                AgencyTeams = new[] { team },
                Identifier = int.Parse(productId),
                Name = "product-name",
                PluralName = "product-plural-name"
            };

            _productsLookup
                .Setup(lookup => lookup.Get(productId))
                .ReturnsAsync(product);

            return product;
        }

        private OrganisationInfo CreateAndSetupOrganisation(int organisationId)
        {
            var organisationIdentifier = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = organisationId.ToString()
            };

            var organisation = new Organisation
            {
                Identifiers = new[] { organisationIdentifier },
                Name = "the-organisation"
            };

            _organisationsLookup
                  .Setup(lookup => lookup.GetAllOrganisations())
                  .ReturnsAsync(new Dictionary<string, Organisation>()
                    {
                        {
                          organisationIdentifier.Value,
                          new Organisation
                          {
                              Name = "the-organisation",
                              Identifiers = new[] { organisationIdentifier }
                          }
                        },
                        {
                          "87654321",
                          new Organisation
                          {
                              Name = $"test-organisation-87654321",
                              Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = "87654321" } }
                          }
                        },
                        {
                          "11223344",
                          new Organisation
                          {
                              Name = $"test-organisation-11223344",
                              Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = "11223344" } }
                          }
                        },
                        {
                          "99999999",
                          new Organisation
                          {
                              Name = $"test-organisation-99999999",
                              Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = "99999999" } }
                          }
                        }
                    });

            return new OrganisationInfo
            {
                OrganisationIdentifier = organisationIdentifier,
                Name = organisation.Name
            };
        }

        private void SetupDirectory(string team, IEnumerable<FileReferenceInfo> files)
        {
            var directory = new Mock<IDirectory>(MockBehavior.Strict);

            directory
                .Setup(d => d.GetFiles())
                .Returns(CreateIAsyncEnumerable(files));

            _directoriesManager
                .Setup(d => d.GetDirectory(team))
                .ReturnsAsync(directory.Object);
        }

        private void SetupLoadOrganisations()
        {
            _fileNameProvider
                .Setup(f => f.GetComponents(It.IsAny<string>(), false))
                .Returns((string fileName, bool throwEx) =>
                {
                    var split = Path.GetFileNameWithoutExtension(fileName).Split("_");

                    return new FileNameComponents
                    {
                        OrganisationIdentifier = int.Parse(split[0]),
                        ProductIdentifier = split[1],
                        AcademicYear = int.Parse(split[2]),
                        Extension = Path.GetExtension(fileName)
                    };
                });
        }

        private async IAsyncEnumerable<T> CreateIAsyncEnumerable<T>(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                yield return item;
            }

            await Task.CompletedTask;
        }

        private async IAsyncEnumerable<T> GetEmptyIAsyncEnumerable<T>()
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}