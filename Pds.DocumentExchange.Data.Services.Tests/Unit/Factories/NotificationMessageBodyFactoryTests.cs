using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.Constants;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Implementations.Factories;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Providers;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Factories
{
    [TestClass]
    public sealed class NotificationMessageBodyFactoryTests :
        MoqTestingTests<NotificationMessageBodyFactory, ICreateNotificationMessageBodies>
    {
        [TestMethod]
        public void ConstructorFailsWithNullPresentationFormatter()
        {
            // arrange
            var organisationLookup = MakeStrictMock<IOrganisationsLookup>();
            var configurationDataService = MakeStrictMock<IConfigurationDataService>();
            var config = MakeStrictMock<IOptions<AgencyServiceConfiguration>>();
            var mockLoggingService = MakeLooseMock<ILoggerAdapter<NotificationMessageBodyFactory>>();

            // act / assert
            Assert.ThrowsException<ArgumentNullException>(
                    () => new NotificationMessageBodyFactory(null, organisationLookup, configurationDataService, config, mockLoggingService));
        }

        [TestMethod]
        public void ConstructorFailsWithNullOrganisationAPI()
        {
            // arrange
            var formatter = MakeStrictMock<IProvidePresentationFormatting>();
            var configurationDataService = MakeStrictMock<IConfigurationDataService>();
            var config = MakeStrictMock<IOptions<AgencyServiceConfiguration>>();
            var mockLoggingService = MakeLooseMock<ILoggerAdapter<NotificationMessageBodyFactory>>();

            // act / assert
            Assert.ThrowsException<ArgumentNullException>(
                () => new NotificationMessageBodyFactory(formatter, null, configurationDataService, config, mockLoggingService));
        }

        [TestMethod]
        public void ConstructorFailsWithNullConfigurationDataService()
        {
            // arrange
            var formatter = MakeStrictMock<IProvidePresentationFormatting>();
            var organisationLookup = MakeStrictMock<IOrganisationsLookup>();
            var config = MakeStrictMock<IOptions<AgencyServiceConfiguration>>();
            var mockLoggingService = MakeLooseMock<ILoggerAdapter<NotificationMessageBodyFactory>>();

            // act / assert
            Assert.ThrowsException<ArgumentNullException>(
                () => new NotificationMessageBodyFactory(formatter, organisationLookup, null, config, mockLoggingService));
        }

        [TestMethod]
        public void ConstructorFailsWithNullConfiguration()
        {
            // arrange
            var formatter = MakeStrictMock<IProvidePresentationFormatting>();
            var organisationLookup = MakeStrictMock<IOrganisationsLookup>();
            var configurationDataService = MakeStrictMock<IConfigurationDataService>();
            var mockLoggingService = MakeLooseMock<ILoggerAdapter<NotificationMessageBodyFactory>>();

            // act / assert
            Assert.ThrowsException<ArgumentNullException>(
                () => new NotificationMessageBodyFactory(formatter, organisationLookup, configurationDataService, null, mockLoggingService));
        }

        [TestMethod]
        public async Task BuildMessageContentFromAnalysisMeetsExpectation()
        {
            // arrange
            // the layout of these strings is a bit ugly, but can’t be helped.
            var nl = Environment.NewLine;

            const string workingTemplate = "document name: '[DocumentName]'" +
                                           " document name ui: '[DocumentNameUI]'" +
                                           " document name list(UL): '[DocumentListUL]'" +
                                           " document name list(UIUL): '[DocumentListUIUL]'" +
                                           " document type: '[DocumentType]'" +
                                           " document types: '[DocumentTypes]'" +
                                           " document types(UL): '[DocumentTypesUL]" +
                                           " document count: '[DocumentCount]'" +
                                           " provider name: '[ProviderName]'" +
                                           " send date time: '[SendDateTime]'" +
                                           " send date: '[SendDate]'" +
                                           " reply email: '[ReplyEmail]'" +
                                           " base URL: '[BaseUrl]'";

            var expectedResult = "document name: 'any old original file name.docx'" +
                                 " document name ui: 'any old formatted response'" +
                                 $" document name list(UL): 'any old original file name.docx<br/>{nl}'" +
                                 $" document name list(UIUL): 'any old formatted response<br/>{nl}'" +
                                 " document type: 'any old product name'" +
                                 " document types: '1 any old product name files'" +
                                 " document types(UL): '<ul><li>1 any old product name files</li></ul>" +
                                 " document count: '1'" +
                                 " provider name: 'Chelsea Flower Show'" +
                                 " send date time: '12:00AM, 22 June 2020'" +
                                 " send date: '22 June 2020'" +
                                 " reply email: 'any old service reply email'" +
                                 " base URL: 'https://skillsfunding.service.gov.uk/'";

            const int testOrganisation = 10239;
            const string testBatchID = "any old batch id";
            const string productIdentifier = "any old product identifier";
            const string productName = "any old product name";
            const string originalFileName = "any old original file name.docx";
            const string formattingResponse = "any old formatted response";
            var creationDate = DateTime.Parse("12:00AM, 22 June 2020");
            const string orgName = "Chelsea Flower Show";
            const string serviceReplyEmail = "any old service reply email";

            var sut = BuildTestSystem();

            // you can't mock  these types.
            var file = new FileMetadata
            {
                ProductIdentifier = productIdentifier,
                OriginalFileName = originalFileName
            };

            var identifier = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = $"{testOrganisation}"
            };

            var organisation = new Organisation
            {
                Identifiers = MakeEnumerableItems(1, identifier),
                Name = orgName
            };

            var analysis = MakeStrictMock<IBatchMetadataAnalysis>();

            GetMock(analysis)
                .SetupGet(x => x.ParentBatchID)
                .Returns(testBatchID);

            GetMock(analysis)
                .SetupGet(x => x.IssuingOrganisation)
                .Returns(testOrganisation);

            GetMock(analysis)
                .SetupGet(x => x.InitialBatchDate)
                .Returns(creationDate);

            GetMock(analysis)
                .Setup(x => x.GetAllFiles())
                .Returns(MakeReadonlyItems<FileMetadata>(1, file));

            GetMock(analysis)
                .Setup(x => x.GetProductNameFor(productIdentifier))
                .Returns(productName);

            GetMock(analysis)
                .Setup(x => x.IsInternal)
                .Returns(false);

            GetMock(sut.Formatter)
                .Setup(x => x.FormatProductNameWithFileExtension(productName, originalFileName))
                .Returns(formattingResponse);

            GetMock(sut.OrganisationsLookup)
                .Setup(
                    x => x.Get(
                        It.Is<OrganisationIdentifier>(
                            id => id.Type == identifier.Type && id.Value == identifier.Value)))
                .Returns(Task.FromResult(organisation));

            GetMock(sut.ConfigurationDataService)
                .Setup(config => config.GetServiceReplyEmail())
                .ReturnsAsync(serviceReplyEmail);

            // act
            var result = await sut.BuildMessageContentFrom(workingTemplate, analysis);

            // assert
            result.Should().Be(expectedResult);
        }

        [TestMethod]
        public async Task BuildMessageContentFromProductGroupsMeetsExpectation()
        {
            // arrange
            // the layout of these strings is a bit ugly, but can't be helped.
            const string workingTemplate = @"
number of documents:    '[NumberOfDocuments]'
document types;         '[DocumentTypeDetails]'
base URL:               '[BaseUrl]'";

            const string expectedResult = @"
number of documents:    '1'
document types;         'You have 1 new any old product name'
base URL:               'https://skillsfunding.service.gov.uk/'";

            var sut = BuildTestSystem();
            var product = MakeStrictMock<IBatchAnalysisProduct>();

            GetMock(product)
                .SetupGet(x => x.Name)
                .Returns("any old product name");

            // you can't mock  these types.
            var productGroup = new KeyValuePair<IBatchAnalysisProduct, int>(product, 1);
            var productGroups = MakeReadonlyItems<KeyValuePair<IBatchAnalysisProduct, int>>(1, productGroup);

            // act
            var result = await sut.BuildMessageContentFrom(workingTemplate, productGroups);

            // assert
            result.Should().Be(expectedResult);
        }

        [TestMethod]
        [DataRow(1, true)]
        [DataRow(1, false)]
        [DataRow(2, true)]
        [DataRow(2, false)]
        public async Task BuildEmailPersonalisationFromAnalysisMeetsExpectation(int numberOfFiles, bool isForInfectedFiles)
        {
            // arrange
            var nl = Environment.NewLine;
            string documentNumberText = numberOfFiles > 1 ? $"{numberOfFiles} documents" : "1 document";

            var analysis = MakeStrictMock<IBatchMetadataAnalysis>();

            var personalisationList = new Dictionary<string, dynamic>
                {
                    { MessageBodyTag.DocumentName, "any old original file name.docx" },
                    { MessageBodyTag.DocumentNameUI, "any old formatted response" },
                    { MessageBodyTag.DocumentListUL, DuplicateStringFor(numberOfFiles, $"* any old original file name.docx{nl}") },
                    { MessageBodyTag.DocumentListUIUL, DuplicateStringFor(numberOfFiles, $"any old formatted response{nl}") },
                    { MessageBodyTag.DocumentType, "any old product name" },
                    { MessageBodyTag.DocumentTypes, $"{numberOfFiles} any old product name files" },
                    { MessageBodyTag.DocumentTypesUL, $"{numberOfFiles} any old product name files" },
                    { MessageBodyTag.DocumentCount, numberOfFiles.ToString() },
                    { MessageBodyTag.SendDateTime, "12:00AM, 22 June 2020" },
                    { MessageBodyTag.SendDate, "22 June 2020" },
                    { MessageBodyTag.ReplyEmail, "any old service reply email" },
                    { MessageBodyTag.BaseUrl, "https://skillsfunding.service.gov.uk/" },
                    { MessageBodyTag.DocumentNumberText, documentNumberText },
                    { MessageBodyTag.ProviderName, "Chelsea Flower Show" },
                    { MessageBodyTag.RecipientName, "IssuingPerson" }
                };

            const int testOrganisation = 10239;
            const string issuingPerson = "IssuingPerson";
            const string testBatchID = "any old batch id";
            const string productIdentifier = "any old product identifier";
            const string productName = "any old product name";
            const string originalFileName = "any old original file name.docx";
            const string formattingResponse = "any old formatted response";
            var creationDate = DateTime.Parse("12:00AM, 22 June 2020");
            const string orgName = "Chelsea Flower Show";
            const string serviceReplyEmail = "any old service reply email";

            var sut = BuildTestSystem();

            // you can't mock  these types.
            var file = new FileMetadata
            {
                ProductIdentifier = productIdentifier,
                OriginalFileName = originalFileName
            };

            var identifier = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = $"{testOrganisation}"
            };

            var organisation = new Organisation
            {
                Identifiers = MakeEnumerableItems(1, identifier),
                Name = orgName
            };

            GetMock(analysis)
                .SetupGet(x => x.ParentBatchID)
                .Returns(testBatchID);

            GetMock(analysis)
                .SetupGet(x => x.IssuingOrganisation)
                .Returns(testOrganisation);

            GetMock(analysis)
              .SetupGet(x => x.IssuingPerson)
              .Returns(issuingPerson);

            GetMock(analysis)
                .SetupGet(x => x.InitialBatchDate)
                .Returns(creationDate);

            GetMock(analysis)
                .Setup(x => x.GetAllFiles())
                .Returns(MakeReadonlyItems<FileMetadata>(numberOfFiles, file));

            if (isForInfectedFiles)
            {
                GetMock(analysis)
              .Setup(x => x.InfectedFiles)
              .Returns(MakeReadonlyItems<FileMetadata>(numberOfFiles, file));
            }
            else
            {
                GetMock(analysis)
               .Setup(x => x.ClearFiles)
               .Returns(MakeReadonlyItems<FileMetadata>(numberOfFiles, file));
            }

            GetMock(analysis)
                .Setup(x => x.GetProductNameFor(productIdentifier))
                .Returns(productName);

            GetMock(analysis)
                .Setup(x => x.IsInternal)
                .Returns(false);

            GetMock(sut.Formatter)
                .Setup(x => x.FormatProductNameWithFileExtension(productName, originalFileName))
                .Returns(formattingResponse);

            GetMock(sut.OrganisationsLookup)
                .Setup(
                    x => x.Get(
                        It.Is<OrganisationIdentifier>(
                            id => id.Type == identifier.Type && id.Value == identifier.Value)))
                .Returns(Task.FromResult(organisation));

            GetMock(sut.ConfigurationDataService)
                .Setup(config => config.GetServiceReplyEmail())
                .ReturnsAsync(serviceReplyEmail);

            // act
            var result = await sut.BuildEmailPersonalisationFrom(analysis, isForInfectedFiles, issuingPerson);

            // assert
            result.Should().BeEquivalentTo(personalisationList);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public async Task BuildEmailPersonalisationFromProductGroupsMeetsExpectation(int numberOfFiles)
        {
            var nl = Environment.NewLine;
            var sut = BuildTestSystem();
            var product = MakeStrictMock<IBatchAnalysisProduct>();

            string documentTypeDetails = numberOfFiles == 1
                      ? $"# You have {numberOfFiles} new any old product name{nl}"
                      : DuplicateStringFor(numberOfFiles, $"* 1 any old product name{nl}");

            var personalisationList = new Dictionary<string, dynamic>
                {
                    { MessageBodyTag.NumberOfDocuments, numberOfFiles.ToString() },
                    { MessageBodyTag.DocumentTypeDetails, documentTypeDetails },
                    { MessageBodyTag.BaseUrl, "https://skillsfunding.service.gov.uk/" }
                };

            GetMock(product)
                .SetupGet(x => x.Name)
                .Returns("any old product name");

            // you can't mock  these types.
            var productGroup = new KeyValuePair<IBatchAnalysisProduct, int>(product, 1);
            var productGroups = MakeReadonlyItems<KeyValuePair<IBatchAnalysisProduct, int>>(numberOfFiles, productGroup);

            // act
            var result = await sut.BuildEmailPersonalisationFrom(productGroups);

            // assert
            result.Should().BeEquivalentTo(personalisationList);
        }

        [TestMethod]
        [DataRow("single name 1", "plural name 1", 0, "single name 1")]
        [DataRow("single name 2", "plural name 2", 1, "single name 2")]
        [DataRow("single name 3", "plural name 3", 2, "plural name 3")]
        public void GetProductNameMeetsExpectation(
            string singleName,
            string pluralName,
            int productCount,
            string expectedName)
        {
            // arrange
            var sut = BuildTestSystem();
            var product = MakeStrictMock<IBatchAnalysisProduct>();

            GetMock(product)
                .SetupGet(x => x.Name)
                .Returns(singleName);

            GetMock(product)
                .SetupGet(x => x.PluralName)
                .Returns(pluralName);

            var candidate = new KeyValuePair<IBatchAnalysisProduct, int>(product, productCount);

            // act
            var result = sut.GetProductName(candidate);

            // assert
            result.Should().Be(expectedName);
        }

        internal string DuplicateStringFor(int times, string value)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < times; i++)
            {
                sb.Append(value);
            }

            return sb.ToString();
        }

        internal override NotificationMessageBodyFactory BuildTestSystem()
        {
            var formatter = MakeStrictMock<IProvidePresentationFormatting>();
            var organisationLookup = MakeStrictMock<IOrganisationsLookup>();
            var configurationDataService = MakeStrictMock<IConfigurationDataService>();
            var mockLoggingService = MakeLooseMock<ILoggerAdapter<NotificationMessageBodyFactory>>();

            var config = MakeStrictMock<IOptions<AgencyServiceConfiguration>>();

            GetMock(config)
                .Setup(x => x.Value)
                .Returns(MakeStrictMock<AgencyServiceConfiguration>());

            return new NotificationMessageBodyFactory(formatter, organisationLookup, configurationDataService, config, mockLoggingService);
        }

        internal override void VerifyAllMocks(NotificationMessageBodyFactory sut)
        {
            GetMock(sut.Formatter).VerifyAll();
            GetMock(sut.OrganisationsLookup).VerifyAll();
        }
    }
}