using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Storage
{
    /// <summary>
    /// The populator Azure File Share.
    /// </summary>
    public class AzureFileShareDirectory
    {
        private readonly ShareDirectoryClient _shareDirectoryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFileShareDirectory"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public AzureFileShareDirectory(Configuration configuration)
        {
            var shareClient = new ShareClient(configuration.ConnectionString, configuration.ShareName);

            if (string.IsNullOrEmpty(configuration.DirectoryName))
            {
                _shareDirectoryClient = shareClient.GetRootDirectoryClient();
            }
            else
            {
                _shareDirectoryClient = shareClient.GetDirectoryClient(configuration.DirectoryName);
            }
        }

        /// <summary>
        /// Creates a file.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="content">The file content.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task CreateFile(string fileName, byte[] content)
        {
            var newFileClient = _shareDirectoryClient.GetFileClient(fileName);

            using var newFileStream = await newFileClient.OpenWriteAsync(true, 0, new ShareFileOpenWriteOptions { MaxSize = content.Length });
            using var writer = new BinaryWriter(newFileStream);
            writer.Write(content);
        }

        /// <summary>
        /// Gets the file names.
        /// </summary>
        /// <returns>The file names collection.</returns>
        public IEnumerable<string> GetFileNames()
        {
            var filesAndDirectories = _shareDirectoryClient.GetFilesAndDirectories();

            return filesAndDirectories.Where(file => !file.IsDirectory).Select(file => file.Name);
        }

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task DeleteFile(string fileName)
        {
            var fileRef = _shareDirectoryClient.GetFileClient(fileName);
            await fileRef.DeleteAsync();
        }

        /// <summary>
        /// The file share configuration.
        /// </summary>
        public class Configuration
        {
            /// <summary>
            /// Gets or sets the connection string.
            /// </summary>
            public string ConnectionString { get; set; }

            /// <summary>
            /// Gets or sets the share name.
            /// </summary>
            public string ShareName { get; set; }

            /// <summary>
            /// Gets or sets the directory name.
            /// </summary>
            public string DirectoryName { get; set; }
        }
    }
}