using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Builders;
using Pds.DocumentExchange.Data.Services.Interfaces.Builders;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using Pds.DocumentExchange.Data.Services.Interfaces.Providers;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Builders
{
    [TestClass]
    public sealed class NotificationMessageBuilderTests :
        MoqTestingTests<NotificationMessageBuilder, INotificationMessageBuilder>
    {
        [TestMethod]
        public void ConstructorWithNullTemplateProviderThrows()
        {
            // arrange
            var headerFactory = MakeStrictMock<ICreateNotificationMessageHeaders>();
            var bodyFactory = MakeStrictMock<ICreateNotificationMessageBodies>();
            var messageFactory = MakeStrictMock<ICreateNotificationMessages>();
            var configurationDataService = MakeStrictMock<IConfigurationDataService>();

            // act / assert
            Assert.ThrowsException<ArgumentNullException>(() => new NotificationMessageBuilder(null, headerFactory, bodyFactory, messageFactory, configurationDataService));
        }

        [TestMethod]
        public void ConstructorWithNullHeaderFactoryThrows()
        {
            // arrange
            var templateProvider = MakeStrictMock<IProvideNotificationMessageBodyTemplates>();
            var bodyFactory = MakeStrictMock<ICreateNotificationMessageBodies>();
            var messageFactory = MakeStrictMock<ICreateNotificationMessages>();
            var configurationDataService = MakeStrictMock<IConfigurationDataService>();

            // act / assert
            Assert.ThrowsException<ArgumentNullException>(() => new NotificationMessageBuilder(templateProvider, null, bodyFactory, messageFactory, configurationDataService));
        }

        [TestMethod]
        public void ConstructorWithNullBodyFactoryThrows()
        {
            // arrange
            var templateProvider = MakeStrictMock<IProvideNotificationMessageBodyTemplates>();
            var headerFactory = MakeStrictMock<ICreateNotificationMessageHeaders>();
            var messageFactory = MakeStrictMock<ICreateNotificationMessages>();
            var configurationDataService = MakeStrictMock<IConfigurationDataService>();

            // act / assert
            Assert.ThrowsException<ArgumentNullException>(() => new NotificationMessageBuilder(templateProvider, headerFactory, null, messageFactory, configurationDataService));
        }

        [TestMethod]
        public void ConstructorWithNullMessageFactoryThrows()
        {
            // arrange
            var templateProvider = MakeStrictMock<IProvideNotificationMessageBodyTemplates>();
            var headerFactory = MakeStrictMock<ICreateNotificationMessageHeaders>();
            var bodyFactory = MakeStrictMock<ICreateNotificationMessageBodies>();
            var configurationDataService = MakeStrictMock<IConfigurationDataService>();

            // act / assert
            Assert.ThrowsException<ArgumentNullException>(() => new NotificationMessageBuilder(templateProvider, headerFactory, bodyFactory, null, configurationDataService));
        }

        [TestMethod]
        public void ConstructorWithNullConfigurationDataServiceThrows()
        {
            // arrange
            var templateProvider = MakeStrictMock<IProvideNotificationMessageBodyTemplates>();
            var headerFactory = MakeStrictMock<ICreateNotificationMessageHeaders>();
            var bodyFactory = MakeStrictMock<ICreateNotificationMessageBodies>();
            var messageFactory = MakeStrictMock<ICreateNotificationMessages>();

            // act / assert
            Assert.ThrowsException<ArgumentNullException>(() => new NotificationMessageBuilder(templateProvider, headerFactory, bodyFactory, messageFactory, null));
        }

        [TestMethod]
        [DataRow("subject 1", true, "email 1", "email name 1", null)]
        [DataRow("subject 2", false, "email 2", "email name 2", "parentBatchId")]
        public async Task GetInfectedFileMessageMeetsExpectation(string expectedSubject, bool isInternal, string recipient, string recipientName, string parentBatchId)
        {
            // arrange
            var sut = BuildTestSystem();
            var analysis = MakeStrictMock<IBatchMetadataAnalysis>();
            var emailPersonalisation = MakeStrictMock<Dictionary<string, dynamic>>();
            var emailMessageType = Convert.ToString(isInternal ? MessageBodyTemplateName.ESFAPublicationInfected : MessageBodyTemplateName.ExternalUploadInfected);

            Mock.Get(analysis)
                .SetupGet(x => x.ParentBatchID)
                .Returns(parentBatchId);

            GetMock(analysis)
                .SetupGet(x => x.IsInternal)
                .Returns(isInternal);

            Mock.Get(analysis)
                .SetupGet(x => x.IssuingOrganisation)
                .Returns(12345678);

            Mock.Get(analysis)
             .SetupGet(x => x.InfectedFiles)
             .Returns(new List<DTOs.FileMetadata> { new DTOs.FileMetadata { FileName = "fileName" } });

            var serviceNoReplyEmail = "any old service no reply email address...";
            var header = MakeStrictMock<INotificationMessageHeader>();
            var message = MakeStrictMock<INotificationMessage>();

            GetMock(sut.ConfigurationDataService)
                .Setup(x => x.GetServiceNoReplyEmail())
                .ReturnsAsync(serviceNoReplyEmail);
            GetMock(sut.MessageHeader)
                .Setup(x => x.Create(expectedSubject, serviceNoReplyEmail, recipient, recipientName))
                .Returns(Task.FromResult(header));
            GetMock(sut.MessageBody)
              .Setup(x => x.BuildEmailPersonalisationFrom(analysis, true, recipientName))
              .Returns(Task.FromResult(emailPersonalisation));

            GetMock(sut.Message)
                .Setup(x => x.Create(header, string.Empty, isInternal, parentBatchId, 12345678, emailMessageType, emailPersonalisation))
                .Returns(Task.FromResult(message));

            // act
            var result = await sut.GetInfectedFileMessage(expectedSubject, analysis, recipient, recipientName);

            // assert
            VerifyAllMocks(sut);
            GetMock(analysis).VerifyAll();
            GetMock(header).VerifyAll();
            GetMock(message).VerifyAll();

            result.Should().BeAssignableTo<INotificationMessage>();
        }

        [TestMethod]
        [DataRow("subject 1", true, "email 1", "email name 1", null, 1)]
        [DataRow("subject 1", true, "email 1", "email name 1", null, 2)]
        [DataRow("subject 2", false, "email 2", "email name 2", "parentBatchId", 1)]
        [DataRow("subject 2", false, "email 2", "email name 2", "parentBatchId", 2)]
        public async Task GetCleanFileMessageMeetsExpectation(string expectedSubject, bool isInternal, string recipient, string recipientName, string parentBatchId, int fileCount)
        {
            // arrange
            var sut = BuildTestSystem();
            var analysis = MakeStrictMock<IBatchMetadataAnalysis>();
            var emailPersonalisation = MakeStrictMock<Dictionary<string, dynamic>>();
            var emailMessageType = Convert.ToString(MessageBodyTemplateName.ExternalUploadCompleted);

            if (isInternal)
            {
                emailMessageType = Convert.ToString(fileCount > 1
                   ? MessageBodyTemplateName.ESFAPublishedMultiple
                   : MessageBodyTemplateName.ESFAPublishedSingle);
            }

            Mock.Get(analysis)
                .SetupGet(x => x.ParentBatchID)
                .Returns(parentBatchId);

            GetMock(analysis)
                .SetupGet(x => x.IsInternal)
                .Returns(isInternal);

            Mock.Get(analysis)
                .SetupGet(x => x.IssuingOrganisation)
                .Returns(12345678);

            if (fileCount == 1)
            {
                Mock.Get(analysis)
               .SetupGet(x => x.ClearFiles)
               .Returns(new List<DTOs.FileMetadata> { new DTOs.FileMetadata { FileName = "fileName" } });
            }

            if (fileCount > 1)
            {
                Mock.Get(analysis)
               .SetupGet(x => x.ClearFiles)
               .Returns(new List<DTOs.FileMetadata>
               {
                   new DTOs.FileMetadata { FileName = "fileName" },
                   new DTOs.FileMetadata { FileName = "fileName2" }
               });
            }

            var serviceNoReplyEmail = "any old service no reply email address...";
            var header = MakeStrictMock<INotificationMessageHeader>();
            var message = MakeStrictMock<INotificationMessage>();

            GetMock(sut.ConfigurationDataService)
                .Setup(x => x.GetServiceNoReplyEmail())
                .ReturnsAsync(serviceNoReplyEmail);
            GetMock(sut.MessageHeader)
                .Setup(x => x.Create(expectedSubject, serviceNoReplyEmail, recipient, recipientName))
                .Returns(Task.FromResult(header));
            GetMock(sut.MessageBody)
              .Setup(x => x.BuildEmailPersonalisationFrom(analysis, false, recipientName))
              .Returns(Task.FromResult(emailPersonalisation));

            GetMock(sut.Message)
                .Setup(x => x.Create(header, string.Empty, isInternal, parentBatchId, 12345678, emailMessageType, emailPersonalisation))
                .Returns(Task.FromResult(message));

            // act
            var result = await sut.GetCleanFileMessage(expectedSubject, analysis, recipient, recipientName);

            // assert
            VerifyAllMocks(sut);
            GetMock(analysis).VerifyAll();
            GetMock(header).VerifyAll();
            GetMock(message).VerifyAll();

            result.Should().BeAssignableTo<INotificationMessage>();
        }

        [TestMethod]
        [DataRow("subject 2", false, "email 2", "email name 2", "parentBatchId", 1)]
        [DataRow("subject 2", false, "email 2", "email name 2", "parentBatchId", 2)]
        public async Task GetAgencyTeamMessageWithBatchAnalysisMeetsExpectation(string expectedSubject, bool isInternal, string recipient, string recipientName, string parentBatchId, int fileCount)
        {
            // arrange
            var sut = BuildTestSystem();
            var analysis = MakeStrictMock<IBatchMetadataAnalysis>();
            var emailPersonalisation = MakeStrictMock<Dictionary<string, dynamic>>();

            var emailMessageType = Convert.ToString(fileCount > 1
              ? MessageBodyTemplateName.ESFAReceivedMultiple
              : MessageBodyTemplateName.ESFAReceivedSingle);

            Mock.Get(analysis)
                .SetupGet(x => x.ParentBatchID)
                .Returns(parentBatchId);

            Mock.Get(analysis)
                .SetupGet(x => x.IssuingOrganisation)
                .Returns(12345678);

            if (fileCount == 1)
            {
                Mock.Get(analysis)
               .SetupGet(x => x.ClearFiles)
               .Returns(new List<DTOs.FileMetadata> { new DTOs.FileMetadata { FileName = "fileName" } });
            }

            if (fileCount > 1)
            {
                Mock.Get(analysis)
               .SetupGet(x => x.ClearFiles)
               .Returns(new List<DTOs.FileMetadata>
               {
                   new DTOs.FileMetadata { FileName = "fileName" },
                   new DTOs.FileMetadata { FileName = "fileName2" }
               });
            }

            var serviceNoReplyEmail = "any old service no reply email address...";
            var header = MakeStrictMock<INotificationMessageHeader>();
            var message = MakeStrictMock<INotificationMessage>();

            GetMock(sut.ConfigurationDataService)
                .Setup(x => x.GetServiceNoReplyEmail())
                .ReturnsAsync(serviceNoReplyEmail);
            GetMock(sut.MessageHeader)
                .Setup(x => x.Create(expectedSubject, serviceNoReplyEmail, recipient, recipientName))
                .Returns(Task.FromResult(header));
            GetMock(sut.MessageBody)
              .Setup(x => x.BuildEmailPersonalisationFrom(analysis, false, recipientName))
              .Returns(Task.FromResult(emailPersonalisation));

            GetMock(sut.Message)
                .Setup(x => x.Create(header, string.Empty, true, parentBatchId, 12345678, emailMessageType, emailPersonalisation))
                .Returns(Task.FromResult(message));

            // act
            var result = await sut.GetAgencyTeamMessage(expectedSubject, analysis, recipient, recipientName);

            // assert
            VerifyAllMocks(sut);
            GetMock(analysis).VerifyAll();
            GetMock(header).VerifyAll();
            GetMock(message).VerifyAll();

            result.Should().BeAssignableTo<INotificationMessage>();
        }

        [TestMethod]
        [DataRow("subject 1", "", 1, "email 1", "email 2", "email 3", "email 4", "name 1", "name 2", "name 3", "name 4")]
        [DataRow("subject 2", "", 2, "email 11", "email 12", "email 13", "email 14", "name 11", "name 12", "name 13", "name 14")]
        [DataRow("subject 2", "", 3, "email 11", "email 12", "email 13", "email 14", "name 11", "name 12", "name 13", "name 14")]
        public async Task GetDocumentUserMessageMeetsExpectation(string expectedSubject, string parentBatchId, int productCount, params string[] candidates)
        {
            // arrange
            var sut = BuildTestSystem();
            var analysis = MakeStrictMock<IBatchMetadataAnalysis>();
            var emailPersonalisation = MakeStrictMock<Dictionary<string, dynamic>>();

            var emailMessageType = Convert.ToString(MessageBodyTemplateName.ProviderReceivedMultipleTypes);
            var productGroups = new List<KeyValuePair<IBatchAnalysisProduct, int>>
            {
                new KeyValuePair<IBatchAnalysisProduct, int>(null, 0),
                new KeyValuePair<IBatchAnalysisProduct, int>(null, 1),
                new KeyValuePair<IBatchAnalysisProduct, int>(null, 2)
            };

            if (productCount == 1)
            {
                emailMessageType = Convert.ToString(MessageBodyTemplateName.ProviderReceivedSingleType);
                productGroups = new List<KeyValuePair<IBatchAnalysisProduct, int>>
                {
                    new KeyValuePair<IBatchAnalysisProduct, int>(null, 0)
                };
            }

            if (productCount == 2)
            {
                emailMessageType = Convert.ToString(MessageBodyTemplateName.ProviderReceivedTwoTypes);
                productGroups = new List<KeyValuePair<IBatchAnalysisProduct, int>>
                {
                    new KeyValuePair<IBatchAnalysisProduct, int>(null, 0),
                    new KeyValuePair<IBatchAnalysisProduct, int>(null, 1)
                };
            }

            var header = MakeStrictMock<INotificationMessageHeader>();
            var message = MakeStrictMock<INotificationMessage>();
            var serviceNoReplyEmail = "any old service no reply email address...";

            var toAddresses = candidates.Take(4).ToArray();
            var toNames = candidates.Skip(4).ToArray();

            GetMock(sut.ConfigurationDataService)
                .Setup(x => x.GetServiceNoReplyEmail())
                .ReturnsAsync(serviceNoReplyEmail);
            GetMock(sut.MessageHeader)
                .Setup(x => x.Create(expectedSubject, serviceNoReplyEmail, toAddresses, toNames))
                .Returns(Task.FromResult(header));
            GetMock(sut.MessageBody)
                .Setup(x => x.BuildEmailPersonalisationFrom(productGroups))
                .Returns(Task.FromResult(emailPersonalisation));
            GetMock(sut.Message)
                .Setup(x => x.Create(header, string.Empty, false, parentBatchId, 12345678, emailMessageType, emailPersonalisation))
                .Returns(Task.FromResult(message));

            // act
            var result = await sut.GetDocumentUserMessage(expectedSubject, productGroups, toAddresses, toNames, parentBatchId, 12345678);

            // assert
            VerifyAllMocks(sut);
            GetMock(analysis).VerifyAll();
            GetMock(header).VerifyAll();
            GetMock(message).VerifyAll();

            result.Should().BeAssignableTo<INotificationMessage>();
        }

        internal override void VerifyAllMocks(NotificationMessageBuilder sut)
        {
            GetMock(sut.TemplateProvider).VerifyAll();
            GetMock(sut.MessageBody).VerifyAll();
            GetMock(sut.MessageHeader).VerifyAll();
            GetMock(sut.Message).VerifyAll();
        }

        internal override NotificationMessageBuilder BuildTestSystem()
        {
            var templateProvider = MakeStrictMock<IProvideNotificationMessageBodyTemplates>();
            var bodyFactory = MakeStrictMock<ICreateNotificationMessageBodies>();
            var headerFactory = MakeStrictMock<ICreateNotificationMessageHeaders>();
            var messageFactory = MakeStrictMock<ICreateNotificationMessages>();
            var configurationDataService = MakeStrictMock<IConfigurationDataService>();

            return new NotificationMessageBuilder(templateProvider, headerFactory, bodyFactory, messageFactory, configurationDataService);
        }
    }
}