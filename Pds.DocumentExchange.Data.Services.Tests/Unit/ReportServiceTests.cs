using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.Core.Utils.Interfaces;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class ReportServiceTests
    {
        private readonly Mock<IDateTimeProvider> _mockDateTimeProvider = new Mock<IDateTimeProvider>(MockBehavior.Strict);
        private readonly Mock<ICosmosDbService> _mockCosmosDbService = new Mock<ICosmosDbService>(MockBehavior.Strict);
        private readonly Mock<IEncryptionService> _mockEncryptionService = new Mock<IEncryptionService>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<ReportService>> _mockLoggerAdapter = new Mock<ILoggerAdapter<ReportService>>(MockBehavior.Strict);
        private readonly Mock<IOrganisationsLookup> _mockOrganisationsLookup = new Mock<IOrganisationsLookup>(MockBehavior.Strict);
        private readonly Mock<IConfigurationDataService> _mockConfigurationDataService = new Mock<IConfigurationDataService>(MockBehavior.Strict);
        private readonly Mock<CosmosDbConfiguration> _mockCosmosDbConfiguration = new Mock<CosmosDbConfiguration>(MockBehavior.Strict);

        private readonly ReportService _reportService;

        public ReportServiceTests()
        {
            _reportService = new ReportService(
                _mockDateTimeProvider.Object,
                _mockCosmosDbService.Object,
                _mockEncryptionService.Object,
                _mockLoggerAdapter.Object,
                _mockOrganisationsLookup.Object,
                _mockConfigurationDataService.Object,
                _mockCosmosDbConfiguration.Object);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task GetMIReport_AfterSuccessfulExecution_ReturnsFileContent_()
        {
            //Arrange
            _mockCosmosDbService.Setup(x => x.GetMIReport(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<MIReport>().AsEnumerable());
            _mockDateTimeProvider.Setup(x => x.ConvertToUKTime(It.IsAny<DateTime>())).Returns(DateTime.Now);
            _mockOrganisationsLookup.Setup(x => x.GetAllOrganisations()).ReturnsAsync(It.IsAny<IDictionary<string, Organisation>>());
            _mockConfigurationDataService.Setup(x => x.GetProducts()).ReturnsAsync(It.IsAny<IReadOnlyCollection<Product>>());
            _mockLoggerAdapter.Setup(x => x.LogInformation(It.IsAny<string>()));

            //Act
            var result = await _reportService.GetMIReport(It.IsAny<DateTime>(), It.IsAny<DateTime>());

            //Assert
            result.Should().BeOfType<byte[]>();
            VerifyMocks();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task GetMIReport_WhenCosmosServiceThrowsException_ThrowsException()
        {
            //Arrange
            _mockCosmosDbService.Setup(x => x.GetMIReport(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Throws<Exception>();

            //Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () => await _reportService.GetMIReport(It.IsAny<DateTime>(), It.IsAny<DateTime>()));
            VerifyMocks();
        }

        private void VerifyMocks()
        {
            Mock.VerifyAll(
                _mockDateTimeProvider,
                _mockCosmosDbService,
                _mockEncryptionService,
                _mockLoggerAdapter,
                _mockOrganisationsLookup,
                _mockConfigurationDataService,
                _mockCosmosDbConfiguration);
        }
    }
}
