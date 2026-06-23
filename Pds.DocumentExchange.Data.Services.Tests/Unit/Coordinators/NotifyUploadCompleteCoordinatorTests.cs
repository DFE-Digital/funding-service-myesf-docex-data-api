using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Implementations.Coordinators;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Builders;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Coordinators
{
    [TestClass]
    public sealed class NotifyUploadCompleteCoordinatorTests
    {
        private readonly IBatchMetadataAnalysisFactory _batchAnalyser
            = Mock.Of<IBatchMetadataAnalysisFactory>(MockBehavior.Strict);

        private readonly INotificationMessageBuilder _messageBuilder
            = Mock.Of<INotificationMessageBuilder>(MockBehavior.Strict);

        private readonly INotificationSummaryBuilder _summaryBuilder
            = Mock.Of<INotificationSummaryBuilder>(MockBehavior.Strict);

        private readonly INotifyEmailService _notifyEmailService
              = Mock.Of<INotifyEmailService>(MockBehavior.Strict);

        private readonly Mock<ILoggerAdapter<NotifyUploadCompleteCoordinator>> _mockLoggingService = new Mock<ILoggerAdapter<NotifyUploadCompleteCoordinator>>(MockBehavior.Loose);

        [TestMethod]
        public void Constructor_WithNullAnalyser_Throws()
        {
            // Act
            Action act = () => new NotifyUploadCompleteCoordinator(null, _messageBuilder, _summaryBuilder, _notifyEmailService, _mockLoggingService.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_WithNullMessageBuilder_Throws()
        {
            // Act
            Action act = () => new NotifyUploadCompleteCoordinator(_batchAnalyser, null, _summaryBuilder, _notifyEmailService, _mockLoggingService.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_WithNullHistoryBuilder_Throws()
        {
            // Act
            Action act = () => new NotifyUploadCompleteCoordinator(_batchAnalyser, _messageBuilder, null, _notifyEmailService, _mockLoggingService.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("emailMessageType")]
        public async Task NotifyUsers_MakesExpectedCalls(string emailMessageType)
        {
            // Arrange
            const string batchID = "any old batch id...";
            const string issuingEmail = "any old email address...";
            const string issuerName = "any old name...";
            const string subject = "We've received your document";
            const int organisation = 103493;
            const string summaryLine = "Uploaded by (mail address) any old email address.... 0 viruses. 1 okay";

            var sut = GetTestCoordinator();

            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);
            var message = Mock.Of<INotificationMessage>(MockBehavior.Strict);
            var summary = Mock.Of<INotificationSummary>(MockBehavior.Strict);
            Mock.Get(message)
              .SetupGet(x => x.EmailMessageType)
              .Returns(emailMessageType);


            Mock.Get(analysis)
                .SetupGet(x => x.InfectedFiles)
                .Returns(Array.Empty<FileMetadata>());
            Mock.Get(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(new[] { new FileMetadata() });
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingEmail)
                .Returns(issuingEmail);
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingPerson)
                .Returns(issuerName);
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingOrganisation)
                .Returns(organisation);
            Mock.Get(analysis)
                .SetupGet(x => x.AgencyTeams)
                .Returns(Array.Empty<IBatchAnalysisTeam>());
            Mock.Get(analysis)
                .SetupGet(x => x.ParentBatchID)
                .Returns(string.Empty);

            Mock.Get(_batchAnalyser)
                .Setup(x => x.AnalyseBatch(batchID))
                .Returns(Task.FromResult(analysis));
            Mock.Get(_messageBuilder)
                .Setup(x => x.GetCleanFileMessage(subject, analysis, issuingEmail, issuerName))
                .Returns(Task.FromResult(message));
            Mock.Get(_summaryBuilder)
                .Setup(x => x.BuildSummary(NotificationType.ProviderUploadClear, organisation, summaryLine, issuingEmail))
                .Returns(Task.FromResult(summary));
            Mock.Get(_summaryBuilder)
                .Setup(x => x.SaveSummary(It.IsAny<BatchNotificationSummary>(), analysis))
                .Returns(Task.CompletedTask);

            if (!string.IsNullOrEmpty(emailMessageType))
            {
                Mock.Get(analysis)
                    .SetupGet(x => x.ParentBatchID)
                    .Returns(string.Empty);

                Mock.Get(_notifyEmailService)
                    .Setup(x => x.Push(message))
                    .Returns(Task.CompletedTask);
            }

            // Act
            await sut.NotifyUsers(batchID);

            // Assert
            VerifyAllMocks();
            Mock.Get(analysis).VerifyAll();
            Mock.Get(message).VerifyAll();
            Mock.Get(summary).VerifyAll();
        }

        [TestMethod]
        [DataRow("We've received your document", "")]
        [DataRow("We've received your document", "emailMessageType")]
        public async Task NotifyDocumentSender_WhenClean_MakesExpectedCalls(string subject, string emailMessageType)
        {
            // Arrange
            const string issuer = "any old issuer...";
            const string issuerName = "any old issuing name...";

            var sut = GetTestCoordinator();
            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);
            Mock.Get(analysis)
                .SetupGet(x => x.InfectedFiles)
                .Returns(Array.Empty<FileMetadata>());
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingEmail)
                .Returns(issuer);
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingPerson)
                .Returns(issuerName);

            var message = Mock.Of<INotificationMessage>(MockBehavior.Strict);
            Mock.Get(message)
               .SetupGet(x => x.EmailMessageType)
               .Returns(emailMessageType);

            Mock.Get(_messageBuilder)
                .Setup(x => x.GetCleanFileMessage(subject, analysis, issuer, issuerName))
                .Returns(Task.FromResult(message));

            if (!string.IsNullOrEmpty(emailMessageType))
            {
                Mock.Get(analysis)
                    .SetupGet(x => x.ParentBatchID)
                    .Returns(string.Empty);

                Mock.Get(_notifyEmailService)
                    .Setup(x => x.Push(message))
                    .Returns(Task.CompletedTask);
            }

            // Act
            await sut.NotifyDocumentSender(analysis);

            // Assert
            VerifyAllMocks();
        }

        [TestMethod]
        [DataRow(1, "There is a problem with your document", "")]
        [DataRow(1, "There is a problem with your document", "emailMessageType")]
        [DataRow(2, "There is a problem with your document", "")]
        [DataRow(2, "There is a problem with your document", "emailMessageType")]
        [DataRow(100, "There is a problem with your document", "")]
        [DataRow(100, "There is a problem with your document", "emailMessageType")]
        public async Task NotifyDocumentSender_WhenInfected_MakesExpectedCalls(int infectedCount, string subject, string emailMessageType)
        {
            // Arrange
            const string issuer = "any old issuer...";
            const string issuerName = "any old issuing name...";

            var sut = GetTestCoordinator();
            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);
            Mock.Get(analysis)
                .SetupGet(x => x.InfectedFiles)
                .Returns(Enumerable.Range(0, infectedCount).Select(id => new FileMetadata()).ToList());
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingEmail)
                .Returns(issuer);
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingPerson)
                .Returns(issuerName);

            var message = Mock.Of<INotificationMessage>(MockBehavior.Strict);
            Mock.Get(message)
               .SetupGet(x => x.EmailMessageType)
               .Returns(emailMessageType);

            Mock.Get(_messageBuilder)
                .Setup(x => x.GetInfectedFileMessage(subject, analysis, issuer, issuerName))
                .Returns(Task.FromResult(message));

            if (!string.IsNullOrEmpty(emailMessageType))
            {
                Mock.Get(analysis)
                    .SetupGet(x => x.ParentBatchID)
                    .Returns(string.Empty);

                Mock.Get(_notifyEmailService)
                    .Setup(x => x.Push(message))
                    .Returns(Task.CompletedTask);
            }

            // Act
            await sut.NotifyDocumentSender(analysis);

            // Assert
            VerifyAllMocks();
        }

        [TestMethod]
        [DataRow(0, 1, NotificationType.ProviderUploadClear, "email 1", 10023)]
        [DataRow(1, 0, NotificationType.ProviderUploadInfected, "email 2", 102030)]
        public async Task GetSenderSummary_ReturnsExpected(int infectedCount, int clearCount, NotificationType expectedResult, string recipient, int organisation)
        {
            // Arrange
            string expectedNotes = $"Uploaded by (mail address) {recipient}. {infectedCount} viruses. {clearCount} okay";

            var sut = GetTestCoordinator();
            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);

            Mock.Get(analysis)
                .SetupGet(x => x.InfectedFiles)
                .Returns(Enumerable.Range(0, infectedCount).Select(id => new FileMetadata()).ToList());
            Mock.Get(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(Enumerable.Range(0, clearCount).Select(id => new FileMetadata()).ToList());
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingEmail)
                .Returns(recipient);
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingOrganisation)
                .Returns(organisation);

            var summary = Mock.Of<INotificationSummary>(MockBehavior.Strict);

            Mock.Get(_summaryBuilder)
                .Setup(x => x.BuildSummary(expectedResult, organisation, expectedNotes, recipient))
                .Returns(Task.FromResult(summary));

            // Act
            var result = await sut.GetSenderSummary(analysis);

            // Assert
            VerifyAllMocks();

            result.Should().BeAssignableTo<INotificationSummary>();
        }

        /// <summary>
        /// this is an empty set test.
        /// </summary>
        /// <returns>the currently running (test) task.</returns>
        [TestMethod]
        public async Task NotifyDocumentRecipients_ReturnsExpected()
        {
            // Arrange
            var sut = GetTestCoordinator();
            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);

            Mock.Get(analysis)
                .SetupGet(x => x.AgencyTeams)
                .Returns(Array.Empty<IBatchAnalysisTeam>());

            Mock.Get(analysis)
               .SetupGet(x => x.ParentBatchID)
               .Returns(string.Empty);

            // Act
            var result = await sut.NotifyDocumentRecipients(analysis);

            // Assert
            VerifyAllMocks();
            Mock.Get(analysis).VerifyAll();

            result.Should().BeAssignableTo<IReadOnlyCollection<INotificationSummary>>();
            result.Count.Should().Be(0);
        }

        [TestMethod]
        [DataRow(1, "team 1", "You have a new document to review", "email 12", 10294, "")]
        [DataRow(1, "team 1", "You have a new document to review", "email 12", 10294, "emailMessageType")]
        [DataRow(2, "team 2", "You have new documents to review", "email 03", 483947525, "")]
        [DataRow(2, "team 2", "You have new documents to review", "email 03", 483947525, "emailMessageType")]
        [DataRow(100, "team 3", "You have new documents to review", "email 11", 239473236, "")]
        [DataRow(100, "team 3", "You have new documents to review", "email 11", 239473236, "emailMessageType")]
        public async Task NotifyAgencyTeam_ReturnsExpected(int fileCount, string teamName, string expectedSubject, string recipient, int organisation, string emailMessageType)
        {
            // Arrange
            var sut = GetTestCoordinator();

            string expectedNotes = $"Team: {teamName}. {fileCount} okay";

            var team = Mock.Of<IBatchAnalysisTeam>(MockBehavior.Strict);
            Mock.Get(team)
                .SetupGet(x => x.Name)
                .Returns(teamName);
            Mock.Get(team)
                .SetupGet(x => x.EmailAddress)
                .Returns(recipient);

            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);
            Mock.Get(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(Enumerable.Range(0, fileCount).Select(id => new FileMetadata()).ToList());
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingOrganisation)
                .Returns(organisation);

            var summary = Mock.Of<INotificationSummary>(MockBehavior.Strict);
            Mock.Get(_summaryBuilder)
                .Setup(x => x.BuildSummary(NotificationType.ProviderUploadReceived, organisation, expectedNotes, recipient))
                .Returns(Task.FromResult(summary));

            var message = Mock.Of<INotificationMessage>(MockBehavior.Strict);
            Mock.Get(message)
              .SetupGet(x => x.EmailMessageType)
              .Returns(emailMessageType);

            Mock.Get(_messageBuilder)
                .Setup(x => x.GetAgencyTeamMessage(expectedSubject, analysis, recipient, teamName))
                .Returns(Task.FromResult(message));

            if (!string.IsNullOrEmpty(emailMessageType))
            {
                Mock.Get(analysis)
                    .SetupGet(x => x.ParentBatchID)
                    .Returns(string.Empty);

                Mock.Get(_notifyEmailService)
                    .Setup(x => x.Push(message))
                    .Returns(Task.CompletedTask);
            }

            // Act
            var result = await sut.NotifyAgencyTeam(team, analysis);

            // Assert
            VerifyAllMocks();
            Mock.Get(team).VerifyAll();
            Mock.Get(analysis).VerifyAll();
            Mock.Get(message).VerifyAll();

            result.Should().BeAssignableTo<INotificationSummary>();
        }

        private void VerifyAllMocks()
        {
            Mock.Get(_batchAnalyser).VerifyAll();
            Mock.Get(_messageBuilder).VerifyAll();
            Mock.Get(_summaryBuilder).VerifyAll();
            Mock.Get(_notifyEmailService).VerifyAll();
        }

        private NotifyUploadCompleteCoordinator GetTestCoordinator()
            => new NotifyUploadCompleteCoordinator(_batchAnalyser, _messageBuilder, _summaryBuilder, _notifyEmailService, _mockLoggingService.Object);
    }
}