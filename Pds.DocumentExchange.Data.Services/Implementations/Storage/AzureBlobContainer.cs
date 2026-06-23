using Microsoft.WindowsAzure.Storage.Blob;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Storage
{
    /// <summary>
    /// Represents an Azure blob container.
    /// </summary>
    public class AzureBlobContainer : IDirectory
    {
        private readonly IFileNameProvider _fileNameProvider;
        private readonly CloudBlobContainer _cloudBlobContainer;
        private readonly ILoggerAdapter<AzureBlobContainer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobContainer"/> class.
        /// </summary>
        /// <param name="blobContainerName">The container name.</param>
        /// <param name="cloudStorageAccount">The cloud storage account.</param>
        /// <param name="fileNameProvider">The file name utilities.</param>
        /// <param name="logger">The logger.</param>
        public AzureBlobContainer(
            string blobContainerName,
            ICloudStorageAccount cloudStorageAccount,
            IFileNameProvider fileNameProvider,
            ILoggerAdapter<AzureBlobContainer> logger)
        {
            _cloudBlobContainer = cloudStorageAccount.GetBlobContainer(blobContainerName);
            _fileNameProvider = fileNameProvider;
            _logger = logger;
        }

        /// <inheritdoc/>
        public string Name => _cloudBlobContainer.Name;

        /// <inheritdoc/>
        public async IAsyncEnumerable<FileReferenceInfo> GetFiles()
        {
            BlobContinuationToken continuationToken = null;

            do
            {
                var response = await _cloudBlobContainer.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;

                var blockBlobs = response.Results.OfType<CloudBlockBlob>();

                foreach (var currentBlobItem in blockBlobs)
                {
                    yield return new FileReferenceInfo
                    {
                        FileName = Uri.UnescapeDataString(currentBlobItem.Uri.Segments[^1]),
                    };
                }
            }
            while (continuationToken != null);
        }

        /// <inheritdoc/>
        public async Task<string> Copy(IDirectory destination, string fileName)
        {
            try
            {
                It.IsNull(destination)
               .AsGuard<ArgumentNullException>();

                It.IsEmpty(fileName)
                    .AsGuard<ArgumentNullException>();

                using (var stream = await Read(fileName))
                {
                    return await destination.Save(stream, fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(Copy)} for fileName: {fileName} and destination:{destination?.Name}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(string fileName)
        {
            try
            {
                It.IsEmpty(fileName)
              .AsGuard<ArgumentNullException>();

                var blob = _cloudBlobContainer.GetBlockBlobReference(fileName);

                (!await blob.ExistsAsync())
                   .AsGuard<FileNotFoundException>();

                await blob.DeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(Delete)} for fileName: {fileName}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> Move(IDirectory destination, string fileName)
        {
            try
            {
                It.IsNull(destination)
               .AsGuard<ArgumentNullException>();

                It.IsEmpty(fileName)
                     .AsGuard<ArgumentNullException>();

                var savedFileName = await Copy(destination, fileName);
                await Delete(fileName);

                return savedFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(Move)} for fileName: {fileName} and destination:{destination?.Name}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Stream> Read(string fileName)
        {
            try
            {
                It.IsEmpty(fileName)
                 .AsGuard<ArgumentNullException>();

                var blob = _cloudBlobContainer.GetBlockBlobReference(fileName);

                (!await blob.ExistsAsync())
                    .AsGuard<FileNotFoundException>();

                var stream = new MemoryStream();
                await blob.DownloadToStreamAsync(stream);

                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(Read)} for fileName: {fileName}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> Save(Stream stream, string fileName)
        {
            try
            {
                It.IsNull(stream)
                 .AsGuard<ArgumentNullException>();

                It.IsEmpty(fileName)
                       .AsGuard<ArgumentNullException>();

                fileName = await _fileNameProvider.GetNextAvailableFileName(
                    fileName,
                    async fileName => await FileExists(fileName));

                var cloudBlob = _cloudBlobContainer.GetBlockBlobReference(fileName);

                stream.Seek(0, SeekOrigin.Begin);
                await cloudBlob.UploadFromStreamAsync(stream);

                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(Save)} for fileName: {fileName}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> FileExists(string fileName)
        {
            try
            {
                It.IsEmpty(fileName)
                 .AsGuard<ArgumentNullException>();

                var blob = _cloudBlobContainer.GetBlockBlobReference(fileName);
                return await blob.ExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(FileExists)} for fileName: {fileName}");
                throw;
            }
        }
    }
}