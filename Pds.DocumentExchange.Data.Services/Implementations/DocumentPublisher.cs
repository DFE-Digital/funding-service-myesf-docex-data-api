using AutoMapper;
using Pds.Core.BulkJobs.Interfaces;
using Pds.Core.BulkJobs.Models;
using Pds.Core.Logging;
using Pds.Core.Utils;
using Pds.Core.Utils.Interfaces;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Service to publish documents.
    /// </summary>
    public class DocumentPublisher : IDocumentPublisher
    {
        private const int AgencyProviderNumber = -999;

        private readonly IDirectoriesManager _directoriesManager;
        private readonly IServiceBusQueueManager _serviceBusQueueManager;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IFileNameProvider _fileNameProvider;
        private readonly IProductsLookup _productsLookup;
        private readonly IFileMetadataUserEncryptor _fileMetadataUserEncryptor;
        private readonly ISystemProvider _systemProvider;
        private readonly IMapper _mapper;
        private readonly ILoggerAdapter<DocumentPublisher> _logger;
        private readonly IAgencyService _agencyService;
        private readonly IBulkJobManager _bulkJobManager;
        private readonly IRetryMechanism _retry;

        private readonly string _agencyProcessingDirectoryKey;
        private readonly string _virusScanRequiredQueueName;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10, 10);

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPublisher"/> class.
        /// </summary>
        /// <param name="directoriesManager">The directories manager.</param>
        /// <param name="serviceBusQueueManager">The service bus manager.</param>
        /// <param name="cosmosDbService">The cosmosDb service.</param>
        /// <param name="fileNameProvider">The file name provider.</param>
        /// <param name="productsLookup">The products lookup.</param>
        /// <param name="fileMetadataUserEncryptor">The file metadata user encryptor.</param>
        /// <param name="systemProvider">The system provider.</param>
        /// <param name="mapper">The type mapper.</param>
        /// <param name="documentPublisherConfiguration">Document publisher configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="agencyService"><see cref="IAgencyService"/>.</param>
        /// <param name="bulkJobManager">The bulk job manager.</param>
        /// <param name="retry">Retry mechanism.</param>
        public DocumentPublisher(
            IDirectoriesManager directoriesManager,
            IServiceBusQueueManager serviceBusQueueManager,
            ICosmosDbService cosmosDbService,
            IFileNameProvider fileNameProvider,
            IProductsLookup productsLookup,
            IFileMetadataUserEncryptor fileMetadataUserEncryptor,
            ISystemProvider systemProvider,
            IMapper mapper,
            DocumentPublisherConfiguration documentPublisherConfiguration,
            ILoggerAdapter<DocumentPublisher> logger,
            IAgencyService agencyService,
            IBulkJobManager bulkJobManager,
            IRetryMechanism retry)
        {
            _directoriesManager = directoriesManager;
            _serviceBusQueueManager = serviceBusQueueManager;
            _cosmosDbService = cosmosDbService;
            _fileNameProvider = fileNameProvider;
            _productsLookup = productsLookup;
            _fileMetadataUserEncryptor = fileMetadataUserEncryptor;
            _systemProvider = systemProvider;
            _mapper = mapper;
            _logger = logger;
            _agencyService = agencyService;
            _bulkJobManager = bulkJobManager;
            _retry = retry;

            _agencyProcessingDirectoryKey = documentPublisherConfiguration.AgencyDestinationDirectoryKey;
            _virusScanRequiredQueueName = documentPublisherConfiguration.VirusScanRequiredQueueName;
            _retry = retry;
        }

        /// <inheritdoc/>
        public async Task<KeyValuePair<Product, int>> PublishDocuments(
            string team,
            AgencyPublishRequest agencyPublishRequest)
        {
            try
            {
                _logger.LogInformation($"Starting {nameof(PublishDocuments)} with team:{team} and product ID {agencyPublishRequest.ProductId}.");

                var product = await _productsLookup.Get(agencyPublishRequest.ProductId);

                if (product is UnknownProduct)
                {
                    throw new InvalidDataException($"Product identifier {agencyPublishRequest.ProductId} not found.");
                }

                if (!product.AgencyTeams.Contains(team))
                {
                    throw new UnauthorizedAccessException(
                        $"Agency team {team} is not authorised to publish {product.PluralName}.");
                }

                var files = await _agencyService.GetDocuments(
                    new[] { team },
                    new AgencyListDocumentOptions
                    {
                        PageNumber = 1,
                        PageSize = int.MaxValue,
                        Validity = AgencyDocumentValidity.Valid,
                        FilterOptions = new[]
                        {
                        new RadioFilterOption()
                        {
                            Key = FilterKey.ProductIdRadio.ToString(),
                            Type = nameof(RadioFilterOption),
                            Value = agencyPublishRequest.ProductId.ToString()
                        }
                        }
                    });

                if (files.Items.Any(doc => doc.Product?.Identifier != agencyPublishRequest.ProductId))
                {
                    throw new InvalidOperationException("Filter returned documents with unexpected product ID. Aborting document publish operation.");
                }

                var sourceDirectory = await _directoriesManager.GetDirectory(team);
                var destinationDirectory = await _directoriesManager.GetDirectory(_agencyProcessingDirectoryKey);

                var fileReferences = await sourceDirectory.GetFiles().ToListAsync();

                var filesToPublish = files.Items.Where(
                    doc => fileReferences.Any(fileReference => fileReference.FileName.Equals(doc.FileName)))
                    .ToList();

                var job = new Job<string, string>
                {
                    Parameters = string.Empty
                };

                _logger.LogInformation($"Starting publish of {filesToPublish.Count} documents with product ID {agencyPublishRequest.ProductId} and team:{team}.");

                await _bulkJobManager.CreateBulkJob(
                    new[] { job },
                    file => PublishDocuments(
                            sourceDirectory,
                            destinationDirectory,
                            agencyPublishRequest.UserInfo,
                            filesToPublish));

                _logger.LogInformation($"Finished {nameof(PublishDocuments)} with product ID {agencyPublishRequest.ProductId} and team:{team}.");

                return new KeyValuePair<Product, int>(product, filesToPublish.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(PublishDocuments)}  with product ID {agencyPublishRequest.ProductId} and team:{team}.");
                throw;
            }
        }

        /// <summary>
        /// Publishes a list of agency documents.
        /// </summary>
        /// <param name="sourceDirectory">The source directory.</param>
        /// <param name="destinationDirectory">The destination directory.</param>
        /// <param name="userInfo">The user info.</param>
        /// <param name="agencyDocuments">The agency documents.</param>
        /// <returns>An awaitable Task.</returns>
        public async Task<string> PublishDocuments(
            IDirectory sourceDirectory,
            IDirectory destinationDirectory,
            UserInfo userInfo,
            IEnumerable<AgencyDocument> agencyDocuments)
        {
            var parentBatchIdentifier = _systemProvider.Guid.NewGuid().ToString();

            var batchInfo =
                await MoveFilesAndCreateBatchMetadata(
                    userInfo,
                    agencyDocuments,
                    sourceDirectory,
                    destinationDirectory,
                    parentBatchIdentifier);

            foreach (var batch in batchInfo)
            {
                await SaveBatchMetadata(batch);
            }

            await SendQueueMessage(parentBatchIdentifier);

            return $"{agencyDocuments.Count()} documents published.";
        }

        private async Task<BatchMetadata[]> MoveFilesAndCreateBatchMetadata(
            UserInfo userInfo,
            IEnumerable<AgencyDocument> filesToPublish,
            IDirectory sourceDirectory,
            IDirectory destinationDirectory,
            string parentBatchIdentifier)
        {
            var fileMetadataUser = _mapper.Map<UserInfo, FileMetadataUser>(userInfo);
            var encryptedFileMetadataUser = _fileMetadataUserEncryptor.Encrypt(fileMetadataUser);
            var createdDateTimeUtc = _systemProvider.DateTime.UtcNow();

            var filesInfoTasks = filesToPublish.Select(
                async fileReference =>
                {
                    try
                    {
                        await _semaphore.WaitAsync();

                        var savedFileName = await sourceDirectory.Copy(destinationDirectory, fileReference.FileName);

                        try
                        {
                            await _retry.TryUntilSuccessOrThrow(
                                async () =>
                                {
                                    await sourceDirectory.Delete(fileReference.FileName);
                                    return 0;
                                }, 5);
                        }
                        catch (Exception e)
                        {
                            _logger.LogWarning(e, e.Message);
                        }

                        _logger.LogInformation($"Moved file {fileReference.FileName} into {destinationDirectory} ready for publishing.");

                        var fileMetadata = CreateFileMetadata(
                            fileReference.FileName,
                            savedFileName,
                            _fileNameProvider.GetComponents(fileReference.FileName),
                            encryptedFileMetadataUser,
                            createdDateTimeUtc);

                        return CreateBatchMetadata(
                            fileMetadata,
                            encryptedFileMetadataUser,
                            createdDateTimeUtc,
                            parentBatchIdentifier);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Error moving {fileReference.FileName} into {destinationDirectory}.");
                        throw;
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                });

            var batches = await Task.WhenAll(filesInfoTasks);
            _logger.LogInformation($"{batches.Length} files moved ready for publishing.");

            return batches;
        }

        private FileMetadata CreateFileMetadata(
            string originalFileName,
            string generatedFileName,
            FileNameComponents fileNameComponents,
            FileMetadataUser fileMetadataUser,
            DateTime createdDateTimeUtc)
        {
            return new FileMetadata
            {
                FileName = generatedFileName,
                ProductIdentifier = fileNameComponents.ProductIdentifier,
                Metadata = new Dictionary<string, string> { ["Year"] = fileNameComponents.AcademicYear?.ToString() },
                OriginalFileName = originalFileName,
                FromUkprn = AgencyProviderNumber,
                ToUkprn = fileNameComponents.OrganisationIdentifier,
                History = new[]
                {
                    new FileMetadataHistory
                    {
                        ActionDateTimeUtc = createdDateTimeUtc,
                        Action = FileAction.Published,
                        User = fileMetadataUser
                    }
                }
            };
        }

        private BatchMetadata CreateBatchMetadata(
            FileMetadata filesInfo,
            FileMetadataUser fileMetadataUser,
            DateTime createdDateTimeUtc,
            string parentBatchIdentifier)
        {
            var batchIdentifier = _systemProvider.Guid.NewGuid().ToString();

            return new BatchMetadata
            {
                Id = batchIdentifier,
                CreatedDate = createdDateTimeUtc,
                UploadedBy = fileMetadataUser,
                Files = new[] { filesInfo },
                ParentBatchIdentifier = parentBatchIdentifier
            };
        }

        private async Task SaveBatchMetadata(BatchMetadata batchMetadata)
        {
            await _cosmosDbService.AddBatch(batchMetadata);

            _logger.LogInformation($"Batch {batchMetadata.Id} metadata saved in database.");
        }

        private async Task SendQueueMessage(string parentBatchIdentifier)
        {
            var virusScanRequiredQueue = _serviceBusQueueManager.GetQueue(_virusScanRequiredQueueName);

            var queueMessage = new VirusScanRequestMessage
            {
                ParentBatchIdentifier = parentBatchIdentifier,
                DocumentDirection = ExchangeDocumentDirection.PublishedByAgency
            };

            await virusScanRequiredQueue.PushMessage(queueMessage);

            _logger.LogInformation($"Batch {parentBatchIdentifier} sent to queue {_virusScanRequiredQueueName}");
        }
    }
}