using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Implementations;
using System;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class FileNameProviderTests
    {
        private readonly FileNameProvider _fileNameProvider = new FileNameProvider();

        #region GetNextAvailableFileName

        [TestMethod, TestCategory("Unit")]
        public async Task GetNextAvailableFileName_WhenFileNameDoesntExist_ReturnsOriginalFileName()
        {
            // Arrange
            var mockExistFunc = new Mock<Func<string, Task<bool>>>();

            var fileExistsSetup = mockExistFunc.Setup(func => func("TestFileName.pdf"));
            fileExistsSetup.ReturnsAsync(false);

            // Act
            string availableFileName = await _fileNameProvider.GetNextAvailableFileName("TestFileName.pdf", mockExistFunc.Object);

            // Assert
            availableFileName.Should().Be("TestFileName.pdf");
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetNextAvailableFileName_WhenFileNameAlreadyExists_ReturnsOriginalFileNameWithTrailingNumberTwo()
        {
            // Arrange
            var mockExistFunc = new Mock<Func<string, Task<bool>>>();

            var fileExistsSetup = mockExistFunc.Setup(func => func("TestFileName.pdf"));
            fileExistsSetup.ReturnsAsync(true);

            // Act
            string availableFileName = await _fileNameProvider.GetNextAvailableFileName("TestFileName.pdf", mockExistFunc.Object);

            // Assert
            availableFileName.Should().Be("TestFileName-2.pdf");
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetNextAvailableFileName_WhenFileNameAlreadyExistsWithDuplication_ReturnsOriginalFileNameWithTrailingNumberThree()
        {
            // Arrange
            var mockExistFunc = new Mock<Func<string, Task<bool>>>();

            var firstFileExistsSetup = mockExistFunc.Setup(func => func("TestFileName.pdf"));
            firstFileExistsSetup.ReturnsAsync(true);

            var secondFileExistsSetup = mockExistFunc.Setup(func => func("TestFileName-2.pdf"));
            secondFileExistsSetup.ReturnsAsync(true);

            // Act
            string availableFileName = await _fileNameProvider.GetNextAvailableFileName("TestFileName.pdf", mockExistFunc.Object);

            // Assert
            availableFileName.Should().Be("TestFileName-3.pdf");
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        public async Task GetNextAvailableFileName_WhenFileNameIsNullOrEmpty_ThrowsArgumentException(string fileName)
        {
            // Arrange
            var mockExistFunc = new Mock<Func<string, Task<bool>>>();

            // Act
            Func<Task<string>> func = async () => await _fileNameProvider.GetNextAvailableFileName(fileName, mockExistFunc.Object);

            // Assert
            await func.Should().ThrowAsync<ArgumentException>();
        }

        #endregion


        #region GetComponents

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678_10002_201819", 12345678)]
        [DataRow("12345678_10090_201819_TestFile.pdf", 12345678)]
        public void GetComponents_WhenFormatIsCorrect_ReturnsOrganisationIdentifier(string filename, int expectedOrganisationIdentifier)
        {
            // Act
            var actualResult = _fileNameProvider.GetComponents(filename);

            // Assert
            Assert.AreEqual(expectedOrganisationIdentifier, actualResult.OrganisationIdentifier);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678_10002_201819", "10002")]
        [DataRow("12345678_10090_201819_TestFile.pdf", "10090")]
        public void GetComponents_WhenFormatIsCorrect_ReturnsFileType(string filename, string expectedFileType)
        {
            // Act
            var actualResult = _fileNameProvider.GetComponents(filename);

            // Assert
            Assert.AreEqual(expectedFileType, actualResult.ProductIdentifier);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678_10002_201819", 201819)]
        [DataRow("12345678_10090_201819_TestFile.pdf", 201819)]
        public void GetComponents_WhenFormatIsCorrect_ReturnsYear(string filename, int? expectedYear)
        {
            // Act
            var actualResult = _fileNameProvider.GetComponents(filename);

            // Assert
            Assert.AreEqual(expectedYear, actualResult.AcademicYear);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678_10090_201819_TestFile.pdf", ".pdf")]
        [DataRow("12345678_10090_201819_TestFile.pdf-2", ".pdf")]
        public void GetComponents_WhenFormatIsCorrect_ReturnsExtension(string filename, string expectedExtension)
        {
            // Act
            var actualResult = _fileNameProvider.GetComponents(filename);

            // Assert
            Assert.AreEqual(expectedExtension, actualResult.Extension);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        public void GetComponents_WhenFileNameIsNullOrEmpty_ThrowsArgumentException(string fileName)
        {
            // Act
            Func<FileNameComponents> func = () => _fileNameProvider.GetComponents(fileName);

            // Assert
            func.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678")] // Not enough parts
        [DataRow("12345678_10002")] // Not enough parts
        [DataRow("1234567X_10002")] // UKPRN not a number
        [DataRow("12345678_10002_20181X")] // Year not a number
        [DataRow("12345678_10002_")] // Year empty
        public void GetComponents_WhenFileNameIsInvalid_ThrowsFormatException(string fileName)
        {
            // Act
            Func<FileNameComponents> func = () => _fileNameProvider.GetComponents(fileName);

            // Assert
            func.Should().Throw<FormatException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678")]
        [DataRow("12345678_10002")]
        public void GetComponents_WhenNotEnoughComponents_ThrowsFormatException(string fileName)
        {
            // Act
            Func<FileNameComponents> func = () => _fileNameProvider.GetComponents(fileName);

            // Assert
            func.Should().Throw<FormatException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("123456XXX_10002_201819")]
        [DataRow("ABCDE_10090_201819_TestFile.pdf")]
        public void GetComponents_WhenOrganisationIdentifierIsNotNumber_ThrowsFormatException(string fileName)
        {
            // Act
            Func<FileNameComponents> func = () => _fileNameProvider.GetComponents(fileName);

            // Assert
            func.Should().Throw<FormatException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678_10002_2018XX")]
        [DataRow("12345678_10090_MDCVII_TestFile.pdf")]
        public void GetComponents_WhenAcademicYearIsNotNumber_ThrowsFormatException(string fileName)
        {
            // Act
            Func<FileNameComponents> func = () => _fileNameProvider.GetComponents(fileName);

            // Assert
            func.Should().Throw<FormatException>();
        }

        #endregion


        #region GenerateFileName

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678", 10002, "201920", "TestFile.docx", "12345678_10002_201920_TestFile.docx")]
        [DataRow("12345678", 10002, "201920", "Test_File.docx", "12345678_10002_201920_Test¬File.docx")]
        [DataRow("12345678", 10099, "201516", "Test_File_Document.docx", "12345678_10099_201516_Test¬File¬Document.docx")]
        public void GenerateFileName_WhenParametersAreCorrect_ReturnsFileName(
            string organisationIdentifier,
            int productIdentifier,
            string academicYear,
            string originalFileName,
            string expectedFileName)
        {
            // Act
            var actualFileName = _fileNameProvider.GenerateFileName(
                organisationIdentifier,
                productIdentifier,
                academicYear,
                originalFileName);

            // Assert
            Assert.AreEqual(actualFileName, expectedFileName);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null, 10002, "201920", "TestFile.docx")]
        [DataRow("", 10002, "201920", "TestFile.docx")]
        public void GenerateFileName_WhenOrganisationIdentifierIsNullOrEmpty_ThrowsArgumentException(
           string organisationIdentifier,
           int productIdentifier,
           string academicYear,
           string originalFileName)
        {
            // Act
            Func<string> func = ()
                   => _fileNameProvider.GenerateFileName(
                       organisationIdentifier,
                       productIdentifier,
                       academicYear,
                       originalFileName);

            // Assert
            func.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678", 10002, null, "TestFile.docx")]
        [DataRow("12345678", 10002, "", "TestFile.docx")]
        public void GenerateFileName_WhenAcademicYearIsNullOrEmpty_ThrowsArgumentException(
           string organisationIdentifier,
           int productIdentifier,
           string academicYear,
           string originalFileName)
        {
            // Act
            Func<string> func = ()
                   => _fileNameProvider.GenerateFileName(
                       organisationIdentifier,
                       productIdentifier,
                       academicYear,
                       originalFileName);

            // Assert
            func.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678", 10002, "201920", null)]
        [DataRow("12345678", 10002, "201920", "")]
        public void GenerateFileName_WhenOriginalFileNameIsNullOrEmpty_ThrowsArgumentException(
           string organisationIdentifier,
           int productIdentifier,
           string academicYear,
           string originalFileName)
        {
            // Act
            Func<string> func = ()
                   => _fileNameProvider.GenerateFileName(
                       organisationIdentifier,
                       productIdentifier,
                       academicYear,
                       originalFileName);

            // Assert
            func.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void GenerateFileName_WhenOriginalFileNameIsTooLong_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            string organisationIdentifier = "12345678";
            int productIdentifier = 10002;
            string academicYear = "201920";

            // A 261 characters string
            string originalFileName = "3jDVmLa4XhhfWDDMjNyZiSu5iyN9gnCCEAvsdOywfSbqbO2XXYkU1Kpjk88RcrutYVnG3g2PC9Lupg1FtOqPV2GhFbUWjhknBFA4Qgd4KShUVH64mJVYW1YBZhRzrDV5HSMbDdWsyJxp38zRqvcItcOKVQS4e82cTZVabGmi0IQyqOivrzu8eDa4CfAWleQwvuZ8ek6bNB4roXpDhrS4jJPlRbCKp09JbphWvVTUt6l2V9TySdQ4C40QfIhw7sKychvTd";

            // Act
            Func<string> func = ()
                       => _fileNameProvider.GenerateFileName(
                           organisationIdentifier,
                           productIdentifier,
                           academicYear,
                           originalFileName);

            // Assert
            func.Should().Throw<ArgumentException>();
        }

        #endregion
    }
}