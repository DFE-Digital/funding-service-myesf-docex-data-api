using AutoMapper;
using Pds.DocumentExchange.Data.Services.DTOs.SupportTools;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using PublishedDocumentApi = Pds.DocumentExchange.Data.Api.Models.SupportTools.PublishedBatch;

namespace Pds.DocumentExchange.Data.Api.AutoMapperProfiles.Converters
{
    /// <summary>
    /// The organisation cache warm-up result converter.
    /// </summary>
    public class PublishedBatchConverter : ITypeConverter<PublishedBatch, PublishedDocumentApi>
    {
        private readonly IFileMetadataUserEncryptor _fileMetadataUserEncryptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedBatchConverter"/> class.
        /// </summary>
        /// <param name="fileMetadataUserEncryptor">The file metadata user encryptor.</param>
        public PublishedBatchConverter(IFileMetadataUserEncryptor fileMetadataUserEncryptor)
        {
            _fileMetadataUserEncryptor = fileMetadataUserEncryptor;
        }

        /// <inheritdoc/>
        public PublishedDocumentApi Convert(PublishedBatch source, PublishedDocumentApi destination, ResolutionContext context)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "The published batch cannot be null.");
            }

            var user = _fileMetadataUserEncryptor.Decrypt(source.UploadedBy);

            return new PublishedDocumentApi
            {
                ParentBatchIdentifier = source.ParentBatchIdentifier,
                DateAndTime = source.DateAndTime,
                NumberOfDocuments = source.NumberOfDocuments,
                NumberOfEmails = source.NumberOfEmails,
                EmailAddress = user.EmailAddress
            };
        }
    }
}
