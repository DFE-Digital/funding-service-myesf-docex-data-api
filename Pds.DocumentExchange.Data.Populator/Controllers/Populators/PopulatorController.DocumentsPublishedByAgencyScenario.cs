using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pds.DocumentExchange.Data.Populator.Models;
using Pds.DocumentExchange.Data.Populator.Scenarios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Controllers
{
    /// <summary>
    /// Populator for "documents published by agency" scenario.
    /// </summary>
    public partial class PopulatorController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> PopulateDocumentsPublishedByAgencyWithCsvFile(IFormFile file)
            => await PopulateDocumentsUploaded<DocumentsPublishedByAgencyScenario>(file);

        [HttpPost]
        public async Task<IActionResult> PopulateDocumentsPublishedByAgency(IEnumerable<DocumentRecord> records)
            => await PopulateDocumentsUploaded<DocumentsPublishedByAgencyScenario>(records);

        [HttpGet]
        public async Task<IActionResult> TearDownDocumentsPublishedByAgency()
            => await TearDownDocumentsUploaded<DocumentsPublishedByAgencyScenario>();
    }
}