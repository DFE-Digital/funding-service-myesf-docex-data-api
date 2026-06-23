using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Utils;
using Pds.Core.Utils.Interfaces;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Implementations.Builders;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Builders
{
    [TestClass]
    public sealed class NotificationSummaryBuilderTests
    {
        private const string EmailSenderAddress = "service.no-reply@education.gov.uk";

        private readonly ICosmosDbService _cosmosDbService
            = Mock.Of<ICosmosDbService>(MockBehavior.Strict);

        private readonly IConfigurationDataService _configurationDataService
            = Mock.Of<IConfigurationDataService>(MockBehavior.Strict);

        private readonly ISystemProvider _systemProvider
            = Mock.Of<ISystemProvider>(MockBehavior.Strict);

        [TestMethod]
        public void Constructor_WithNullCosmosDb_Throws()
        {
            // Act
            Action act = () => new NotificationSummaryBuilder(null, _configurationDataService, _systemProvider);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_WithNullConfigurationDataService_Throws()
        {
            // Act
            Action act = () => new NotificationSummaryBuilder(_cosmosDbService, null, _systemProvider);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_WithNullSystemProvider_Throws()
        {
            // Act
            Action act = () => new NotificationSummaryBuilder(_cosmosDbService, _configurationDataService, null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow(NotificationType.ProviderUploadClear, 10234, "any old notes...", "email1.AnyOrg.com")]
        [DataRow(NotificationType.ProviderUploadInfected, 10345, "some more old notes...", "email2.AnyOrg.com")]
        public async Task BuildSummary_ReturnsExpected(NotificationType notificationType, int ukprn, string notes, string recipient)
        {
            // arrange
            var sut = GetTestBuilder();

            Mock.Get(_configurationDataService)
                .Setup(x => x.GetServiceNoReplyEmail())
                .ReturnsAsync(EmailSenderAddress);

            // act
            var result = await sut.BuildSummary(notificationType, ukprn, notes, recipient);

            // assert
            VerifyAllMocks();

            result.Should().BeEquivalentTo(new NotificationSummary
            {
                NotificationType = notificationType,
                Ukprn = ukprn,
                Notes = notes,
                From = new[] { EmailSenderAddress },
                To = new[] { recipient }
            });
        }

        [TestMethod]
        [DataRow(NotificationType.ProviderUploadClear, 10234, "any old notes...", "email1.AnyOrg.com", "email3.AnyOrg.com", "email2.AnyOrg.com")]
        [DataRow(NotificationType.ProviderUploadInfected, 10345, "some more old notes...", "email20.AnyOrg.com", "email21.AnyOrg.com")]
        public async Task BuildSummary_WithMultipleRecipients_ReturnsExpected(NotificationType notificationType, int ukprn, string notes, params string[] recipients)
        {
            // arrange
            var sut = GetTestBuilder();

            Mock.Get(_configurationDataService)
                .Setup(x => x.GetServiceNoReplyEmail())
                .ReturnsAsync(EmailSenderAddress);

            // act
            var result = await sut.BuildSummary(notificationType, ukprn, notes, recipients);

            // assert
            VerifyAllMocks();

            result.Should().BeEquivalentTo(new NotificationSummary
            {
                NotificationType = notificationType,
                Ukprn = ukprn,
                Notes = notes,
                From = new[] { EmailSenderAddress },
                To = recipients
            });
        }

        [TestMethod]
        public async Task SaveSummary_CreatesTheSummaryInCosmosDb()
        {
            // Arrange
            const string parentBatchId = "12345";
            const string guid = "12259610-2b63-42d2-a54c-a3f58d45a82a";
            var currentDateTime = new DateTime(2020, 12, 25, 15, 32, 37);

            var summaryBuilder = GetTestBuilder();

            var batchNotificationSummary = new BatchNotificationSummary
            {
                ParentBatchIdentifier = parentBatchId
            };

            var batchAnalysis = new Mock<IBatchMetadataAnalysis>(MockBehavior.Strict);

            var batches = new[]
            {
                new BatchMetadata
                {
                    Id = "batch-id",
                    Files = new[]
                    {
                        new FileMetadata
                        {
                            FileName = "file-name"
                        }
                    }
                }
            };

            batchAnalysis
                .SetupGet(ba => ba.Batches)
                .Returns(batches);

            var guidProvider = new Mock<IGuidProvider>(MockBehavior.Strict);

            guidProvider
                .Setup(g => g.NewGuid())
                .Returns(Guid.Parse(guid));

            Mock.Get(_systemProvider)
                .SetupGet(s => s.Guid)
                .Returns(guidProvider.Object);

            var dateTimeProvider = new Mock<IDateTimeProvider>(MockBehavior.Strict);

            dateTimeProvider
                .Setup(g => g.UtcNow())
                .Returns(currentDateTime);

            Mock.Get(_systemProvider)
                .SetupGet(s => s.DateTime)
                .Returns(dateTimeProvider.Object);

            BatchNotificationSummary actualSummarySaved = null;

            Mock.Get(_cosmosDbService)
                .Setup(c => c.AddBatchNotificationSummary(It.IsAny<BatchNotificationSummary>()))
                .Returns((BatchNotificationSummary captured) =>
                {
                    actualSummarySaved = captured;
                    return Task.CompletedTask;
                });

            FileMetadata actualFileMetadata = null;

            Mock.Get(_cosmosDbService)
                .Setup(c => c.AddHistoryToFileMetadata(batches[0].Id, It.IsAny<FileMetadata>()))
                .ReturnsAsync((string _, FileMetadata captured) =>
                {
                    actualFileMetadata = captured;
                    return "result";
                });

            // Act
            await summaryBuilder.SaveSummary(batchNotificationSummary, batchAnalysis.Object);

            // Assert
            actualSummarySaved.ParentBatchIdentifier.Should().Be(parentBatchId);
            actualSummarySaved.Id.Should().Be(guid);

            actualFileMetadata.FileName.Should().Be(batches[0].Files.Single().FileName);
            actualFileMetadata.History.Should().AllBeEquivalentTo(new FileMetadataHistory
            {
                Action = Enums.FileAction.EmailSent,
                ActionDateTimeUtc = currentDateTime
            });

            VerifyAllMocks();
            Mock.VerifyAll(guidProvider);
        }

        private void VerifyAllMocks()
        {
            Mock.Get(_cosmosDbService).VerifyAll();
            Mock.Get(_configurationDataService).VerifyAll();
            Mock.Get(_systemProvider).VerifyAll();
        }

        private NotificationSummaryBuilder GetTestBuilder()
            => new NotificationSummaryBuilder(_cosmosDbService, _configurationDataService, _systemProvider);
    }
}