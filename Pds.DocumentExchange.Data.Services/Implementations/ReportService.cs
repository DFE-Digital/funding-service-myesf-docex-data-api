using FreeDataExports;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.Core.Utils.Interfaces;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <inheritdoc/>
    public class ReportService : IReportService
    {
        private readonly IDateTimeProvider _dateTime;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IEncryptionService _encryptionService;
        private readonly ILoggerAdapter<ReportService> _logger;
        private readonly IOrganisationsLookup _organisationLookupService;
        private readonly IConfigurationDataService _configurationDataService;
        private readonly string _encryptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportService"/> class.
        /// </summary>
        /// <param name="dateTime">The DateTime service.</param>
        /// <param name="cosmosDbService">The CosmosDb service.</param>
        /// <param name="encryptionService">The encryption service.</param>
        /// <param name="logger">The logger service.</param>
        /// <param name="organisationsLookupService">The Organisations Lookup service.</param>
        /// <param name="configurationDataService">The Configuration Data service.</param>
        /// <param name="cosmosDbConfiguration">The cosmosDb configuration.</param>
        public ReportService(
            IDateTimeProvider dateTime,
            ICosmosDbService cosmosDbService,
            IEncryptionService encryptionService,
            ILoggerAdapter<ReportService> logger,
            IOrganisationsLookup organisationsLookupService,
            IConfigurationDataService configurationDataService,
            CosmosDbConfiguration cosmosDbConfiguration)
        {
            _dateTime = dateTime;
            _cosmosDbService = cosmosDbService;
            _encryptionService = encryptionService;
            _logger = logger;
            _organisationLookupService = organisationsLookupService;
            _configurationDataService = configurationDataService;
            _encryptionKey = cosmosDbConfiguration.DataEncryptionKey;
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetMIReport(DateTime from, DateTime to)
        {
            var cosmosData = await _cosmosDbService.GetMIReport(from, to.AddDays(1).AddTicks(-1));

            return await BuildMIReport(from, to, cosmosData);
        }

        private async Task<byte[]> BuildMIReport(DateTime from, DateTime to, IEnumerable<MIReport> cosmosDocuments)
        {
            _logger.LogInformation($"MIReport: retrieved documents count between the given range {from} and {to}: " + cosmosDocuments.Count());

            var reportDate = _dateTime.ConvertToUKTime(DateTime.UtcNow);

            var organisationsData = await _organisationLookupService.GetAllOrganisations();

            var products = await _configurationDataService.GetProducts();

            foreach (var document in cosmosDocuments)
            {
                if (document.IsFromAgency ?
                    organisationsData.TryGetValue(document.ToUKPRN.ToString(), out Organisation providerDetails) :
                    organisationsData.TryGetValue(document.FromUKPRN.ToString(), out providerDetails))
                {
                    document.ProviderName = providerDetails.Name;
                    document.ProviderType = providerDetails.OrganisationSubType;
                }
                else
                {
                    document.ProviderName = "Data not available in FDS";
                    document.ProviderType = "Data not available in FDS";
                }

                document.FileTypeName = products.FirstOrDefault(x => x.Identifier == document.FileType).Name;
            }

            cosmosDocuments = cosmosDocuments.OrderBy(o => o.FileTypeName);

            var dataUploadedByBusiness = cosmosDocuments.Where(x => x.IsFromAgency);
            var dataUploadedByProvider = cosmosDocuments.Where(x => !x.IsFromAgency);

            // Create a new workbook
            var workbook = new DataExport().CreateODSv1_3();
            workbook.CreatedBy = "MYESF";
            workbook.FontSize = 11;
            workbook.Format(DataType.Decimal, "decimals=2");

            // Create worksheets
            var summary = workbook.AddWorksheet("Summary");
            var uploadedByBusiness = workbook.AddWorksheet("Uploaded by business");
            var uploadedByProvider = workbook.AddWorksheet("Uploaded by provider");

            // Build Report
            var reportSheetsMetadata = new Dictionary<string, IDataWorksheet>
            {
                { "uploaded by provider", uploadedByProvider },
                { "uploaded by business", uploadedByBusiness }
            };

            foreach (var pages in reportSheetsMetadata)
            {
                reportSheetsMetadata[pages.Key].AddRow().AddCell($"Document exchange report - documents {pages.Key}", DataType.String);
                reportSheetsMetadata[pages.Key].AddRow();
                reportSheetsMetadata[pages.Key].AddRow().AddCell("Date and time of report", DataType.String).AddCell(reportDate.ToString("dd/MM/yyyy, HH:mm"), DataType.String);
                reportSheetsMetadata[pages.Key].AddRow();
                reportSheetsMetadata[pages.Key].AddRow().AddCell("From", DataType.String).AddCell(from.ToString("dd/MM/yyyy"), DataType.String);
                reportSheetsMetadata[pages.Key].AddRow().AddCell("To", DataType.String).AddCell(to.ToString("dd/MM/yyyy"), DataType.String);
                reportSheetsMetadata[pages.Key].AddRow();

                var headers = reportSheetsMetadata[pages.Key].AddRow()
                .AddCell("UKPRN", DataType.String)
                .AddCell("Provider name", DataType.String)
                .AddCell("Provider type", DataType.String)
                .AddCell("Document type name", DataType.String)
                .AddCell("Document type product code", DataType.String)
                .AddCell("Funding period", DataType.String)
                .AddCell("Version number", DataType.String)
                .AddCell("Download date", DataType.String)
                .AddCell("Downloaded last by", DataType.String);

                headers.AddCell("Number of downloads", DataType.String);

                foreach (var row in pages.Key == "uploaded by provider" ? dataUploadedByProvider : dataUploadedByBusiness)
                {
                    var rowData = reportSheetsMetadata[pages.Key].AddRow()
                        .AddCell(row.IsFromAgency ? row.ToUKPRN : row.FromUKPRN, DataType.Number)
                        .AddCell(row.ProviderName, DataType.String)
                        .AddCell(row.ProviderType, DataType.String)
                        .AddCell(row.FileTypeName, DataType.String)
                        .AddCell(row.FileType, DataType.Number)
                        .AddCell(row.Year, DataType.Number)
                        .AddCell(row.Version, DataType.Number)
                        .AddCell(row.History?.LastOrDefault()?.ActionDateTimeUtc.ToString("dd/MM/yyyy"), DataType.String)
                        .AddCell(DecryptedFullName(row?.History?.LastOrDefault()?.User.FullName), DataType.String);

                    rowData.AddCell(row.History.Count(), DataType.Number);
                }
            }


            summary.AddRow().AddCell("Document exchange report - summary", DataType.String);
            summary.AddRow();
            summary.AddRow().AddCell("Date and time of report", DataType.String).AddCell(reportDate.ToString("dd/MM/yyyy, HH:mm"), DataType.String);
            summary.AddRow();
            summary.AddRow().AddCell("From", DataType.String).AddCell(from.ToString("dd/MM/yyyy"), DataType.String);
            summary.AddRow().AddCell("To", DataType.String).AddCell(to.ToString("dd/MM/yyyy"), DataType.String);
            summary.AddRow();

            var summarySheetMetadata = new Dictionary<string, IEnumerable<MIReport>>
            {
                { "uploaded by business", dataUploadedByBusiness },
                { "uploaded by provider", dataUploadedByProvider }
            };

            foreach (var item in summarySheetMetadata)
            {
                var isUploadedByProvider = item.Key == "uploaded by provider";

                summary.AddRow().AddCell($"Documents {(isUploadedByProvider ? "received from" : "published to")} providers", DataType.String);
                summary.AddRow();
                summary.AddRow()
                    .AddCell("Document type name", DataType.String)
                    .AddCell("Document type product code", DataType.String)
                    .AddCell($"Number of documents {(isUploadedByProvider ? "received from providers" : "published by business team")}", DataType.String)
                    .AddCell($"Of which, number that have been downloaded at least once by {(isUploadedByProvider ? "business team" : "providers")} as at {reportDate.ToString("dd/MM/yyyy")}", DataType.String)
                    .AddCell("Percentage downloaded", DataType.String);

                var summaryData = item.Value.GroupBy(x => x.FileType).Select(x =>
                {
                    var fileSummaryData = new
                    {
                        FileType = x.Key,
                        FileTypeName = x.Select(y => y.FileTypeName).FirstOrDefault(),
                        NumberOfDocumentsPublished = x.Select(y => y.Version).Count(),
                        NumberOfDocumentsDownloadedAtleatOnce = x.Count(y => y.History.Any()),
                    };

                    return new
                    {
                        fileSummaryData.FileType,
                        fileSummaryData.FileTypeName,
                        fileSummaryData.NumberOfDocumentsPublished,
                        fileSummaryData.NumberOfDocumentsDownloadedAtleatOnce,
                        PercentageDownloaded = ((double)fileSummaryData.NumberOfDocumentsDownloadedAtleatOnce / fileSummaryData.NumberOfDocumentsPublished) * 100
                    };
                }).ToList();

                foreach (var row in summaryData)
                {
                    summary.AddRow()
                        .AddCell(row.FileTypeName, DataType.String)
                        .AddCell(row.FileType, DataType.Number)
                        .AddCell(row.NumberOfDocumentsPublished, DataType.Number)
                        .AddCell(row.NumberOfDocumentsDownloadedAtleatOnce, DataType.Number)
                        .AddCell(row.PercentageDownloaded, DataType.Decimal);
                }

                summary.AddRow();
            }

            return await workbook.GetBytesAsync();
        }

        private string DecryptedFullName(string encryptedFullName)
        {
            try
            {
                return _encryptionService.Decrypt(encryptedFullName, _encryptionKey);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Decryption error: " + ex.ToString());

                return string.Empty;
            }
        }
    }
}
