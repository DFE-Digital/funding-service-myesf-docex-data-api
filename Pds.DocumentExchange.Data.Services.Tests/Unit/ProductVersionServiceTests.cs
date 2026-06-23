using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class ProductVersionServiceTests
    {
        private readonly Mock<ICosmosDbService> _cosmosDbService = new Mock<ICosmosDbService>(MockBehavior.Strict);
        private readonly Mock<IAcademicYearCalculator> _academicYearCalculator = new Mock<IAcademicYearCalculator>(MockBehavior.Strict);

        private readonly ProductVersionService _productVersionService;

        public ProductVersionServiceTests()
        {
            _productVersionService = new ProductVersionService(
                _cosmosDbService.Object,
                _academicYearCalculator.Object);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetCurrentProductVersion_WhenOrganisationIdentifierIsNull_ThrowsArgumentNullException()
        {
            // Act
            Func<Task<int>> func = () => _productVersionService.GetCurrentProductVersion(null, 1000);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetCurrentProductVersion_WhenOrganisationIdentifierIsValid_ReturnsProductVersion()
        {
            // Arrange
            var organisationIdentifier = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            int academicYear = 202021;
            int productIdentifier = 1000;

            int expectedProductVersion = 10;

            _academicYearCalculator
                .Setup(calc => calc.GetCurrentAcademicYear())
                .Returns(academicYear);

            _cosmosDbService
                .Setup(db => db.GetCurrentVersionNumber(
                    organisationIdentifier,
                    It.IsAny<OrganisationIdentifier>(),
                    academicYear,
                    productIdentifier))
                .ReturnsAsync(expectedProductVersion);

            // Act
            var result = await _productVersionService.GetCurrentProductVersion(organisationIdentifier, productIdentifier);

            // Assert
            result.Should().Be(expectedProductVersion);
        }
    }
}