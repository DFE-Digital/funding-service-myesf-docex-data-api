using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Extensions;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Storage
{
    /// <summary>
    /// Represents a manager for Azure directories (file shares and blob containers).
    /// </summary>
    public class DirectoriesManager : IDirectoriesManager
    {
        private readonly IFileNameProvider _fileNameProvider;
        private readonly IConfigurationDataService _configurationDataService;
        private readonly DirectoriesManagerConfiguration _configuration;
        private readonly ILoggerAdapter<AzureBlobContainer> _azureBlobContainerLogger;
        private readonly ILoggerAdapter<AzureFileShareDirectory> _azureFileShareDirectoryLogger;

        private readonly ConcurrentDictionary<string, IDirectory> _directories = new ConcurrentDictionary<string, IDirectory>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoriesManager"/> class.
        /// </summary>
        /// <param name="fileNameProvider">The file name utilities.</param>
        /// <param name="configurationDataService">The configuration data service.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="azureBlobContainerLogger">The logger for Azure Blob Container.</param>
        /// <param name="azureFileShareDirectoryLogger">The logger for Azure FileShare Directory.</param>
        public DirectoriesManager(
            IFileNameProvider fileNameProvider,
            IConfigurationDataService configurationDataService,
            DirectoriesManagerConfiguration configuration,
            ILoggerAdapter<AzureBlobContainer> azureBlobContainerLogger,
            ILoggerAdapter<AzureFileShareDirectory> azureFileShareDirectoryLogger)
        {
            _fileNameProvider = fileNameProvider;
            _configurationDataService = configurationDataService;
            _configuration = configuration;
            _azureBlobContainerLogger = azureBlobContainerLogger;
            _azureFileShareDirectoryLogger = azureFileShareDirectoryLogger;
        }

        /// <inheritdoc/>
        public async Task<IDirectory> GetDirectory(string directoryKey)
        {
            bool found = _directories.TryGetValue(directoryKey, out IDirectory directory);

            if (!found)
            {
                await InitialiseDirectories();
                directory = _directories[directoryKey];
            }

            return directory;
        }

        private async Task InitialiseDirectories()
        {
            LoadFileShareDirectories(_configuration.FileShareDirectories, _fileNameProvider);
            LoadBlobContainers(_configuration.BlobContainers, _fileNameProvider);

            await LoadTeamsFileShareDirectories(_configuration.TeamsFileShareConnectionString, _fileNameProvider);
        }

        private async Task LoadTeamsFileShareDirectories(string connectionString, IFileNameProvider fileNameProvider)
        {
            var teams = await _configurationDataService.GetTeams();
            var teamIdentifiers = teams.Select(team => team.Identifier.ToLower());

            LoadFileShareDirectories(teamIdentifiers, connectionString, fileNameProvider);
        }

        private void LoadFileShareDirectories(AzureFileShareDirectoriesConfiguration configuration, IFileNameProvider fileNameProvider)
        {
            var fileShareDirectories = configuration.FileShareDirectoryPairs.SplitAndTrim(',');
            LoadFileShareDirectories(fileShareDirectories, configuration.ConnectionString, fileNameProvider);
        }

        private void LoadFileShareDirectories(
            IEnumerable<string> fileShareDirectories,
            string connectionString,
            IFileNameProvider fileNameProvider)
        {
            var azureCloudStorageAccount = new AzureCloudStorageAccount(connectionString);

            foreach (var currentFileShareDirPair in fileShareDirectories)
            {
                var fileShareDirPairSplit = currentFileShareDirPair.SplitAndTrim('-');

                var fileShare = fileShareDirPairSplit[0];
                var directory = fileShareDirPairSplit.Length > 1 ? fileShareDirPairSplit[1] : string.Empty;

                var fileShareDirectory = new AzureFileShareDirectory(
                    fileShare,
                    directory,
                    azureCloudStorageAccount,
                    fileNameProvider,
                    _azureFileShareDirectoryLogger);

                _directories.TryAdd(currentFileShareDirPair, fileShareDirectory);
            }
        }

        private void LoadBlobContainers(AzureBlobContainerConfiguration configuration, IFileNameProvider fileNameProvider)
        {
            var azureCloudStorageAccount = new AzureCloudStorageAccount(configuration.ConnectionString);
            var containers = configuration.Containers.SplitAndTrim('-');

            foreach (var currentContainer in containers)
            {
                var blobContainer = new AzureBlobContainer(
                    currentContainer,
                    azureCloudStorageAccount,
                    fileNameProvider,
                    _azureBlobContainerLogger);

                _directories.TryAdd(currentContainer, blobContainer);
            }
        }
    }
}