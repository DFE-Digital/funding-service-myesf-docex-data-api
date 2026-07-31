using FluentAssertions;
using MapsterMapper;
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
    public class RadioFilterServiceTests
    {
        private readonly Mock<IMatchingValueSelectionFilter<TestUser>> _filterByName = new Mock<IMatchingValueSelectionFilter<TestUser>>(MockBehavior.Strict);
        private readonly Mock<IMatchingValueSelectionFilter<TestUser>> _filterBySurname = new Mock<IMatchingValueSelectionFilter<TestUser>>(MockBehavior.Strict);
        private readonly Mock<IMapper> _mapper = new Mock<IMapper>(MockBehavior.Strict);

        private readonly IEnumerable<IMatchingValueSelectionFilter<TestUser>> _filters;

        private readonly RadioFilterService<TestUser> _filteringService;

        private readonly TestUser[] _users = new[]
            {
                new TestUser { Name = "Dave", Surname = "Cooper" },
                new TestUser { Name = "Lewis", Surname = "White" },
                new TestUser { Name = "Brian", Surname = "Cooper" },
                new TestUser { Name = "Lucy", Surname = "Richards" },
                new TestUser { Name = "Christina", Surname = "Copper" },
                new TestUser { Name = "Lucy", Surname = "White" }
            };

        public RadioFilterServiceTests()
        {
            InitializeFilterByName();
            InitializeFilterBySurname();

            _filters = new[] { _filterByName.Object, _filterBySurname.Object };

            _filteringService = new RadioFilterService<TestUser>(_mapper.Object);
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterAndGetUpdatedFilters_WhenFilterListIsEmpty_ThrowsArgumentNullException()
        {
            // Arrange
            var filterByName = new RadioFilterOption
            {
                Key = "name",
                Value = "Dave"
            };

            // Act
            Func<Task<FilterResult<TestUser>>> func = () => _filteringService.FilterAndGetUpdatedFilters(
                _users,
                Collection.Empty<IMatchingValueSelectionFilter<TestUser>>(),
                new[] { filterByName });

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterAndGetUpdatedFilters_WhenFilterOptionContainsNonExistingKey_ThrowsKeyNotFoundException()
        {
            // Arrange
            var filterByNonExistingKey = new RadioFilterOption
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

            var filterByName = new RadioFilterOption
            {
                Key = "name",
                Value = "Brian"
            };

            var filters = GetInitialFiltersResult();

            var nameFilter = filters.Single(filter => filter.Key == "name");
            var selectedFilter = nameFilter.Values.Single(value => value.Value == "Brian");
            selectedFilter.Selected = true;

            var expectedResult = new FilterResult<TestUser>
            {
                Items = Collection.Empty<TestUser>(),
                Filters = filters
            };

            _mapper
                .Setup(mapper => mapper.Map<IEnumerable<RadioFilter>, IEnumerable<RadioFilterOption>>(It.IsAny<IEnumerable<RadioFilter>>()))
                .Returns(new[] { filterByName });

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                sourceList,
                _filters,
                new[] { filterByName });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_mapper);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenFilterOptionsListIsNull_ReturnsFilteredListByDefaultOption()
        {
            // Arrange
            var filters = GetInitialFiltersResult();

            foreach (var currentFilter in filters)
            {
                var selectedFilter = currentFilter.Values.First();
                selectedFilter.Selected = true;
            }

            var mappedDefaultFilterOption = new RadioFilterOption { Key = "name", Value = "Brian" };

            _mapper
                .Setup(mapper => mapper.Map<IEnumerable<RadioFilter>, IEnumerable<RadioFilterOption>>(It.IsAny<IEnumerable<RadioFilter>>()))
                .Returns(new[] { mappedDefaultFilterOption });

            var expectedResult = new FilterResult<TestUser>
            {
                Items = _users.Where(user => user.Name == "Brian"),
                Filters = filters
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _users,
                _filters,
                null);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_mapper);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenFilterOptionsListIsEmpty_ReturnsFilteredListByDefaultOption()
        {
            // Arrange
            var filters = GetInitialFiltersResult();

            foreach (var currentFilter in filters)
            {
                var selectedFilter = currentFilter.Values.First();
                selectedFilter.Selected = true;
            }

            var mappedDefaultFilterOption = new RadioFilterOption { Key = "name", Value = "Brian" };

            _mapper
                .Setup(mapper => mapper.Map<IEnumerable<RadioFilter>, IEnumerable<RadioFilterOption>>(It.IsAny<IEnumerable<RadioFilter>>()))
                .Returns(new[] { mappedDefaultFilterOption });

            var expectedResult = new FilterResult<TestUser>
            {
                Items = _users.Where(user => user.Name == "Brian"),
                Filters = filters
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _users,
                _filters,
                Collection.Empty<RadioFilterOption>());

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_mapper);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenNoFilterValueIsSelected_ReturnsFilteredListByDefaultOption()
        {
            // Arrange
            var filterByName = new RadioFilterOption
            {
                Key = "name",
                Value = string.Empty
            };

            var filterBySurname = new RadioFilterOption
            {
                Key = "surname",
                Value = null
            };

            var nameDefaultFilterOption = new RadioFilterOption { Key = "name", Value = "Brian" };
            var surnameDefaultFilterOption = new RadioFilterOption { Key = "surname", Value = "Cooper" };

            _mapper
                .Setup(mapper => mapper.Map<IEnumerable<RadioFilter>, IEnumerable<RadioFilterOption>>(It.IsAny<IEnumerable<RadioFilter>>()))
                .Returns(new[] { nameDefaultFilterOption, surnameDefaultFilterOption });

            var expectedResult = new FilterResult<TestUser>
            {
                Items = _users.Where(user => user.Name == "Brian" && user.Surname == "Cooper"),
                Filters = GetInitialFiltersResult()
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _users,
                _filters,
                new[] { filterByName, filterBySurname });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_mapper);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenValueIsSelected_ReturnsFilteredListAndFilters()
        {
            // Arrange
            var filterByName = new RadioFilterOption
            {
                Key = "name",
                Value = "Christina"
            };

            var filters = GetInitialFiltersResult();
            var nameFilter = filters.Single(filter => filter.Key == "name");

            nameFilter.Values = _users.Select(user => new RadioFilterValue
            {
                Title = user.Name,
                Value = user.Name,
                Selected = user.Name == filterByName.Value
            });

            _mapper
                .Setup(mapper => mapper.Map<IEnumerable<RadioFilter>, IEnumerable<RadioFilterOption>>(It.IsAny<IEnumerable<RadioFilter>>()))
                .Returns(new[] { filterByName });

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
            Mock.VerifyAll(_mapper);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenValuesAreSelectedInDifferentFilters_ReturnsFilteredListAndFilters()
        {
            // Arrange
            var filterByName = new RadioFilterOption
            {
                Key = "name",
                Value = "Christina"
            };

            var filterBySurname = new RadioFilterOption
            {
                Key = "surname",
                Value = "Copper"
            };

            var filters = GetInitialFiltersResult();
            var nameFilter = filters.Single(filter => filter.Key == "name");

            nameFilter.Values = _users.Select(user => new RadioFilterValue
            {
                Title = user.Name,
                Value = user.Name,
                Selected = user.Name == filterByName.Value
            });

            var surnameFilter = filters.Single(filter => filter.Key == "surname");

            surnameFilter.Values = _users.Select(user => new RadioFilterValue
            {
                Title = user.Surname,
                Value = user.Surname,
                Selected = user.Surname == filterBySurname.Value
            });

            _mapper
                .Setup(mapper => mapper.Map<IEnumerable<RadioFilter>, IEnumerable<RadioFilterOption>>(It.IsAny<IEnumerable<RadioFilter>>()))
                .Returns(new[] { filterByName, filterBySurname });

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
            Mock.VerifyAll(_mapper);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenValuesAreSelectedInDifferentFiltersAndNoResultsForOneOfThem_ReturnsEmptyListAndFilters()
        {
            // Arrange
            var filterByName = new RadioFilterOption
            {
                Key = "name",
                Value = "Christina"
            };

            var filterBySurname = new RadioFilterOption
            {
                Key = "surname",
                Value = "A Surname That Isn't Found In The List"
            };

            var filters = GetInitialFiltersResult();
            var nameFilter = filters.Single(filter => filter.Key == "name");

            nameFilter.Values = _users.Select(user => new RadioFilterValue
            {
                Title = user.Name,
                Value = user.Name,
                Selected = user.Name == filterByName.Value
            });

            var surnameFilter = filters.Single(filter => filter.Key == "surname");

            surnameFilter.Values = _users.Select(user => new RadioFilterValue
            {
                Title = user.Surname,
                Value = user.Surname,
                Selected = user.Surname == filterBySurname.Value
            });

            _mapper
                .Setup(mapper => mapper.Map<IEnumerable<RadioFilter>, IEnumerable<RadioFilterOption>>(It.IsAny<IEnumerable<RadioFilter>>()))
                .Returns(new[] { filterByName, filterBySurname });

            var expectedResult = new FilterResult<TestUser>
            {
                Items = Enumerable.Empty<TestUser>(),
                Filters = filters
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _users,
                _filters,
                new[] { filterByName, filterBySurname });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_mapper);
        }

        private IEnumerable<RadioFilter> GetInitialFiltersResult()
        => new[]
        {
                new RadioFilter
                {
                    Title = "Filter by name",
                    Key = "name",
                    Values = _users.Select(user => new RadioFilterValue
                    {
                        Title = user.Name,
                        Value = user.Name,
                        Selected = false
                    })
                },
                new RadioFilter
                {
                    Title = "Filter by surname",
                    Key = "surname",
                    Values = _users.Select(user => new RadioFilterValue
                    {
                        Title = user.Surname,
                        Value = user.Surname,
                        Selected = false
                    })
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
                .Setup(f => f.DoesElementMatchValue(It.IsAny<TestUser>(), It.IsAny<string>()))
                .ReturnsAsync((TestUser user, string value) => user.Name == value);

            _filterByName
               .Setup(f => f.GetAllTitleValuePairs())
               .ReturnsAsync(_users.Select(user => (Title: user.Name, Value: user.Name)));
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
                .Setup(f => f.DoesElementMatchValue(It.IsAny<TestUser>(), It.IsAny<string>()))
                .ReturnsAsync((TestUser user, string value) => user.Surname == value);

            _filterBySurname
               .Setup(f => f.GetAllTitleValuePairs())
               .ReturnsAsync(_users.Select(user => (Title: user.Surname, Value: user.Surname)));
        }
    }
}