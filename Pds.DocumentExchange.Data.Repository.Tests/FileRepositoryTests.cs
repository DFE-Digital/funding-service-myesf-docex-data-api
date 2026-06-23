using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Repository.Implementations;

namespace Pds.DocumentExchange.Data.Repository.Tests
{
    [TestClass]
    public class FileRepositoryTests
    {
        private readonly FileRepository _fileRepository;

        public FileRepositoryTests()
        {
            _fileRepository = new FileRepository();
        }


        [DataRow("TestJavaScriptWithVar.js", "var", true)]
        [DataRow("TestJavaScriptWithFunction.js", "function", true)]
        [DataRow("NonExistentResource.js", "DoesNotMatter", false)]
        [TestMethod, TestCategory("Unit")]
        public void ReadSingleFileContentsTest(string fileName, string expectedSubString, bool expectedToExist)
        {
            var fileContents = _fileRepository.ReadSingleFileContents(fileName);

            if (expectedToExist)
            {
                fileContents.Should().Contain(expectedSubString);
            }
            else
            {
                fileContents.Should().BeNullOrEmpty();
            }
        }

        [DataRow("TestJavaScriptWithVar", "var", true, ".js")]
        [DataRow("TestJavaScriptWithFunction", "function", true, ".js")]
        [DataRow("NonExistentResource", "DoesNotMatter", false, ".js")]
        [DataRow("TextResourceFile1", "Text Resource File1", true, ".txt")]
        [DataRow("TextResourceFile2", "Text Resource File2", true, ".txt")]
        [DataRow("NonExistentResource", "DoesNotMatter", false, ".txt")]
        [TestMethod, TestCategory("Unit")]
        public void ReadMultipleFilesMatchingAPatternTest(string fileName, string expectedFileContent, bool expectedToExist, string fileNamePattern)
        {
            var fileNameAndContents = _fileRepository.ReadMultipleFilesMatchingAPattern(fileNamePattern);

            if (expectedToExist)
            {
                fileNameAndContents.Should().ContainKey(fileName);

                fileNameAndContents[fileName].Should().Contain(expectedFileContent);
            }
            else
            {
                fileNameAndContents.Should().NotContainKey(fileName);
            }
        }
    }
}