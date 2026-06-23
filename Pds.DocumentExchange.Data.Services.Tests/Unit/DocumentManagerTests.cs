using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class DocumentManagerTests
    {
        private readonly Mock<IDirectoriesManager> _mockDirectoriesManager = new Mock<IDirectoriesManager>();
        private readonly Mock<IDirectory> _downloadMockDirectory = new Mock<IDirectory>();
        private readonly Mock<ILoggerAdapter<DocumentManager>> _mockLogger = new Mock<ILoggerAdapter<DocumentManager>>();
        private readonly DocumentManager _documentManager;

        public DocumentManagerTests()
        {
            _documentManager = new DocumentManager(
                _mockDirectoriesManager.Object,
                _mockLogger.Object);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task DownloadAgencyDocument_ReturnsFileContent_WhenCalled()
        {
            // Arrange
            var team = "team";
            var fileNames = new List<string> { "file name1", "file name2" };

            _mockDirectoriesManager
                .Setup(manager => manager.GetDirectory(team))
                .ReturnsAsync(_downloadMockDirectory.Object)
                .Verifiable();

            _downloadMockDirectory.Setup(a => a.Delete(fileNames[0]))
                .Verifiable();

            _downloadMockDirectory.Setup(a => a.Delete(fileNames[1]))
                .Verifiable();

            // Act
            await _documentManager.RemoveAgencyDocuments(team, fileNames);

            // Assert
            _downloadMockDirectory
                .Verify(
                    a => a.Delete(
                        fileNames[0]),
                    Times.Once);

            _downloadMockDirectory
                .Verify(
                    a => a.Delete(
                        fileNames[1]),
                    Times.Once);
        }
    }
}