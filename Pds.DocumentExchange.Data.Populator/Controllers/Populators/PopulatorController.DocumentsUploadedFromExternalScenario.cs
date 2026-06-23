using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pds.DocumentExchange.Data.Populator.Models;
using Pds.DocumentExchange.Data.Populator.Scenarios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Controllers
{
    /// <summary>
    /// Populator for "documents uploaded from external" scenario.
    /// </summary>
    public partial class PopulatorController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> PopulateDocumentsUploadedFromExternalWithCsvFile(IFormFile file)
            => await PopulateDocumentsUploaded<DocumentsUploadedFromExternalScenario>(file);

        [HttpPost]
        public async Task<IActionResult> PopulateDocumentsUploadedFromExternal(IEnumerable<DocumentRecord> records)
            => await PopulateDocumentsUploaded<DocumentsUploadedFromExternalScenario>(records);

        [HttpGet]
        public async Task<IActionResult> TearDownDocumentsUploadedFromExternal()
            => await TearDownDocumentsUploaded<DocumentsUploadedFromExternalScenario>();
    }
}