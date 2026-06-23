using Microsoft.Extensions.Options;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.Constants;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Exceptions;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Providers;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Factories
{
    /// <summary>
    /// The notification message body factory (implementation).
    /// </summary>
    public sealed class NotificationMessageBodyFactory :
        ICreateNotificationMessageBodies
    {
        /// <summary>
        /// Gets the presentation formatter.
        /// </summary>
        internal IProvidePresentationFormatting Formatter { get; }

        /// <summary>
        /// Gets the organisation lookup.
        /// </summary>
        internal IOrganisationsLookup OrganisationsLookup { get; }

        /// <summary>
        /// Gets the configuration data service.
        /// </summary>
        internal IConfigurationDataService ConfigurationDataService { get; }

        /// <summary>
        /// Gets the service base URL.
        /// </summary>
        internal string ServiceBaseURL { get; }

        private readonly ILoggerAdapter<NotificationMessageBodyFactory> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationMessageBodyFactory"/> class.
        /// </summary>
        /// <param name="presentationFormatter">the presentation formatter.</param>
        /// <param name="organisationsLookup">The organisations lookup.</param>
        /// <param name="configurationDataService">The configuration data service.</param>
        /// <param name="agencyConfiguration">the agency configuration.</param>
        /// <param name="logger">The logger.</param>
        public NotificationMessageBodyFactory(
            IProvidePresentationFormatting presentationFormatter,
            IOrganisationsLookup organisationsLookup,
            IConfigurationDataService configurationDataService,
            IOptions<AgencyServiceConfiguration> agencyConfiguration,
            ILoggerAdapter<NotificationMessageBodyFactory> logger)
        {
            It.IsNull(presentationFormatter)
                .AsGuard<ArgumentNullException>();
            It.IsNull(organisationsLookup)
                .AsGuard<ArgumentNullException>();
            It.IsNull(configurationDataService)
               .AsGuard<ArgumentNullException>();
            It.IsNull(agencyConfiguration)
                .AsGuard<ArgumentNullException>();

            Formatter = presentationFormatter;
            OrganisationsLookup = organisationsLookup;
            ConfigurationDataService = configurationDataService;

            var config = agencyConfiguration.Value;
            ServiceBaseURL = config.ServiceBaseURL;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string> BuildMessageContentFrom(string bodyTemplate, IBatchMetadataAnalysis batchAnalysis)
        {
            try
            {
                var documentListSb = new StringBuilder();
                var documentListUiSb = new StringBuilder();
                var documentTypesSb = new StringBuilder();
                var documentTypesULSb = new StringBuilder();

                var files = batchAnalysis.GetAllFiles();

                It.IsEmpty(files)
                    .AsGuard<MessageBodyFactoryCardinalityException>(batchAnalysis.ParentBatchID);

                files.ForEach(file =>
                {
                    documentListSb.AppendLine($"{file.OriginalFileName}<br/>");
                    documentListUiSb.AppendLine($"{Formatter.FormatProductNameWithFileExtension(batchAnalysis.GetProductNameFor(file.ProductIdentifier), file.OriginalFileName)}<br/>");
                });

                var productGroups = files
                    .GroupBy(theFile => theFile.ProductIdentifier)
                    .Select(theGroup => FormatGroupingDetailsFor(theGroup, batchAnalysis));

                var plainDetails = string.Join(", ", productGroups);
                documentTypesSb.Append(plainDetails);

                var listDetails = $"<li>{string.Join("</li><li>", productGroups)}</li>";
                documentTypesULSb.Append($"<ul>{listDetails}</ul>");

                var requiresOrganisation = bodyTemplate.Contains(MessageBodyTag.ProviderName, StringComparison.OrdinalIgnoreCase);
                var orgDetail = requiresOrganisation
                    ? await GetOrganisation(batchAnalysis)
                    : default;

                var firstFile = files.First();
                var productName = batchAnalysis.GetProductNameFor(firstFile.ProductIdentifier);
                var docNameUI = Formatter.FormatProductNameWithFileExtension(productName, firstFile.OriginalFileName);
                var createdOn = batchAnalysis.InitialBatchDate;

                var replyEmailAddress = await ConfigurationDataService.GetServiceReplyEmail();

                return bodyTemplate
                    .Replace(MessageBodyTag.DocumentName, firstFile.OriginalFileName)
                    .Replace(MessageBodyTag.DocumentNameUI, docNameUI)
                    .Replace(MessageBodyTag.DocumentListUL, $"{documentListSb}")
                    .Replace(MessageBodyTag.DocumentListUIUL, $"{documentListUiSb}")
                    .Replace(MessageBodyTag.DocumentType, productName)
                    .Replace(MessageBodyTag.DocumentTypes, $"{documentTypesSb}")
                    .Replace(MessageBodyTag.DocumentTypesUL, $"{documentTypesULSb}")
                    .Replace(MessageBodyTag.DocumentCount, $"{files.Count}")
                    .Replace(MessageBodyTag.ProviderName, orgDetail?.Name)
                    .Replace(MessageBodyTag.SendDateTime, $"{createdOn:h:mmtt, d MMMM yyyy}")
                    .Replace(MessageBodyTag.SendDate, $"{createdOn:d MMMM yyyy}")
                    .Replace(MessageBodyTag.ReplyEmail, replyEmailAddress)
                    .Replace(MessageBodyTag.BaseUrl, ServiceBaseURL);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(BuildMessageContentFrom)} for parentBatchId: {batchAnalysis?.ParentBatchID}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> BuildMessageContentFrom(string bodyTemplate, IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>> productGroups) =>
            await Task.Run(() =>
            {
                var documentCount = productGroups.Sum(item => item.Value);
                var documentTypeDetails = string.Empty;

                productGroups.ForEach(groupItem =>
                {
                    var itemName = GetProductName(groupItem);

                    documentTypeDetails += documentCount == 1
                        ? $"You have {groupItem.Value} new {itemName}"
                        : $"<li>{groupItem.Value} {itemName}</li>";
                });

                return bodyTemplate
                    .Replace(MessageBodyTag.NumberOfDocuments, $"{documentCount}")
                    .Replace(MessageBodyTag.DocumentTypeDetails, documentTypeDetails)
                    .Replace(MessageBodyTag.BaseUrl, ServiceBaseURL);
            });

        /// <inheritdoc/>
        public async Task<Dictionary<string, dynamic>> BuildEmailPersonalisationFrom(IBatchMetadataAnalysis batchAnalysis, bool isForInfectedFiles, string recipientName)
        {
            try
            {
                var documentListSb = new StringBuilder();
                var documentListUiSb = new StringBuilder();
                var documentTypesSb = new StringBuilder();
                var documentTypesULSb = new StringBuilder();

                var files = batchAnalysis.GetAllFiles();

                It.IsEmpty(files)
                    .AsGuard<MessageBodyFactoryCardinalityException>(batchAnalysis.ParentBatchID);

                if (isForInfectedFiles)
                {
                    batchAnalysis.InfectedFiles.ForEach(file =>
                    {
                        documentListSb.AppendLine($"* {file.OriginalFileName}");
                        documentListUiSb.AppendLine($"{Formatter.FormatProductNameWithFileExtension(batchAnalysis.GetProductNameFor(file.ProductIdentifier), file.OriginalFileName)}");
                    });
                }
                else
                {
                    files.ForEach(file =>
                    {
                        documentListSb.AppendLine($"* {file.OriginalFileName}");
                        documentListUiSb.AppendLine($"{Formatter.FormatProductNameWithFileExtension(batchAnalysis.GetProductNameFor(file.ProductIdentifier), file.OriginalFileName)}");
                    });
                }

                var productGroups = files
                    .GroupBy(theFile => theFile.ProductIdentifier)
                    .Select(theGroup => FormatGroupingDetailsFor(theGroup, batchAnalysis));

                var plainDetails = string.Join(", ", productGroups);
                documentTypesSb.Append(plainDetails);

                documentTypesULSb.Append(plainDetails);

                var firstFile = files.First();
                var productName = batchAnalysis.GetProductNameFor(firstFile.ProductIdentifier);
                var docNameUI = Formatter.FormatProductNameWithFileExtension(productName, firstFile.OriginalFileName);
                var createdOn = batchAnalysis.InitialBatchDate;

                var replyEmailAddress = await ConfigurationDataService.GetServiceReplyEmail();

                var orgDetail = !firstFile.IsFromAgency
                     ? await GetOrganisation(batchAnalysis)
                     : null;

                string documentNumberText = string.Empty;
                if (isForInfectedFiles)
                {
                    documentNumberText = batchAnalysis.InfectedFiles.Count > 1 ? $"{batchAnalysis.InfectedFiles.Count} documents" : "1 document";
                }
                else
                {
                    documentNumberText = batchAnalysis.ClearFiles.Count > 1 ? $"{batchAnalysis.ClearFiles.Count} documents" : "1 document";
                }

                var personalisationList = new Dictionary<string, dynamic>
                {
                    { MessageBodyTag.DocumentName, firstFile.OriginalFileName },
                    { MessageBodyTag.DocumentNameUI, docNameUI },
                    { MessageBodyTag.DocumentListUL, $"{documentListSb}" },
                    { MessageBodyTag.DocumentListUIUL, $"{documentListUiSb}" },
                    { MessageBodyTag.DocumentType, productName },
                    { MessageBodyTag.DocumentTypes, $"{documentTypesSb}" },
                    { MessageBodyTag.DocumentTypesUL, $"{documentTypesULSb}" },
                    { MessageBodyTag.DocumentCount, $"{files.Count}" },
                    { MessageBodyTag.SendDateTime, $"{createdOn:h:mmtt, d MMMM yyyy}" },
                    { MessageBodyTag.SendDate, $"{createdOn:d MMMM yyyy}" },
                    { MessageBodyTag.ReplyEmail, replyEmailAddress },
                    { MessageBodyTag.BaseUrl, ServiceBaseURL },
                    { MessageBodyTag.DocumentNumberText, documentNumberText },
                    { MessageBodyTag.RecipientName, recipientName }
                };

                if (orgDetail != null)
                {
                    personalisationList.Add(MessageBodyTag.ProviderName, orgDetail?.Name ?? "Unknown organisation");
                }

                return personalisationList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(BuildEmailPersonalisationFrom)} for parentBatchId: {batchAnalysis?.ParentBatchID}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, dynamic>> BuildEmailPersonalisationFrom(IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>> productGroups) =>
            await Task.Run(() =>
            {
                var documentCount = productGroups.Sum(item => item.Value);
                var documentTypeDetailsSb = new StringBuilder();

                productGroups.ForEach(groupItem =>
                {
                    var itemName = GetProductName(groupItem);

                    documentTypeDetailsSb.AppendLine(documentCount == 1
                        ? $"# You have {groupItem.Value} new {itemName}"
                        : $"* {groupItem.Value} {itemName}");
                });

                return new Dictionary<string, dynamic>
                {
                    { MessageBodyTag.NumberOfDocuments, $"{documentCount}" },
                    { MessageBodyTag.DocumentTypeDetails, $"{documentTypeDetailsSb}" },
                    { MessageBodyTag.BaseUrl, ServiceBaseURL }
                };
            });


        /// <summary>
        /// Get's the organisation.
        /// </summary>
        /// <param name="analysis">the batch analysis.</param>
        /// <returns>The organisation.</returns>
        internal async Task<Organisation> GetOrganisation(IBatchMetadataAnalysis analysis)
        {
            try
            {
                var orgDetail = await OrganisationsLookup.Get(new OrganisationIdentifier
                {
                    Type = OrganisationIdentifierType.Ukprn,
                    Value = analysis.IssuingOrganisation.ToString()
                });

                It.IsNull(orgDetail)
                    .AsGuard<MessageBodyFactoryOrganisationUnknownException>($"{analysis.IssuingOrganisation}");

                return orgDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(GetOrganisation)} for parentBatchId: {analysis?.ParentBatchID},  UKPRN: {analysis?.IssuingOrganisation}");
                throw;
            }
        }

        /// <summary>
        /// Format the grouping details for...
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <returns>The group details formatted.</returns>
        internal string FormatGroupingDetailsFor(IGrouping<string, FileMetadata> group, IBatchMetadataAnalysis batchAnalysis) =>
            $"{group.Count()} {batchAnalysis.GetProductNameFor(group.Key)} files";

        /// <summary>
        /// Get product name.
        /// </summary>
        /// <param name="groupItem">The group item.</param>
        /// <returns>A singular or plural product name based on the given count.</returns>
        internal string GetProductName(KeyValuePair<IBatchAnalysisProduct, int> groupItem) =>
            groupItem.Value > 1
                ? groupItem.Key.PluralName
                : groupItem.Key.Name;
    }
}