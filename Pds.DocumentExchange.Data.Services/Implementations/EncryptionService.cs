using Microsoft.AspNetCore.WebUtilities;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>Class for encryption and decryption using a string cipher.</summary>
    public class EncryptionService : IEncryptionService
    {
        /// <summary>This constant is used to determine the key size of the encryption algorithm in bits.
        /// We divide this by 8 within the code below to get the equivalent number of bytes.</summary>
        private const int KeySize = 256;

        /// <summary>This constant determines the number of iterations for the password bytes generation function.</summary>
        private const int DerivationIterations = 1000;

        /// <inheritdoc />
        public string Encrypt(string plainText, string passPhrase)
        {
            if (plainText == null)
            {
                return null;
            }

            // Salt and IV is randomly generated each time, but is prepended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherTextBytes = PerformCipherProcessing(true, passPhrase, saltStringBytes, ivStringBytes, plainTextBytes);

            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
            var finalBytes = saltStringBytes.Concat(ivStringBytes).Concat(cipherTextBytes).ToArray();

            return Convert.ToBase64String(finalBytes);
        }

        /// <inheritdoc />
        public string Decrypt(string cipherText, string passPhrase)
        {
            if (cipherText == null)
            {
                return null;
            }

            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = WebEncoders.Base64UrlDecode(cipherText);

            // Get the salt bytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(KeySize / 8).ToArray();

            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(KeySize / 8).Take(KeySize / 8).ToArray();

            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((KeySize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((KeySize / 8) * 2)).ToArray();

            var finalBytes = PerformCipherProcessing(false, passPhrase, saltStringBytes, ivStringBytes, cipherTextBytes);

            return Encoding.UTF8.GetString(finalBytes);
        }

        /// <summary>Prepares the cipher processed bytes.</summary>
        /// <param name="encryptionFlag">if set to <c>true</c> it will encrypt, else decrypt.</param>
        /// <param name="passPhrase">The pass phrase.</param>
        /// <param name="saltStringBytes">The salt string bytes.</param>
        /// <param name="ivStringBytes">The iv string bytes.</param>
        /// <param name="inputBytes">The input bytes.</param>
        /// <returns>The resultant encrypted/decrypted output bytes.</returns>
        private static byte[] PerformCipherProcessing(bool encryptionFlag, string passPhrase, byte[] saltStringBytes, byte[] ivStringBytes, byte[] inputBytes)
        {
            using var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations, HashAlgorithmName.SHA1);
            var keyBytes = password.GetBytes(KeySize / 8);
            var engine = new RijndaelEngine(256);
            var blockCipher = new CbcBlockCipher(engine);
            var cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());
            var keyParam = new KeyParameter(keyBytes);
            var keyParamWithIv = new ParametersWithIV(keyParam, ivStringBytes, 0, 32);
            cipher.Init(encryptionFlag, keyParamWithIv);
            var finalBytes = cipher.DoFinal(inputBytes);
            return finalBytes;
        }

        /// <summary>Get a randomly populated array of 32 bytes.</summary>
        /// <returns>A randomly populated array of 32 bytes.</returns>
        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rng = RandomNumberGenerator.Create())
            {
                // Fill the array with cryptographically secure random bytes.
                rng.GetBytes(randomBytes);
            }

            return randomBytes;
        }
    }
}
