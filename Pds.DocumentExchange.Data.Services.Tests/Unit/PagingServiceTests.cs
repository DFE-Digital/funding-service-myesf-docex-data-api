using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.Implementations;
using System;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class PagingServiceTests
    {
        private readonly PagingService _pagingService = new PagingService();

        private readonly string[] _sourceList = new[]
        {
            "rabbit",
            "mouse",
            "dog",
            "cat",
            "lion",
            "tiger",
            "whale",
            "wolf",
            "fox",
            "crocodile"
        };

        [TestMethod, TestCategory("Unit")]
        public void Page_WhenPageSizeIsSmallerThanListLengthAndDivisionRemainderIsZero_ReturnsPagedList()
        {
            // Arrange
            var expectedResult = new[]
            {
                new[] { "rabbit", "mouse", "dog", "cat", "lion" },
                new[] { "tiger", "whale", "wolf", "fox", "crocodile" }
            };

            // Act
            var result = _pagingService.Paginate(_sourceList, 5);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public void Page_WhenPageSizeIsSmallerThanListLengthAndDivisionRemainderIsNotZero_ReturnsPagedList()
        {
            // Arrange
            var expectedResult = new[]
            {
                new[] { "rabbit", "mouse", "dog" },
                new[] { "cat", "lion", "tiger" },
                new[] { "whale", "wolf", "fox" },
                new[] { "crocodile" }
            };

            // Act
            var result = _pagingService.Paginate(_sourceList, 3);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public void Page_WhenPageSizeIsEqualToListLength_ReturnsOnePagePagedList()
        {
            // Arrange
            var expectedResult = new[]
            {
                new[] { "rabbit", "mouse", "dog", "cat", "lion", "tiger", "whale", "wolf", "fox", "crocodile" }
            };

            // Act
            var result = _pagingService.Paginate(_sourceList, 10);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public void Page_WhenPageSizeIsBiggerThanListLength_ReturnsOnePagePagedList()
        {
            // Arrange
            var expectedResult = new[]
            {
                new[] { "rabbit", "mouse", "dog", "cat", "lion", "tiger", "whale", "wolf", "fox", "crocodile" }
            };

            // Act
            var result = _pagingService.Paginate(_sourceList, 20);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public void Page_WhenSourceListIsEmpty_ReturnsEmptyPagedList()
        {
            // Arrange
            var emptySourceList = new List<string>();
            var expectedResult = new List<List<string>>();

            // Act
            var result = _pagingService.Paginate(emptySourceList, 10);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public void Page_WhenSourceListIsNull_ThrowsArgumentException()
        {
            //Arrange
            List<string> nullSourceList = null;

            // Act
            Func<IEnumerable<IEnumerable<string>>> func = () => _pagingService.Paginate(nullSourceList, 5);

            // Assert
            func.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void Page_WhenPageSizeIsSmallerThanOne_ThrowsArgumentException()
        {
            // Act
            Func<IEnumerable<IEnumerable<string>>> func = () => _pagingService.Paginate(_sourceList, 0);

            // Assert
            func.Should().Throw<ArgumentException>();
        }
    }
}