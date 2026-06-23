using Pds.DocumentExchange.Data.Services.DTOs;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Settings
{
    /// <summary>
    /// Interface providing methods to add and update configuration data.
    /// </summary>
    public interface ISettingsWriter
    {
        /// <summary>
        /// Adds or updates a Document Exchange product.
        /// </summary>
        /// <param name="oldIdentifier">The old product identifier.</param>
        /// <param name="newProductValue">The new product to add or update.</param>
        /// <returns>The product that was added or updated.</returns>
        Task<Product> AddOrUpdateProduct(int oldIdentifier, Product newProductValue);

        /// <summary>
        /// Adds or updates an agency team.
        /// </summary>
        /// <param name="oldIdentifier">The old team identifier.</param>
        /// <param name="newTeamValue">The new team to add or update.</param>
        /// <returns>The team that was added or updated.</returns>
        Task<AgencyTeam> AddOrUpdateTeam(string oldIdentifier, AgencyTeam newTeamValue);

        /// <summary>
        /// Adds or updates a supported file extension.
        /// </summary>
        /// <param name="oldIdentifier">The old file identifier.</param>
        /// <param name="newFileExtensionValue">The new file extension to add or update.</param>
        /// <returns>The file extension that was added or updated.</returns>
        Task<FileExtensionInfo> AddOrUpdateFileExtension(string oldIdentifier, FileExtensionInfo newFileExtensionValue);

        /// <summary>
        /// Sets a value indicating whether the Document Exchange service is enabled.
        /// </summary>
        /// <param name="isEnabled">The new configuration value.</param>
        /// <returns>The configuration value after the operation was completed.</returns>
        Task<bool> SetDocumentExchangeEnabledStatus(bool isEnabled);

        /// <summary>
        /// Sets a value indicating whether organisations are allowed to upload new documents.
        /// </summary>
        /// <param name="isEnabled">The new configuration value.</param>
        /// <returns>The configuration value after the operation was completed.</returns>
        Task<bool> SetOrganisationUploadEnabledStatus(bool isEnabled);

        /// <summary>
        /// Sets the service reply email.
        /// </summary>
        /// <param name="email">The new configuration value.</param>
        /// <returns>The configuration value after the operation was completed.</returns>
        Task<string> SetServiceReplyEmail(string email);

        /// <summary>
        /// Sets the service no-reply email.
        /// </summary>
        /// <param name="email">The new configuration value.</param>
        /// <returns>The configuration value after the operation was completed.</returns>
        Task<string> SetServiceNoReplyEmail(string email);

        /// <summary>
        /// Upserts an email setting.
        /// </summary>
        /// <param name="oldEmailType">The old Email Type.</param>
        /// <param name="newEmailSetting">The new email setting to add or update.</param>
        /// <returns>The configuration value after the operation was completed.</returns>
        Task<EmailSetting> SetEmailSetting(string oldEmailType, EmailSetting newEmailSetting);

        /// <summary>
        /// Sets the agency publish complete provider notification status.
        /// </summary>
        /// <param name="isEnabled">The new configuration value.</param>
        /// <returns>The configuration value after the operation was completed.</returns>
        Task<bool> SetAgencyPublishCompleteNotificationsStatus(bool isEnabled);

        /// <summary>
        /// Sets the maximum file upload size in kilobytes.
        /// </summary>
        /// <param name="fileUploadSize">The new configuration value in kilobytes.</param>
        /// <returns>The configuration value after the operation was completed.</returns>
        Task<long> SetMaxFileUploadSize(long fileUploadSize);
    }
}