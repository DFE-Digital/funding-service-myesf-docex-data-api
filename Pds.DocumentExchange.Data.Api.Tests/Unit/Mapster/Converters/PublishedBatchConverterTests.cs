using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Api.Mapster.Converters;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.SupportTools;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Mapster.Converters
{
    [TestClass]
    [TestCategory("Unit")]
    public class PublishedBatchConverterTests
    {
        private readonly Mock<IFileMetadataUserEncryptor> _fileMetadataUserEncryptor = new Mock<IFileMetadataUserEncryptor>(MockBehavior.Strict);
        private readonly PublishedBatchConverter _converter;

        public PublishedBatchConverterTests()
        {
            _converter = new PublishedBatchConverter(_fileMetadataUserEncryptor.Object);
        }

        [TestMethod]
        public void Convert_FromNull_Throws()
        {
            // Act
            Func<Models.SupportTools.PublishedBatch> func = () => _converter.Convert(null);

            // Assert
            func.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        public void Convert_Returns()
        {
            // Arrange
            var publishedBatch = new PublishedBatch
            {
                ParentBatchIdentifier = Guid.Parse("fff4bd95-7bb8-4891-9b76-a97811a394b7"),
                DateAndTime = new DateTime(2020, 1, 1),
                NumberOfDocuments = 10,
                NumberOfEmails = 15,
                UploadedBy = new FileMetadataUser
                {
                    Principal = "encrypted-string",
                    FullName = "encrypted-string",
                    EmailAddress = "encrypted-string",
                    IsEncrypted = true
                }
            };

            var decryptedUser = new FileMetadataUser
            {
                Principal = "the-principal",
                FullName = "full-name",
                EmailAddress = "email.address@education.co.uk",
                IsEncrypted = false
            };

            var expected = new Models.SupportTools.PublishedBatch
            {
                ParentBatchIdentifier = publishedBatch.ParentBatchIdentifier,
                DateAndTime = publishedBatch.DateAndTime,
                NumberOfDocuments = publishedBatch.NumberOfDocuments,
                NumberOfEmails = publishedBatch.NumberOfEmails,
                EmailAddress = decryptedUser.EmailAddress
            };

            _fileMetadataUserEncryptor
                .Setup(e => e.Decrypt(publishedBatch.UploadedBy))
                .Returns(decryptedUser);

            // Act
            var result = _converter.Convert(publishedBatch);

            // Assert
            result.Should().BeEquivalentTo(expected);
        }
    }
}