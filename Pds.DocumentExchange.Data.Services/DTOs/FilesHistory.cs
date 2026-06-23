using Pds.DocumentExchange.Data.Services.DTOs;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Implementations.CosmosDb
{
    public class FilesHistory
    {
        /// <summary>
        /// The identifier of the batch the files are in.
        /// </summary>
        public string BatchId { get; set; }

        /// <summary>
        /// The files themselves.
        /// </summary>
        public IEnumerable<FileMetadata> Metadatas { get; set; }
    }
}