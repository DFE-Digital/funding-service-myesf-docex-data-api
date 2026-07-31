using Pds.DocumentExchange.Data.Services.DTOs.SupportTools;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using ApiPublishedBatch = Pds.DocumentExchange.Data.Api.Models.SupportTools.PublishedBatch;

namespace Pds.DocumentExchange.Data.Api.Mapster.Converters
{
    /// <summary>
    /// The organisation cache warm-up result converter.
    /// </summary>
    public class PublishedBatchConverter
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

        public ApiPublishedBatch Convert(PublishedBatch source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "The published batch cannot be null.");
            }

            var user = _fileMetadataUserEncryptor.Decrypt(source.UploadedBy);

            return new ApiPublishedBatch
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