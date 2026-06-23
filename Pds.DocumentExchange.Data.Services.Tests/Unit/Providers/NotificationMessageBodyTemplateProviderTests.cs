using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.Exceptions;
using Pds.DocumentExchange.Data.Services.Implementations.Providers;
using Pds.DocumentExchange.Data.Services.Interfaces.Providers;
using Pds.Services.Common.Interfaces.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Providers
{
    [TestClass]
    public sealed class NotificationMessageBodyTemplateProviderTests :
        MoqTestingTests<NotificationMessageBodyTemplateProvider, IProvideNotificationMessageBodyTemplates>
    {
        [TestMethod]
        public void ConstructorFailsWithNullAssetProvider() =>
            Assert.ThrowsException<ArgumentNullException>(() => new NotificationMessageBodyTemplateProvider(null));

        [TestMethod]
        public async Task GetCleanFileTemplateForEmptyCleanFilesThrowsException()
        {
            // arrange
            var sut = BuildTestSystem();

            var analysis = MakeStrictMock<IBatchMetadataAnalysis>();
            GetMock(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(MakeReadonlyItems<FileMetadata>(0));
            GetMock(analysis)
                .SetupGet(x => x.InfectedFiles)
                .Returns(MakeReadonlyItems<FileMetadata>(1));

            // act / assert
            await Assert.ThrowsExceptionAsync<MessageBodyTemplateProviderCardinalityException>(() => sut.GetCleanFileTemplateFor(analysis));
        }

        [TestMethod]
        public async Task GetInfectedFileTemplateForEmptyInfectedFilesThrowsException()
        {
            // arrange
            var sut = BuildTestSystem();

            var analysis = MakeStrictMock<IBatchMetadataAnalysis>();
            GetMock(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(MakeReadonlyItems<FileMetadata>(1));
            GetMock(analysis)
                .SetupGet(x => x.InfectedFiles)
                .Returns(MakeReadonlyItems<FileMetadata>(0));

            // act / assert
            await Assert.ThrowsExceptionAsync<MessageBodyTemplateProviderCardinalityException>(() => sut.GetInfectedFileTemplateFor(analysis));
        }

        [TestMethod]
        public async Task GetTeamNotificationTemplateForEmptyCleanFilesThrowsException()
        {
            // arrange
            var sut = BuildTestSystem();

            var analysis = MakeStrictMock<IBatchMetadataAnalysis>();
            GetMock(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(MakeReadonlyItems<FileMetadata>(0));
            GetMock(analysis)
                .SetupGet(x => x.InfectedFiles)
                .Returns(MakeReadonlyItems<FileMetadata>(1));

            // act / assert
            await Assert.ThrowsExceptionAsync<MessageBodyTemplateProviderCardinalityException>(() => sut.GetTeamNotificationTemplateFor(analysis));
        }

        [TestMethod]
        public async Task GetDocumentUserMessageTemplateForEmptyProductGroupsThrowsException()
        {
            // arrange
            var sut = BuildTestSystem();

            var productGroups = MakeReadonlyItems<KeyValuePair<IBatchAnalysisProduct, int>>(0);

            // act / assert
            await Assert.ThrowsExceptionAsync<MessageBodyTemplateProviderCardinalityException>(() => sut.GetDocumentUserMessageTemplateFor(productGroups));
        }

        [TestMethod]
        [DataRow(1, true, "ESFAPublishedSingle.txt")]
        [DataRow(1, false, "ExternalUploadCompleted.txt")]
        [DataRow(2, true, "ESFAPublishedMultiple.txt")]
        [DataRow(2, false, "ExternalUploadCompleted.txt")]
        [DataRow(3, true, "ESFAPublishedMultiple.txt")]
        [DataRow(3, false, "ExternalUploadCompleted.txt")]
        public async Task GetCleanFileTemplateForMeetsExpectation(int testCount, bool testInternal, string expectedTemplateName)
        {
            // arrange
            const string templateContent = "any old template content...";
            var sut = BuildTestSystem();

            GetMock(sut.Assets)
                .Setup(x => x.GetTextAsset(expectedTemplateName))
                .Returns(Task.FromResult(templateContent));

            var analysis = MakeStrictMock<IBatchMetadataAnalysis>();
            GetMock(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(MakeReadonlyItems<FileMetadata>(testCount));
            GetMock(analysis)
                .SetupGet(x => x.IsInternal)
                .Returns(testInternal);

            // act
            var result = await sut.GetCleanFileTemplateFor(analysis);

            // assert
            VerifyAllMocks(sut);

            result.Should().Be(templateContent);
        }

        [TestMethod]
        [DataRow(true, "ESFAPublicationInfected.txt")]
        [DataRow(false, "ExternalUploadInfected.txt")]
        public async Task GetInfectedFileTemplateForMeetsExpectation(bool testInternal, string expectedTemplateName)
        {
            // arrange
            const string templateContent = "any old template content...";
            var sut = BuildTestSystem();

            GetMock(sut.Assets)
                .Setup(x => x.GetTextAsset(expectedTemplateName))
                .Returns(Task.FromResult(templateContent));

            var analysis = MakeStrictMock<IBatchMetadataAnalysis>();
            GetMock(analysis)
                .SetupGet(x => x.InfectedFiles)
                .Returns(MakeReadonlyItems<FileMetadata>(1));
            GetMock(analysis)
                .SetupGet(x => x.IsInternal)
                .Returns(testInternal);

            // act
            var result = await sut.GetInfectedFileTemplateFor(analysis);

            // assert
            VerifyAllMocks(sut);

            result.Should().Be(templateContent);
        }

        [TestMethod]
        [DataRow(1, "ESFAReceivedSingle.txt")]
        [DataRow(2, "ESFAReceivedMultiple.txt")]
        [DataRow(3, "ESFAReceivedMultiple.txt")]
        public async Task GetTeamNotificationTemplateForMeetsExpectation(int testCount, string expectedTemplateName)
        {
            // arrange
            const string templateContent = "any old template content...";
            var sut = BuildTestSystem();

            GetMock(sut.Assets)
                .Setup(x => x.GetTextAsset(expectedTemplateName))
                .Returns(Task.FromResult(templateContent));

            var analysis = MakeStrictMock<IBatchMetadataAnalysis>();
            GetMock(analysis)
                .SetupGet(x => x.ClearFiles)
                .Returns(MakeReadonlyItems<FileMetadata>(testCount));

            // act
            var result = await sut.GetTeamNotificationTemplateFor(analysis);

            // assert
            VerifyAllMocks(sut);

            result.Should().Be(templateContent);
        }

        [TestMethod]
        [DataRow(1, "ProviderReceivedSingleType.txt")]
        [DataRow(2, "ProviderReceivedTwoTypes.txt")]
        [DataRow(3, "ProviderReceivedMultipleTypes.txt")]
        [DataRow(4, "ProviderReceivedMultipleTypes.txt")]
        public async Task GetDocumentUserMessageTemplateForMeetsExpectation(int testCount, string expectedTemplateName)
        {
            // arrange
            const string templateContent = "any old template content...";
            var sut = BuildTestSystem();

            GetMock(sut.Assets)
                .Setup(x => x.GetTextAsset(expectedTemplateName))
                .Returns(Task.FromResult(templateContent));

            var productGroups = MakeReadonlyItems<KeyValuePair<IBatchAnalysisProduct, int>>(testCount);

            // act
            var result = await sut.GetDocumentUserMessageTemplateFor(productGroups);

            // assert
            VerifyAllMocks(sut);

            result.Should().Be(templateContent);
        }

        internal override NotificationMessageBodyTemplateProvider BuildTestSystem()
        {
            var assets = MakeStrictMock<IProvideAssets>();

            return new NotificationMessageBodyTemplateProvider(assets);
        }

        internal override void VerifyAllMocks(NotificationMessageBodyTemplateProvider sut)
        {
            GetMock(sut.Assets).VerifyAll();
        }
    }
}