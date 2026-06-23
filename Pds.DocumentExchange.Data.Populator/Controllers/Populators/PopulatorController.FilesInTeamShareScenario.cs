using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Pds.Core.BulkJobs.Models;
using Pds.DocumentExchange.Data.Populator.Models;
using Pds.DocumentExchange.Data.Populator.Scenarios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Controllers
{
    /// <summary>
    /// Populator for "files in team share" scenario.
    /// </summary>
    public partial class PopulatorController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> PopulateFilesInTeamShareWithCsvFile(
            IFormFile file,
            string shareName = "documentexchangeadministratorfundingcentre")
        {
            var csvFileReader = new CsvFileReader();
            var records = await csvFileReader.Read<DocumentRecord>(file);

            var bulkJobId = await PopulateFilesInTeamShareInternal(records, shareName);
            return Ok(bulkJobId);
        }

        [HttpPost]
        public async Task<IActionResult> PopulateFilesInTeamShare(
            IEnumerable<DocumentRecord> records,
            string shareName = "documentexchangeadministratorfundingcentre")
        {
            var bulkJobId = await PopulateFilesInTeamShareInternal(records, shareName);
            return Ok(bulkJobId);
        }

        [HttpGet]
        public async Task<IActionResult> TearDownFilesInTeamShare(
            string shareName = "documentexchangeadministratorfundingcentre")
        {
            var job = new Job<bool, string>
            {
                Parameters = true
            };

            var bulkJobId = await _bulkJobManager.CreateBulkJob(
                new[] { job },
                file =>
                {
                    var populator = new FilesInTeamShareScenario();

                    return populator.TearDownScenarioData(
                        GetFilesInTeamShareConfiguration(shareName));
                });

            return Ok(bulkJobId);
        }

        private async Task<Guid> PopulateFilesInTeamShareInternal(IEnumerable<DocumentRecord> records, string shareName)
        {
            var job = new Job<IEnumerable<DocumentRecord>, string>
            {
                Parameters = records
            };

            var bulkJobId = await _bulkJobManager.CreateBulkJob(
                new[] { job },
                file =>
                {
                    var populator = new FilesInTeamShareScenario();
                    return populator.PopulateScenarioData(
                        GetFilesInTeamShareConfiguration(shareName),
                        new FilesInTeamShareScenario.Parameters
                        {
                            Records = records
                        });
                });

            return bulkJobId;
        }

        private FilesInTeamShareScenario.Configuration GetFilesInTeamShareConfiguration(string shareName)
        {
            return new FilesInTeamShareScenario.Configuration
            {
                FileShareDirectory = new Storage.AzureFileShareDirectory.Configuration
                {
                    ShareName = shareName,
                    ConnectionString = _configuration.GetValue<string>("TeamsFileShareConnectionString")
                }
            };
        }
    }
}