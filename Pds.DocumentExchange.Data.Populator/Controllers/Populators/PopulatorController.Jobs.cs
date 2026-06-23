using Microsoft.AspNetCore.Mvc;
using Pds.Core.BulkJobs.Exceptions;
using Pds.DocumentExchange.Data.Populator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Controllers
{
    /// <summary>
    /// Endpoints to get the status of a given job.
    /// </summary>
    public partial class PopulatorController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> PopulateDocumentsJob(Guid jobId)
            => await GetJobResult<IEnumerable<DocumentRecord>>(jobId);

        [HttpGet]
        public async Task<IActionResult> TearDownDocumentsJob(Guid jobId)
            => await GetJobResult<bool>(jobId);

        private async Task<IActionResult> GetJobResult<TParameters>(Guid jobId)
        {
            try
            {
                var job = await _bulkJobManager.GetBulkJob<TParameters, string>(jobId);

                if (job.Completed)
                {
                    return Ok($"Job {jobId} completed. ({job.Jobs.FirstOrDefault()?.Result})");
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (JobNotFoundException)
            {
                return NotFound($"Job {jobId} not found.");
            }
        }
    }
}