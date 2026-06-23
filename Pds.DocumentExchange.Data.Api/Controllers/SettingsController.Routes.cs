namespace Pds.DocumentExchange.Data.Api.Controllers
{
    /// <inheritdoc cref="SettingsController"/>
    public partial class SettingsController
    {
        private static class Routes
        {
            public const string Products = "/api/[controller]/products";
            public const string Product = "/api/[controller]/product/{identifier}";
            public const string ProductsPut = "/api/[controller]/products/{oldIdentifier}";
            public const string Teams = "/api/[controller]/teams";
            public const string TeamsPut = "/api/[controller]/teams/{oldIdentifier}";
            public const string DocumentExchangeEnabled = "/api/[controller]/document-exchange-enabled";
            public const string OrganisationUploadEnabled = "/api/[controller]/organisation-upload-enabled";
            public const string FileExtensions = "/api/[controller]/file-extensions";
            public const string FileExtensionsPut = "/api/[controller]/file-extensions/{oldIdentifier}";
            public const string ServiceReplyEmail = "/api/[controller]/service-reply-email";
            public const string ServiceNoReplyEmail = "/api/[controller]/service-no-reply-email";
            public const string MaxFileUploadSize = "/api/[controller]/max-file-upload-size";
            public const string AgencyPublishCompleteProviderNotificationEnabled = "/api/[controller]/agency-publish-complete-provider-notification-enabled";
            public const string EmailSettings = "/api/[controller]/email-settings";
            public const string EmailSettingsPut = "/api/[controller]/email-settings/{oldEmailType}";
        }
    }
}