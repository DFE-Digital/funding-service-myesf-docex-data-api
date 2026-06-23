using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Storage
{
    /// <summary>
    /// Interface representing a cloud storage account.
    /// </summary>
    public interface ICloudStorageAccount
    {
        /// <summary>
        /// Gets the specified file share.
        /// </summary>
        /// <param name="fileShareName">The file share name.</param>
        /// <returns>The cloud file share.</returns>
        CloudFileShare GetFileShare(string fileShareName);

        /// <summary>
        /// Gets the specified blob container.
        /// </summary>
        /// <param name="blobContainerName">The blob container name.</param>
        /// <returns>The cloud blob container.</returns>
        CloudBlobContainer GetBlobContainer(string blobContainerName);
    }
}