using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Controllers
{
    /// <summary>The Settings Controller.</summary>
    [ApiController]
    public partial class SettingsController : BaseApiController
    {
        private readonly IAdminSettingsService _adminSettingsService;
        private readonly IMapper _mapper;
        private readonly ILoggerAdapter<SettingsController> _logger;

        /// <summary>Initializes a new instance of the <see cref="SettingsController" /> class.</summary>
        /// <param name="adminSettingsService">The admin settings service.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="logger">The logger.</param>
        public SettingsController(
            IAdminSettingsService adminSettingsService,
            IMapper mapper,
            ILoggerAdapter<SettingsController> logger)
        {
            _adminSettingsService = adminSettingsService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>Products that the organisation can upload.</summary>
        /// <returns>The action result containing the list of supported products if successful.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<IEnumerable<Product>>> ProductsThatOrganisationsCanUpload()
        {
            try
            {
                _logger.LogTrace("Settings controller getting products that organisations can upload from configuration...");

                var allowedProducts = await _adminSettingsService.GetProductsThatOrganisationsCanUpload();

                _logger.LogTrace($"Settings controller getting products that organisations can upload from configuration... Done with {allowedProducts?.Count()} products.");

                if (allowedProducts?.Any() != true)
                {
                    return NoContent();
                }

                return Ok(_mapper.Map<IEnumerable<Product>>(allowedProducts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(ProductsThatOrganisationsCanUpload)}");
                throw;
            }
        }

        /// <summary>
        /// Gets the list of all products.
        /// </summary>
        /// <returns>The list of all products.</returns>
        [HttpGet(Routes.Products)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                _logger.LogTrace("Settings controller getting all products from configuration...");

                var products = await _adminSettingsService.GetProducts();
                var mappedProducts = _mapper.Map<IEnumerable<Product>>(products);

                _logger.LogTrace($"Settings controller getting all products from configuration... Done with {mappedProducts.Count()} products.");

                return Ok(mappedProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(GetProducts)}");
                throw;
            }
        }

        /// <summary>
        /// Gets a product by its identifier.
        /// </summary>
        /// <param name="identifier">The product identifier.</param>
        /// <returns>The product.</returns>
        [HttpGet(Routes.Product)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Product>> GetProduct(int identifier)
        {
            try
            {
                _logger.LogTrace($"Settings controller getting the product {identifier} from configuration...");

                var product = await _adminSettingsService.GetProduct(identifier);
                var mappedProduct = _mapper.Map<Product>(product);

                _logger.LogTrace($"Settings controller getting the product {identifier} from configuration... Done.");

                return Ok(mappedProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(GetProduct)} for identifier: {identifier}");
                throw;
            }
        }

        /// <summary>
        /// Adds or updates a product.
        /// </summary>
        /// <param name="oldIdentifier">The old product identifier.</param>
        /// <param name="newProductValue">The new product to add or update.</param>
        /// <returns>The product that was added or updated.</returns>
        [HttpPut(Routes.ProductsPut)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult<Product>> AddOrUpdateProduct([FromRoute] int oldIdentifier, [FromBody] Product newProductValue)
        {
            try
            {
                _logger.LogTrace($"Settings controller adding or updating product {oldIdentifier} in configuration...");

                var serviceProduct = _mapper.Map<Services.DTOs.Product>(newProductValue);

                var product = await _adminSettingsService.AddOrUpdateProduct(oldIdentifier, serviceProduct);

                var mappedProduct = _mapper.Map<Product>(product);

                _logger.LogTrace($"Settings controller adding or updating product {oldIdentifier} in configuration... Done.");

                return Accepted(mappedProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(AddOrUpdateProduct)} for oldIdentifier: {oldIdentifier} and newProductValue: {JsonSerializer.Serialize(newProductValue)}");
                throw;
            }
        }

        /// <summary>
        /// Gets the list of all agency teams.
        /// </summary>
        /// <returns>The list of all teams.</returns>
        [HttpGet(Routes.Teams)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AgencyTeam>>> GetTeams()
        {
            try
            {
                _logger.LogTrace("Settings controller getting all teams from configuration...");

                var teams = await _adminSettingsService.GetTeams();
                var mappedTeam = _mapper.Map<IEnumerable<AgencyTeam>>(teams);

                _logger.LogTrace($"Settings controller getting all teams from configuration... Done with {mappedTeam.Count()} teams.");

                return Ok(mappedTeam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(GetTeams)}");
                throw;
            }
        }

        /// <summary>
        /// Adds or updates an agency team.
        /// </summary>
        /// <param name="oldIdentifier">The old agency team identifier.</param>
        /// <param name="newAgencyTeamValue">The new agency team to add or update.</param>
        /// <returns>The agency team that was added or updated.</returns>
        [HttpPut(Routes.TeamsPut)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult<AgencyTeam>> AddOrUpdateTeam([FromRoute] string oldIdentifier, [FromBody] AgencyTeam newAgencyTeamValue)
        {
            try
            {
                _logger.LogTrace($"Settings controller adding or updating team {oldIdentifier} in configuration...");

                var serviceTeam = _mapper.Map<Services.DTOs.AgencyTeam>(newAgencyTeamValue);
                var result = await _adminSettingsService.AddOrUpdateTeam(oldIdentifier, serviceTeam);

                var mappedTeam = _mapper.Map<AgencyTeam>(result);

                _logger.LogTrace($"Settings controller adding or updating team {oldIdentifier} in configuration... Done.");

                return Accepted(mappedTeam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(AddOrUpdateTeam)} for oldIdentifier: {oldIdentifier} and newAgencyTeamValue: {JsonSerializer.Serialize(newAgencyTeamValue)}");
                throw;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the Document Exchange service is enabled.
        /// </summary>
        /// <returns>The value indicating whether the Document Exchange service is enabled.</returns>
        [HttpGet(Routes.DocumentExchangeEnabled)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> IsDocumentExchangeEnabled()
        {
            try
            {
                _logger.LogTrace("Settings controller getting document exchange enabled from configuration...");

                var result = await _adminSettingsService.IsDocumentExchangeEnabled();

                _logger.LogTrace("Settings controller getting document exchange enabled from configuration... Done.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(IsDocumentExchangeEnabled)}");
                throw;
            }
        }

        /// <summary>
        /// Sets the value indicating whether the Document Exchange service is enabled.
        /// </summary>
        /// <param name="enabled">The new value to set.</param>
        /// <returns>The value indicating whether the Document Exchange service is enabled.</returns>
        [HttpPut(Routes.DocumentExchangeEnabled)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult<bool>> SetDocumentExchangeEnabledStatus([FromBody] bool enabled)
        {
            try
            {
                _logger.LogTrace($"Settings controller setting document exchange enabled to {enabled} in configuration...");

                var result = await _adminSettingsService.SetDocumentExchangeEnabledStatus(enabled);

                _logger.LogTrace($"Settings controller setting document exchange enabled to {enabled} in configuration... Done.");

                return Accepted(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(SetDocumentExchangeEnabledStatus)}");
                throw;
            }
        }

        /// <summary>
        /// Gets the value indicating whether organisations are allowed to upload new documents.
        /// </summary>
        /// <returns>The value indicating whether organisations are allowed to upload new documents.</returns>
        [HttpGet(Routes.OrganisationUploadEnabled)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> IsOrganisationUploadEnabled()
        {
            try
            {
                _logger.LogTrace("Settings controller getting document exchange organisation upload enabled from configuration...");

                var result = await _adminSettingsService.IsOrganisationUploadEnabled();

                _logger.LogTrace("Settings controller getting document exchange organisation upload enabled from configuration... Done.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(IsOrganisationUploadEnabled)}");
                throw;
            }
        }

        /// <summary>
        /// Sets the value indicating whether organisations are allowed to upload new documents.
        /// </summary>
        /// <param name="enabled">The new value to set.</param>
        /// <returns>The value indicating whether organisations are allowed to upload new documents.</returns>
        [HttpPut(Routes.OrganisationUploadEnabled)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult<bool>> SetOrganisationUploadEnabledStatus([FromBody] bool enabled)
        {
            try
            {
                _logger.LogTrace($"Settings controller setting document exchange organisation upload enabled to {enabled} in configuration...");

                var result = await _adminSettingsService.SetOrganisationUploadEnabledStatus(enabled);

                _logger.LogTrace($"Settings controller setting document exchange organisation upload enabled to {enabled} in configuration... Done.");

                return Accepted(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(SetOrganisationUploadEnabledStatus)}");
                throw;
            }
        }

        /// <summary>
        /// Gets the list of all supported file extensions.
        /// </summary>
        /// <returns>The list of all supported file extensions.</returns>
        [HttpGet(Routes.FileExtensions)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FileExtensionInfo>>> GetFileExtensions()
        {
            try
            {
                _logger.LogTrace("Settings controller getting all allowed file extensions from configuration...");

                var extensions = await _adminSettingsService.GetFileExtensions();
                var mappedExtensions = _mapper.Map<IEnumerable<FileExtensionInfo>>(extensions);

                _logger.LogTrace("Settings controller getting all allowed file extensions from configuration... Done.");

                return Ok(mappedExtensions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(GetFileExtensions)}");
                throw;
            }
        }

        /// <summary>
        /// Adds or updates a supported file extension.
        /// </summary>
        /// <param name="oldIdentifier">The old file identifier.</param>
        /// <param name="newFileExtensionValue">The new file extension to add or update.</param>
        /// <returns>The file extension that was added or updated.</returns>
        [HttpPut(Routes.FileExtensionsPut)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult<FileExtensionInfo>> AddOrUpdateFileExtension([FromRoute] string oldIdentifier, [FromBody] FileExtensionInfo newFileExtensionValue)
        {
            try
            {
                _logger.LogTrace($"Settings controller adding or updating file extension {oldIdentifier} in configuration...");

                var serviceFileExtension = _mapper.Map<Services.DTOs.FileExtensionInfo>(newFileExtensionValue);
                var result = await _adminSettingsService.AddOrUpdateFileExtension(oldIdentifier, serviceFileExtension);

                var mappedExtension = _mapper.Map<FileExtensionInfo>(result);

                _logger.LogTrace($"Settings controller adding or updating file extension {oldIdentifier} in configuration... Done.");

                return Accepted(mappedExtension);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(AddOrUpdateFileExtension)} for oldIdentifier: {oldIdentifier} and newFileExtensionValue: {JsonSerializer.Serialize(newFileExtensionValue)}");
                throw;
            }
        }

        /// <summary>
        /// Gets the service reply email.
        /// </summary>
        /// <returns>The service reply email.</returns>
        [HttpGet(Routes.ServiceReplyEmail)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> GetServiceReplyEmail()
        {
            try
            {
                _logger.LogTrace("Settings controller getting service reply email from configuration...");

                var result = await _adminSettingsService.GetServiceReplyEmail();

                _logger.LogTrace("Settings controller getting service reply email from configuration... Done.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(GetServiceReplyEmail)}");
                throw;
            }
        }

        /// <summary>
        /// Sets the service reply email.
        /// </summary>
        /// <param name="email">The new value to set.</param>
        /// <returns>The service reply email.</returns>
        [HttpPut(Routes.ServiceReplyEmail)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult<string>> SetServiceReplyEmail([FromBody] string email)
        {
            try
            {
                _logger.LogTrace($"Settings controller setting service reply email to {email} in configuration...");

                var result = await _adminSettingsService.SetServiceReplyEmail(email);

                _logger.LogTrace($"Settings controller setting service reply email to {email} in configuration... Done.");

                return Accepted(value: result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(SetServiceReplyEmail)} with email: {email}");
                throw;
            }
        }

        /// <summary>
        /// Gets the service no-reply email.
        /// </summary>
        /// <returns>The service no-reply email.</returns>
        [HttpGet(Routes.ServiceNoReplyEmail)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> GetServiceNoReplyEmail()
        {
            try
            {
                _logger.LogTrace("Settings controller getting service no reply email from configuration...");

                var result = await _adminSettingsService.GetServiceNoReplyEmail();

                _logger.LogTrace("Settings controller getting service no reply email from configuration... Done.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(GetServiceNoReplyEmail)}");
                throw;
            }
        }

        /// <summary>
        /// Sets the service no-reply email.
        /// </summary>
        /// <param name="email">The new value to set.</param>
        /// <returns>The service no-reply email.</returns>
        [HttpPut(Routes.ServiceNoReplyEmail)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult<string>> SetServiceNoReplyEmail([FromBody] string email)
        {
            try
            {
                _logger.LogTrace($"Settings controller setting service no reply email to {email} in configuration...");

                var result = await _adminSettingsService.SetServiceNoReplyEmail(email);

                _logger.LogTrace($"Settings controller setting service no reply email to {email} in configuration... Done,");

                return Accepted(value: result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(SetServiceNoReplyEmail)} with email: {email}");
                throw;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the agency publish complete provider notification is enabled.
        /// </summary>
        /// <returns>The value indicating whether the agency publish complete provider notification is enabled.</returns>
        [HttpGet(Routes.AgencyPublishCompleteProviderNotificationEnabled)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> IsAgencyPublishCompleteProviderNotificationEnabled()
        {
            try
            {
                _logger.LogTrace("Settings controller getting agency publish complete provider notification from configuration...");

                var result = await _adminSettingsService.IsAgencyPublishCompleteProviderNotificationEnabled();

                _logger.LogTrace("Settings controller getting agency publish complete provider notification from configuration... Done.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(IsAgencyPublishCompleteProviderNotificationEnabled)}");
                throw;
            }
        }

        /// <summary>
        /// Sets the value indicating whether the Agency Publish complete notification is enabled.
        /// </summary>
        /// <param name="enabled">The new value to set.</param>
        /// <returns>The value indicating whether the Agency Publish complete notification is enabled.</returns>
        [HttpPut(Routes.AgencyPublishCompleteProviderNotificationEnabled)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult<bool>> SetAgencyPublishCompleteNotificationsStatus([FromBody] bool enabled)
        {
            try
            {
                _logger.LogTrace($"Settings controller setting agency publish complete provider notification to {enabled} in configuration...");

                var result = await _adminSettingsService.SetAgencyPublishCompleteNotificationsStatus(enabled);

                _logger.LogTrace($"Settings controller setting agency publish complete provider notification to {enabled} in configuration... Done.");

                return Accepted(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(SetAgencyPublishCompleteNotificationsStatus)}");
                throw;
            }
        }

        /// <summary>
        /// Gets the maximum file upload size in kilobytes.
        /// </summary>
        /// <returns>The maximum file upload size in kilobytes.</returns>
        [HttpGet(Routes.MaxFileUploadSize)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetMaxFileUploadSize()
        {
            _logger.LogTrace("Settings controller getting maximum file upload size from configuration...");

            var result = await _adminSettingsService.GetMaxFileUploadSize();

            _logger.LogTrace("Settings controller getting maximum file upload size from configuration... Done.");

            return Ok(result);
        }

        /// <summary>
        /// Sets the maximum file upload size in kilobytes.
        /// </summary>
        /// <param name="fileUploadSize">The new configuration value in kilobytes.</param>
        /// <returns>The configuration value after the operation was completed.</returns>
        [HttpPut(Routes.MaxFileUploadSize)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult<bool>> SetMaxFileUploadSize([FromBody] long fileUploadSize)
        {
            _logger.LogTrace($"Settings controller setting maximum file size for upload to {fileUploadSize} in configuration...");

            var result = await _adminSettingsService.SetMaxFileUploadSize(fileUploadSize);

            _logger.LogTrace($"Settings controller setting maximum file size for upload to {fileUploadSize} in configuration... Done.");

            return Accepted(result);
        }

        /// <summary>
        /// Gets the email settings.
        /// </summary>
        /// <returns>The email settings.</returns>
        [HttpGet(Routes.EmailSettings)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmailSetting>>> GetEmailSettings()
        {
            try
            {
                _logger.LogTrace($"Settings controller getting email settings from configuration...");

                var emailSettings = await _adminSettingsService.GetEmailSettings();
                var mappedemailSettings = _mapper.Map<IEnumerable<EmailSetting>>(emailSettings);

                _logger.LogTrace($"Settings controller getting email settings from configuration... Done.");

                return Ok(mappedemailSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(GetEmailSettings)}");
                throw;
            }
        }

        /// <summary>
        /// Adds or updates a product.
        /// </summary>
        /// <param name="oldEmailType">The old email type.</param>
        /// <param name="newEmailSettingValue">The new email setting to add or update.</param>
        /// <returns>The product that was added or updated.</returns>
        [HttpPut(Routes.EmailSettingsPut)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult<EmailSetting>> AddOrUpdateEmailSetting([FromRoute] string oldEmailType, [FromBody] EmailSetting newEmailSettingValue)
        {
            try
            {
                _logger.LogTrace($"Settings controller adding or updating email setting {oldEmailType} in configuration...");

                var serviceEmailSetting = _mapper.Map<Services.DTOs.EmailSetting>(newEmailSettingValue);

                var emailSetting = await _adminSettingsService.SetEmailSetting(oldEmailType, serviceEmailSetting);

                var mappedemailSetting = _mapper.Map<Product>(emailSetting);

                _logger.LogTrace($"Settings controller adding or updating email setting {oldEmailType} in configuration... Done.");

                return Accepted(mappedemailSetting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(AddOrUpdateEmailSetting)} for oldEmailType: {oldEmailType} and newEmailSettingValue: {JsonSerializer.Serialize(newEmailSettingValue)}");
                throw;
            }
        }
    }
}