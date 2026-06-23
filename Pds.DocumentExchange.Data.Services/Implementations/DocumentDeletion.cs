using AutoMapper;
using Pds.Core.Logging;
using Pds.Core.Utils;
using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// The document deletion service.
    /// </summary>
    public class DocumentDeletion : IDocumentDeletion
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IDirectoriesManager _directoriesManager;
        private readonly ISystemProvider _systemProvider;
        private readonly IFileMetadataUserEncryptor _fileMetadataUserEncryptor;
        private readonly IBatchToExchangeDocumentConverter _batchToExchangeDocumentConverter;
        private readonly IMapper _mapper;
        private readonly ILoggerAdapter<DocumentDeletion> _logger;

        private readonly string _directoryName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDeletion"/> class.
        /// </summary>
        /// <param name="cosmosDbService">The Cosmos DB service.</param>
        /// <param name="directoriesManager">The directories manager.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="systemProvider">The system provider.</param>
        /// <param name="fileMetadataUserEncryptor">The file metadata user encryption.</param>
        /// <param name="batchToExchangeDocumentConverter">The batch to exchange document converter.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="logger">The logger.</param>
        public DocumentDeletion(
            ICosmosDbService cosmosDbService,
            IDirectoriesManager directoriesManager,
            ISystemProvider systemProvider,
            IFileMetadataUserEncryptor fileMetadataUserEncryptor,
            IBatchToExchangeDocumentConverter batchToExchangeDocumentConverter,
            IMapper mapper,
            VirusScanResultProcessorConfiguration config,
            ILoggerAdapter<DocumentDeletion> logger)
        {
            _cosmosDbService = cosmosDbService;
            _directoriesManager = directoriesManager;
            _systemProvider = systemProvider;
            _fileMetadataUserEncryptor = fileMetadataUserEncryptor;
            _batchToExchangeDocumentConverter = batchToExchangeDocumentConverter;
            _mapper = mapper;
            _logger = logger;

            _directoryName = config.FileCopyDestinationDirectoryKey;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ExchangeDocument>> DeleteDocuments(ExchangeDocumentDeleteRequest exchangeDocumentDeleteRequest)
        {
            var result = Collection.Empty<ExchangeDocument>();

            _logger.LogInformation($"Start {nameof(DeleteDocuments)} for {exchangeDocumentDeleteRequest?.DocumentReferences?.Count()} documents.");

            It.IsNull(exchangeDocumentDeleteRequest)
                .AsGuard<ArgumentNullException>();

            It.IsNull(exchangeDocumentDeleteRequest?.UserInfo)
                .AsGuard<ArgumentNullException>();

            It.IsEmpty(exchangeDocumentDeleteRequest?.DocumentReferences)
                .AsGuard<ArgumentNullException>();

            foreach (var currentDocumentReference in exchangeDocumentDeleteRequest?.DocumentReferences)
            {
                var deleteResult = await DeleteDocumentReferenceWithPreviousVersions(exchangeDocumentDeleteRequest?.UserInfo, currentDocumentReference);

                if (deleteResult != null)
                {
                    result.Add(deleteResult);
                }
            }

            _logger.LogInformation($"Finished {nameof(DeleteDocuments)} with {result?.Count} documents.");

            return result;
        }

        /// <inheritdoc/>
        public async Task<ExchangeDocument> DeleteDocument(ExchangeDocumentDeleteRequest exchangeDocumentDeleteRequest)
        {
            _logger.LogInformation($"Start {nameof(DeleteDocument)}");

            It.IsNull(exchangeDocumentDeleteRequest)
                .AsGuard<ArgumentNullException>();

            It.IsNull(exchangeDocumentDeleteRequest.UserInfo)
                .AsGuard<ArgumentNullException>();

            It.IsEmpty(exchangeDocumentDeleteRequest.DocumentReferences)
                .AsGuard<ArgumentNullException>();

            It.IsNull(exchangeDocumentDeleteRequest.DocumentReferences.FirstOrDefault())
                .AsGuard<ArgumentNullException>();

            var documentToDelete = exchangeDocumentDeleteRequest.DocumentReferences.FirstOrDefault();

            _logger.LogInformation($"Finished {nameof(DeleteDocument)}");

            return await DeleteSingleDocumentReference(exchangeDocumentDeleteRequest.UserInfo, documentToDelete);
        }

        private async Task<ExchangeDocument> DeleteDocumentReferenceWithPreviousVersions(UserInfo userInfo, DocumentReferenceWithPreviousVersions documentReference)
        {
            var currentVersionDocument = await DeleteSingleDocumentReference(userInfo, documentReference);

            if (currentVersionDocument == null)
            {
                return null;
            }

            var previousVersionTasks = documentReference.PreviousVersions.Select(versionDocumentReference => DeleteSingleDocumentReference(userInfo, versionDocumentReference));
            var previousVersions = await Task.WhenAll(previousVersionTasks);

            currentVersionDocument.PreviousVersions = previousVersions.Where(document => document != null);

            return currentVersionDocument;
        }

        private async Task<ExchangeDocument> DeleteSingleDocumentReference(UserInfo userInfo, DocumentReference documentReference)
        {
            try
            {
                _logger.LogInformation($"Deleting fileName: {documentReference.FileName}");

                var fileMetadata = await _cosmosDbService.GetFile(documentReference);

                if (fileMetadata == null)
                {
                    _logger.LogInformation($"Document {documentReference.FileName} in batch {documentReference.BatchIdentifier} was not found.");
                    return null;
                }

                await DeleteDocument(documentReference.FileName);
                await UpdateFileMetadata(documentReference.BatchIdentifier, fileMetadata, userInfo);

                var documentDirection = (fileMetadata.IsFromAgency || fileMetadata.FromUkprn == -999) ? ExchangeDocumentDirection.PublishedByAgency : ExchangeDocumentDirection.SentByOrganisation;

                return await _batchToExchangeDocumentConverter.ConvertToAgencyExchangeDocument(
                    documentReference.ParentBatchIdentifier,
                    documentReference.BatchIdentifier,
                    fileMetadata,
                    documentDirection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(DeleteSingleDocumentReference)} for fileName: {documentReference.FileName}");
                throw;
            }
        }

        private async Task UpdateFileMetadata(string batchIdentifier, FileMetadata fileMetadata, UserInfo userInfo)
        {
            var fieldsToSet = new List<string>
            {
              nameof(FileMetadata.Deleted),
              nameof(FileMetadata.History)
            };

            fileMetadata.Deleted = true;

            var deletedHistory = CreateUserDeletedMetadataHistory(userInfo);

            fileMetadata.History = fileMetadata.History == null
                       ? new[] { deletedHistory }
                       : fileMetadata.History.Append(deletedHistory);

            await _cosmosDbService.UpdateDocumentMetadata(fileMetadata, batchIdentifier, fieldsToSet);
            _logger.LogInformation($"Document {fileMetadata.FileName} in batch {batchIdentifier} set as deleted and UserDeleted history record added.");
        }

        private FileMetadataHistory CreateUserDeletedMetadataHistory(UserInfo userInfo)
        {
            var fileMetadataUser = _mapper.Map<UserInfo, FileMetadataUser>(userInfo);
            var encryptedFileMetadataUser = _fileMetadataUserEncryptor.Encrypt(fileMetadataUser);

            return new FileMetadataHistory
            {
                ActionDateTimeUtc = _systemProvider.DateTime.UtcNow(),
                Action = FileAction.UserDeleted,
                User = encryptedFileMetadataUser
            };
        }

        private async Task DeleteDocument(string fileName)
        {
            var directory = await _directoriesManager.GetDirectory(_directoryName);
            await directory.Delete(fileName);

            _logger.LogInformation($"Document {fileName} has been deleted from directory {_directoryName}.");
        }
    }
}