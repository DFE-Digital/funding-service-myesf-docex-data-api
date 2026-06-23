using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.Implementations;
using System;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class ZipServiceTests
    {
        private readonly ZipService _zipService = new ZipService();

        [TestMethod, TestCategory("Unit")]
        public void ZipFiles_WhenFileCollectionIsNull_ThrowsArgumentNullException()
        {
            // Act
            Func<byte[]> func = () => _zipService.ZipFiles(null);

            // Assert
            func.Should().Throw<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        public void ZipFiles_WhenFileCollectionContainsEmptyFileName_ThrowsArgumentException(string fileName)
        {
            // Arrange
            var files = new[]
            {
                ("test-file-01.txt", new byte[] { 0, 1, 2, 3, 4, 5 }),
                (fileName, new byte[] { 5, 4, 3, 2, 1, 0 })
            };

            // Act
            Func<byte[]> func = () => _zipService.ZipFiles(files);

            // Assert
            func.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void ZipFiles_WhenFileCollectionContainsNullFileContent_ThrowsArgumentException()
        {
            // Arrange
            var files = new[]
            {
                ("test-file-01.txt", new byte[] { 0, 1, 2, 3, 4, 5 }),
                ("test-file-02.txt", null)
            };

            // Act
            Func<byte[]> func = () => _zipService.ZipFiles(files);

            // Assert
            func.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void ZipFiles_WhenFileCollectionContainsEmptyFileContent_ThrowsArgumentException()
        {
            // Arrange
            var files = new[]
            {
                ("test-file-01.txt", new byte[] { 0, 1, 2, 3, 4, 5 }),
                ("test-file-02.txt", new byte[] { })
            };

            // Act
            Func<byte[]> func = () => _zipService.ZipFiles(files);

            // Assert
            func.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void ZipFiles_WhenFileCollectionIsValid_ReturnsZipContent()
        {
            // Arrange
            var files = new[]
            {
                ("test-file-01.txt", new byte[] { 0, 1, 2, 3, 4, 5 }),
                ("test-file-02.txt", new byte[] { 5, 4, 3, 2, 1, 0 })
            };

            // Act
            var result = _zipService.ZipFiles(files);

            // Assert
            result.Should().NotBeNullOrEmpty();
        }
    }
}