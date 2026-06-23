using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <inheritdoc cref="IDocumentManager"/>
    public class DocumentManager : IDocumentManager
    {
        private readonly IDirectoriesManager _directoriesManager;
        private readonly ILoggerAdapter<DocumentManager> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentManager"/> class.
        /// </summary>
        /// <param name="directoriesManager">The directories manager.</param>
        /// <param name="logger">The logger.</param>
        public DocumentManager(
            IDirectoriesManager directoriesManager,
            ILoggerAdapter<DocumentManager> logger)
        {
            _directoriesManager = directoriesManager;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task RemoveAgencyDocuments(string team, IEnumerable<string> fileNames)
        {
            var directory = await _directoriesManager.GetDirectory(team);
            foreach (var fileName in fileNames)
            {
                await directory.Delete(fileName);

                _logger.LogInformation($"File {fileName} for team {team} has been removed.");
            }
        }
    }
}