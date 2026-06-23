using Microsoft.AspNetCore.Mvc;
using Pds.DocumentExchange.Data.Populator.Storage;
using Pds.DocumentExchange.Data.Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Controllers
{
    /// <summary>
    /// Code for accessing CosmosDb.
    /// </summary>
    public partial class PopulatorController : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<FileMetadata>> GetFileMetadata(string fileName)
        {
            var batchMetadata = new List<FileMetadata>();

            using (var documentClient = new DocumentsCosmosDb(GetDocumentsUploadedConfiguration().CosmosDb))
            {
                batchMetadata = await documentClient.GetFileMetadataByFileName(fileName);
            }

            return batchMetadata;
        }
    }
}