using Pds.DocumentExchange.Data.Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Settings
{
    /// <summary>
    /// Interface providing methods to read configuration data.
    /// </summary>
    public interface ISettingsReader
    {
        /// <summary>
        /// Gets a list of the Document Exchange products.
        /// </summary>
        /// <returns>A collection containing the Document Exchange products.</returns>
        Task<IReadOnlyCollection<Product>> GetProducts();

        /// <summary>
        /// Gets a list of the agency teams.
        /// </summary>
        /// <returns>A collection containing the agency teams.</returns>
        Task<IReadOnlyCollection<AgencyTeam>> GetTeams();

        /// <summary>
        /// Gets the list of supported file extensions.
        /// </summary>
        /// <returns>A collection containing the supported file extensions.</returns>
        Task<IReadOnlyCollection<FileExtensionInfo>> GetFileExtensions();

        /// <summary>
        /// Gets a value indicating whether the Document Exchange service is enabled.
        /// </summary>
        /// <returns>A value indicating whether the Document Exchange service is enabled.</returns>
        Task<bool> IsDocumentExchangeEnabled();

        /// <summary>
        /// Gets a value indicating whether organisations are allowed to upload new documents.
        /// </summary>
        /// <returns>A value indicating whether organisations are allowed to upload new documents.</returns>
        Task<bool> IsOrganisationUploadEnabled();

        /// <summary>
        /// Gets the service reply email.
        /// </summary>
        /// <returns>The service reply email address.</returns>
        Task<string> GetServiceReplyEmail();

        /// <summary>
        /// Gets the service no-reply email.
        /// </summary>
        /// <returns>The service no-reply email address.</returns>
        Task<string> GetServiceNoReplyEmail();

        /// <summary>
        /// Determines whether agency publish complete provider notification is enabled.
        /// </summary>
        /// <returns>A value indicating whether agency publish notifications is enabled.</returns>
        Task<bool> IsAgencyPublishCompleteProviderNotificationEnabled();

        /// <summary>
        /// Gets the maximum file upload size in kilobytes.
        /// </summary>
        /// <returns>The maximum file upload size in kilobytes.</returns>
        Task<int> GetMaxFileUploadSize();

        /// <summary>
        /// Gets the email settings.
        /// </summary>
        /// <returns>The configured settings for email types.</returns>
        Task<IReadOnlyCollection<EmailSetting>> GetEmailSettings();
    }
}
