using Pds.DocumentExchange.Data.Services.DTOs;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Provides methods to encrypt and decrypt the fields of FileMetadataUser objects.
    /// </summary>
    public interface IFileMetadataUserEncryptor
    {
        /// <summary>
        /// Decrypts fields of a FileMetadataUser object.
        /// </summary>
        /// <param name="fileMetadataUser">The FileMetadataUser object.</param>
        /// <returns>A decrypted FileMetadataUser.</returns>
        FileMetadataUser Decrypt(FileMetadataUser fileMetadataUser);

        /// <summary>
        /// Encrypts fields of a FileMetadataUser object.
        /// </summary>
        /// <param name="fileMetadataUser">The FileMetadataUser object.</param>
        /// <returns>An encrypted FileMetadataUser.</returns>
        FileMetadataUser Encrypt(FileMetadataUser fileMetadataUser);
    }
}