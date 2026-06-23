using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Tests.Unit.Filters.Models;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using It = Moq.It;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass]
    public class TextBoxFilterServiceTests
    {
        private readonly Mock<IMatchingTextBoxValueFilter<TestUser>> _filterByName = new Mock<IMatchingTextBoxValueFilter<TestUser>>(MockBehavior.Strict);
        private readonly Mock<IMatchingTextBoxValueFilter<TestUser>> _filterBySurname = new Mock<IMatchingTextBoxValueFilter<TestUser>>(MockBehavior.Strict);

        private readonly IEnumerable<IMatchingTextBoxValueFilter<TestUser>> _filters;

        private readonly TextBoxFilterService<TestUser> _filteringService;

        private readonly TestUser[] _users = new[]
            {
                new TestUser { Name = "Dave", Surname = "Cooper" },
                new TestUser { Name = "Lewis", Surname = "White" },
                new TestUser { Name = "Brian", Surname = "Cooper" },
                new TestUser { Name = "Lucy", Surname = "Richards" },
                new TestUser { Name = "Christina", Surname = "Copper" },
                new TestUser { Name = "Lucy", Surname = "White" }
            };

        public TextBoxFilterServiceTests()
        {
            InitializeFilterByName();
            InitializeFilterBySurname();

            _filters = new[] { _filterByName.Object, _filterBySurname.Object };

            _filteringService = new TextBoxFilterService<TestUser>();
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterAndGetUpdatedFilters_WhenFilterListIsEmpty_ThrowsArgumentNullException()
        {
            // Arrange
            var filterByName = new TextBoxFilterOption
            {
                Key = "name",
                Value = "Dave"
            };

            // Act
            Func<Task<FilterResult<TestUser>>> func = () => _filteringService.FilterAndGetUpdatedFilters(
                _users,
                Collection.Empty<IMatchingTextBoxValueFilter<TestUser>>(),
                new[] { filterByName });

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterAndGetUpdatedFilters_WhenFilterOptionContainsNonExistingKey_ThrowsKeyNotFoundException()
        {
            // Arrange
            var filterByNonExistingKey = new TextBoxFilterOption
            {
                Key = "non_existing_key",
                Value = "Christina"
            };

            // Act
            Func<Task<FilterResult<TestUser>>> func = () => _filteringService.FilterAndGetUpdatedFilters(
                _users,
                _filters,
                new[] { filterByNonExistingKey });

            // Assert
            func.Should().ThrowAsync<KeyNotFoundException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenSourceListIsEmpty_ReturnSourceListAndFilters()
        {
            // Arrange
            var sourceList = Collection.Empty<TestUser>();

            var filterByName = new TextBoxFilterOption
            {
                Key = "name",
                Value = "Brian"
            };

            var filters = GetInitialFiltersResult();

            var expectedResult = new FilterResult<TestUser>
            {
                Items = Collection.Empty<TestUser>(),
                Filters = filters
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                sourceList,
                _filters,
                new[] { filterByName });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenFilterOptionsListIsNull_ReturnSourceListAndFilters()
        {
            // Arrange
            var filters = GetInitialFiltersResult();

            var expectedResult = new FilterResult<TestUser>
            {
                Items = _users,
                Filters = filters
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _users,
                _filters,
                null);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenFilterOptionsListIsEmpty_ReturnSourceListAndFilters()
        {
            // Arrange
            var filters = GetInitialFiltersResult();

            var expectedResult = new FilterResult<TestUser>
            {
                Items = _users,
                Filters = filters
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _users,
                _filters,
                Collection.Empty<TextBoxFilterOption>());

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenNoFilterValueIsSelected_ReturnSourceListAndFilters()
        {
            // Arrange
            var filterByName = new TextBoxFilterOption
            {
                Key = "name",
                Value = string.Empty
            };

            var filterBySurname = new TextBoxFilterOption
            {
                Key = "surname",
                Value = null
            };

            var expectedResult = new FilterResult<TestUser>
            {
                Items = _users,
                Filters = GetInitialFiltersResult()
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _users,
                _filters,
                new[] { filterByName, filterBySurname });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenValueIsSelected_ReturnsFilteredListAndFilters()
        {
            // Arrange
            var filterByName = new TextBoxFilterOption
            {
                Key = "name",
                Value = "Christina"
            };

            var filters = GetInitialFiltersResult();

            var nameFilter = filters.Single(filter => filter.Key == "name");
            nameFilter.Value = filterByName.Value;

            var expectedResult = new FilterResult<TestUser>
            {
                Items = _users.Where(user => user.Name == filterByName.Value),
                Filters = new[] { nameFilter }
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _users,
                new[] { _filterByName.Object },
                new[] { filterByName });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenValuesAreSelectedInDifferentFilters_ReturnsFilteredListAndFilters()
        {
            // Arrange
            var filterByName = new TextBoxFilterOption
            {
                Key = "name",
                Value = "Christina"
            };


            var filterBySurname = new TextBoxFilterOption
            {
                Key = "surname",
                Value = "Copper"
            };

            var filters = GetInitialFiltersResult();

            var nameFilter = filters.Single(filter => filter.Key == "name");
            nameFilter.Value = filterByName.Value;

            var surnameFilter = filters.Single(filter => filter.Key == "surname");
            surnameFilter.Value = filterBySurname.Value;

            var expectedResult = new FilterResult<TestUser>
            {
                Items = _users.Where(user => user.Name == filterByName.Value && user.Surname == filterBySurname.Value),
                Filters = filters
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _users,
                _filters,
                new[] { filterByName, filterBySurname });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        private IEnumerable<TextBoxFilter> GetInitialFiltersResult()
        => new[]
        {
                new TextBoxFilter
                {
                    Title = "Filter by name",
                    Key = "name",
                    Hint = "Enter the name here",
                    Value = string.Empty
                },
                new TextBoxFilter
                {
                    Title = "Filter by surname",
                    Key = "surname",
                    Hint = "Enter the surname here",
                    Value = string.Empty
                }
        };

        private void InitializeFilterByName()
        {
            _filterByName
                .SetupGet(f => f.FilterTitle)
                .Returns("Filter by name");

            _filterByName
                .SetupGet(f => f.FilterKey)
                .Returns("name");

            _filterByName
                .SetupGet(f => f.TextBoxHint)
                .Returns("Enter the name here");

            _filterByName
                .SetupGet(f => f.Regex)
                .Returns("[a-zA-Z]");

            _filterByName
               .SetupGet(f => f.ValidationErrorMessage)
               .Returns("Enter a valid name");

            _filterByName
                .Setup(f => f.DoesElementMatchValue(It.IsAny<TestUser>(), It.IsAny<string>()))
                .ReturnsAsync((TestUser user, string value) => user.Name == value);
        }

        private void InitializeFilterBySurname()
        {
            _filterBySurname
                .SetupGet(f => f.FilterTitle)
                .Returns("Filter by surname");

            _filterBySurname
                .SetupGet(f => f.FilterKey)
                .Returns("surname");

            _filterBySurname
               .SetupGet(f => f.TextBoxHint)
               .Returns("Enter the surname here");

            _filterBySurname
                .SetupGet(f => f.Regex)
                .Returns("[a-zA-Z]");

            _filterBySurname
               .SetupGet(f => f.ValidationErrorMessage)
               .Returns("Enter a valid name");

            _filterBySurname
                .Setup(f => f.DoesElementMatchValue(It.IsAny<TestUser>(), It.IsAny<string>()))
                .ReturnsAsync((TestUser user, string value) => user.Surname == value);
        }
    }
}