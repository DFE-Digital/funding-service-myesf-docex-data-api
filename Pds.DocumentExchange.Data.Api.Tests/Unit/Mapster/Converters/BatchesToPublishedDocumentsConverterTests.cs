using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Mapster.Converters;
using Pds.DocumentExchange.Data.Api.Models.SupportTools;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Mapster.Converters
{
    [TestClass]
    [TestCategory("Unit")]
    public class BatchesToPublishedDocumentsConverterTests
    {
        private readonly BatchesToPublishedDocumentsConverter _converter = new BatchesToPublishedDocumentsConverter();

        [TestMethod]
        public void Convert_FromNull_ReturnsEmpty()
        {
            // Act
            var result = _converter.Convert(null);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Convert_Returns()
        {
            // Arrange
            var file1 = new FileMetadata
            {
                FileName = "file-01.xlsx",
                ProductIdentifier = "product-01",
                Metadata = new Dictionary<string, string>
                {
                    ["Year"] = "202021"
                },
                ToUkprn = 12345678,
                Version = 1,
                VirusScanSuccessful = true,
                History = new[]
                {
                    new FileMetadataHistory { Action = FileAction.Published },
                    new FileMetadataHistory { Action = FileAction.EmailSent }
                }
            };

            var file2 = new FileMetadata
            {
                FileName = "file-02.xlsx",
                ProductIdentifier = "product-02",
                Metadata = new Dictionary<string, string>
                {
                    ["Year"] = "201920"
                },
                ToUkprn = 87654321,
                Version = 8,
                VirusScanSuccessful = false,
                History = new[]
                {
                    new FileMetadataHistory { Action = FileAction.Published }
                }
            };

            var file3 = new FileMetadata
            {
                FileName = "file-03.xlsx",
                ProductIdentifier = "product-03",
                Metadata = new Dictionary<string, string>
                {
                    ["Year"] = "202021"
                },
                ToUkprn = 87654321,
                Version = 5,
                VirusScanSuccessful = true,
                History = null
            };

            var batch = new BatchMetadata
            {
                Files = new[]
                {
                    file1,
                    file2,
                    file3
                }
            };

            var expected = new[]
            {
                new PublishedDocument
                {
                    FileName = file1.FileName,
                    FileType = file1.ProductIdentifier,
                    Year = file1.Metadata["Year"],
                    ToUKPRN = file1.ToUkprn,
                    Version = file1.Version,
                    VirusScanSuccessful = file1.VirusScanSuccessful,
                    EmailPrepared = true
                },
                new PublishedDocument
                {
                    FileName = file2.FileName,
                    FileType = file2.ProductIdentifier,
                    Year = file2.Metadata["Year"],
                    ToUKPRN = file2.ToUkprn,
                    Version = file2.Version,
                    VirusScanSuccessful = file2.VirusScanSuccessful,
                    EmailPrepared = false
                },
                new PublishedDocument
                {
                    FileName = file3.FileName,
                    FileType = file3.ProductIdentifier,
                    Year = file3.Metadata["Year"],
                    ToUKPRN = file3.ToUkprn,
                    Version = file3.Version,
                    VirusScanSuccessful = file3.VirusScanSuccessful,
                    EmailPrepared = false
                }
            };

            // Act
            var result = _converter.Convert(new[] { batch });

            // Assert
            result.Should().BeEquivalentTo(expected);
        }
    }
}