using MapsterMapper;
using Pds.Core.Logging;
using Pds.Core.Utils;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Service to upload documents.
    /// </summary>
    public class DocumentUploader : IDocumentUploader
    {
        private const int AgencyProviderNumber = -999;

        private readonly IDirectoriesManager _directoriesManager;
        private readonly IServiceBusQueueManager _serviceBusQueueManager;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IFileNameProvider _fileNameProvider;
        private readonly IFileMetadataUserEncryptor _fileMetadataUserEncryptor;
        private readonly IAcademicYearCalculator _academicYearCalculator;
        private readonly ISystemProvider _systemProvider;
        private readonly IMapper _mapper;
        private readonly ILoggerAdapter<DocumentUploader> _logger;

        private readonly string _uploadDirectoryKey;
        private readonly string _virusScanRequiredQueueName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentUploader"/> class.
        /// </summary>
        /// <param name="directoriesManager">The directories manager.</param>
        /// <param name="serviceBusQueueManager">The service bus queue.</param>
        /// <param name="cosmosDbService">The CosmosDb service.</param>
        /// <param name="fileNameProvider">The file name utilities.</param>
        /// <param name="fileMetadataUserEncryptor">The encryption service.</param>
        /// <param name="systemProvider">The system utilities.</param>
        /// <param name="academicYearCalculator">The academic year calculator.</param>
        /// <param name="mapper">The type mapper.</param>
        /// <param name="documentUploaderConfiguration">The document uploading configuration.</param>
        /// <param name="logger">The logger.</param>
        public DocumentUploader(
            IDirectoriesManager directoriesManager,
            IServiceBusQueueManager serviceBusQueueManager,
            ICosmosDbService cosmosDbService,
            IFileNameProvider fileNameProvider,
            IFileMetadataUserEncryptor fileMetadataUserEncryptor,
            IAcademicYearCalculator academicYearCalculator,
            ISystemProvider systemProvider,
            IMapper mapper,
            DocumentUploaderConfiguration documentUploaderConfiguration,
            ILoggerAdapter<DocumentUploader> logger)
        {
            _directoriesManager = directoriesManager;
            _serviceBusQueueManager = serviceBusQueueManager;
            _cosmosDbService = cosmosDbService;
            _fileNameProvider = fileNameProvider;
            _academicYearCalculator = academicYearCalculator;
            _systemProvider = systemProvider;
            _mapper = mapper;
            _fileMetadataUserEncryptor = fileMetadataUserEncryptor;
            _logger = logger;

            _uploadDirectoryKey = documentUploaderConfiguration.UploadDirectoryKey;
            _virusScanRequiredQueueName = documentUploaderConfiguration.VirusScanRequiredQueueName;
        }

        /// <inheritdoc/>
        public async Task UploadDocument(UploadDocumentRequest uploadDocumentRequest)
        {
            var academicYear = _academicYearCalculator.GetCurrentAcademicYear().ToString();

            var savedFileName = await SaveDocument(uploadDocumentRequest, academicYear);

            var batchMetadata = CreateBatchMetadataObject(uploadDocumentRequest, savedFileName, academicYear);

            await SaveBatchMetadata(batchMetadata);

            await SendQueueMessage(batchMetadata.ParentBatchIdentifier);
        }

        private async Task<string> SaveDocument(UploadDocumentRequest uploadDocumentRequest, string academicYear)
        {
            _logger.LogInformation($"Generate filename using FromOrganisation.Value: [{uploadDocumentRequest.FromOrganisation.Value}]");
            _logger.LogInformation($"Generate filename using ProductIdentifier: [{uploadDocumentRequest.ProductIdentifier}]");
            _logger.LogInformation($"Generate filename using academicYear: [{academicYear}]");
            _logger.LogInformation($"Generate filename using uploadDocumentRequest.FileName: [{uploadDocumentRequest.FileName}]");

            var generatedFileName = _fileNameProvider.GenerateFileName(
                uploadDocumentRequest.FromOrganisation.Value,
                uploadDocumentRequest.ProductIdentifier,
                academicYear,
                uploadDocumentRequest.FileName);

            _logger.LogInformation($"GeneratedFileName is [{generatedFileName}] based on original file [{uploadDocumentRequest.FileName}]");

            var uploadedFileName = await SaveDocument(generatedFileName, uploadDocumentRequest.Bytes);
            return uploadedFileName;
        }

        private async Task<string> SaveDocument(string fileName, byte[] fileBytes)
        {
            using (var fileStream = new MemoryStream(fileBytes))
            {
                var uploadDirectory = await _directoriesManager.GetDirectory(_uploadDirectoryKey);
                string savedFileName = await uploadDirectory.Save(new MemoryStream(fileBytes), fileName);

                _logger.LogInformation($"File [{fileName}] saved in [{uploadDirectory.Name}] directory. SavedFileName in fileShare [{savedFileName}]");

                return savedFileName;
            }
        }

        private async Task SaveBatchMetadata(BatchMetadata batchMetadata)
        {
            await _cosmosDbService.AddBatch(batchMetadata);

            _logger.LogInformation($"Batch {batchMetadata.Id} metadata saved in database.");
        }

        private BatchMetadata CreateBatchMetadataObject(
            UploadDocumentRequest uploadDocumentRequest,
            string generatedFileName,
            string academicYear)
        {
            var utcNow = _systemProvider.DateTime.Now();
            var batchIdentifier = _systemProvider.Guid.NewGuid().ToString();

            var fileMetadataUser = _mapper.Map<UserInfo, FileMetadataUser>(uploadDocumentRequest.UserInfo);
            var encryptedFileMetadataUser = _fileMetadataUserEncryptor.Encrypt(fileMetadataUser);

            var fileInfo = new FileMetadata
            {
                FileName = generatedFileName,
                ProductIdentifier = uploadDocumentRequest.ProductIdentifier.ToString(),
                Metadata = new Dictionary<string, string> { ["Year"] = academicYear },
                OriginalFileName = uploadDocumentRequest.FileName,
                FromUkprn = int.Parse(uploadDocumentRequest.FromOrganisation.Value),
                ToUkprn = AgencyProviderNumber,
                History = new[]
                {
                    new FileMetadataHistory
                    {
                        ActionDateTimeUtc = utcNow,
                        Action = FileAction.UploadedExternal,
                        User = encryptedFileMetadataUser
                    }
                }
            };

            return new BatchMetadata
            {
                Id = batchIdentifier,
                CreatedDate = utcNow,
                UploadedBy = encryptedFileMetadataUser,
                Files = new[] { fileInfo },
                ParentBatchIdentifier = batchIdentifier
            };
        }

        private async Task SendQueueMessage(string parentBatchIdentifier)
        {
            var queueMessage = new VirusScanRequestMessage
            {
                ParentBatchIdentifier = parentBatchIdentifier,
                DocumentDirection = ExchangeDocumentDirection.SentByOrganisation
            };

            var virusScanRequiredQueue = _serviceBusQueueManager.GetQueue(_virusScanRequiredQueueName);
            await virusScanRequiredQueue.PushMessage(queueMessage);

            _logger.LogInformation($"Batch {parentBatchIdentifier} sent to queue {virusScanRequiredQueue.QueueName}");
        }
    }
}