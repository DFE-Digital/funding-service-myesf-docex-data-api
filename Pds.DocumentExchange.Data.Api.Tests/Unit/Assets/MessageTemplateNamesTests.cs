using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.Enums;
using System;
using System.IO;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Assets
{
    [TestClass]
    [TestCategory("Unit")]
    public sealed class MessageTemplateNamesTests
    {
        [TestMethod]
        [DataRow(MessageBodyTemplateName.ESFAPublicationInfected)]
        [DataRow(MessageBodyTemplateName.ESFAPublishedMultiple)]
        [DataRow(MessageBodyTemplateName.ESFAPublishedSingle)]
        [DataRow(MessageBodyTemplateName.ESFAReceivedMultiple)]
        [DataRow(MessageBodyTemplateName.ESFAReceivedSingle)]
        [DataRow(MessageBodyTemplateName.ExternalUploadCompleted)]
        [DataRow(MessageBodyTemplateName.ExternalUploadInfected)]
        [DataRow(MessageBodyTemplateName.ProviderReceivedMultipleTypes)]
        [DataRow(MessageBodyTemplateName.ProviderReceivedSingleType)]
        [DataRow(MessageBodyTemplateName.ProviderReceivedTwoTypes)]
        public void MessageTemplateNameFilesExistMeetExpectations(MessageBodyTemplateName bodyTemplateName)
        {
            // arrange
            var filePath = Path.Combine(AppContext.BaseDirectory, "Assets", $"{bodyTemplateName}.txt");

            // act / assert
            File.Exists(filePath).Should().Be(true);
        }
    }
}
