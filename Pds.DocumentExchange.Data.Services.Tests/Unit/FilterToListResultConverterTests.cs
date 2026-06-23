using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class FilterToListResultConverterTests
    {
        private readonly Mock<IPagingService> _pagingService = new Mock<IPagingService>(MockBehavior.Strict);
        private readonly FilterToListResultConverter<int> _filterToListResultConverter;

        public FilterToListResultConverterTests()
        {
            _filterToListResultConverter = new FilterToListResultConverter<int>(_pagingService.Object);
        }

        [TestMethod]
        public void Convert_WhenFilterResultIsNull_Throws()
        {
            // Act
            Func<ListResult<int>> func = () => _filterToListResultConverter.Convert(null, 1, 1);

            // Assert
            func.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-50)]
        public void Convert_WhenSelectedPageNumberZeroOrLess_ReturnsListResultWithNoItems(int pageNumber)
        {
            // Arrange
            var filterResult = new FilterResult<int>
            {
                Items = CreateTestItems(100),
                Filters = CreateTestFilters()
            };

            var pageSize = 10;
            var pagedResults = CreatePagedTestItems(filterResult.Items, pageSize);

            _pagingService
                .Setup(p => p.Paginate(filterResult.Items, pageSize))
                .Returns(pagedResults);

            var expectedResult = new ListResult<int>
            {
                Items = Enumerable.Empty<int>(),
                TotalItems = filterResult.Items.Count(),
                TotalPages = pagedResults.Count(),
                Filters = filterResult.Filters
            };

            // Act
            var result = _filterToListResultConverter.Convert(filterResult, pageSize, pageNumber);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        [DataRow(100, 10, 11)]
        [DataRow(100, 10, 20)]
        [DataRow(20, 5, 5)]
        public void Convert_WhenSelectedPageNumberHigherThanNumberOfPages_ReturnsListResultWithNoItems(int numberOfItems, int pageSize, int pageNumber)
        {
            // Arrange
            var filterResult = new FilterResult<int>
            {
                Items = CreateTestItems(numberOfItems),
                Filters = CreateTestFilters()
            };

            var pagedResults = CreatePagedTestItems(filterResult.Items, pageSize);

            _pagingService
                .Setup(p => p.Paginate(filterResult.Items, pageSize))
                .Returns(pagedResults);

            var expectedResult = new ListResult<int>
            {
                Items = Enumerable.Empty<int>(),
                TotalItems = filterResult.Items.Count(),
                TotalPages = pagedResults.Count(),
                Filters = filterResult.Filters
            };

            // Act
            var result = _filterToListResultConverter.Convert(filterResult, pageSize, pageNumber);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        [DataRow(100, 10, 1)]
        [DataRow(100, 10, 5)]
        [DataRow(20, 5, 3)]
        public void Convert_WhenSelectedPageNumberEqualOrLessThanNumberOfPages_ReturnsListResultWithItemsInPage(int numberOfItems, int pageSize, int pageNumber)
        {
            // Arrange
            var filterResult = new FilterResult<int>
            {
                Items = CreateTestItems(numberOfItems),
                Filters = CreateTestFilters()
            };

            var pagedResults = CreatePagedTestItems(filterResult.Items, pageSize);

            _pagingService
                .Setup(p => p.Paginate(filterResult.Items, pageSize))
                .Returns(pagedResults);

            var expectedResult = new ListResult<int>
            {
                Items = pagedResults.ElementAt(pageNumber - 1),
                TotalItems = filterResult.Items.Count(),
                TotalPages = pagedResults.Count(),
                Filters = filterResult.Filters
            };

            // Act
            var result = _filterToListResultConverter.Convert(filterResult, pageSize, pageNumber);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        private IEnumerable<int> CreateTestItems(int numberOfItems)
            => Enumerable.Range(1, numberOfItems);

        private IEnumerable<IEnumerable<int>> CreatePagedTestItems(IEnumerable<int> items, int pageSize)
        {
            var pages = new List<IEnumerable<int>>();

            for (int i = 0; i < pageSize; i++)
            {
                IEnumerable<int> currentPage = items.Skip(i * pageSize).Take(pageSize);
                pages.Add(currentPage);
            }

            return pages;
        }

        private IEnumerable<IFilter> CreateTestFilters()
            => new[]
            {
                CreateListFilter(),
                CreateDateRangeFilter()
            };

        private IFilter CreateListFilter()
            => new ListFilter
            {
                Title = "The list filter",
                Key = "list-filter-key",
                Values = new[]
                {
                    new FilterValue
                    {
                        Title = "First value",
                        Value = "1",
                        Selected = false,
                        Count = 10
                    }
                }
            };

        private IFilter CreateDateRangeFilter()
            => new DateRangeFilter
            {
                Title = "The date filter",
                Key = "date-filter-key",
                FromTitle = "from",
                From = null,
                ToTitle = "to",
                To = null
            };
    }
}