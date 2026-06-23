using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.Implementations.Providers;
using Pds.DocumentExchange.Data.Services.Interfaces.Providers;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Providers
{
    [TestClass]
    public sealed class PresentationFormattingProviderTests :
        MoqTestingTests<PresentationFormattingProvider, IProvidePresentationFormatting>
    {
        [TestMethod]
        [DataRow("", "")]
        [DataRow("ImAYampy-File@@Name.Blah", "ImAYampy-FileNameBlah")]
        [DataRow("Im--A--Yampy--File--Name.Blah", "Im-A-Yampy-File-NameBlah")]
        [DataRow("I'm_A_Yampy_File_Name.Blah", "Im-A-Yampy-File-NameBlah")]
        [DataRow("I'm.A.Yampy.File.Name.Blah", "ImAYampyFileNameBlah")]
        public void ConvertToAllowedFilenameMeetsExpectation(string candidate, string expectedResult)
        {
            // arrange
            var sut = BuildTestSystem();

            // act
            var result = sut.ConvertToAllowedFilename(candidate);

            // assert
            result.Should().Be(expectedResult);
        }

        [TestMethod]
        [DataRow("", "doesn't matter not used...", "")]
        [DataRow("business case", "mybusinesscase.pdf", "business-case.pdf")]
        [DataRow("my funky spreadsheet", "boringlynamedspreadsheet.xlsx", "my-funky-spreadsheet.xlsx")]
        [DataRow("tediously long document", "excitingopportunities.docx", "tediously-long-document.docx")]
        public void FormatProductNameWithFileExtension(string product, string fileName, string expectedResult)
        {
            // arrange
            var sut = BuildTestSystem();

            // act
            var result = sut.FormatProductNameWithFileExtension(product, fileName);

            // assert
            result.Should().Be(expectedResult);
        }

        internal override PresentationFormattingProvider BuildTestSystem() =>
            new PresentationFormattingProvider();

        internal override void VerifyAllMocks(PresentationFormattingProvider sut)
        {
            // nothing to do...
        }
    }
}