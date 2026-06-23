using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Pds.Core.BulkJobs.Interfaces;
using Pds.Core.BulkJobs.Models;
using Pds.DocumentExchange.Data.Populator.Models;
using Pds.DocumentExchange.Data.Populator.Scenarios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Controllers
{
    /// <summary>
    /// Populator controller.
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public partial class PopulatorController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IBulkJobManager _bulkJobManager;

        public PopulatorController(IConfiguration configuration, IBulkJobManager bulkJobManager)
        {
            _configuration = configuration;
            _bulkJobManager = bulkJobManager;
        }

        private async Task<IActionResult> PopulateDocumentsUploaded<TUploadScenario>(IFormFile file)
           where TUploadScenario : DocumentsUploadedScenario, new()
        {
            var csvFileReader = new CsvFileReader();
            var records = await csvFileReader.Read<DocumentRecord>(file);

            var bulkJobId = await PopulateDocumentsUploadedInternal<TUploadScenario>(records);
            return Ok(bulkJobId);
        }

        private async Task<IActionResult> PopulateDocumentsUploaded<TUploadScenario>(IEnumerable<DocumentRecord> records)
           where TUploadScenario : DocumentsUploadedScenario, new()
        {
            var bulkJobId = await PopulateDocumentsUploadedInternal<TUploadScenario>(records);
            return Ok(bulkJobId);
        }

        private async Task<Guid> PopulateDocumentsUploadedInternal<TUploadScenario>(IEnumerable<DocumentRecord> records)
            where TUploadScenario : DocumentsUploadedScenario, new()
        {
            var job = new Job<IEnumerable<DocumentRecord>, string>
            {
                Parameters = records
            };

            var bulkJobId = await _bulkJobManager.CreateBulkJob(
               new[] { job },
               file =>
               {
                   var uploadScenario = new TUploadScenario();

                   return uploadScenario.PopulateScenarioData(
                       GetDocumentsUploadedConfiguration(),
                       new DocumentsUploadedScenario.Parameters
                       {
                           Records = records
                       });
               });

            return bulkJobId;
        }

        private async Task<IActionResult> TearDownDocumentsUploaded<TUploadScenario>()
            where TUploadScenario : DocumentsUploadedScenario, new()
        {
            var job = new Job<bool, string>
            {
                Parameters = true
            };

            var bulkJobId = await _bulkJobManager.CreateBulkJob(
                new[] { job },
                file =>
                {
                    var uploadScenario = new TUploadScenario();

                    return uploadScenario.TearDownScenarioData(
                        GetDocumentsUploadedConfiguration());
                });

            return Ok(bulkJobId);
        }

        private DocumentsUploadedScenario.Configuration GetDocumentsUploadedConfiguration()
        {
            return new DocumentsUploadedScenario.Configuration
            {
                CosmosDb = new Storage.DocumentsCosmosDb.Configuration
                {
                    EndPoint = _configuration.GetValue<string>("CosmosDb:ServiceEndpoint"),
                    AuthKeyOrResourceToken = _configuration.GetValue<string>("CosmosDb:AuthKeyOrResourceToken"),
                    DatabaseName = _configuration.GetValue<string>("CosmosDb:DatabaseName") ?? "docx",
                    CollectionName = _configuration.GetValue<string>("CosmosDb:CollectionName") ?? "collection1"
                },
                BlobContainer = new Storage.AzureBlobContainer.Configuration
                {
                    ConnectionString = _configuration.GetValue<string>("BlobContainers:ConnectionString"),
                    ContainerName = _configuration.GetValue<string>("BlobContainers:ContainerName") ?? "all"
                }
            };
        }
    }
}