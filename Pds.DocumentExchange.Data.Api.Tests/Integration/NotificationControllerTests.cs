using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.BulkJobs.Models;
using Pds.Core.Common.Identity.Enums;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.DfESignIn.Interfaces;
using Pds.Core.DfESignIn.Models;
using Pds.Core.Notification.Interfaces;
using Pds.Core.Utils.Implementations;
using Pds.Core.Utils.Interfaces;
using Pds.DocumentExchange.Data.Api.Controllers;
using Pds.DocumentExchange.Data.Populator.Models;
using Pds.DocumentExchange.Data.Populator.Scenarios;
using Pds.DocumentExchange.Data.Populator.Storage;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Implementations.Builders;
using Pds.DocumentExchange.Data.Services.Implementations.Coordinators;
using Pds.DocumentExchange.Data.Services.Implementations.Factories;
using Pds.DocumentExchange.Data.Services.Implementations.Providers;
using Pds.DocumentExchange.Data.Services.Implementations.Settings;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.Services.Common.Implementations.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotifyNotificationModels = Pds.Core.Notification.Models;

namespace Pds.DocumentExchange.Data.Api.Tests.Integration
{
    [TestClass, TestCategory("Integration")]
    public class NotificationControllerTests : BaseIntegration, IDisposable
    {
        private IDfESignInPublicApi DfESignInPublicApi { get; } = Mock.Of<IDfESignInPublicApi>(MockBehavior.Strict);

        private INotificationEmailQueueService MockNotificationEmailQueueService { get; } = Mock.Of<INotificationEmailQueueService>(MockBehavior.Strict);

        public NotificationControllerTests()
        {
            SetupSystemProvider();
            SetUpConfig();
        }

        #region AgencyPublishComplete

        [TestMethod]
        public async Task AgencyPublishComplete_When_ParentBatchIdentifierIsNull_ArgumentNullException()
        {
            // Arrange
            var controller = GetNotificationController();

            // Act
            Func<Task> func = () => controller.AgencyPublishComplete(null);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task AgencyPublishComplete_When_ParentBatchIdentifierIsEmpty_ArgumentException()
        {
            // Arrange
            var controller = GetNotificationController();

            // Act
            Func<Task> func = () => controller.AgencyPublishComplete(string.Empty);

            // Assert
            await func.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public async Task AgencyPublishComplete_ReturnsOkResult()
        {
            // Arrange
            var controller = GetNotificationController();

            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpAllCaches<IEnumerable<Product>>();
            SetUpAllCaches<string>();

            var organisation = new Pds.Core.Common.Organisation.Models.Organisation
            {
                Name = "organisation",
                Identifiers = new[]
                {
                    new OrganisationIdentifier
                    {
                        Type = Pds.Core.Common.Organisation.Enums.OrganisationIdentifierType.Ukprn,
                        Value = "12345678"
                    }
                }
            };

            var guid = Guid.NewGuid();
            var systemDate = new DateTime(2021, 03, 09);

            Mock.Get(SystemProvider.Guid)
               .Setup(gp => gp.NewGuid())
               .Returns(guid);

            Mock.Get(SystemProvider.DateTime)
               .Setup(dtp => dtp.UtcNow())
               .Returns(systemDate);

            var userContact = new UserContactLookupResponse()
            {
                Ukprn = "12345678",
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

            Mock.Get(DfESignInPublicApi)
             .Setup(x => x.GetUserContactsForOrganisation(
                 It.Is<int>(x => x == 12345678),
                 It.Is<string[]>(roles => roles.Contains(UserRole.DocumentExchangeUser.ToString()))))
             .ReturnsAsync(userContact);

            var parentBatchIdentifier = Guid.NewGuid().ToString();
            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters()
            {
                Records = new[]
                {
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB",
                        ParentBatchIdentifier = parentBatchIdentifier,
                        BatchCreatedDate = systemDate
                    }
                }
            };
            var expectedNotificationMessage1 = new NotifyNotificationModels.NotificationMessage
            {
                EmailAddresses = new List<string> { "test@test.com" },
                EmailMessageType = "ESFAPublishedSingle",
                RequestingService = "DocEx",
                EmailPersonalisation = new NotifyNotificationModels.GovUkNotifyPersonalisation()
            };
            expectedNotificationMessage1.EmailPersonalisation.Personalisation = new Dictionary<string, object>()
            {
                { "DocumentName", (dynamic)"example.pdf" },
                { "DocumentNameUI", (dynamic)$"Allocation-calculation-toolkit.pdf" },
                { "DocumentListUL", (dynamic)"* example.pdf" },
                { "DocumentListUIUL", (dynamic)"Allocation-calculation-toolkit.pdf" },
                { "DocumentType", (dynamic)"Allocation calculation toolkit" },
                { "DocumentTypes", (dynamic)"1 Allocation calculation toolkit files" },
                { "DocumentTypesUL", (dynamic)"1 Allocation calculation toolkit files" },
                { "DocumentCount", (dynamic)"1" },
                { "SendDateTime", (dynamic)systemDate.ToString("h:mmtt, d MMMM yyyy") },
                { "SendDate", (dynamic)systemDate.ToString("d MMMM yyyy") },
                { "ReplyEmail", (dynamic)"reply_to@docex.education.gov.uk" },
                { "BaseUrl", (dynamic)@"https://skillsfunding.service.gov.uk/" },
                { "DocumentNumberText", (dynamic)"1 document" }
            };

            var expectedNotificationMessage2 = new NotifyNotificationModels.NotificationMessage
            {
                EmailAddresses = new List<string> { "test@test.com" },
                EmailMessageType = "ProviderReceivedSingleType",
                RequestingService = "DocEx",
                EmailPersonalisation = new NotifyNotificationModels.GovUkNotifyPersonalisation()
            };
            expectedNotificationMessage2.EmailPersonalisation.Personalisation = new Dictionary<string, object>()
            {
                { "NumberOfDocuments", "1" },
                { "DocumentTypeDetails", "You have 1 new Allocation calculation toolkit" },
                { "BaseUrl", "https://skillsfunding.service.gov.uk/" }
            };

            Mock.Get(MockNotificationEmailQueueService)
             .Setup(gp => gp.SendAsync(It.Is<NotifyNotificationModels.NotificationMessage>(
                 x => expectedNotificationMessage1.EmailAddresses.All(ea => x.EmailAddresses.Contains(ea)) &&
                 x.EmailMessageType == expectedNotificationMessage1.EmailMessageType &&
                 x.RequestingService == expectedNotificationMessage1.RequestingService &&
                 expectedNotificationMessage1.EmailPersonalisation.Personalisation.All(expected => x.EmailPersonalisation.Personalisation.Any(ep => ep.Key == expected.Key && Convert.ToString(ep.Value).Contains(Convert.ToString(expected.Value)))))))
             .Returns(Task.CompletedTask);

            Mock.Get(MockNotificationEmailQueueService)
             .Setup(gp => gp.SendAsync(It.Is<NotifyNotificationModels.NotificationMessage>(
                 x => expectedNotificationMessage2.EmailAddresses.All(ea => x.EmailAddresses.Contains(ea)) &&
                 x.EmailMessageType == expectedNotificationMessage2.EmailMessageType &&
                 x.RequestingService == expectedNotificationMessage2.RequestingService &&
                 expectedNotificationMessage2.EmailPersonalisation.Personalisation.All(expected => x.EmailPersonalisation.Personalisation.Any(ep => ep.Key == expected.Key && Convert.ToString(ep.Value).Contains(Convert.ToString(expected.Value)))))))
             .Returns(Task.CompletedTask);

            await DocumentsPublishedByAgencyPopulator.PopulateScenarioData(configuration, parameters);

            // Act
            var result = await controller.AgencyPublishComplete(parentBatchIdentifier);

            // Assert
            result.Should().BeOfType<OkResult>();

            List<BatchMetadata> batchMetadataList;
            List<BatchNotificationSummary> batchNotificationSummaryList;

            using (var client = new DocumentsCosmosDb(configuration.CosmosDb))
            {
                batchMetadataList = await client.GetEntityByParentId<BatchMetadata>(parentBatchIdentifier);
                batchNotificationSummaryList = await client.GetEntityByParentId<BatchNotificationSummary>(parentBatchIdentifier);
            }

            batchMetadataList.Where(b => b.Id == guid.ToString()).Select(b => b.ParentBatchIdentifier).Should().Equal(parentBatchIdentifier);
            batchMetadataList.First(b => b.Id == parentBatchIdentifier).Files.First().History.Select(h => h.Action).Should().Contain(Services.Enums.FileAction.EmailSent);

            batchNotificationSummaryList.First(ns => ns.Id == guid.ToString()).RecipientNotificationSummaries.First().To.Should().Contain(userContact.Users.First().Email);
            batchNotificationSummaryList.First(ns => ns.Id == guid.ToString()).SenderNotificationSummary.To.Should().Contain(userContact.Users.First().Email);
            batchNotificationSummaryList.First(ns => ns.Id == guid.ToString()).SenderNotificationSummary.From.Should().Contain("service_relay_noreply@docex.education.gov.uk");

            Mock.VerifyAll(
               Mock.Get(MockMemoryCache),
               Mock.Get(MockDistributedCache),
               Mock.Get(SystemProvider.Guid),
               Mock.Get(SystemProvider.DateTime));
        }
        #endregion


        #region OrganisationUploadComplete

        [TestMethod]
        public async Task OrganisationUploadComplete_When_ParentBatchIdentifierIsNull_ArgumentNullException()
        {
            // Arrange
            var controller = GetNotificationController();

            // Act
            Func<Task> func = () => controller.OrganisationUploadComplete(null);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task OrganisationUploadComplete_When_ParentBatchIdentifierIsEmpty_ArgumentException()
        {
            // Arrange
            var controller = GetNotificationController();

            // Act
            Func<Task> func = () => controller.OrganisationUploadComplete(string.Empty);

            // Assert
            await func.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public async Task OrganisationUploadComplete_ReturnsOkResult()
        {
            // Arrange
            var controller = GetNotificationController();

            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpAllCaches<IEnumerable<Product>>();
            SetUpAllCaches<string>();

            Mock.Get(OrganisationsLookup)
                .Setup(o => o.Get(It.IsAny<OrganisationIdentifier>()))
                .ReturnsAsync(new Pds.Core.Common.Organisation.Models.Organisation()
                {
                    Identifiers = new[]
                    {
                        new OrganisationIdentifier
                        {
                            Type = OrganisationIdentifierType.Ukprn,
                            Value = "12345678"
                        }
                    },
                    Name = "Organisation1"
                });

            var guid = Guid.NewGuid();
            var systemDate = new DateTime(2021, 03, 09, 11, 0, 0);

            Mock.Get(SystemProvider.Guid)
               .Setup(gp => gp.NewGuid())
               .Returns(guid);

            Mock.Get(SystemProvider.DateTime)
               .Setup(dtp => dtp.UtcNow())
               .Returns(systemDate);


            var userContact = new UserContactLookupResponse()
            {
                Ukprn = "12345678",
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

            Mock.Get(DfESignInPublicApi)
             .Setup(x => x.GetUserContactsForOrganisation(
                 It.Is<int>(x => x == 12345678),
                 It.Is<string[]>(roles => roles.Contains(UserRole.DocumentExchangeUser.ToString()))))
             .ReturnsAsync(userContact);

            var parentBatchIdentifier = "3c869e7a-ce64-4b6a-bb15-745eb3322deb";
            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters()
            {
                Records = new[]
                {
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB",
                        ParentBatchIdentifier = parentBatchIdentifier,
                        BatchCreatedDate = systemDate
                    }
                }
            };
            var expectedNotificationMessage1 = new NotifyNotificationModels.NotificationMessage
            {
                EmailAddresses = new List<string> { "test@test.com" },
                EmailMessageType = "ExternalUploadCompleted",
                RequestingService = "DocEx",
                EmailPersonalisation = new NotifyNotificationModels.GovUkNotifyPersonalisation()
            };
            expectedNotificationMessage1.EmailPersonalisation.Personalisation = new Dictionary<string, object>()
            {
                { "DocumentName", (dynamic)"example.pdf" },
                { "DocumentNameUI", (dynamic)$"Allocation-calculation-toolkit.pdf" },
                { "DocumentListUL", (dynamic)"* example.pdf" },
                { "DocumentListUIUL", (dynamic)"Allocation-calculation-toolkit.pdf" },
                { "DocumentType", (dynamic)"Allocation calculation toolkit" },
                { "DocumentTypes", (dynamic)"1 Allocation calculation toolkit files" },
                { "DocumentTypesUL", (dynamic)"1 Allocation calculation toolkit files" },
                { "DocumentCount", (dynamic)"1" },
                { "SendDateTime", (dynamic)systemDate.ToString("h:mmtt, d MMMM yyyy") },
                { "SendDate", (dynamic)systemDate.ToString("d MMMM yyyy") },
                { "ReplyEmail", (dynamic)"reply_to@docex.education.gov.uk" },
                { "BaseUrl", (dynamic)@"https://skillsfunding.service.gov.uk/" },
                { "DocumentNumberText", (dynamic)"1 document" },
                { "ProviderName", (dynamic)"Organisation1" }
            };

            var expectedNotificationMessage2 = new NotifyNotificationModels.NotificationMessage
            {
                EmailAddresses = new List<string> { "pds.sfs.test.dat+updated1@gmail.com" },
                EmailMessageType = "ESFAReceivedSingle",
                RequestingService = "DocEx",
                EmailPersonalisation = new NotifyNotificationModels.GovUkNotifyPersonalisation()
            };
            expectedNotificationMessage2.EmailPersonalisation.Personalisation = new Dictionary<string, object>()
            {
                { "DocumentName", (dynamic)"example.pdf" },
                { "DocumentNameUI", (dynamic)$"Allocation-calculation-toolkit.pdf" },
                { "DocumentListUL", (dynamic)"* example.pdf" },
                { "DocumentListUIUL", (dynamic)"Allocation-calculation-toolkit.pdf" },
                { "DocumentType", (dynamic)"Allocation calculation toolkit" },
                { "DocumentTypes", (dynamic)"1 Allocation calculation toolkit files" },
                { "DocumentTypesUL", (dynamic)"1 Allocation calculation toolkit files" },
                { "DocumentCount", (dynamic)"1" },
                { "SendDateTime", (dynamic)systemDate.ToString("h:mmtt, d MMMM yyyy") },
                { "SendDate", (dynamic)systemDate.ToString("d MMMM yyyy") },
                { "ReplyEmail", (dynamic)"reply_to@docex.education.gov.uk" },
                { "BaseUrl", (dynamic)@"https://skillsfunding.service.gov.uk/" },
                { "DocumentNumberText", (dynamic)"1 document" },
                { "ProviderName", (dynamic)"Organisation1" }
            };

            Mock.Get(MockNotificationEmailQueueService)
             .Setup(gp => gp.SendAsync(It.Is<NotifyNotificationModels.NotificationMessage>(
                 x => expectedNotificationMessage1.EmailAddresses.All(ea => x.EmailAddresses.Contains(ea)) &&
                 x.EmailMessageType == expectedNotificationMessage1.EmailMessageType &&
                 x.RequestingService == expectedNotificationMessage1.RequestingService &&
                 expectedNotificationMessage1.EmailPersonalisation.Personalisation.All(expected => x.EmailPersonalisation.Personalisation.Any(ep => ep.Key == expected.Key && Convert.ToString(ep.Value).Contains(Convert.ToString(expected.Value)))))))
             .Returns(Task.CompletedTask);

            Mock.Get(MockNotificationEmailQueueService)
             .Setup(gp => gp.SendAsync(It.Is<NotifyNotificationModels.NotificationMessage>(
                 x => expectedNotificationMessage2.EmailAddresses.All(ea => x.EmailAddresses.Contains(ea)) &&
                 x.EmailMessageType == expectedNotificationMessage2.EmailMessageType &&
                 x.RequestingService == expectedNotificationMessage2.RequestingService &&
                 expectedNotificationMessage2.EmailPersonalisation.Personalisation.All(expected => x.EmailPersonalisation.Personalisation.Any(ep => ep.Key == expected.Key && Convert.ToString(ep.Value).Contains(Convert.ToString(expected.Value)))))))
             .Returns(Task.CompletedTask);

            await DocumentsUploadedByExternalPopulator.PopulateScenarioData(configuration, parameters);

            // Act
            var result = await controller.OrganisationUploadComplete(parentBatchIdentifier);

            // Assert
            result.Should().BeOfType<OkResult>();

            List<BatchMetadata> batchMetadataList;
            List<BatchNotificationSummary> batchNotificationSummaryList;

            using (var client = new DocumentsCosmosDb(configuration.CosmosDb))
            {
                batchMetadataList = await client.GetEntityByParentId<BatchMetadata>(parentBatchIdentifier);
                batchNotificationSummaryList = await client.GetEntityByParentId<BatchNotificationSummary>(parentBatchIdentifier);
            }

            batchMetadataList.Where(b => b.Id == guid.ToString()).Select(b => b.ParentBatchIdentifier).Should().Equal(parentBatchIdentifier);
            batchMetadataList.First(b => b.Id == parentBatchIdentifier).Files.First().History.Select(h => h.Action).Should().Contain(Services.Enums.FileAction.EmailSent);

            batchNotificationSummaryList.First(ns => ns.Id == guid.ToString()).RecipientNotificationSummaries.First().To.Should().Contain(expectedNotificationMessage2.EmailAddresses.ElementAt(0));
            batchNotificationSummaryList.First(ns => ns.Id == guid.ToString()).SenderNotificationSummary.To.Should().Contain(expectedNotificationMessage1.EmailAddresses.ElementAt(0));
            batchNotificationSummaryList.First(ns => ns.Id == guid.ToString()).SenderNotificationSummary.From.Should().Contain("service_relay_noreply@docex.education.gov.uk");

            Mock.VerifyAll(
               Mock.Get(MockMemoryCache),
               Mock.Get(MockDistributedCache),
               Mock.Get(SystemProvider.Guid),
               Mock.Get(SystemProvider.DateTime));
        }

        #endregion


        public void Dispose()
        {
            Task.Run(() => DocumentsPublishedByAgencyPopulator.TearDownScenarioData(GetFilesInDocumentsUploadedConfiguration())).GetAwaiter().GetResult();
        }

        public NotificationController GetNotificationController()
        {
            var bulkJobManager = CreateMockBulkJobManager();

            Mock.Get(bulkJobManager)
                .Setup(manager => manager.CreateBulkJob(
                    It.IsAny<IEnumerable<Job<string, BatchNotificationSummary>>>(),
                    It.IsAny<Func<string, Task<BatchNotificationSummary>>>()))
                .ReturnsAsync(
                    (IEnumerable<Job<string, BatchNotificationSummary>> _, Func<string, Task<BatchNotificationSummary>> notifyUsersFunction) =>
                    {
                        notifyUsersFunction(string.Empty).GetAwaiter().GetResult();
                        return Guid.NewGuid();
                    });

            return new NotificationController(
                GetNotifyPublishCompleteCoordinator(),
                GetNotifyUploadCompleteCoordinator(),
                bulkJobManager,
                CreateMockLoggerAdapter<NotificationController>());
        }

        private NotifyUploadCompleteCoordinator GetNotifyUploadCompleteCoordinator()
        {
            return new NotifyUploadCompleteCoordinator(
                GetBatchMetadataAnalysisFactory(),
                GetNotificationMessageBuilder(),
                GetNotificationSummaryBuilder(),
                GetNotifyEmailService(),
                CreateMockLoggerAdapter<NotifyUploadCompleteCoordinator>());
        }

        private NotifyPublishCompleteCoordinator GetNotifyPublishCompleteCoordinator()
        {
            return new NotifyPublishCompleteCoordinator(
                GetBatchMetadataAnalysisFactory(),
                GetNotificationMessageBuilder(),
                GetNotificationSummaryBuilder(),
                new NotificationConfiguration(),
                GetDfESignInPublicApiService(),
                GetAdminSettingsService(),
                GetNotifyEmailService(),
                CreateMockLoggerAdapter<NotifyPublishCompleteCoordinator>());
        }

        private IAdminSettingsService GetAdminSettingsService()
        {
            return new AdminSettingsService(
                GetCosmosDbService(),
                GetCacheManager(),
                GetDateTimeProvider(),
                CreateMockLoggerAdapter<AdminSettingsService>());
        }

        private IDateTimeProvider GetDateTimeProvider()
        {
            return new DateTimeProvider();
        }

        private NotificationMessageBuilder GetNotificationMessageBuilder()
        {
            var options = Options.Create(new AgencyServiceConfiguration());
            var configurationDataService = new ConfigurationDataService(GetCosmosDbService(), GetCacheManager(), CreateMockLoggerAdapter<ConfigurationDataService>());
            var messageBodyFactory = new NotificationMessageBodyFactory(
                new PresentationFormattingProvider(),
                OrganisationsLookup,
                configurationDataService,
                options,
                CreateMockLoggerAdapter<NotificationMessageBodyFactory>());

            return new NotificationMessageBuilder(
                new NotificationMessageBodyTemplateProvider(new AssetProvider()),
                new NotificationMessageHeaderFactory(),
                messageBodyFactory,
                new NotificationMessageFactory(),
                configurationDataService);
        }

        private NotifyEmailService GetNotifyEmailService()
        {
            return new NotifyEmailService(MockNotificationEmailQueueService);
        }

        private BatchMetadataAnalysisFactory GetBatchMetadataAnalysisFactory()
        {
            var cosmosDbService = GetCosmosDbService();
            var configurationDataService = new ConfigurationDataService(cosmosDbService, GetCacheManager(), CreateMockLoggerAdapter<ConfigurationDataService>());
            var mockLoggingService = CreateMockLoggerAdapter<BatchMetadataAnalysisFactory>();

            return new BatchMetadataAnalysisFactory(
                cosmosDbService,
                new EncryptionService(),
                configurationDataService,
                GetCosmosDbConfiguration(),
                mockLoggingService);
        }

        private NotificationSummaryBuilder GetNotificationSummaryBuilder()
        {
            var cosmosDbService = GetCosmosDbService();
            var configurationDataService = new ConfigurationDataService(cosmosDbService, GetCacheManager(), CreateMockLoggerAdapter<ConfigurationDataService>());

            return new NotificationSummaryBuilder(cosmosDbService, configurationDataService, SystemProvider);
        }

        private IDfESignInPublicApi GetDfESignInPublicApiService()
        {
            return DfESignInPublicApi;
        }
    }
}