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
    public class DateRangeFilterServiceTests
    {
        private readonly Mock<IDateRangeFilter<TestUser>> _filterByDOB = new Mock<IDateRangeFilter<TestUser>>(MockBehavior.Strict);
        private readonly Mock<IDateRangeFilter<TestUser>> _filterByLastLogin = new Mock<IDateRangeFilter<TestUser>>(MockBehavior.Strict);

        private readonly IEnumerable<IDateRangeFilter<TestUser>> _filters;

        private readonly DateRangeFilterService<TestUser> _filteringService = new DateRangeFilterService<TestUser>();

        private readonly TestUser[] _users = new[]
            {
                new TestUser
                {
                    Name = "user-1",
                    DOB = new DateTime(1960, 1, 1),
                    LastLogin = new DateTime(2020, 1, 1)
                },
                new TestUser
                {
                    Name = "user-2",
                    DOB = new DateTime(1970, 1, 1),
                    LastLogin = new DateTime(2020, 2, 1)
                },
                new TestUser
                {
                    Name = "user-3",
                    DOB = new DateTime(1980, 1, 1),
                    LastLogin = new DateTime(2020, 3, 1)
                },
                new TestUser
                {
                    Name = "user-4",
                    DOB = new DateTime(1990, 1, 1),
                    LastLogin = new DateTime(2020, 5, 1)
                },
                new TestUser
                {
                    Name = "user-5",
                    DOB = new DateTime(2000, 1, 1),
                    LastLogin = new DateTime(2020, 6, 1)
                }
            };

        public DateRangeFilterServiceTests()
        {
            InitializeFilterByDOB();
            InitializeFilterByLastLogin();

            _filters = new[] { _filterByDOB.Object, _filterByLastLogin.Object };
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterAndGetUpdatedFilters_WhenFilterListIsEmpty_ThrowsArgumentNullException()
        {
            // Arrange
            var filterByDOB = new DateRangeFilterOption
            {
                Key = "dob",
                From = new DateTime(1990, 1, 1),
                To = new DateTime(2020, 1, 1)
            };

            // Act
            Func<Task<FilterResult<TestUser>>> func = () => _filteringService.FilterAndGetUpdatedFilters(
                _users,
                Collection.Empty<IDateRangeFilter<TestUser>>(),
                new[] { filterByDOB });

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterAndGetUpdatedFilters_WhenFilterOptionContainsNonExistingKey_ThrowsKeyNotFoundException()
        {
            // Arrange
            // Arrange
            var filterByNonExistingKey = new DateRangeFilterOption
            {
                Key = "non-existing-key",
                From = new DateTime(1990, 1, 1),
                To = new DateTime(2020, 1, 1)
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
        public async Task FilterAndGetUpdatedFilters_WhenSourceListIsEmpty_ReturnsEmptyListAndFilterWithNoValues()
        {
            // Arrange
            var sourceList = Collection.Empty<TestUser>();

            var filterByDOB = new DateRangeFilterOption
            {
                Key = "dob",
                From = new DateTime(1990, 1, 1),
                To = new DateTime(2020, 1, 1)
            };

            var filters = GetInitialFiltersResult();
            var dboFilter = filters.Single(filter => filter.Key == "dob");

            dboFilter.From = filterByDOB.From;
            dboFilter.To = filterByDOB.To;

            var expectedResult = new FilterResult<TestUser>
            {
                Items = Collection.Empty<TestUser>(),
                Filters = filters
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                sourceList,
                _filters,
                new[] { filterByDOB });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenFilterOptionsListIsNull_ReturnSourceListAndFilters()
        {
            // Arrange
            var expectedResult = new FilterResult<TestUser>
            {
                Items = _users,
                Filters = GetInitialFiltersResult()
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
            var expectedResult = new FilterResult<TestUser>
            {
                Items = _users,
                Filters = GetInitialFiltersResult()
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _users,
                _filters,
                Collection.Empty<DateRangeFilterOption>());

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenNoFilterValueIsSelected_ReturnSourceListAndFilters()
        {
            // Arrange
            var filterByDOB = new DateRangeFilterOption
            {
                Key = "dob",
                From = null,
                To = null
            };

            var filterByLastLogin = new DateRangeFilterOption
            {
                Key = "lastlogin",
                From = null,
                To = null
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
                new[] { filterByDOB, filterByLastLogin });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenRangeIsSelected_ReturnSourceListAndFilters()
        {
            // Arrange
            var filterByDOB = new DateRangeFilterOption
            {
                Key = "dob",
                From = new DateTime(1990, 1, 1),
                To = new DateTime(2005, 6, 1)
            };

            var filters = GetInitialFiltersResult();
            var dboFilter = filters.Single(filter => filter.Key == "dob");

            dboFilter.From = filterByDOB.From;
            dboFilter.To = filterByDOB.To;

            var expectedResult = new FilterResult<TestUser>
            {
                Items = _users.Where(user => user.DOB >= filterByDOB.From && user.DOB <= filterByDOB.To),
                Filters = filters
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _users,
                _filters,
                new[] { filterByDOB });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FilterAndGetUpdatedFilters_WhenRangesAreSelectedInDifferentFilters_ReturnSourceListAndFilters()
        {
            // Arrange
            var filterByDOB = new DateRangeFilterOption
            {
                Key = "dob",
                From = new DateTime(1990, 1, 1),
                To = new DateTime(2005, 6, 1)
            };

            var filterByLastLogin = new DateRangeFilterOption
            {
                Key = "lastlogin",
                From = new DateTime(2020, 5, 1),
                To = new DateTime(2025, 12, 31)
            };

            var filters = GetInitialFiltersResult();
            var dboFilter = filters.Single(filter => filter.Key == "dob");
            dboFilter.From = filterByDOB.From;
            dboFilter.To = filterByDOB.To;

            var lastLoginFilter = filters.Single(filter => filter.Key == "lastlogin");
            lastLoginFilter.From = filterByLastLogin.From;
            lastLoginFilter.To = filterByLastLogin.To;

            var expectedResult = new FilterResult<TestUser>
            {
                Items = _users.Where(user => (user.DOB >= filterByDOB.From && user.DOB <= filterByDOB.To)
                 && (user.LastLogin >= filterByLastLogin.From && user.LastLogin <= filterByLastLogin.To)),
                Filters = filters
            };

            // Act
            var result = await _filteringService.FilterAndGetUpdatedFilters(
                _users,
                _filters,
                new[] { filterByDOB, filterByLastLogin });

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        private IEnumerable<DateRangeFilter> GetInitialFiltersResult()
        => new[]
        {
                new DateRangeFilter
                {
                    Title = "Filter by DOB",
                    Key = "dob",
                    FromTitle = "From DOB",
                    From = null,
                    ToTitle = "To DOB",
                    To = null
                },
                new DateRangeFilter
                {
                    Title = "Filter by last login",
                    Key = "lastlogin",
                    FromTitle = "From last login",
                    From = null,
                    ToTitle = "To last login",
                    To = null
                }
        };

        private void InitializeFilterByDOB()
        {
            _filterByDOB
                .SetupGet(f => f.FilterTitle)
                .Returns("Filter by DOB");

            _filterByDOB
                .SetupGet(f => f.FilterKey)
                .Returns("dob");

            _filterByDOB
                .Setup(f => f.IsElementInsideRange(It.IsAny<TestUser>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns((TestUser user, DateTime from, DateTime to) =>
                {
                    return user.DOB >= from && user.DOB <= to;
                });
        }

        private void InitializeFilterByLastLogin()
        {
            _filterByLastLogin
                .SetupGet(f => f.FilterTitle)
                .Returns("Filter by last login");

            _filterByLastLogin
                .SetupGet(f => f.FilterKey)
                .Returns("lastlogin");

            _filterByLastLogin
                .Setup(f => f.IsElementInsideRange(It.IsAny<TestUser>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns((TestUser user, DateTime from, DateTime to) =>
                {
                    return user.LastLogin >= from && user.LastLogin <= to;
                });
        }
    }
}