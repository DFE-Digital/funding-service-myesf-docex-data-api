using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces.Converters;

namespace Pds.DocumentExchange.Data.Services.Implementations.Converters
{
    /// <summary>
    /// Converts AgencyDocumentErrorType values to their corresponding description.
    /// </summary>
    public class AgencyDocumentErrorTypeConverter : IConvertAgencyDocumentErrorTypes
    {
        /// <inheritdoc/>
        public string Convert(AgencyDocumentErrorType input)
            => input switch
            {
                AgencyDocumentErrorType.NoError => "No error found",
                AgencyDocumentErrorType.DocumentNameInvalidFormat => "File name is not a valid format",
                AgencyDocumentErrorType.DocumentNameContainsInvalidCharacters => "The document name contains invalid characters",
                AgencyDocumentErrorType.ProductIdentifierInvalidFormat => "Document type code is not a valid format",
                AgencyDocumentErrorType.OrganisationIdentifierInvalidFormat => "UKPRN does not contain 8 numbers",
                AgencyDocumentErrorType.OrganisationIdentifierNotRecognised => "UKPRN is not recognised in the system",
                AgencyDocumentErrorType.AcademicYearInvalidFormat => "Academic year is not a valid format",
                AgencyDocumentErrorType.TeamNotAuthorisedToPublish => "Team is not authorised to publish",
                _ => "Unknown error type"
            };
    }
}