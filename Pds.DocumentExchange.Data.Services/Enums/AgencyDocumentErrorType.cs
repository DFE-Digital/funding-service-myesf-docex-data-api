namespace Pds.DocumentExchange.Data.Services.Enums
{
    /// <summary>
    /// Enumeration of the different error types that can be found in an agency's document.
    /// </summary>
    public enum AgencyDocumentErrorType
    {
        /// <summary>
        /// No error.
        /// </summary>
        NoError,

        /// <summary>
        /// Document name invalid format.
        /// </summary>
        DocumentNameInvalidFormat,

        /// <summary>
        /// Document contains invalid characters.
        /// </summary>
        DocumentNameContainsInvalidCharacters,

        /// <summary>
        /// Document extension not accepted.
        /// </summary>
        DocumentExtensionNotAccepted,

        /// <summary>
        /// Product identifier invalid format.
        /// </summary>
        ProductIdentifierInvalidFormat,

        /// <summary>
        /// Organisation identifier invalid format.
        /// </summary>
        OrganisationIdentifierInvalidFormat,

        /// <summary>
        /// Organisation identifier not recognised.
        /// </summary>
        OrganisationIdentifierNotRecognised,

        /// <summary>
        /// Academic year invalid format.
        /// </summary>
        AcademicYearInvalidFormat,

        /// <summary>
        /// Team not authorised to publish.
        /// </summary>
        TeamNotAuthorisedToPublish
    }
}