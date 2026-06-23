using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ExchangeDocumentSelectorTests
    {
        private readonly Mock<IAgencyExchangeDocumentsService> _agencyExchangeDocumentsService = new Mock<IAgencyExchangeDocumentsService>(MockBehavior.Strict);
        private readonly Mock<IOrganisationExchangeDocumentsService> _organisationExchangeDocumentsService = new Mock<IOrganisationExchangeDocumentsService>(MockBehavior.Strict);
        private readonly Mock<ITeamsLookup> _teamsLookup = new Mock<ITeamsLookup>(MockBehavior.Strict);
        private readonly Mock<IOrganisationsLookup> _organisationsLookup = new Mock<IOrganisationsLookup>(MockBehavior.Strict);
        private readonly Mock<IFiltersExecutionManager<ExchangeDocument>> _filtersManager = new Mock<IFiltersExecutionManager<ExchangeDocument>>(MockBehavior.Strict);
        private readonly Mock<IFilterToListResultConverter<ExchangeDocument>> _resultConverter = new Mock<IFilterToListResultConverter<ExchangeDocument>>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<ExchangeDocumentSelector>> _logger = new Mock<ILoggerAdapter<ExchangeDocumentSelector>>();

        private readonly ExchangeDocumentSelector _exchangeDocumentSelector;

        public ExchangeDocumentSelectorTests()
        {
            _exchangeDocumentSelector = new ExchangeDocumentSelector(
                _agencyExchangeDocumentsService.Object,
                _organisationExchangeDocumentsService.Object,
                _teamsLookup.Object,
                _organisationsLookup.Object,
                _filtersManager.Object,
                _resultConverter.Object,
                _logger.Object);
        }

        #region GetAgencyTeamFiles

        [TestMethod]
        public void GetAgencyTeamFiles_WhenTeamsListIsNull_Throws()
        {
            // Act
            Func<Task<ListResult<ExchangeDocument>>> func =
                () => _exchangeDocumentSelector.GetAgencyTeamFiles(null, new ExchangeListDocumentOptions());

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public void GetAgencyTeamFiles_WhenOptionsIsNull_Throws()
        {
            // Act
            Func<Task<ListResult<ExchangeDocument>>> func =
                () => _exchangeDocumentSelector.GetAgencyTeamFiles(new[] { "the-team" }, null);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task GetAgencyTeamFiles_WhenNonExistingTeamPassed_ReturnsEmptyListResult()
        {
            // Arrange
            var team = "the-team";

            _teamsLookup
                .Setup(lookup => lookup.Exists(team))
                .ReturnsAsync(false);

            var expectedResult = new ListResult<ExchangeDocument>
            {
                Items = Enumerable.Empty<ExchangeDocument>(),
                Filters = Enumerable.Empty<ListFilter>()
            };

            // Act
            var result = await _exchangeDocumentSelector.GetAgencyTeamFiles(new[] { team }, new ExchangeListDocumentOptions());

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_teamsLookup);
        }

        [TestMethod]
        public async Task GetAgencyTeamFiles_WhenSingleTeamsAndOptionsAreValid_ReturnsListResult()
        {
            // Arrange
            var teams = new[] { "team-01" };

            var options = new ExchangeListDocumentOptions()
            {
                DocumentStatusOption = ExchangeDocumentDirection.SentByOrganisation,
                PageSize = 5,
                PageNumber = 1
            };

            _teamsLookup
                .Setup(lookup => lookup.Exists(It.IsAny<string>()))
                .ReturnsAsync(true);

            var exchangeDocuments = CreateAgencyExchangeDocuments();

            _agencyExchangeDocumentsService
                .Setup(a => a.GetExchangeDocuments(teams, options.DocumentStatusOption))
                .ReturnsAsync(exchangeDocuments);

            var filterResult = new FilterResult<ExchangeDocument>
            {
                Items = exchangeDocuments,
                Filters = GetFilters()
            };

            var filterKeys = new[]
            {
                FilterKey.Ukprn,
                FilterKey.Status,
                FilterKey.ProductIdList,
                FilterKey.UploadDate,
                FilterKey.ProviderType
            };

            SetupFiltersManager(exchangeDocuments, options.FilterOptions, filterKeys, filterResult);

            var expectedResult = new ListResult<ExchangeDocument>
            {
                Items = filterResult.Items,
                Filters = filterResult.Filters,
                TotalItems = 10,
                TotalPages = 2
            };

            _resultConverter
                .Setup(c => c.Convert(filterResult, options.PageSize, options.PageNumber))
                .Returns(expectedResult);

            // Act
            var result = await _exchangeDocumentSelector.GetAgencyTeamFiles(teams, options);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_teamsLookup, _agencyExchangeDocumentsService, _filtersManager, _resultConverter);
        }

        [TestMethod]
        public async Task GetAgencyTeamFiles_WhenMultipleTeamsAndOptionsAreValid_ReturnsListResult()
        {
            // Arrange
            var teams = new[] { "team-01", "team-02" };

            var options = new ExchangeListDocumentOptions()
            {
                DocumentStatusOption = ExchangeDocumentDirection.PublishedByAgency,
                PageSize = 5,
                PageNumber = 1
            };

            _teamsLookup
                .Setup(lookup => lookup.Exists(It.IsAny<string>()))
                .ReturnsAsync(true);

            var exchangeDocuments = CreateAgencyExchangeDocuments();

            _agencyExchangeDocumentsService
                .Setup(a => a.GetExchangeDocuments(teams, options.DocumentStatusOption))
                .ReturnsAsync(exchangeDocuments);

            var filterResult = new FilterResult<ExchangeDocument>
            {
                Items = exchangeDocuments,
                Filters = GetFilters()
            };

            var filterKeys = new[]
            {
                FilterKey.Team,
                FilterKey.Ukprn,
                FilterKey.Status,
                FilterKey.ProductIdList,
                FilterKey.UploadDate,
                FilterKey.ProviderType
            };

            SetupFiltersManager(exchangeDocuments, options.FilterOptions, filterKeys, filterResult);

            var expectedResult = new ListResult<ExchangeDocument>
            {
                Items = filterResult.Items,
                Filters = filterResult.Filters,
                TotalItems = 10,
                TotalPages = 2
            };

            _resultConverter
                .Setup(c => c.Convert(filterResult, options.PageSize, options.PageNumber))
                .Returns(expectedResult);

            // Act
            var result = await _exchangeDocumentSelector.GetAgencyTeamFiles(teams, options);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_teamsLookup, _agencyExchangeDocumentsService, _filtersManager, _resultConverter);
        }

        [TestMethod]
        public async Task GetAgencyTeamFiles_WhenDocumentReferencesSpecified_ReturnsListResult()
        {
            // Arrange
            var teams = new[] { "team-01", "team-02" };

            var options = new ExchangeListDocumentOptions()
            {
                DocumentStatusOption = ExchangeDocumentDirection.PublishedByAgency,
                PageSize = 5,
                PageNumber = 1,
                DocumentReferences = new[]
                {
                    new DocumentReference
                    {
                        BatchIdentifier = "batch-id-1",
                        ParentBatchIdentifier = "parent-batch-id-1",
                        FileName = "file-1.pdf"
                    }
                }
            };

            _teamsLookup
                .Setup(lookup => lookup.Exists(It.IsAny<string>()))
                .ReturnsAsync(true);

            var exchangeDocuments = CreateAgencyExchangeDocuments();

            _agencyExchangeDocumentsService
                .Setup(a => a.GetExchangeDocuments(teams, options.DocumentStatusOption))
                .ReturnsAsync(exchangeDocuments);

            var exchangeDocumentsByDocRef = CreateExchangeDocument(1, ExchangeDocumentEventType.SentByOrganisation);

            var filterResult = new FilterResult<ExchangeDocument>
            {
                Items = exchangeDocuments,
                Filters = GetFilters()
            };

            var filterKeys = new[]
            {
                FilterKey.Ukprn,
                FilterKey.Status,
                FilterKey.ProductIdList,
                FilterKey.UploadDate,
                FilterKey.ProviderType
            };

            SetupFiltersManager(options.FilterOptions, filterKeys, exchangeDocumentsByDocRef.DocumentReference, filterResult);

            var expectedResult = new ListResult<ExchangeDocument>
            {
                Items = filterResult.Items,
                Filters = filterResult.Filters,
                TotalItems = 10,
                TotalPages = 2
            };

            _resultConverter
                .Setup(c => c.Convert(filterResult, options.PageSize, options.PageNumber))
                .Returns(expectedResult);

            // Act
            var result = await _exchangeDocumentSelector.GetAgencyTeamFiles(teams, options);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_teamsLookup, _agencyExchangeDocumentsService, _filtersManager, _resultConverter);
        }

        #endregion


        #region GetOrganisationFiles

        [TestMethod]
        public void GetOrganisationFiles_WhenOptionsIsNull_Throws()
        {
            // Act
            Func<Task<ListResult<ExchangeDocument>>> func =
                () => _exchangeDocumentSelector.GetOrganisationFiles(null);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public void GetOrganisationFiles_WhenOrganisationIdentifierIsNull_Throws()
        {
            // Arrange
            var options = new ExchangeListOrganisationDocumentOptions
            {
                OrganisationIdentifier = null
            };

            // Act
            Func<Task<ListResult<ExchangeDocument>>> func =
                () => _exchangeDocumentSelector.GetOrganisationFiles(options);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetOrganisationFiles_WhenOrganisationIdentifierValueIsNullOrEmpty_Throws(string value)
        {
            // Arrange
            var options = new ExchangeListOrganisationDocumentOptions
            {
                OrganisationIdentifier = new OrganisationIdentifier
                {
                    Value = value
                }
            };

            // Act
            Func<Task<ListResult<ExchangeDocument>>> func =
                () => _exchangeDocumentSelector.GetOrganisationFiles(options);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task GetOrganisationFiles_WhenOrganisationIsNotParent_ReturnsListResult()
        {
            // Arrange
            var options = new ExchangeListOrganisationDocumentOptions
            {
                OrganisationIdentifier = new OrganisationIdentifier
                {
                    Value = "the-organisation"
                },
                DocumentStatusOption = ExchangeDocumentDirection.PublishedByAgency,
                PageSize = 5,
                PageNumber = 1
            };

            var exchangeDocuments = CreateOrganisationExchangeDocuments();

            _organisationExchangeDocumentsService
                .Setup(a => a.GetExchangeDocuments(options.OrganisationIdentifier, options.DocumentStatusOption))
                .ReturnsAsync(exchangeDocuments);

            var filterResult = new FilterResult<ExchangeDocument>
            {
                Items = exchangeDocuments,
                Filters = GetFilters()
            };

            var organisation = new Organisation
            {
                Identifiers = new[] { options.OrganisationIdentifier }
            };

            _organisationsLookup
                .Setup(lookup => lookup.Get(options.OrganisationIdentifier))
                .ReturnsAsync(organisation);

            var filterKeys = new[] { FilterKey.ProductIdList, FilterKey.AcademicYear };
            SetupFiltersManager(exchangeDocuments, options.FilterOptions, filterKeys, filterResult);

            var expectedResult = new ListResult<ExchangeDocument>
            {
                Items = filterResult.Items,
                Filters = filterResult.Filters,
                TotalItems = 10,
                TotalPages = 2
            };

            _resultConverter
                .Setup(c => c.Convert(filterResult, options.PageSize, options.PageNumber))
                .Returns(expectedResult);

            // Act
            var result = await _exchangeDocumentSelector.GetOrganisationFiles(options);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_teamsLookup, _organisationExchangeDocumentsService, _organisationsLookup, _filtersManager, _resultConverter);
        }

        [TestMethod]
        public async Task GetOrganisationFiles_WhenOrganisationIsParentAndDirectionIsPublishedByAgency_ReturnsListResult()
        {
            // Arrange
            var options = new ExchangeListOrganisationDocumentOptions
            {
                OrganisationIdentifier = new OrganisationIdentifier
                {
                    Value = "the-organisation"
                },
                DocumentStatusOption = ExchangeDocumentDirection.PublishedByAgency,
                PageSize = 5,
                PageNumber = 1
            };

            var exchangeDocuments = CreateOrganisationExchangeDocuments();

            _organisationExchangeDocumentsService
                .Setup(a => a.GetExchangeDocuments(options.OrganisationIdentifier, options.DocumentStatusOption))
                .ReturnsAsync(exchangeDocuments);

            var filterResult = new FilterResult<ExchangeDocument>
            {
                Items = exchangeDocuments,
                Filters = GetFilters()
            };

            var organisation = new Organisation
            {
                Identifiers = new[] { options.OrganisationIdentifier },
                ChildOrganisations = new[]
                {
                    new Organisation
                    {
                        Identifiers = new[]
                        {
                            new OrganisationIdentifier
                            {
                                Value = "the-child-organisation"
                            }
                        }
                    }
                }
            };

            _organisationsLookup
                .Setup(lookup => lookup.Get(options.OrganisationIdentifier))
                .ReturnsAsync(organisation);

            var filterKeys = new[] { FilterKey.Status, FilterKey.Organisation, FilterKey.ProductIdList };
            SetupFiltersManager(exchangeDocuments, options.FilterOptions, filterKeys, filterResult);

            var expectedResult = new ListResult<ExchangeDocument>
            {
                Items = filterResult.Items,
                Filters = filterResult.Filters,
                TotalItems = 10,
                TotalPages = 2
            };

            _resultConverter
                .Setup(c => c.Convert(filterResult, options.PageSize, options.PageNumber))
                .Returns(expectedResult);

            // Act
            var result = await _exchangeDocumentSelector.GetOrganisationFiles(options);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_teamsLookup, _organisationExchangeDocumentsService, _organisationsLookup, _filtersManager, _resultConverter);
        }

        [TestMethod]
        public async Task GetOrganisationFiles_WhenOrganisationIsParentAndDirectionIsSentByOrganisation_ReturnsListResult()
        {
            // Arrange
            var options = new ExchangeListOrganisationDocumentOptions
            {
                OrganisationIdentifier = new OrganisationIdentifier
                {
                    Value = "the-organisation"
                },
                DocumentStatusOption = ExchangeDocumentDirection.SentByOrganisation,
                PageSize = 5,
                PageNumber = 1
            };

            var exchangeDocuments = CreateOrganisationExchangeDocuments();

            _organisationExchangeDocumentsService
                .Setup(a => a.GetExchangeDocuments(options.OrganisationIdentifier, options.DocumentStatusOption))
                .ReturnsAsync(exchangeDocuments);

            var filterResult = new FilterResult<ExchangeDocument>
            {
                Items = exchangeDocuments,
                Filters = GetFilters()
            };

            var organisation = new Organisation
            {
                Identifiers = new[] { options.OrganisationIdentifier },
                ChildOrganisations = new[]
                {
                    new Organisation
                    {
                        Identifiers = new[]
                        {
                            new OrganisationIdentifier
                            {
                                Value = "the-child-organisation"
                            }
                        }
                    }
                }
            };

            _organisationsLookup
                .Setup(lookup => lookup.Get(options.OrganisationIdentifier))
                .ReturnsAsync(organisation);

            var filterKeys = new[] { FilterKey.Organisation, FilterKey.ProductIdList, FilterKey.AcademicYear };
            SetupFiltersManager(exchangeDocuments, options.FilterOptions, filterKeys, filterResult);

            var expectedResult = new ListResult<ExchangeDocument>
            {
                Items = filterResult.Items,
                Filters = filterResult.Filters,
                TotalItems = 10,
                TotalPages = 2
            };

            _resultConverter
                .Setup(c => c.Convert(filterResult, options.PageSize, options.PageNumber))
                .Returns(expectedResult);

            // Act
            var result = await _exchangeDocumentSelector.GetOrganisationFiles(options);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_teamsLookup, _organisationExchangeDocumentsService, _organisationsLookup, _filtersManager, _resultConverter);
        }

        [TestMethod]
        public async Task GetOrganisationFiles_WhenDocumentReferencesSpecified_ReturnsListResult()
        {
            // Arrange
            var options = new ExchangeListOrganisationDocumentOptions
            {
                OrganisationIdentifier = new OrganisationIdentifier
                {
                    Value = "the-organisation"
                },
                DocumentStatusOption = ExchangeDocumentDirection.PublishedByAgency,
                PageSize = 5,
                PageNumber = 1,
                DocumentReferences = new[]
                {
                    new DocumentReference
                    {
                        BatchIdentifier = "batch-id-1",
                        ParentBatchIdentifier = "parent-batch-id-1",
                        FileName = "file-1.pdf"
                    }
                }
            };

            var exchangeDocuments = CreateOrganisationExchangeDocuments();

            _organisationExchangeDocumentsService
                .Setup(a => a.GetExchangeDocuments(options.OrganisationIdentifier, options.DocumentStatusOption))
                .ReturnsAsync(exchangeDocuments);

            var filterResult = new FilterResult<ExchangeDocument>
            {
                Items = exchangeDocuments,
                Filters = GetFilters()
            };

            var organisation = new Organisation
            {
                Identifiers = new[] { options.OrganisationIdentifier }
            };

            _organisationsLookup
                .Setup(lookup => lookup.Get(options.OrganisationIdentifier))
                .ReturnsAsync(organisation);

            var exchangeDocumentsByDocRef = CreateExchangeDocument(1, ExchangeDocumentEventType.PublishedByAgency);

            var filterKeys = new[] { FilterKey.ProductIdList, FilterKey.AcademicYear };
            SetupFiltersManager(options.FilterOptions, filterKeys, exchangeDocumentsByDocRef.DocumentReference, filterResult);

            var expectedResult = new ListResult<ExchangeDocument>
            {
                Items = filterResult.Items,
                Filters = filterResult.Filters,
                TotalItems = 10,
                TotalPages = 2
            };

            _resultConverter
                .Setup(c => c.Convert(filterResult, options.PageSize, options.PageNumber))
                .Returns(expectedResult);

            // Act
            var result = await _exchangeDocumentSelector.GetOrganisationFiles(options);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_teamsLookup, _organisationExchangeDocumentsService, _organisationsLookup, _filtersManager, _resultConverter);
        }

        #endregion

        private IEnumerable<ExchangeDocument> CreateAgencyExchangeDocuments()
           => Enumerable.Range(1, 10)
               .Select(i => CreateExchangeDocument(i, ExchangeDocumentEventType.SentByOrganisation))
               .ToList();

        private IEnumerable<ExchangeDocument> CreateOrganisationExchangeDocuments()
           => Enumerable.Range(1, 10)
               .Select(i => CreateExchangeDocument(i, ExchangeDocumentEventType.PublishedByAgency))
               .ToList();

        private ExchangeDocument CreateExchangeDocument(int number, ExchangeDocumentEventType eventType)
           => new ExchangeDocument
           {
               DocumentReference = new DocumentReference
               {
                   BatchIdentifier = $"batch-id-{number}",
                   ParentBatchIdentifier = $"parent-batch-id-{number}",
                   FileName = $"file-{number}.pdf"
               },
               EventHistory = new[]
               {
                   new ExchangeDocumentEvent
                   {
                       EventType = eventType,
                       EventDateTime = DateTime.Now.AddDays(number)
                   }
               }
           };

        private IEnumerable<ListFilter> GetFilters() =>
            Enumerable.Range(1, 3)
            .Select(index => CreateFilter(index))
            .ToList();

        private ListFilter CreateFilter(int index)
            => new ListFilter
            {
                Key = $"filter-{index}",
                Values = new[] { CreateFilterValue(index) }
            };

        private FilterValue CreateFilterValue(int index)
            => new FilterValue
            {
                Title = $"filter-title-{index}",
                Value = $"filter-value-{index}",
                Count = index,
                Selected = index % 2 == 0
            };

        private void SetupFiltersManager(
            IEnumerable<ExchangeDocument> exchangeDocuments,
            IEnumerable<IFilterOption> filterOptions,
            IEnumerable<FilterKey> filterKeys,
            FilterResult<ExchangeDocument> filtersManagerResult)
                => _filtersManager
                    .Setup(f => f.Filter(
                        exchangeDocuments,
                        filterOptions,
                        It.Is<IEnumerable<FilterKey>>(keys => filterKeys.All(filterKey => keys.Contains(filterKey)))))
                    .ReturnsAsync(filtersManagerResult);

        private void SetupFiltersManager(
            IEnumerable<IFilterOption> filterOptions,
            IEnumerable<FilterKey> filterKeys,
            DocumentReference documentReference,
            FilterResult<ExchangeDocument> filtersManagerResult)
                => _filtersManager
                    .Setup(f => f.Filter(
                        It.Is<IEnumerable<ExchangeDocument>>(docs =>
                            docs.Count(doc => doc.DocumentReference.BatchIdentifier == documentReference.BatchIdentifier
                            && doc.DocumentReference.ParentBatchIdentifier == documentReference.ParentBatchIdentifier
                            && doc.DocumentReference.FileName == documentReference.FileName) == 1),
                        filterOptions,
                        It.Is<IEnumerable<FilterKey>>(keys => filterKeys.All(filterKey => keys.Contains(filterKey)))))
                    .ReturnsAsync(filtersManagerResult);
    }
}