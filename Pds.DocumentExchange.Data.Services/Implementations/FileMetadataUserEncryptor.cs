using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Service to encrypt and decrypt the fields of FileMetadataUser objects.
    /// </summary>
    public class FileMetadataUserEncryptor : IFileMetadataUserEncryptor
    {
        private readonly IEncryptionService _encryptionService;
        private readonly string _encryptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileMetadataUserEncryptor"/> class.
        /// </summary>
        /// <param name="encryptionService">The encryption service.</param>
        /// <param name="cosmosDbConfiguration">The cosmosDb configuration.</param>
        public FileMetadataUserEncryptor(
            IEncryptionService encryptionService,
            CosmosDbConfiguration cosmosDbConfiguration)
        {
            _encryptionService = encryptionService;
            _encryptionKey = cosmosDbConfiguration.DataEncryptionKey;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when the file metadata user is null.</exception>
        public FileMetadataUser Encrypt(FileMetadataUser fileMetadataUser)
        {
            if (fileMetadataUser == null)
            {
                throw new ArgumentNullException(nameof(fileMetadataUser), "The file metadata user cannot be null.");
            }

            var encryptedUser = ApplyFunctionToFileMetadataUser(
                fileMetadataUser,
                userField => _encryptionService.Encrypt(userField, _encryptionKey));

            encryptedUser.IsEncrypted = true;

            return encryptedUser;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when the file metadata user is null.</exception>
        public FileMetadataUser Decrypt(FileMetadataUser fileMetadataUser)
        {
            if (fileMetadataUser == null)
            {
                throw new ArgumentNullException(nameof(fileMetadataUser), "The file metadata user cannot be null.");
            }

            if (!fileMetadataUser.IsEncrypted)
            {
                return fileMetadataUser;
            }

            var decryptedUser = ApplyFunctionToFileMetadataUser(
                fileMetadataUser,
                userField => _encryptionService.Decrypt(userField, _encryptionKey));

            decryptedUser.IsEncrypted = false;

            return decryptedUser;
        }

        private FileMetadataUser ApplyFunctionToFileMetadataUser(
            FileMetadataUser fileMetadataUser,
            Func<string, string> func)
            => new FileMetadataUser
            {
                Principal = func(fileMetadataUser.Principal),
                FullName = func(fileMetadataUser.FullName),
                EmailAddress = func(fileMetadataUser.EmailAddress)
            };
    }
}