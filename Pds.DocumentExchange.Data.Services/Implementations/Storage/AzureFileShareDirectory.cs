using Microsoft.WindowsAzure.Storage.File;
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
    /// Represents an Azure file share directory.
    /// </summary>
    public class AzureFileShareDirectory : IDirectory
    {
        private readonly IFileNameProvider _fileNameProvider;

        private readonly CloudFileDirectory _cloudFileDirectory;
        private readonly ILoggerAdapter<AzureFileShareDirectory> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFileShareDirectory"/> class.
        /// </summary>
        /// <param name="fileShareName">The file share name.</param>
        /// <param name="directory">The directory.</param>
        /// <param name="cloudStorageAccount">The cloud storage account.</param>
        /// <param name="fileNameProvider">The file name utilities.</param>
        /// <param name="logger">The logger.</param>
        public AzureFileShareDirectory(
            string fileShareName,
            string directory,
            ICloudStorageAccount cloudStorageAccount,
            IFileNameProvider fileNameProvider,
            ILoggerAdapter<AzureFileShareDirectory> logger)
        {
            var cloudFileShare = cloudStorageAccount.GetFileShare(fileShareName);
            var cloudFileRootDirectory = cloudFileShare.GetRootDirectoryReference();

            _cloudFileDirectory = string.IsNullOrEmpty(directory)
                ? cloudFileRootDirectory
                : cloudFileRootDirectory.GetDirectoryReference(directory);

            _fileNameProvider = fileNameProvider;
            _logger = logger;
        }

        /// <inheritdoc/>
        public string Name => _cloudFileDirectory.Name;

        /// <inheritdoc/>
        public async IAsyncEnumerable<FileReferenceInfo> GetFiles()
        {
            (!await _cloudFileDirectory.ExistsAsync())
                .AsGuard<DirectoryNotFoundException>();

            FileContinuationToken continuationToken = null;

            do
            {
                var response = await _cloudFileDirectory.ListFilesAndDirectoriesSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;

                var cloudFiles = response.Results.OfType<CloudFile>();

                foreach (var currentCloudFile in cloudFiles)
                {
                    yield return new FileReferenceInfo
                    {
                        FileName = Uri.UnescapeDataString(currentCloudFile.Uri.Segments[^1]),
                    };
                }
            }
            while (continuationToken != null);
        }

        /// <inheritdoc/>
        public async Task<Stream> Read(string fileName)
        {
            try
            {
                It.IsEmpty(fileName)
                .AsGuard<ArgumentNullException>();

                var cloudFile = GetFile(fileName);

                (!await cloudFile.ExistsAsync())
                     .AsGuard<FileNotFoundException>();

                var stream = new MemoryStream();
                await cloudFile.DownloadToStreamAsync(stream);

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

                await _cloudFileDirectory.CreateIfNotExistsAsync();

                fileName = await _fileNameProvider.GetNextAvailableFileName(
                    fileName,
                    async fileName => await FileExists(fileName));

                var cloudFile = GetFile(fileName);

                stream.Seek(0, SeekOrigin.Begin);
                await cloudFile.UploadFromStreamAsync(stream);

                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(Save)} for fileName: {fileName}");
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

                var cloudFile = GetFile(fileName);

                (!await cloudFile.ExistsAsync())
                   .AsGuard<FileNotFoundException>();

                await cloudFile.DeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(Delete)} for fileName: {fileName}");
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

                try
                {
                    var cloudFile = GetFile(fileName);
                    return await cloudFile.ExistsAsync();
                }
                catch (Exception e)
                {
                    var newExceptionWithFileName = new Exception(fileName, e);
                    throw newExceptionWithFileName;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(FileExists)} for fileName: {fileName}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> Move(
           IDirectory destination,
           string fileName)
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

        private CloudFile GetFile(string fileName) => _cloudFileDirectory.GetFileReference(fileName);
    }
}