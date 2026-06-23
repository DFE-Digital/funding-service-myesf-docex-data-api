using Pds.DocumentExchange.Data.Populator.Models;
using Pds.DocumentExchange.Data.Services.DTOs;
using System;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Populator.Scenarios
{
    public class DocumentsUploadedFromExternalScenario : DocumentsUploadedScenario
    {
        protected override BatchMetadata CreateBatch(DocumentRecord record)
        {
            var id = string.IsNullOrWhiteSpace(record.ParentBatchIdentifier) ? Guid.NewGuid().ToString() : record.ParentBatchIdentifier;
            var batchCreatedDate = record.BatchCreatedDate ?? DateTime.UtcNow;

            var user = new FileMetadataUser
            {
                EmailAddress = "test@test.com",
                FullName = "Test",
                IsEncrypted = false,
                Principal = "Test1"
            };

            return new BatchMetadata
            {
                Id = id,
                ParentBatchIdentifier = id,
                CreatedDate = batchCreatedDate,
                UploadedBy = user,
                Files = new[]
                {
                    new FileMetadata
                    {
                        FileName = $"{record.Ukprn}_{record.ProviderId}_{record.AcademicYear}.{record.FileType}",
                        FromUkprn = record.Ukprn,
                        ToUkprn = -999,
                        Metadata = new Dictionary<string, string>
                        {
                            { "Year", record.AcademicYear }
                        },
                        ProductIdentifier = record.ProviderId,
                        Version = 1,
                        OriginalFileName = $"example.{record.FileType}",
                        Processed = true,
                        VirusScanSuccessful = true,
                        Deleted = false,
                        History = new[]
                        {
                            new FileMetadataHistory
                            {
                                Action = Services.Enums.FileAction.UploadedExternal,
                                ActionDateTimeUtc = batchCreatedDate,
                                User = user
                            }
                        }
                    }
                }
            };
        }
    }
}