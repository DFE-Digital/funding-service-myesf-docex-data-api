using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;

namespace Pds.DocumentExchange.Data.Services.Implementations.Storage
{
    /// <summary>
    /// Represents an Azure cloud storage account.
    /// </summary>
    public class AzureCloudStorageAccount : ICloudStorageAccount
    {
        private readonly CloudStorageAccount _cloudStorageAccount;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCloudStorageAccount"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public AzureCloudStorageAccount(string connectionString)
        {
            _cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
        }

        /// <inheritdoc/>
        public CloudFileShare GetFileShare(string fileShareName)
        {
            var cloudFileClient = _cloudStorageAccount.CreateCloudFileClient();
            return cloudFileClient.GetShareReference(fileShareName);
        }

        /// <inheritdoc/>
        public CloudBlobContainer GetBlobContainer(string blobContainerName)
        {
            var cloudBlobClient = _cloudStorageAccount.CreateCloudBlobClient();
            return cloudBlobClient.GetContainerReference(blobContainerName);
        }
    }
}