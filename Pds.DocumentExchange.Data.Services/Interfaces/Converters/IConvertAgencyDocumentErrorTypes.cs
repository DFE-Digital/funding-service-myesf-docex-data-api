using Pds.DocumentExchange.Data.Services.Enums;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Converters
{
    /// <summary>
    /// Interface providing a method to convert an AgencyDocumentErrorType enum value to a string.
    /// </summary>
    public interface IConvertAgencyDocumentErrorTypes : IConverter<AgencyDocumentErrorType, string>
    {
    }
}