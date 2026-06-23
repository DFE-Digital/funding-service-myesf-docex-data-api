using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Converters;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Converters
{
    [TestClass]
    public class AgencyDocumentErrorTypeToDescriptionConverterTests
    {
        private readonly AgencyDocumentErrorTypeConverter _converter
            = new AgencyDocumentErrorTypeConverter();

        [TestMethod, TestCategory("Unit")]
        [DataRow(AgencyDocumentErrorType.NoError, "No error found")]
        [DataRow(AgencyDocumentErrorType.DocumentNameInvalidFormat, "File name is not a valid format")]
        [DataRow(AgencyDocumentErrorType.DocumentNameContainsInvalidCharacters, "The document name contains invalid characters")]
        [DataRow(AgencyDocumentErrorType.ProductIdentifierInvalidFormat, "Document type code is not a valid format")]
        [DataRow(AgencyDocumentErrorType.OrganisationIdentifierInvalidFormat, "UKPRN does not contain 8 numbers")]
        [DataRow(AgencyDocumentErrorType.OrganisationIdentifierNotRecognised, "UKPRN is not recognised in the system")]
        [DataRow(AgencyDocumentErrorType.AcademicYearInvalidFormat, "Academic year is not a valid format")]
        [DataRow(AgencyDocumentErrorType.TeamNotAuthorisedToPublish, "Team is not authorised to publish")]
        public void Convert_ReturnsExpectedDescription(AgencyDocumentErrorType errorType, string expectedDescription)
        {
            // Act
            var result = _converter.Convert(errorType);

            // Assert
            result.Should().Be(expectedDescription);
        }
    }
}