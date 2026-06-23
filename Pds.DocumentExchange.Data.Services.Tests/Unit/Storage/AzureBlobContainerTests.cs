using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Blob;
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
    public class AzureBlobContainerTests
    {
        private readonly Mock<ICloudStorageAccount> _cloudStorageAccount = new Mock<ICloudStorageAccount>(MockBehavior.Strict);
        private readonly Mock<IFileNameProvider> _fileNameProvider = new Mock<IFileNameProvider>(MockBehavior.Strict);
        private readonly Mock<CloudBlobContainer> _cloudBlobContainer = new Mock<CloudBlobContainer>(MockBehavior.Strict, new Uri("https://www.blob-container.com/"));
        private readonly Mock<ILoggerAdapter<AzureBlobContainer>> _mockLoggingService = new Mock<ILoggerAdapter<AzureBlobContainer>>(MockBehavior.Loose);

        private readonly AzureBlobContainer _azureBlobContainer;

        public AzureBlobContainerTests()
        {
            var blobContainerName = "the-container-name";

            _cloudStorageAccount
                .Setup(storageAccount => storageAccount.GetBlobContainer(blobContainerName))
                .Returns(_cloudBlobContainer.Object);

            _azureBlobContainer = new AzureBlobContainer(
                blobContainerName,
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
            Func<Task<Stream>> func = () => _azureBlobContainer.Read(fileName);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Read_WhenFileDoesntExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var fileName = "non-existing-file.docx";
            var blobUri = new Uri($"https://www.blob-container.com/{fileName}");

            var cloudBlock = new Mock<CloudBlockBlob>(MockBehavior.Strict, blobUri);

            _cloudBlobContainer
                .Setup(blobContainer => blobContainer.GetBlockBlobReference(fileName))
                .Returns(cloudBlock.Object);

            cloudBlock
                .Setup(block => block.ExistsAsync())
                .ReturnsAsync(false);

            // Act
            Func<Task<Stream>> func = () => _azureBlobContainer.Read(fileName);

            // Assert
            await func.Should().ThrowAsync<FileNotFoundException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Read_WhenFileExists_ReturnsStream()
        {
            // Arrange
            var fileName = "existing-file.docx";
            var blobUri = new Uri($"https://www.blob-container.com/{fileName}");

            var cloudBlock = new Mock<CloudBlockBlob>(MockBehavior.Strict, blobUri);

            _cloudBlobContainer
                .Setup(blobContainer => blobContainer.GetBlockBlobReference(fileName))
                .Returns(cloudBlock.Object);

            cloudBlock
                .Setup(block => block.ExistsAsync())
                .ReturnsAsync(true);

            cloudBlock
                .Setup(block => block.DownloadToStreamAsync(It.IsAny<MemoryStream>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _azureBlobContainer.Read(fileName);

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
            Func<Task<bool>> func = () => _azureBlobContainer.FileExists(fileName);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FileExists_WhenFileExists_ReturnsTrue()
        {
            // Arrange
            var fileName = "existing-file.docx";
            var blobUri = new Uri($"https://www.blob-container.com/{fileName}");

            var cloudBlock = new Mock<CloudBlockBlob>(MockBehavior.Strict, blobUri);

            _cloudBlobContainer
                .Setup(blobContainer => blobContainer.GetBlockBlobReference(fileName))
                .Returns(cloudBlock.Object);

            cloudBlock
                .Setup(block => block.ExistsAsync())
                .ReturnsAsync(true);

            // Act
            var result = await _azureBlobContainer.FileExists(fileName);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FileExists_WhenFileDoesntExists_ReturnsFalse()
        {
            // Arrange
            var fileName = "non-existing-file.docx";
            var blobUri = new Uri($"https://www.blob-container.com/{fileName}");

            var cloudBlock = new Mock<CloudBlockBlob>(MockBehavior.Strict, blobUri);

            _cloudBlobContainer
                .Setup(blobContainer => blobContainer.GetBlockBlobReference(fileName))
                .Returns(cloudBlock.Object);

            cloudBlock
                .Setup(block => block.ExistsAsync())
                .ReturnsAsync(false);

            // Act
            var result = await _azureBlobContainer.FileExists(fileName);

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
            Func<Task> func = () => _azureBlobContainer.Delete(fileName);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Delete_WhenFileNameDoesntExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var fileName = "non-existing-file.docx";
            var blobUri = new Uri($"https://www.blob-container.com/{fileName}");

            var cloudBlock = new Mock<CloudBlockBlob>(MockBehavior.Strict, blobUri);

            _cloudBlobContainer
                .Setup(blobContainer => blobContainer.GetBlockBlobReference(fileName))
                .Returns(cloudBlock.Object);

            cloudBlock
                .Setup(block => block.ExistsAsync())
                .ReturnsAsync(false);

            // Act
            Func<Task> func = () => _azureBlobContainer.Delete(fileName);

            // Assert
            await func.Should().ThrowAsync<FileNotFoundException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Delete_WhenFileExist_DeletesFile()
        {
            // Arrange
            var fileName = "existing-file.docx";
            var blobUri = new Uri($"https://www.blob-container.com/{fileName}");

            var cloudBlock = new Mock<CloudBlockBlob>(MockBehavior.Strict, blobUri);

            _cloudBlobContainer
                .Setup(blobContainer => blobContainer.GetBlockBlobReference(fileName))
                .Returns(cloudBlock.Object);

            cloudBlock
                .Setup(block => block.ExistsAsync())
                .ReturnsAsync(true);

            cloudBlock
                .Setup(block => block.DeleteAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _azureBlobContainer.Delete(fileName);

            // Assert
            cloudBlock
                .Verify(block => block.DeleteAsync(), Times.Once);
        }

        #endregion


        #region Save

        [TestMethod, TestCategory("Unit")]
        public async Task Save_WhenStreamIsNull_ThrowsArgumentNullException()
        {
            // Act
            Func<Task<string>> func = () => _azureBlobContainer.Save(null, "file-to-save.pdf");

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
            Func<Task<string>> func = () => _azureBlobContainer.Save(Mock.Of<Stream>(), fileName);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Save_WhenStreamAndFileNameHaveValues_FileGetsSaved()
        {
            // Arrange
            var fileStream = Mock.Of<Stream>();
            var fileName = "file-to-save.pdf";
            var blobUri = new Uri($"https://www.blob-container.com/{fileName}");
            var cloudBlock = new Mock<CloudBlockBlob>(MockBehavior.Strict, blobUri);

            _fileNameProvider
                .Setup(fileNameProvider => fileNameProvider.GetNextAvailableFileName(fileName, It.IsAny<Func<string, Task<bool>>>()))
                .ReturnsAsync(fileName);

            _cloudBlobContainer
                .Setup(blobContainer => blobContainer.GetBlockBlobReference(fileName))
                .Returns(cloudBlock.Object);

            cloudBlock
                .Setup(block => block.UploadFromStreamAsync(fileStream))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _azureBlobContainer.Save(fileStream, fileName);

            // Assert
            result.Should().Be(fileName);
        }

        #endregion


        #region Copy

        [TestMethod, TestCategory("Unit")]
        public async Task Copy_WhenDestinationIsNull_ThrowsArgumentNullException()
        {
            // Act
            Func<Task<string>> func = () => _azureBlobContainer.Copy(null, "file-to-copy.pdf");

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
            Func<Task<string>> func = () => _azureBlobContainer.Copy(Mock.Of<IDirectory>(), fileName);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Copy_WhenFileDoesntExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var fileName = "non-existing-file.docx";
            var blobUri = new Uri($"https://www.blob-container.com/{fileName}");

            var cloudBlock = new Mock<CloudBlockBlob>(MockBehavior.Strict, blobUri);

            _cloudBlobContainer
                .Setup(blobContainer => blobContainer.GetBlockBlobReference(fileName))
                .Returns(cloudBlock.Object);

            cloudBlock
                .Setup(block => block.ExistsAsync())
                .ReturnsAsync(false);

            // Act
            Func<Task<string>> func = () => _azureBlobContainer.Copy(Mock.Of<IDirectory>(), fileName);

            // Assert
            await func.Should().ThrowAsync<FileNotFoundException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Copy_WhenDestinationAndFileNameAreValid_FileGetsCopied()
        {
            // Arrange
            var fileName = "existing-file.docx";
            var blobUri = new Uri($"https://www.blob-container.com/{fileName}");

            var cloudBlock = new Mock<CloudBlockBlob>(MockBehavior.Strict, blobUri);

            _cloudBlobContainer
                .Setup(blobContainer => blobContainer.GetBlockBlobReference(fileName))
                .Returns(cloudBlock.Object);

            cloudBlock
                .Setup(block => block.ExistsAsync())
                .ReturnsAsync(true);

            cloudBlock
                .Setup(block => block.DownloadToStreamAsync(It.IsAny<MemoryStream>()))
                .Returns(Task.CompletedTask);

            var destinationDirectory = new Mock<IDirectory>(MockBehavior.Strict);

            destinationDirectory
                .Setup(dir => dir.Save(It.IsAny<Stream>(), fileName))
                .ReturnsAsync(fileName);

            // Act
            var result = await _azureBlobContainer.Copy(destinationDirectory.Object, fileName);

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
            Func<Task<string>> func = () => _azureBlobContainer.Move(null, "file-to-move.pdf");

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
            Func<Task<string>> func = () => _azureBlobContainer.Move(Mock.Of<IDirectory>(), fileName);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Move_WhenFileDoesntExist_ThrowsFileNotFoundException()
        {
            // Arrange
            var fileName = "non-existing-file.docx";
            var blobUri = new Uri($"https://www.blob-container.com/{fileName}");

            var cloudBlock = new Mock<CloudBlockBlob>(MockBehavior.Strict, blobUri);

            _cloudBlobContainer
                .Setup(blobContainer => blobContainer.GetBlockBlobReference(fileName))
                .Returns(cloudBlock.Object);

            cloudBlock
                .Setup(block => block.ExistsAsync())
                .ReturnsAsync(false);

            // Act
            Func<Task<string>> func = () => _azureBlobContainer.Move(Mock.Of<IDirectory>(), fileName);

            // Assert
            await func.Should().ThrowAsync<FileNotFoundException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Move_WhenDestinationAndFileNameAreValid_FileGetsMoved()
        {
            // Arrange
            var fileName = "existing-file.docx";
            var blobUri = new Uri($"https://www.blob-container.com/{fileName}");

            var cloudBlock = new Mock<CloudBlockBlob>(MockBehavior.Strict, blobUri);

            _cloudBlobContainer
                .Setup(blobContainer => blobContainer.GetBlockBlobReference(fileName))
                .Returns(cloudBlock.Object);

            cloudBlock
                .Setup(block => block.ExistsAsync())
                .ReturnsAsync(true);

            cloudBlock
                .Setup(block => block.DownloadToStreamAsync(It.IsAny<MemoryStream>()))
                .Returns(Task.CompletedTask);

            cloudBlock
                .Setup(block => block.DeleteAsync())
                .Returns(Task.CompletedTask);

            var destinationDirectory = new Mock<IDirectory>(MockBehavior.Strict);

            destinationDirectory
                .Setup(dir => dir.Save(It.IsAny<Stream>(), fileName))
                .ReturnsAsync(fileName);

            // Act
            var result = await _azureBlobContainer.Move(destinationDirectory.Object, fileName);

            // Assert
            result.Should().Be(fileName);

            destinationDirectory
                .Verify(dir => dir.Save(It.IsAny<Stream>(), fileName), Times.Once);
        }

        #endregion

    }
}