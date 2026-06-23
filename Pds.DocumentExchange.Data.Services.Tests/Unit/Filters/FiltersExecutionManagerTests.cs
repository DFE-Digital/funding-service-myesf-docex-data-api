using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Tests.Unit.Filters.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass, TestCategory("Unit")]
    public class FiltersExecutionManagerTests
    {
        private readonly Mock<IListFilter<TestDocument>> _filterByDocumentType = new Mock<IListFilter<TestDocument>>(MockBehavior.Strict);
        private readonly Mock<IListFilter<TestDocument>> _filterByOrganisation = new Mock<IListFilter<TestDocument>>(MockBehavior.Strict);
        private readonly Mock<IDateRangeFilter<TestDocument>> _filterByUploadedDateTime = new Mock<IDateRangeFilter<TestDocument>>(MockBehavior.Strict);
        private readonly Mock<IMatchingValueSelectionFilter<TestDocument>> _filterByTeam = new Mock<IMatchingValueSelectionFilter<TestDocument>>(MockBehavior.Strict);
        private readonly Mock<IMatchingTextBoxValueFilter<TestDocument>> _filterByUkprn = new Mock<IMatchingTextBoxValueFilter<TestDocument>>(MockBehavior.Strict);

        private readonly Mock<IFiltersFactory<TestDocument>> _filtersFactory = new Mock<IFiltersFactory<TestDocument>>(MockBehavior.Strict);
        private readonly Mock<IDateRangeFilterService<TestDocument>> _dateRangeFilterService = new Mock<IDateRangeFilterService<TestDocument>>(MockBehavior.Strict);
        private readonly Mock<IListFilterService<TestDocument>> _listFilterService = new Mock<IListFilterService<TestDocument>>(MockBehavior.Strict);
        private readonly Mock<IRadioFilterService<TestDocument>> _radioFilterService = new Mock<IRadioFilterService<TestDocument>>(MockBehavior.Strict);
        private readonly Mock<ITextBoxFilterService<TestDocument>> _textBoxFilterService = new Mock<ITextBoxFilterService<TestDocument>>();

        private readonly FiltersExecutionManager<TestDocument> _manager;

        private readonly TestDocument[] _documents = new[]
        {
            new TestDocument
            {
                Name = "document-01",
                DocumentType = "document-type-01",
                Organisation = "organisation-01",
                UploadedDateTime = new DateTime(2020, 1, 1),
                Team = "team-01",
                Ukprn = "12345678"
            },
            new TestDocument
            {
                Name = "document-02",
                DocumentType = "document-type-02",
                Organisation = "organisation-01",
                UploadedDateTime = new DateTime(2020, 6, 1),
                Team = "team-01",
                Ukprn = "12345678"
            },
            new TestDocument
            {
                Name = "document-03",
                DocumentType = "document-type-02",
                Organisation = "organisation-02",
                UploadedDateTime = new DateTime(2020, 12, 1),
                Team = "team-02",
                Ukprn = "99999999"
            }
        };

        public FiltersExecutionManagerTests()
        {
            _filterByDocumentType
                .SetupGet(filter => filter.FilterType)
                .Returns(FilterType.ListFilter);

            _filterByOrganisation
               .SetupGet(filter => filter.FilterType)
               .Returns(FilterType.ListFilter);

            _filterByUploadedDateTime
               .SetupGet(filter => filter.FilterType)
               .Returns(FilterType.DateRangeFilter);

            _filterByTeam
                .SetupGet(filter => filter.FilterType)
                .Returns(FilterType.RadioFilter);

            _filterByUkprn
                .SetupGet(filter => filter.FilterType)
                .Returns(FilterType.TextBoxFilter);

            _manager = new FiltersExecutionManager<TestDocument>(
                _filtersFactory.Object,
                _dateRangeFilterService.Object,
                _listFilterService.Object,
                _radioFilterService.Object,
                _textBoxFilterService.Object);
        }

        [TestMethod]
        public async Task Filter_WhenOnlyUsingDateFilters_ReturnsDateFilterServiceResult()
        {
            // Arrange
            _filtersFactory
                .Setup(factory => factory.GetFilters(_documents, It.IsAny<IEnumerable<FilterKey>>()))
                .Returns(new IFilter<TestDocument>[] { _filterByUploadedDateTime.Object });

            var filters = GetInitialFiltersResult().OfType<DateRangeFilter>();

            var expectedResult = new FilterResult<TestDocument>
            {
                Items = _documents,
                Filters = filters
            };

            _dateRangeFilterService
                .Setup(dateFilterService => dateFilterService.FilterAndGetUpdatedFilters(
                    _documents,
                    It.IsAny<IEnumerable<IDateRangeFilter<TestDocument>>>(),
                    It.IsAny<IEnumerable<DateRangeFilterOption>>()))
                .ReturnsAsync(new FilterResult<TestDocument>
                {
                    Items = _documents,
                    Filters = filters
                });

            // Act
            var result = await _manager.Filter(_documents, Enumerable.Empty<IFilterOption>(), new[] { FilterKey.UploadDate });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_filtersFactory, _dateRangeFilterService);
        }

        [TestMethod]
        public async Task Filter_WhenOnlyUsingListFilters_ReturnsListFilterServiceResult()
        {
            // Arrange
            _filtersFactory
                .Setup(factory => factory.GetFilters(_documents, It.IsAny<IEnumerable<FilterKey>>()))
                .Returns(new IFilter<TestDocument>[] { _filterByDocumentType.Object, _filterByOrganisation.Object });

            var filters = GetInitialFiltersResult().OfType<ListFilter>();

            var expectedResult = new FilterResult<TestDocument>
            {
                Items = _documents,
                Filters = filters
            };

            _listFilterService
                .Setup(listFilterService => listFilterService.FilterAndGetUpdatedFilters(
                    _documents,
                    It.IsAny<IEnumerable<IListFilter<TestDocument>>>(),
                    It.IsAny<IEnumerable<ListFilterOption>>()))
                .ReturnsAsync(new FilterResult<TestDocument>
                {
                    Items = _documents,
                    Filters = filters
                });

            // Act
            var result = await _manager.Filter(_documents, Enumerable.Empty<IFilterOption>(), new[] { FilterKey.AcademicYear });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_filtersFactory, _listFilterService);
        }

        [TestMethod]
        public async Task Filter_WhenOnlyUsingRadioFilters_ReturnsRadioFilterServiceResult()
        {
            // Arrange
            _filtersFactory
                .Setup(factory => factory.GetFilters(_documents, It.IsAny<IEnumerable<FilterKey>>()))
                .Returns(new IFilter<TestDocument>[] { _filterByTeam.Object });

            var filters = GetInitialFiltersResult().OfType<RadioFilter>();

            var expectedResult = new FilterResult<TestDocument>
            {
                Items = _documents,
                Filters = filters
            };

            _radioFilterService
                .Setup(listFilterService => listFilterService.FilterAndGetUpdatedFilters(
                    _documents,
                    It.IsAny<IEnumerable<IMatchingValueSelectionFilter<TestDocument>>>(),
                    It.IsAny<IEnumerable<RadioFilterOption>>()))
                .ReturnsAsync(new FilterResult<TestDocument>
                {
                    Items = _documents,
                    Filters = filters
                });

            // Act
            var result = await _manager.Filter(_documents, Enumerable.Empty<IFilterOption>(), new[] { FilterKey.Team });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_filtersFactory, _radioFilterService);
        }

        [TestMethod]
        public async Task Filter_WhenOnlyUsingTextBoxFilters_ReturnsRadioFilterServiceResult()
        {
            // Arrange
            _filtersFactory
                .Setup(factory => factory.GetFilters(_documents, It.IsAny<IEnumerable<FilterKey>>()))
                .Returns(new IFilter<TestDocument>[] { _filterByUkprn.Object });

            var filters = GetInitialFiltersResult().OfType<TextBoxFilter>();

            var expectedResult = new FilterResult<TestDocument>
            {
                Items = _documents,
                Filters = filters
            };

            _textBoxFilterService
                .Setup(listFilterService => listFilterService.FilterAndGetUpdatedFilters(
                    _documents,
                    It.IsAny<IEnumerable<IMatchingTextBoxValueFilter<TestDocument>>>(),
                    It.IsAny<IEnumerable<TextBoxFilterOption>>()))
                .ReturnsAsync(new FilterResult<TestDocument>
                {
                    Items = _documents,
                    Filters = filters
                });

            // Act
            var result = await _manager.Filter(_documents, Enumerable.Empty<IFilterOption>(), new[] { FilterKey.Ukprn });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_filtersFactory, _listFilterService, _textBoxFilterService);
        }

        [TestMethod]
        public async Task Filter_WhenUsingCombinedFilterTypes_ReturnsServicesResult()
        {
            // Arrange
            var filterKeys = new[] { FilterKey.ProductIdList, FilterKey.Organisation, FilterKey.UploadDate, FilterKey.Team, FilterKey.Ukprn };

            var filters = GetInitialFiltersResult();

            var listFilterDocumentsResult = _documents.Where(doc => doc.DocumentType == "document-type-02" || doc.DocumentType == "document-type-03");
            var dateFilterDocumentsResult = listFilterDocumentsResult.Where(doc => doc.UploadedDateTime >= new DateTime(2020, 5, 1));
            var radioFilterDocumentsResult = dateFilterDocumentsResult.Where(doc => doc.Team == "team-01");
            var textBoxFilterDocumentsResult = radioFilterDocumentsResult.Where(doc => doc.Ukprn == "12345678");

            _filtersFactory
                .SetupSequence(factory => factory.GetFilters(_documents, It.Is<IEnumerable<FilterKey>>(keys => keys.All(key => key == FilterKey.ProductIdList || key == FilterKey.Organisation))))
                .Returns(new IFilter<TestDocument>[] { _filterByDocumentType.Object, _filterByOrganisation.Object });

            _filtersFactory
                .SetupSequence(factory => factory.GetFilters(listFilterDocumentsResult, It.Is<IEnumerable<FilterKey>>(keys => keys.Contains(FilterKey.UploadDate))))
                .Returns(new IFilter<TestDocument>[] { _filterByUploadedDateTime.Object });

            _filtersFactory
               .SetupSequence(factory => factory.GetFilters(dateFilterDocumentsResult, It.Is<IEnumerable<FilterKey>>(keys => keys.Contains(FilterKey.Team))))
               .Returns(new IFilter<TestDocument>[] { _filterByTeam.Object });

            _filtersFactory
               .SetupSequence(factory => factory.GetFilters(radioFilterDocumentsResult, It.Is<IEnumerable<FilterKey>>(keys => keys.Contains(FilterKey.Ukprn))))
               .Returns(new IFilter<TestDocument>[] { _filterByUkprn.Object });

            var expectedResult = new FilterResult<TestDocument>
            {
                Items = textBoxFilterDocumentsResult,
                Filters = filters
            };

            _listFilterService
                .Setup(listFilterService => listFilterService.FilterAndGetUpdatedFilters(
                    _documents,
                    It.IsAny<IEnumerable<IListFilter<TestDocument>>>(),
                    It.IsAny<IEnumerable<ListFilterOption>>()))
                .ReturnsAsync(new FilterResult<TestDocument>
                {
                    Items = listFilterDocumentsResult,
                    Filters = filters.OfType<ListFilter>()
                });

            _dateRangeFilterService
                .Setup(dateFilterService => dateFilterService.FilterAndGetUpdatedFilters(
                    listFilterDocumentsResult,
                    It.IsAny<IEnumerable<IDateRangeFilter<TestDocument>>>(),
                    It.IsAny<IEnumerable<DateRangeFilterOption>>()))
                .ReturnsAsync(new FilterResult<TestDocument>
                {
                    Items = dateFilterDocumentsResult,
                    Filters = filters.OfType<DateRangeFilter>()
                });

            _radioFilterService
                .Setup(radioFilterService => radioFilterService.FilterAndGetUpdatedFilters(
                    dateFilterDocumentsResult,
                    It.IsAny<IEnumerable<IMatchingValueSelectionFilter<TestDocument>>>(),
                    It.IsAny<IEnumerable<RadioFilterOption>>()))
                .ReturnsAsync(new FilterResult<TestDocument>
                {
                    Items = radioFilterDocumentsResult,
                    Filters = filters.OfType<RadioFilter>()
                });

            _textBoxFilterService
                .Setup(textBoxFilterService => textBoxFilterService.FilterAndGetUpdatedFilters(
                    radioFilterDocumentsResult,
                    It.IsAny<IEnumerable<IMatchingTextBoxValueFilter<TestDocument>>>(),
                    It.IsAny<IEnumerable<TextBoxFilterOption>>()))
                .ReturnsAsync(new FilterResult<TestDocument>
                {
                    Items = textBoxFilterDocumentsResult,
                    Filters = filters.OfType<TextBoxFilter>()
                });

            // Act
            var result = await _manager.Filter(_documents, Enumerable.Empty<IFilterOption>(), filterKeys);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_filtersFactory, _dateRangeFilterService, _listFilterService, _radioFilterService, _textBoxFilterService);
        }

        private IEnumerable<IFilter> GetInitialFiltersResult()
            => new IFilter[]
            {
                new ListFilter
                {
                    Title = "Filter by document type",
                    Key = "docType",
                    Values = new[]
                    {
                        new FilterValue
                        {
                            Title = "document-type-01",
                            Value = "document-type-01",
                            Selected = false,
                            Count = 2,
                            Category = string.Empty
                        },
                        new FilterValue
                        {
                            Title = "document-type-02",
                            Value = "document-type-02",
                            Selected = false,
                            Count = 1,
                            Category = string.Empty
                        }
                    }
                },
                new ListFilter
                {
                    Title = "Filter by organisation",
                    Key = "organisation",
                    Values = new[]
                    {
                        new FilterValue
                        {
                            Title = "organisation-01",
                            Value = "organisation-01",
                            Selected = false,
                            Count = 2,
                            Category = string.Empty
                        },
                        new FilterValue
                        {
                            Title = "organisation-02",
                            Value = "organisation-02",
                            Selected = false,
                            Count = 1,
                            Category = string.Empty
                        }
                    }
                },
                new DateRangeFilter
                {
                    Title = "Filter by date",
                    Key = "date",
                    FromTitle = "From",
                    From = null,
                    ToTitle = "To",
                    To = null
                },
                new RadioFilter
                {
                    Title = "Filter by team",
                    Key = "team",
                    Values = new[]
                    {
                        new RadioFilterValue
                        {
                            Title = "Team 1",
                            Value = "team-01",
                            Selected = false
                        },
                        new RadioFilterValue
                        {
                            Title = "Team 2",
                            Value = "team-02",
                            Selected = false
                        }
                    }
                },
                new TextBoxFilter
                {
                    Title = "Filter by UKPRN",
                    Key = "ukprn",
                    Value = string.Empty
                }
            };
    }
}