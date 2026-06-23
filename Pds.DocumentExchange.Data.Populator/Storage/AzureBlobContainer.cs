using Azure.Storage.Blobs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Storage
{
    /// <summary>
    /// The populator Azure Blob container.
    /// </summary>
    public class AzureBlobContainer
    {
        private readonly BlobContainerClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobContainer"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public AzureBlobContainer(Configuration configuration)
        {
            var blobServiceClient = new BlobServiceClient(configuration.ConnectionString);
            _client = blobServiceClient.GetBlobContainerClient(configuration.ContainerName);
        }

        /// <summary>
        /// Creates a file.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="content">The file content.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task CreateFile(string fileName, byte[] content)
        {
            var blobClient = _client.GetBlobClient(fileName);
            await blobClient.UploadAsync(new MemoryStream(content));
        }

        /// <summary>
        /// Gets the file names.
        /// </summary>
        /// <returns>The file names collection.</returns>
        public IEnumerable<string> GetFileNames()
        {
            var blobs = _client.GetBlobs();
            return blobs.Select(blob => blob.Name);
        }

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task DeleteFile(string fileName)
        {
            await _client.DeleteBlobAsync(fileName);
        }

        /// <summary>
        /// The blob container configuration.
        /// </summary>
        public class Configuration
        {
            /// <summary>
            /// Gets or sets the connection string.
            /// </summary>
            public string ConnectionString { get; set; }

            /// <summary>
            /// Gets or sets the container name.
            /// </summary>
            public string ContainerName { get; set; }
        }
    }
}