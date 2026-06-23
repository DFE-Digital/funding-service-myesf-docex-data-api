using AutoMapper;
using Pds.Core.Logging;
using Pds.Core.Utils;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Extensions;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <inheritdoc/>
    public class DocumentDownloader : IDocumentDownloader
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IDirectoriesManager _directoriesManager;
        private readonly IFileMetadataUserEncryptor _fileMetadataUserEncryptor;
        private readonly ISystemProvider _systemProvider;
        private readonly IMapper _mapper;
        private readonly IZipService _zipService;
        private readonly DocumentDownloaderConfiguration _configuration;
        private readonly ILoggerAdapter<DocumentDownloader> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDownloader"/> class.
        /// </summary>
        /// <param name="directoriesManager">The directories manager.</param>
        /// <param name="cosmosDbService">The CosmosDb service.</param>
        /// <param name="fileMetadataUserEncryptor">The encryption service.</param>
        /// <param name="systemProvider">The system provider.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The type mapper.</param>
        /// <param name="zipService">The zip service.</param>
        /// <param name="cosmosDbConfiguration">The cosmos database encryption configuration.</param>
        /// <param name="logger">The logger.</param>
        public DocumentDownloader(
            ICosmosDbService cosmosDbService,
            IDirectoriesManager directoriesManager,
            IFileMetadataUserEncryptor fileMetadataUserEncryptor,
            ISystemProvider systemProvider,
            IMapper mapper,
            IZipService zipService,
            DocumentDownloaderConfiguration configuration,
            ILoggerAdapter<DocumentDownloader> logger)
        {
            _cosmosDbService = cosmosDbService;
            _directoriesManager = directoriesManager;
            _fileMetadataUserEncryptor = fileMetadataUserEncryptor;
            _systemProvider = systemProvider;
            _mapper = mapper;
            _zipService = zipService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<byte[]> DownloadExchangeDocument(
            ExchangeDocumentDownloadRequest exchangeDocumentDownloadRequest,
            bool zipSingleDocument = false)
        {
            var documentReferences = exchangeDocumentDownloadRequest.ListOptions.DocumentReferences;
            var userInfo = exchangeDocumentDownloadRequest.UserInfo;

            var documentList = documentReferences.ToList();
            if (!documentList.Any())
            {
                _logger.LogError("Empty request received, cannot process further.");
                return null;
            }

            var fileMetadataUser = _mapper.Map<UserInfo, FileMetadataUser>(userInfo);
            var encryptedFileMetadataUser = _fileMetadataUserEncryptor.Encrypt(fileMetadataUser);

            // Check if multiple document or single.
            var numberOfDocsToProcess = documentList.Count;

            if (numberOfDocsToProcess == 1 && !zipSingleDocument)
            {
                _logger.LogInformation("Downloading single document.");

                return await DownloadSingleDocument(
                    encryptedFileMetadataUser,
                    userInfo,
                    documentList.First());
            }

            // More than one document is requested.
            _logger.LogInformation("Zipping one or more documents.");

            var files = await LoadFiles(documentList, encryptedFileMetadataUser, userInfo);
            return _zipService.ZipFiles(files);
        }

        /// <inheritdoc/>
        public async Task<byte[]> DownloadAgencyDocument(string team, string fileName)
        {
            return await GetFileBytes(fileName, team);
        }

        private async Task<IEnumerable<(string fileName, byte[] fileContent)>> LoadFiles(
            List<DocumentReference> documentReferences,
            FileMetadataUser fileMetadataUser,
            UserInfo userInfo)
        {
            var documents = new List<(string, byte[])>();

            foreach (var currentDocumentReference in documentReferences)
            {
                var document = await DownloadSingleDocument(fileMetadataUser, userInfo, currentDocumentReference);
                documents.Add((currentDocumentReference.FileName, document));
            }

            return documents;
        }

        private async Task<byte[]> DownloadSingleDocument(
            FileMetadataUser fileMetadataUser,
            UserInfo userInfo,
            DocumentReference docRef)
        {
            _logger.LogInformation($"Retrieving file {docRef.FileName} Parent batch id: {docRef.ParentBatchIdentifier} - Batch id: {docRef.BatchIdentifier}.");

            var fileMetadata = await _cosmosDbService.GetFile(docRef);

            if (fileMetadata == null)
            {
                _logger.LogError($"File: {docRef.FileName} not found.");
                return null;
            }

            var fileBytes = await GetFileBytes(fileMetadata.FileName, _configuration.DownloadDirectoryKey);
            await UpdateDownloadedFileMetadata(docRef.BatchIdentifier, fileMetadata, fileMetadataUser, userInfo);

            return fileBytes;
        }

        private async Task<byte[]> GetFileBytes(string fileName, string directoryKey)
        {
            var directory = await _directoriesManager.GetDirectory(directoryKey);
            var document = await directory.Read(fileName);

            return document.ToByteArray();
        }

        private async Task UpdateDownloadedFileMetadata(
            string batchIdentifier,
            FileMetadata fileMetadata,
            FileMetadataUser fileMetadataUser,
            UserInfo userInfo)
        {
            var organisationIdentifier = userInfo.OrganisationInfo.OrganisationIdentifier.Value;
            var isUserInternal = string.IsNullOrWhiteSpace(organisationIdentifier);

            // Mark it as viewed
            var additionalHistory = new[]
            {
                new FileMetadataHistory
                {
                    Action = GetFileViewActionByUserInfo(fileMetadata.FromUkprn, userInfo),
                    ActionDateTimeUtc = _systemProvider.DateTime.UtcNow(),
                    User = fileMetadataUser
                }
            };

            var fieldsToSet = new List<string>
            {
                "History"
            };

            // Is internal user and not yet viewed by the ESFA
            if (isUserInternal && fileMetadata.History?.Any(x => x.Action == FileAction.ViewedByReceiver) != true)
            {
                fileMetadata.ExpiresAtDateTime = _systemProvider.DateTime.UtcNow().AddHours(_configuration.ExpiresInHours);
                fieldsToSet.Add("ExpiresAt");
            }

            fileMetadata.History = fileMetadata.History == null
                            ? additionalHistory
                            : fileMetadata.History.Concat(additionalHistory);

            await _cosmosDbService.UpdateDocumentMetadata(
                fileMetadata,
                batchIdentifier,
                fieldsToSet,
                null);
        }

        private FileAction GetFileViewActionByUserInfo(int fromUkprn, UserInfo userInfo)
        {
            var organisationIdentifier = userInfo.OrganisationInfo.OrganisationIdentifier.Value;

            if (fromUkprn == Convert.ToInt32(organisationIdentifier))
            {
                return userInfo.IsViewAsProvider
                    ? FileAction.ViewAsProviderDownloadedSent
                    : FileAction.ViewedBySender;
            }
            else
            {
                return userInfo.IsViewAsProvider
                    ? FileAction.ViewAsProviderDownloadedReceived
                    : FileAction.ViewedByReceiver;
            }
        }
    }
}