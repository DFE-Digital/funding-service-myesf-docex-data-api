using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.File;
using Moq;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.Implementations.Storage;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Storage
{
    [TestClass]
    public class AzureFileShareDirectoryTests
    {
        private readonly Mock<ICloudStorageAccount> _cloudStorageAccount = new Mock<ICloudStorageAccount>(MockBehavior.Strict);
        private readonly Mock<IFileNameProvider> _fileNameProvider = new Mock<IFileNameProvider>(MockBehavior.Strict);
        private readonly Mock<CloudFileDirectory> _cloudFileDirectory = new Mock<CloudFileDirectory>(MockBehavior.Strict, new Uri("https://www.file-share.com/directory"));
        private readonly Mock<ILoggerAdapter<AzureFileShareDirectory>> _mockLoggingService = new Mock<ILoggerAdapter<AzureFileShareDirectory>>(MockBehavior.Loose);

        private readonly AzureFileShareDirectory _azureFileShareDirectory;

        public AzureFileShareDirectoryTests()
        {
            var fileShareName = "the-file-share-name";
            var fileShareDirectory = "the-directory";

            var cloudFileShare = new Mock<CloudFileShare>(MockBehavior.Strict, new Uri("https://www.file-share.com/"));

            _cloudStorageAccount
                    .Setup(storageAccount => storageAccount.GetFileShare(fileShareName))
                    .Returns(cloudFileShare.Object);

            cloudFileShare
                .Setup(fileShare => fileShare.GetRootDirectoryReference())
                .Returns(_cloudFileDirectory.Object);

            _cloudFileDirectory
                .Setup(dir => dir.GetDirectoryReference(fileShareDirectory))
                .Returns(_cloudFileDirectory.Object);

            _azureFileShareDirectory = new AzureFileShareDirectory(
                fileShareName,
                fileShareDirectory,
                _cloudStorageAccount.Object,
                _fileNameProvider.Object,
                _mockLoggingService.Object);
        }

        #region Read

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task Read_WhenFileNameIsEmpty_ThrowsArgumentNullException(string fileName)
        {
            // Act
            Func<Task<Stream>> func = () => _azureFileShareDirectory.Read(fileName);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Read_WhenFileDoesntExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var fileName = "non-existing-file.docx";
            var fileUri = new Uri($"https://www.file-share.com/directory/{fileName}");

            var cloudFile = new Mock<CloudFile>(MockBehavior.Strict, fileUri);

            _cloudFileDirectory
                .Setup(dir => dir.GetFileReference(fileName))
                .Returns(cloudFile.Object);

            cloudFile
                .Setup(file => file.ExistsAsync())
                .ReturnsAsync(false);

            // Act
            Func<Task<Stream>> func = () => _azureFileShareDirectory.Read(fileName);

            // Assert
            await func.Should().ThrowAsync<FileNotFoundException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Read_WhenFileExists_ReturnsStream()
        {
            // Arrange
            var fileName = "existing-file.docx";
            var fileUri = new Uri($"https://www.file-share.com/directory/{fileName}");

            var cloudFile = new Mock<CloudFile>(MockBehavior.Strict, fileUri);

            _cloudFileDirectory
                .Setup(dir => dir.GetFileReference(fileName))
                .Returns(cloudFile.Object);

            cloudFile
                .Setup(file => file.ExistsAsync())
                .ReturnsAsync(true);

            cloudFile
                .Setup(file => file.DownloadToStreamAsync(It.IsAny<MemoryStream>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _azureFileShareDirectory.Read(fileName);

            // Assert
            result.Should().BeOfType<MemoryStream>();
        }

        #endregion


        #region FileExists

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task FileExists_WhenFileNameIsEmpty_ThrowsArgumentNullException(string fileName)
        {
            // Act
            Func<Task<bool>> func = () => _azureFileShareDirectory.FileExists(fileName);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FileExists_WhenFileExists_ReturnsTrue()
        {
            // Arrange
            var fileName = "existing-file.docx";
            var fileUri = new Uri($"https://www.file-share.com/directory/{fileName}");

            var cloudFile = new Mock<CloudFile>(MockBehavior.Strict, fileUri);

            _cloudFileDirectory
                .Setup(dir => dir.GetFileReference(fileName))
                .Returns(cloudFile.Object);

            cloudFile
                .Setup(file => file.ExistsAsync())
                .ReturnsAsync(true);

            // Act
            var result = await _azureFileShareDirectory.FileExists(fileName);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FileExists_WhenFileNameHasInvalidCharacters_ThrowException()
        {
            // Arrange
            var fileName = "existing-::file.docx";
            var fileUri = new Uri($"https://www.file-share.com/directory/{fileName}");

            var cloudFile = new Mock<CloudFile>(MockBehavior.Strict, fileUri);

            _cloudFileDirectory
                .Setup(dir => dir.GetFileReference(fileName))
                .Returns(cloudFile.Object);

            cloudFile
                .Setup(file => file.ExistsAsync())
                .ThrowsAsync(new Exception("The specified resource name contains invalid characters."));

            //Act
            Task FileExistsTask() => _azureFileShareDirectory.FileExists(fileName);

            //Assert
            var result = await Assert.ThrowsExceptionAsync<Exception>(FileExistsTask);
            result.Message.Should().Be(fileName);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FileExists_WhenFileDoesntExists_ReturnsFalse()
        {
            // Arrange
            var fileName = "non-existing-file.docx";
            var fileUri = new Uri($"https://www.file-share.com/directory/{fileName}");

            var cloudFile = new Mock<CloudFile>(MockBehavior.Strict, fileUri);

            _cloudFileDirectory
                .Setup(dir => dir.GetFileReference(fileName))
                .Returns(cloudFile.Object);

            cloudFile
                .Setup(file => file.ExistsAsync())
                .ReturnsAsync(false);

            // Act
            var result = await _azureFileShareDirectory.FileExists(fileName);

            // Assert
            result.Should().BeFalse();
        }

        #endregion


        #region Delete

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task Delete_WhenFileNameIsEmpty_ThrowsArgumentNullException(string fileName)
        {
            // Act
            Func<Task> func = () => _azureFileShareDirectory.Delete(fileName);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Delete_WhenFileNameDoesntExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var fileName = "non-existing-file.docx";
            var fileUri = new Uri($"https://www.file-share.com/directory/{fileName}");

            var cloudFile = new Mock<CloudFile>(MockBehavior.Strict, fileUri);

            _cloudFileDirectory
                .Setup(dir => dir.GetFileReference(fileName))
                .Returns(cloudFile.Object);

            cloudFile
                .Setup(file => file.ExistsAsync())
                .ReturnsAsync(false);

            // Act
            Func<Task> func = () => _azureFileShareDirectory.Delete(fileName);

            // Assert
            await func.Should().ThrowAsync<FileNotFoundException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Delete_WhenFileExist_DeletesFile()
        {
            // Arrange
            var fileName = "existing-file.docx";
            var fileUri = new Uri($"https://www.file-share.com/directory/{fileName}");

            var cloudFile = new Mock<CloudFile>(MockBehavior.Strict, fileUri);

            _cloudFileDirectory
                .Setup(dir => dir.GetFileReference(fileName))
                .Returns(cloudFile.Object);

            cloudFile
                .Setup(file => file.ExistsAsync())
                .ReturnsAsync(true);

            cloudFile
                .Setup(file => file.DeleteAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _azureFileShareDirectory.Delete(fileName);

            // Assert
            cloudFile
                .Verify(file => file.DeleteAsync(), Times.Once);
        }

        #endregion


        #region Save

        [TestMethod, TestCategory("Unit")]
        public async Task Save_WhenStreamIsNull_ThrowsArgumentNullException()
        {
            // Act
            Func<Task<string>> func = () => _azureFileShareDirectory.Save(null, "file-to-save.pdf");

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task Save_WhenFileNameIsEmpty_ThrowsArgumentNullException(string fileName)
        {
            // Act
            Func<Task<string>> func = () => _azureFileShareDirectory.Save(Mock.Of<Stream>(), fileName);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Save_WhenStreamAndFileNameHaveValues_FileGetsSaved()
        {
            // Arrange
            var fileStream = Mock.Of<Stream>();

            var fileName = "file-name.docx";
            var fileUri = new Uri($"https://www.file-share.com/directory/{fileName}");

            var cloudFile = new Mock<CloudFile>(MockBehavior.Strict, fileUri);

            _fileNameProvider
                .Setup(fileNameProvider => fileNameProvider.GetNextAvailableFileName(fileName, It.IsAny<Func<string, Task<bool>>>()))
                .ReturnsAsync(fileName);

            _cloudFileDirectory
                .Setup(dir => dir.GetFileReference(fileName))
                .Returns(cloudFile.Object);

            _cloudFileDirectory
                .Setup(dir => dir.CreateIfNotExistsAsync())
                .ReturnsAsync(true);

            cloudFile
                .Setup(file => file.UploadFromStreamAsync(fileStream))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _azureFileShareDirectory.Save(fileStream, fileName);

            // Assert
            result.Should().Be(fileName);
        }

        #endregion


        #region Copy

        [TestMethod, TestCategory("Unit")]
        public async Task Copy_WhenDestinationIsNull_ThrowsArgumentNullException()
        {
            // Act
            Func<Task<string>> func = () => _azureFileShareDirectory.Copy(null, "file-to-copy.pdf");

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task Copy_WhenFileNameIsEmpty_ThrowsArgumentNullException(string fileName)
        {
            // Act
            Func<Task<string>> func = () => _azureFileShareDirectory.Copy(Mock.Of<IDirectory>(), fileName);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Copy_WhenFileDoesntExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var fileName = "non-existing-file.docx";
            var fileUri = new Uri($"https://www.file-share.com/directory/{fileName}");

            var cloudFile = new Mock<CloudFile>(MockBehavior.Strict, fileUri);

            _cloudFileDirectory
                .Setup(dir => dir.GetFileReference(fileName))
                .Returns(cloudFile.Object);

            cloudFile
                .Setup(file => file.ExistsAsync())
                .ReturnsAsync(false);

            // Act
            Func<Task<string>> func = () => _azureFileShareDirectory.Copy(Mock.Of<IDirectory>(), fileName);

            // Assert
            await func.Should().ThrowAsync<FileNotFoundException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Copy_WhenDestinationAndFileNameAreValid_FileGetsCopied()
        {
            // Arrange
            var fileName = "existing-file.docx";
            var fileUri = new Uri($"https://www.file-share.com/directory/{fileName}");

            var cloudFile = new Mock<CloudFile>(MockBehavior.Strict, fileUri);

            _cloudFileDirectory
                .Setup(dir => dir.GetFileReference(fileName))
                .Returns(cloudFile.Object);

            cloudFile
                .Setup(file => file.ExistsAsync())
                .ReturnsAsync(true);

            cloudFile
                .Setup(file => file.DownloadToStreamAsync(It.IsAny<MemoryStream>()))
                .Returns(Task.CompletedTask);

            var destinationDirectory = new Mock<IDirectory>(MockBehavior.Strict);

            destinationDirectory
                .Setup(dir => dir.Save(It.IsAny<Stream>(), fileName))
                .ReturnsAsync(fileName);

            // Act
            var result = await _azureFileShareDirectory.Copy(destinationDirectory.Object, fileName);

            // Assert
            result.Should().Be(fileName);

            destinationDirectory
                .Verify(dir => dir.Save(It.IsAny<Stream>(), fileName), Times.Once);
        }

        #endregion


        #region Move

        [TestMethod, TestCategory("Unit")]
        public async Task Move_WhenDestinationIsNull_ThrowsArgumentNullException()
        {
            // Act
            Func<Task<string>> func = () => _azureFileShareDirectory.Move(null, "file-to-move.pdf");

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task Move_WhenFileNameIsEmpty_ThrowsArgumentNullException(string fileName)
        {
            // Act
            Func<Task<string>> func = () => _azureFileShareDirectory.Move(Mock.Of<IDirectory>(), fileName);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Move_WhenFileDoesntExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var fileName = "non-existing-file.docx";
            var fileUri = new Uri($"https://www.file-share.com/directory/{fileName}");

            var cloudFile = new Mock<CloudFile>(MockBehavior.Strict, fileUri);

            _cloudFileDirectory
                .Setup(dir => dir.GetFileReference(fileName))
                .Returns(cloudFile.Object);

            cloudFile
                .Setup(file => file.ExistsAsync())
                .ReturnsAsync(false);

            // Act
            Func<Task<string>> func = () => _azureFileShareDirectory.Move(Mock.Of<IDirectory>(), fileName);

            // Assert
            await func.Should().ThrowAsync<FileNotFoundException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Move_WhenDestinationAndFileNameAreValid_FileGetsMoved()
        {
            // Arrange
            var fileName = "existing-file.docx";
            var fileUri = new Uri($"https://www.file-share.com/directory/{fileName}");

            var cloudFile = new Mock<CloudFile>(MockBehavior.Strict, fileUri);

            _cloudFileDirectory
                .Setup(dir => dir.GetFileReference(fileName))
                .Returns(cloudFile.Object);

            cloudFile
                .Setup(file => file.ExistsAsync())
                .ReturnsAsync(true);

            cloudFile
                .Setup(file => file.DownloadToStreamAsync(It.IsAny<MemoryStream>()))
                .Returns(Task.CompletedTask);

            cloudFile
                .Setup(file => file.DeleteAsync())
                .Returns(Task.CompletedTask);

            var destinationDirectory = new Mock<IDirectory>(MockBehavior.Strict);

            destinationDirectory
                .Setup(dir => dir.Save(It.IsAny<Stream>(), fileName))
                .ReturnsAsync(fileName);

            // Act
            var result = await _azureFileShareDirectory.Move(destinationDirectory.Object, fileName);

            // Assert
            result.Should().Be(fileName);

            destinationDirectory
                .Verify(dir => dir.Save(It.IsAny<Stream>(), fileName), Times.Once);
        }

        #endregion
    }
}