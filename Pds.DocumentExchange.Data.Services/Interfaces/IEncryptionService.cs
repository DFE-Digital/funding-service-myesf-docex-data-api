namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>The IEncryptionService interface that provides methods for encryption and decryption.</summary>
    public interface IEncryptionService
    {
        /// <summary>Encrypts a string.</summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="passPhrase">The pass phrase to use during encryption.</param>
        /// <returns>The encrypted string in Base 64 format.</returns>
        string Encrypt(string plainText, string passPhrase);

        /// <summary>Decrypts the specified cipher text.</summary>
        /// <param name="cipherText">The cipher text in Base 64 format.</param>
        /// <param name="passPhrase">The pass phrase to use for decryption. Must match the value that was used during encryption.</param>
        /// <returns>The decrypted text.</returns>
        string Decrypt(string cipherText, string passPhrase);
    }
}