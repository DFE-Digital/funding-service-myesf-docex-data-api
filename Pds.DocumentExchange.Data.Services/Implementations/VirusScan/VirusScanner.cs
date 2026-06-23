using Newtonsoft.Json;
using Pds.Core.Logging;
using Pds.Core.Utils;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.VirusScan
{
    /// <summary>
    /// Service to scan files and get the scan result and the updated file metadata.
    /// </summary>
    public class VirusScanner : IVirusScanner
    {
        private readonly IDirectoriesManager _directoriesManager;
        private readonly IAntivirus _antivirus;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly ISystemProvider _systemProvider;
        private readonly VirusScannerConfiguration _virusScannerConfiguration;
        private readonly ILoggerAdapter<VirusScanner> _logger;


        /// <summary>
        /// Initializes a new instance of the <see cref="VirusScanner"/> class.
        /// </summary>
        /// <param name="directoriesManager">The directories manager.</param>
        /// <param name="antivirus">The antivirus.</param>
        /// <param name="cosmosDbService">The cosmosDb service.</param>
        /// <param name="systemProvider">The system provider.</param>
        /// <param name="virusScannerConfiguration">The virusScanner Configuration.</param>
        /// <param name="logger">The logger.</param>
        public VirusScanner(
            IDirectoriesManager directoriesManager,
            IAntivirus antivirus,
            ICosmosDbService cosmosDbService,
            ISystemProvider systemProvider,
            VirusScannerConfiguration virusScannerConfiguration,
            ILoggerAdapter<VirusScanner> logger)
        {
            _directoriesManager = directoriesManager;
            _antivirus = antivirus;
            _cosmosDbService = cosmosDbService;
            _systemProvider = systemProvider;
            _virusScannerConfiguration = virusScannerConfiguration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ScannedFileInfo> ScanFile(
            FileMetadata fileInfo,
            string batchIdentifier,
            string parentBatchIdentifier,
            ExchangeDocumentDirection documentDirection)
        {
            string directoryKey = documentDirection == ExchangeDocumentDirection.SentByOrganisation
                ? _virusScannerConfiguration.OrganisationDirectoryKey
                : _virusScannerConfiguration.AgencyDirectoryKey;

            var directory = await _directoriesManager.GetDirectory(directoryKey);

            var scanResult = await ScanFile(fileInfo, batchIdentifier, directory);

            return new ScannedFileInfo
            {
                BatchIdentifier = batchIdentifier,
                ParentBatchIdentifier = parentBatchIdentifier,
                FileInfo = fileInfo,
                ScanResult = scanResult,
                DocumentDirection = documentDirection
            };
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ScannedFileInfo> ScanFiles(IEnumerable<FileMetadata> filesInfo, string batchIdentifier, string parentBatchIdentifier, ExchangeDocumentDirection documentDirection)
        {
            foreach (var currentFileInfo in filesInfo)
            {
                yield return await ScanFile(currentFileInfo, batchIdentifier, parentBatchIdentifier, documentDirection);
            }
        }

        private async Task<FileSafety> ScanFile(FileMetadata fileInfo, string batchIdentifier, IDirectory directory)
        {
            _logger.LogInformation($"Start scanning file {fileInfo.FileName}");

            if (fileInfo.Processed || fileInfo.History?.Any(h => h.Action == FileAction.FileScanBad || h.Action == FileAction.FileScanGood) == true)
            {
                _logger.LogInformation($"File {fileInfo.FileName} has already been scanned.");
                return FileSafety.AlreadyScanned;
            }

            try
            {
                FileSafety result;

                using (var fileStream = await directory.Read(fileInfo.FileName))
                {
                    if (fileStream == null)
                    {
                        fileInfo.Processed = true;
                        result = FileSafety.InternalError;

                        await _cosmosDbService.UpdateDocumentMetadata(fileInfo, batchIdentifier, new[] { "Processed" });

                        _logger.LogInformation($"File {fileInfo.FileName} not found in directory {directory.Name}.");
                    }
                    else
                    {
                        var scanResult = await _antivirus.Scan(fileInfo.FileName, fileStream);
                        await AddScannedFileHistory(batchIdentifier, fileInfo, scanResult);

                        result = scanResult.FileSafety;

                        _logger.LogInformation($"File {fileInfo.FileName} scanned with result {result}");
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred when scanning file {fileInfo.FileName}");
                return FileSafety.InternalError;
            }
        }

        private async Task AddScannedFileHistory(string batchIdentifier, FileMetadata fileInfo, ScanResult scanResult)
        {
            var fileScannedHistory = new FileMetadataHistory
            {
                ActionDateTimeUtc = _systemProvider.DateTime.UtcNow(),
                Action = FileAction.FileScanned,
                Message = JsonConvert.SerializeObject(scanResult)
            };

            fileInfo.History = new FileMetadataHistory[] { fileScannedHistory };

            await _cosmosDbService.AddHistoryToFileMetadata(batchIdentifier, fileInfo);
        }
    }
}