using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Repository.Interfaces;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Implementations.CosmosDb;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    [TestCategory("Unit")]
    public class CosmosDbServiceQueryTests
    {
        private readonly IDocumentsRepository _documentsRepository = Mock.Of<IDocumentsRepository>(MockBehavior.Strict);
        private readonly IFileRepository _fileRepository = Mock.Of<IFileRepository>(MockBehavior.Strict);
        private readonly ILoggerAdapter<CosmosDbServiceBase> _logger = Mock.Of<ILoggerAdapter<CosmosDbServiceBase>>(MockBehavior.Strict);

        [TestMethod]
        public async Task MetadataOfDocumentsReceivedByOrganisations_OnSuccess_ReturnsQueryResults()
        {
            // Arrange
            var ukprns = new[] { 123, 456, 789 };
            var ukprnsParamName = "@ukprns";

            var expectedSql = "select * from this fake query";

            var expectedResult = new[] { new BatchMetadata() };

            Mock.Get(_fileRepository)
                .Setup(repo => repo.ReadSingleFileContents("MetadataOfDocumentsReceivedByOrganisations.sql"))
                .Returns(expectedSql);

            (string Name, object Value)[] actualParameters = null;

            Mock.Get(_documentsRepository)
                .Setup(repo => repo.RunQuery<BatchMetadata>(
                    expectedSql,
                    It.IsAny<(string Name, object Value)[]>()))
                .ReturnsAsync((string sql, (string Name, object Value)[] parameters) =>
                    {
                        actualParameters = parameters;
                        return expectedResult;
                    });

            Mock.Get(_logger)
                .Setup(logger => logger.LogInformation(It.IsAny<string>()));

            var service = GetTestService();

            // Act
            var actual = await service.MetadataOfDocumentsReceivedByOrganisations(ukprns.Select(ukprn => new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = ukprn.ToString()
            }));

            // Assert
            actual.Should().BeEquivalentTo(expectedResult);
            actualParameters.Should().BeEquivalentTo(
                new[]
                {
                    (ukprnsParamName, ukprns)
                });

            VerifyAllMocks();
        }

        [TestMethod]
        public async Task MetadataOfDocumentsReceivedByOrganisations_OnError_LogsErrorAndReturnsNull()
        {
            // Arrange
            var expectedSql = "select * from this fake query";

            var expectedResult = new[] { new BatchMetadata() };

            Mock.Get(_fileRepository)
                .Setup(repo => repo.ReadSingleFileContents("MetadataOfDocumentsReceivedByOrganisations.sql"))
                .Returns(expectedSql);

            Mock.Get(_documentsRepository)
                .Setup(repo => repo.RunQuery<BatchMetadata>(
                    expectedSql,
                    It.IsAny<(string Name, object Value)[]>()))
                .ThrowsAsync(new Exception("Error"));

            Mock.Get(_logger)
                .Setup(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()));

            var service = GetTestService();

            // Act
            var actual = await service.MetadataOfDocumentsReceivedByOrganisations(new[] { new OrganisationIdentifier { Value = "0" } });

            // Assert
            actual.Should().BeNull();
            VerifyAllMocks();
        }

        [TestMethod]
        public async Task MetadataOfDocumentsSentByOrganisations_OnSuccess_ReturnsQueryResults()
        {
            // Arrange
            var ukprns = new[] { 123, 456, 789 };
            var ukprnsParamName = "@ukprns";

            var expectedSql = "select * from this fake query";

            var expectedResult = new[] { new BatchMetadata() };

            Mock.Get(_fileRepository)
                .Setup(repo => repo.ReadSingleFileContents("MetadataOfDocumentsSentByOrganisations.sql"))
                .Returns(expectedSql);

            (string Name, object Value)[] actualParameters = null;

            Mock.Get(_documentsRepository)
                .Setup(repo => repo.RunQuery<BatchMetadata>(
                    expectedSql,
                    It.IsAny<(string Name, object Value)[]>()))
                .ReturnsAsync((string sql, (string Name, object Value)[] parameters) =>
                {
                    actualParameters = parameters;
                    return expectedResult;
                });

            Mock.Get(_logger)
                .Setup(logger => logger.LogInformation(It.IsAny<string>()));

            var service = GetTestService();

            // Act
            var actual = await service.MetadataOfDocumentsSentByOrganisations(ukprns.Select(ukprn => new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = ukprn.ToString()
            }));

            // Assert
            actual.Should().BeEquivalentTo(expectedResult);
            actualParameters.Should().BeEquivalentTo(
                new[]
                {
                    (ukprnsParamName, ukprns)
                });

            VerifyAllMocks();
        }

        [TestMethod]
        public async Task MetadataOfDocumentsSentByOrganisations_OnError_LogsErrorAndReturnsNull()
        {
            // Arrange
            var expectedSql = "select * from this fake query";

            var expectedResult = new[] { new BatchMetadata() };

            Mock.Get(_fileRepository)
                .Setup(repo => repo.ReadSingleFileContents("MetadataOfDocumentsSentByOrganisations.sql"))
                .Returns(expectedSql);

            Mock.Get(_documentsRepository)
                .Setup(repo => repo.RunQuery<BatchMetadata>(
                    expectedSql,
                    It.IsAny<(string Name, object Value)[]>()))
                .ThrowsAsync(new Exception("Error"));

            Mock.Get(_logger)
                .Setup(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()));

            var service = GetTestService();

            // Act
            var actual = await service.MetadataOfDocumentsSentByOrganisations(new[] { new OrganisationIdentifier { Value = "0" } });

            // Assert
            actual.Should().BeNull();
            VerifyAllMocks();
        }

        private CosmosDbService GetTestService()
            => new CosmosDbService(_documentsRepository, _fileRepository, _logger);

        private void VerifyAllMocks()
            => Mock.VerifyAll(
                Mock.Get(_documentsRepository),
                Mock.Get(_fileRepository),
                Mock.Get(_logger));
    }
}