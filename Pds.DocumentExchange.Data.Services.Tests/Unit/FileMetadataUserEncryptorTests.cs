using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class FileMetadataUserEncryptorTests
    {
        private readonly Mock<IEncryptionService> _encryptionService = new Mock<IEncryptionService>(MockBehavior.Strict);

        private readonly CosmosDbConfiguration _config = new CosmosDbConfiguration
        {
            DataEncryptionKey = "the-data-encryption-key"
        };

        private readonly FileMetadataUserEncryptor _fileMetadataUserEncryptor;

        public FileMetadataUserEncryptorTests()
        {
            _fileMetadataUserEncryptor = new FileMetadataUserEncryptor(
                _encryptionService.Object,
                _config);
        }

        [TestMethod, TestCategory("Unit")]
        public void Encrypt_WhenUserIsNull_ThrowArgumentNullException()
        {
            // Act
            Func<FileMetadataUser> func = () => _fileMetadataUserEncryptor.Encrypt(null);

            // Assert
            func.Should().Throw<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void Encrypt_WhenUserIsValid_ReturnsEncryptedUser()
        {
            // Arrange
            var user = new FileMetadataUser
            {
                Principal = "the-principal",
                FullName = "the-full-name",
                EmailAddress = "the-email-address",
                IsEncrypted = false
            };

            var encryptedUser = new FileMetadataUser
            {
                Principal = "the-principal-encrypted",
                FullName = "the-full-name-encrypted",
                EmailAddress = "the-email-address-encrypted",
                IsEncrypted = true
            };

            _encryptionService
                .Setup(e => e.Encrypt(user.Principal, _config.DataEncryptionKey))
                .Returns(encryptedUser.Principal);

            _encryptionService
                .Setup(e => e.Encrypt(user.FullName, _config.DataEncryptionKey))
                .Returns(encryptedUser.FullName);

            _encryptionService
                .Setup(e => e.Encrypt(user.EmailAddress, _config.DataEncryptionKey))
                .Returns(encryptedUser.EmailAddress);

            // Act
            var result = _fileMetadataUserEncryptor.Encrypt(user);

            // Assert
            result.Should().BeEquivalentTo(encryptedUser);
        }

        [TestMethod, TestCategory("Unit")]
        public void Decrypt_WhenUserIsNull_ThrowArgumentNullException()
        {
            // Act
            Func<FileMetadataUser> func = () => _fileMetadataUserEncryptor.Decrypt(null);

            // Assert
            func.Should().Throw<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void Decrypt_WhenUserIsEncrypted_ReturnsDecryptedUser()
        {
            // Arrange
            var encryptedUser = new FileMetadataUser
            {
                Principal = "the-principal-encrypted",
                FullName = "the-full-name-encrypted",
                EmailAddress = "the-email-address-encrypted",
                IsEncrypted = true
            };

            var decryptedUser = new FileMetadataUser
            {
                Principal = "the-principal-decrypted",
                FullName = "the-full-name-decrypted",
                EmailAddress = "the-email-address-decrypted",
                IsEncrypted = false
            };

            _encryptionService
                .Setup(e => e.Decrypt(encryptedUser.Principal, _config.DataEncryptionKey))
                .Returns(decryptedUser.Principal);

            _encryptionService
                .Setup(e => e.Decrypt(encryptedUser.FullName, _config.DataEncryptionKey))
                .Returns(decryptedUser.FullName);

            _encryptionService
                .Setup(e => e.Decrypt(encryptedUser.EmailAddress, _config.DataEncryptionKey))
                .Returns(decryptedUser.EmailAddress);

            // Act
            var result = _fileMetadataUserEncryptor.Decrypt(encryptedUser);

            // Assert
            result.Should().BeEquivalentTo(decryptedUser);
        }

        [TestMethod, TestCategory("Unit")]
        public void Decrypt_WhenUserIsNotEncrypted_ReturnsUser()
        {
            // Arrange
            var nonEncryptedUser = new FileMetadataUser
            {
                Principal = "the-principal",
                FullName = "the-full-name",
                EmailAddress = "the-email-address",
                IsEncrypted = false
            };

            // Act
            var result = _fileMetadataUserEncryptor.Decrypt(nonEncryptedUser);

            // Assert
            result.Should().BeEquivalentTo(nonEncryptedUser);
        }
    }
}