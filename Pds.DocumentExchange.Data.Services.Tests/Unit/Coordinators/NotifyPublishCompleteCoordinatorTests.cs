using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Identity.Enums;
using Pds.Core.DfESignIn.Interfaces;
using Pds.Core.DfESignIn.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Implementations.Coordinators;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Builders;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Coordinators
{
    [TestClass]
    public sealed class NotifyPublishCompleteCoordinatorTests
    {
        private readonly IBatchMetadataAnalysisFactory _batchAnalyser
            = Mock.Of<IBatchMetadataAnalysisFactory>(MockBehavior.Strict);

        private readonly INotificationMessageBuilder _messageBuilder
            = Mock.Of<INotificationMessageBuilder>(MockBehavior.Strict);

        private readonly INotificationSummaryBuilder _summaryBuilder
            = Mock.Of<INotificationSummaryBuilder>(MockBehavior.Strict);

        private readonly IDfESignInPublicApi _dfeSignInPublicApi
            = Mock.Of<IDfESignInPublicApi>(MockBehavior.Strict);

        private readonly IAdminSettingsService _adminSettingsService
            = Mock.Of<IAdminSettingsService>();

        private readonly INotifyEmailService _notifyEmailService
         = Mock.Of<INotifyEmailService>(MockBehavior.Strict);

        private readonly Mock<ILoggerAdapter<NotifyPublishCompleteCoordinator>> _mockLoggingService = new Mock<ILoggerAdapter<NotifyPublishCompleteCoordinator>>(MockBehavior.Loose);

        [TestMethod]
        public void Constructor_WithNullAnalyser_Throws()
        {
            // Arrange
            var config = new NotificationConfiguration();

            // Act
            Action act = () => new NotifyPublishCompleteCoordinator(null, _messageBuilder, _summaryBuilder, config, _dfeSignInPublicApi, _adminSettingsService, _notifyEmailService, _mockLoggingService.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_WithNullMessageBuilder_Throws()
        {
            // Arrange
            var config = new NotificationConfiguration();

            // Act
            Action act = () => new NotifyPublishCompleteCoordinator(_batchAnalyser, null, _summaryBuilder, config, _dfeSignInPublicApi, _adminSettingsService, _notifyEmailService, _mockLoggingService.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_WithNullSummaryBuilder_Throws()
        {
            // Arrange
            var config = new NotificationConfiguration();

            // Act
            Action act = () => new NotifyPublishCompleteCoordinator(_batchAnalyser, _messageBuilder, null, config, _dfeSignInPublicApi, _adminSettingsService, _notifyEmailService, _mockLoggingService.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("emailMessageType")]
        public async Task NotifyUsers_MakesExpectedCalls(string emailMessageType)
        {
            // Arrange
            const string batchId = "any old batch id...";
            const string issuingEmail = "any old email address...";
            const string issuerName = "any old email name...";
            const string subject1 = "You published 1 document";
            const string subject2 = "New documents to view in document exchange";
            const int organisation = 1034931;
            const string summaryLine = "Uploaded by any old email address.... 0 viruses. 1 okay";
            const string outgoingSummaryLine = "DocEx Users: 1. 1 okay files for provider.";
            const string productName = "any old product name";
            const int targetUkprn = 10382741;

            var sut = GetTestCoordinator();

            var userContact = new UserContactLookupResponse()
            {
                Ukprn = "10382741",
                Users = new[]
                {
                    new UserContact
                    {
                        Email = "test@test.com",
                        FirstName = "First Name",
                        LastName = "Last Name",
                        Roles = new[] { UserRole.DocumentExchangeUser.ToString() }
                    }
                }
            };

            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);
            var message = Mock.Of<INotificationMessage>(MockBehavior.Strict);
            var summary = Mock.Of<INotificationSummary>(MockBehavior.Strict);
            var product = Mock.Of<IBatchAnalysisProduct>(MockBehavior.Strict);
            var recipientUkprns = new[] { targetUkprn };

            var file = new FileMetadata
            {
                ToUkprn = targetUkprn,
                ProductIdentifier = productName
            };

            Mock.Get(analysis)
                .SetupGet(x => x.InfectedFiles)
                .Returns(Array.Empty<FileMetadata>());
            Mock.Get(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(new[] { file });
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
                .Setup(x => x.GetProductFor(productName))
                .Returns(product);
            Mock.Get(analysis)
                .SetupGet(x => x.RecipientOrganisations)
                .Returns(recipientUkprns);
            Mock.Get(analysis)
                .SetupGet(x => x.ParentBatchID)
                .Returns(string.Empty);

            Mock.Get(message)
             .SetupGet(x => x.EmailMessageType)
             .Returns(emailMessageType);

            Mock.Get(_dfeSignInPublicApi)
             .Setup(x => x.GetUserContactsForOrganisation(
                 It.Is<int>(x => x == 10382741),
                 It.Is<string[]>(roles => roles.Contains(UserRole.DocumentExchangeUser.ToString()))))
             .ReturnsAsync(userContact);

            Mock.Get(_batchAnalyser)
                .Setup(x => x.AnalyseBatch(batchId))
                .Returns(Task.FromResult(analysis));

            Mock.Get(_messageBuilder)
                .Setup(x => x.GetCleanFileMessage(subject1, analysis, issuingEmail, issuerName))
                .Returns(Task.FromResult(message));
            Mock.Get(_messageBuilder)
                .Setup(x => x.GetDocumentUserMessage(subject2, It.IsAny<IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(message));
            Mock.Get(_summaryBuilder)
                .Setup(x => x.BuildSummary(NotificationType.AgencyPublishClear, organisation, summaryLine, issuingEmail))
                .Returns(Task.FromResult(summary));

            Mock.Get(_summaryBuilder)
                .Setup(x => x.BuildSummary(NotificationType.AgencyPublishClear, targetUkprn, outgoingSummaryLine, It.IsAny<IReadOnlyCollection<string>>()))
                .Returns(Task.FromResult(summary));
            Mock.Get(_summaryBuilder)
                .Setup(x => x.SaveSummary(It.IsAny<BatchNotificationSummary>(), analysis))
                .Returns(Task.CompletedTask);

            Mock.Get(_adminSettingsService)
                .Setup(x => x.IsAgencyPublishCompleteProviderNotificationEnabled()).ReturnsAsync(true);

            if (!string.IsNullOrEmpty(emailMessageType))
            {
                Mock.Get(_notifyEmailService)
                .Setup(x => x.Push(message))
                .Returns(Task.CompletedTask);
            }

            // Act
            await sut.NotifyUsers(batchId);

            // Assert
            VerifyAllMocks();
            Mock.Get(analysis).VerifyAll();
            Mock.Get(message).VerifyAll();
            Mock.Get(summary).VerifyAll();
        }

        [TestMethod]
        [DataRow(1, "You published 1 document", "")]
        [DataRow(1, "You published 1 document", "emailMessageType")]
        [DataRow(2, "You published 2 documents", "")]
        [DataRow(2, "You published 2 documents", "emailMessageType")]
        [DataRow(3, "You published 3 documents", "")]
        [DataRow(3, "You published 3 documents", "emailMessageType")]
        [DataRow(10, "You published 10 documents", "")]
        [DataRow(10, "You published 10 documents", "emailMessageType")]
        public async Task NotifyDocumentSender_WhenClean_MakesExpectedCalls(int clearCount, string subject, string emailMessageType)
        {
            // Arrange
            const string recipient = "any old issuer...";
            const string recipientName = "any old name...";

            var sut = GetTestCoordinator();

            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);

            Mock.Get(analysis)
                .SetupGet(x => x.InfectedFiles)
                .Returns(Array.Empty<FileMetadata>());
            Mock.Get(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(Enumerable.Range(0, clearCount).Select(id => new FileMetadata()).ToList());
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingEmail)
                .Returns(recipient);
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingPerson)
                .Returns(recipientName);

            var message = Mock.Of<INotificationMessage>(MockBehavior.Strict);

            Mock.Get(message)
            .SetupGet(x => x.EmailMessageType)
            .Returns(emailMessageType);

            Mock.Get(_messageBuilder)
                .Setup(x => x.GetCleanFileMessage(subject, analysis, recipient, recipientName))
                .Returns(Task.FromResult(message));

            if (!string.IsNullOrEmpty(emailMessageType))
            {
                Mock.Get(_notifyEmailService)
                .Setup(x => x.Push(message))
                .Returns(Task.CompletedTask);
                Mock.Get(analysis)
                .SetupGet(x => x.ParentBatchID)
                .Returns(string.Empty);
            }

            // Act
            await sut.NotifyDocumentSender(analysis);

            // Assert
            VerifyAllMocks();
            Mock.Get(analysis).VerifyAll();
            Mock.Get(message).VerifyAll();
        }

        [TestMethod]
        [DataRow(1, "You have an issue with 1 document you tried to publish.", "")]
        [DataRow(1, "You have an issue with 1 document you tried to publish.", "emailMessageType")]
        [DataRow(2, "You have an issue with 2 documents you tried to publish.", "")]
        [DataRow(2, "You have an issue with 2 documents you tried to publish.", "emailMessageType")]
        [DataRow(100, "You have an issue with 100 documents you tried to publish.", "")]
        [DataRow(100, "You have an issue with 100 documents you tried to publish.", "emailMessageType")]
        public async Task NotifyDocumentSender_WhenInfected_MakesExpectedCalls(int infectedCount, string subject, string emailMessageType)
        {
            // Arrange
            const string issuer = "any old issuer...";
            const string recipientName = "any old name...";

            var sut = GetTestCoordinator();
            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);
            Mock.Get(analysis)
                .SetupGet(x => x.InfectedFiles)
                .Returns(Enumerable.Range(0, infectedCount).Select(id => new FileMetadata()).ToList());
            Mock.Get(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(Array.Empty<FileMetadata>());
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingEmail)
                .Returns(issuer);
            Mock.Get(analysis)
                .SetupGet(x => x.IssuingPerson)
                .Returns(recipientName);

            var message = Mock.Of<INotificationMessage>(MockBehavior.Strict);
            Mock.Get(message)
            .SetupGet(x => x.EmailMessageType)
            .Returns(emailMessageType);

            Mock.Get(_messageBuilder)
                .Setup(x => x.GetInfectedFileMessage(subject, analysis, issuer, recipientName))
                .Returns(Task.FromResult(message));

            if (!string.IsNullOrEmpty(emailMessageType))
            {
                Mock.Get(_notifyEmailService)
                .Setup(x => x.Push(message))
                .Returns(Task.CompletedTask);

                Mock.Get(analysis)
                .SetupGet(x => x.ParentBatchID)
                .Returns(string.Empty);
            }

            // Act
            await sut.NotifyDocumentSender(analysis);

            // Assert
            VerifyAllMocks();
            Mock.Get(analysis).VerifyAll();
            Mock.Get(message).VerifyAll();
        }

        [TestMethod]
        [DataRow(0, 1, NotificationType.AgencyPublishClear, "email 1", 100203)]
        [DataRow(1, 0, NotificationType.AgencyPublishInfected, "email 2", 20134)]
        public async Task GetSenderSummary_ReturnsExpected(int infectedCount, int clearCount, NotificationType notificationType, string recipient, int ukprn)
        {
            // Arrange
            string notes = $"Uploaded by {recipient}. {infectedCount} viruses. {clearCount} okay";

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
                .Returns(ukprn);

            var summary = Mock.Of<INotificationSummary>(MockBehavior.Strict);

            Mock.Get(_summaryBuilder)
                .Setup(x => x.BuildSummary(notificationType, ukprn, notes, recipient))
                .ReturnsAsync(summary);

            // Act
            var result = await sut.GetSenderSummary(analysis);

            // Assert
            VerifyAllMocks();
            Mock.Get(analysis).VerifyAll();
            Mock.Get(summary).VerifyAll();

            result.Should().BeAssignableTo<INotificationSummary>();
        }

        [TestMethod]
        public async Task NotifyDocumentRecipients_ReturnsExpected()
        {
            // Arrange
            var sut = GetTestCoordinator();

            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);

            Mock.Get(analysis)
                .SetupGet(x => x.RecipientOrganisations)
                .Returns(Array.Empty<int>());

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
        [DataRow(1, 10023, "", "email 1", "email 2", "email 3")]
        [DataRow(1, 10023, "emailMessageType", "email 1", "email 2", "email 3")]
        [DataRow(2, 20103, "", "email 21", "email 22")]
        [DataRow(2, 20103, "emailMessageType", "email 21", "email 22")]
        [DataRow(100, 3010293, "", "email 31", "email 32", "email 33", "email 34", "email 35")]
        [DataRow(100, 3010293, "emailMessageType", "email 31", "email 32", "email 33", "email 34", "email 35")]
        public async Task NotifyRecipientOrganisation_ReturnsExpected(int fileCount, int organisation, string emailMessageType, params string[] recipients)
        {
            // Arrange
            const string expectedSubject = "New documents to view in document exchange";
            var sut = GetTestCoordinator();

            var expectedNotes = $"DocEx Users: {recipients.Length}. 0 okay files for provider.";

            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);

            Mock.Get(analysis)
                .SetupGet(x => x.ParentBatchID)
                .Returns(string.Empty);

            Mock.Get(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(Enumerable.Range(0, fileCount).Select(id => new FileMetadata()).ToList());

            var summary = Mock.Of<INotificationSummary>(MockBehavior.Strict);

            Mock.Get(_summaryBuilder)
                .Setup(x => x.BuildSummary(NotificationType.AgencyPublishClear, organisation, expectedNotes, recipients))
                .Returns(Task.FromResult(summary));

            Mock.Get(_dfeSignInPublicApi)
                     .Setup(x => x.GetUserContactsForOrganisation(
                         It.Is<int>(x => x == organisation),
                         It.Is<string[]>(roles => roles.Contains(UserRole.DocumentExchangeUser.ToString()))))
                .Returns(Task.FromResult(MakeUserList(recipients)));

            var message = Mock.Of<INotificationMessage>(MockBehavior.Strict);
            Mock.Get(message)
            .SetupGet(x => x.EmailMessageType)
            .Returns(emailMessageType);

            Mock.Get(_messageBuilder)
                .Setup(x => x.GetDocumentUserMessage(expectedSubject, It.IsAny<IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(message));

            Mock.Get(_adminSettingsService)
                .Setup(x => x.IsAgencyPublishCompleteProviderNotificationEnabled())
                .ReturnsAsync(true);

            if (!string.IsNullOrEmpty(emailMessageType))
            {
                Mock.Get(_notifyEmailService)
                .Setup(x => x.Push(message))
                .Returns(Task.CompletedTask);
            }

            // Act
            var result = await sut.NotifyRecipientOrganisation(organisation, analysis);

            // Assert
            VerifyAllMocks();
            Mock.Get(analysis).VerifyAll();
            Mock.Get(summary).VerifyAll();
            Mock.Get(message).VerifyAll();

            result.Should().BeAssignableTo<INotificationSummary>();
        }

        [TestMethod]
        [DataRow(1, 10023, "", "email 1", "email 2", "email 3")]
        [DataRow(1, 10023, "emailMessageType", "email 1", "email 2", "email 3")]
        [DataRow(2, 20103, "", "email 21", "email 22")]
        [DataRow(2, 20103, "emailMessageType", "email 21", "email 22")]
        [DataRow(100, 3010293, "", "email 31", "email 32", "email 33", "email 34", "email 35")]
        [DataRow(100, 3010293, "emailMessageType", "email 31", "email 32", "email 33", "email 34", "email 35")]
        public async Task NotifyRecipientOrganisation_ForDsiContacts_ReturnsExpected(int fileCount, int organisation, string emailMessageType, params string[] recipients)
        {
            // Arrange
            const string expectedSubject = "New documents to view in document exchange";
            var sut = GetTestCoordinator(useDsiContacts: true);

            var expectedNotes = $"DocEx Users: {recipients.Length}. 0 okay files for provider.";

            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);

            Mock.Get(analysis)
                .SetupGet(x => x.ParentBatchID)
                .Returns(string.Empty);

            Mock.Get(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(Enumerable.Range(0, fileCount).Select(id => new FileMetadata()).ToList());

            var summary = Mock.Of<INotificationSummary>(MockBehavior.Strict);

            Mock.Get(_summaryBuilder)
                .Setup(x => x.BuildSummary(NotificationType.AgencyPublishClear, organisation, expectedNotes, recipients))
                .Returns(Task.FromResult(summary));

            Mock.Get(_dfeSignInPublicApi)
                .Setup(x => x.GetUserContactsForOrganisation(
                    It.Is<int>(x => x == organisation),
                    It.Is<string[]>(roles => roles.Contains(UserRole.DocumentExchangeUser.ToString()))))
                .Returns(Task.FromResult(MakeUserList(recipients)));

            var message = Mock.Of<INotificationMessage>(MockBehavior.Strict);
            Mock.Get(message)
            .SetupGet(x => x.EmailMessageType)
            .Returns(emailMessageType);

            Mock.Get(_messageBuilder)
                .Setup(x => x.GetDocumentUserMessage(expectedSubject, It.IsAny<IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(message));

            Mock.Get(_adminSettingsService)
                .Setup(x => x.IsAgencyPublishCompleteProviderNotificationEnabled())
                .ReturnsAsync(true);

            if (!string.IsNullOrEmpty(emailMessageType))
            {
                Mock.Get(_notifyEmailService)
                .Setup(x => x.Push(message))
                .Returns(Task.CompletedTask);
            }

            // Act
            var result = await sut.NotifyRecipientOrganisation(organisation, analysis);

            // Assert
            VerifyAllMocks();
            Mock.Get(analysis).VerifyAll();
            Mock.Get(summary).VerifyAll();
            Mock.Get(message).VerifyAll();

            result.Should().BeAssignableTo<INotificationSummary>();
        }

        [TestMethod]
        [DataRow(1, 10023)]
        public async Task NotifyRecipientOrganisation_ForDsiContacts_Returns_Null(int fileCount, int organisation, params string[] recipients)
        {
            // Arrange
            var sut = GetTestCoordinator(useDsiContacts: true);

            var expectedNotes = $"DocEx Users: {recipients.Length}. No provider to send to. 0 okay files for provider.";

            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);

            Mock.Get(analysis)
                .SetupGet(x => x.ParentBatchID)
                .Returns(string.Empty);

            Mock.Get(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(Enumerable.Range(0, fileCount).Select(id => new FileMetadata()).ToList());

            var summary = Mock.Of<INotificationSummary>(MockBehavior.Strict);

            Mock.Get(_summaryBuilder)
                .Setup(x => x.BuildSummary(NotificationType.AgencyPublishClear, organisation, expectedNotes, recipients))
                .ReturnsAsync(summary);

            Mock.Get(_dfeSignInPublicApi)
                .Setup(x => x.GetUserContactsForOrganisation(
                    It.Is<int>(x => x == organisation),
                    It.Is<string[]>(roles => roles.Contains(UserRole.DocumentExchangeUser.ToString()))))
                .ReturnsAsync(new UserContactLookupResponse() { Ukprn = "12345678", Users = null });

            Mock.Get(_adminSettingsService)
                .Setup(x => x.IsAgencyPublishCompleteProviderNotificationEnabled())
                .ReturnsAsync(true);

            // Act
            var result = await sut.NotifyRecipientOrganisation(organisation, analysis);

            // Assert
            Mock.Get(_summaryBuilder).VerifyAll();
            Mock.Get(_dfeSignInPublicApi).VerifyAll();
            Mock.Get(summary).VerifyAll();

            result.Should().BeAssignableTo<INotificationSummary>();
        }

        [TestMethod]
        [DataRow(1, 10023, "", "email 1", "email 2", "email 3")]
        [DataRow(1, 10023, "emailMessageType", "email 1", "email 2", "email 3")]
        [DataRow(2, 20103, "", "email 21", "email 22")]
        [DataRow(2, 20103, "emailMessageType", "email 21", "email 22")]
        [DataRow(100, 3010293, "", "email 31", "email 32", "email 33", "email 34", "email 35")]
        [DataRow(100, 3010293, "emailMessageType", "email 31", "email 32", "email 33", "email 34", "email 35")]
        public async Task NotifyRecipientOrganisation_ForDsiContacts_ReturnsExpected_WhenAgencyPublishCompleteProviderNotificationIsDisabled(int fileCount, int organisation, string emailMessageType, params string[] recipients)
        {
            // Arrange
            const string expectedSubject = "New documents to view in document exchange";
            var sut = GetTestCoordinator(useDsiContacts: true);

            var expectedNotes = $"DocEx Users: {recipients.Length}. 0 okay files for provider.";

            var analysis = Mock.Of<IBatchMetadataAnalysis>(MockBehavior.Strict);

            Mock.Get(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(Enumerable.Range(0, fileCount).Select(id => new FileMetadata()).ToList());

            var summary = Mock.Of<INotificationSummary>(MockBehavior.Strict);

            Mock.Get(_summaryBuilder)
                .Setup(x => x.BuildSummary(NotificationType.AgencyPublishClear, organisation, expectedNotes, recipients))
                .Returns(Task.FromResult(summary));

            Mock.Get(_dfeSignInPublicApi)
                .Setup(x => x.GetUserContactsForOrganisation(
                    It.Is<int>(x => x == organisation),
                    It.Is<string[]>(roles => roles.Contains(UserRole.DocumentExchangeUser.ToString()))))
                .Returns(Task.FromResult(MakeUserList(recipients)));

            var message = Mock.Of<INotificationMessage>(MockBehavior.Strict);

            Mock.Get(_messageBuilder)
                .Setup(x => x.GetDocumentUserMessage(expectedSubject, It.IsAny<IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(message));

            Mock.Get(_adminSettingsService)
                .Setup(x => x.IsAgencyPublishCompleteProviderNotificationEnabled())
                .ReturnsAsync(false);

            if (!string.IsNullOrEmpty(emailMessageType))
            {
                Mock.Get(_notifyEmailService)
                .Setup(x => x.Push(message))
                .Returns(Task.CompletedTask);
            }

            // Act
            var result = await sut.NotifyRecipientOrganisation(organisation, analysis);

            // Assert
            Mock.Get(_messageBuilder).Verify(
                x => x.GetDocumentUserMessage(
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>>>(),
                    It.IsAny<IReadOnlyCollection<string>>(),
                    It.IsAny<IReadOnlyCollection<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()), Times.Never);

            result.Should().BeAssignableTo<INotificationSummary>();
        }

        internal UserContactLookupResponse MakeUserList(string[] users)
        {
            var userList = new List<UserContact>();
            users
                .ToList()
                .ForEach(userName =>
                {
                    var user = new UserContact
                    {
                        Email = userName,
                        FirstName = $"firstname: {userName}",
                        LastName = $"lastname: {userName}",
                        Roles = new[] { UserRole.DocumentExchangeUser.ToString() }
                    };

                    userList.Add(user);
                });

            return new UserContactLookupResponse()
            {
                Ukprn = "12345678",
                Users = userList
            };
        }

        private void VerifyAllMocks()
        {
            Mock.Get(_batchAnalyser).VerifyAll();
            Mock.Get(_messageBuilder).VerifyAll();
            Mock.Get(_summaryBuilder).VerifyAll();
            Mock.Get(_dfeSignInPublicApi).VerifyAll();
            Mock.Get(_notifyEmailService).VerifyAll();
        }

        private NotifyPublishCompleteCoordinator GetTestCoordinator(bool useDsiContacts = false)
        {
            var config = new NotificationConfiguration
            {
                UseDfeSignInContactsApi = useDsiContacts
            };

            return new NotifyPublishCompleteCoordinator(
                _batchAnalyser,
                _messageBuilder,
                _summaryBuilder,
                config,
                _dfeSignInPublicApi,
                _adminSettingsService,
                _notifyEmailService,
                _mockLoggingService.Object);
        }
    }
}