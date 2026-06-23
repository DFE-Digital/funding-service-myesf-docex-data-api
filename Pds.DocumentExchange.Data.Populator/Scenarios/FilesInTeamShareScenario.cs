using Pds.DocumentExchange.Data.Populator.Models;
using Pds.DocumentExchange.Data.Populator.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Scenarios
{
    public class FilesInTeamShareScenario : IPopulatorScenario<
        FilesInTeamShareScenario.Configuration,
        FilesInTeamShareScenario.Parameters,
        string,
        string>
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10, 10);

        public async Task<string> PopulateScenarioData(
            Configuration configuration,
            Parameters parameters)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var directory = GetFileShareDirectory(configuration);
                await LocalFileHelper.LoadExampleDocuments();

                var tasks = new List<Task>();

                foreach (var currentRecord in parameters.Records)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await _semaphore.WaitAsync();

                        var fileName = $"{currentRecord.Ukprn}_{currentRecord.ProviderId}_{currentRecord.AcademicYear}.{currentRecord.FileType}";
                        var document = LocalFileHelper.GetExampleDocumentBySize(currentRecord.FileSize);

                        await directory.CreateFile(fileName, document);

                        _semaphore.Release();
                    }));
                }

                await Task.WhenAll(tasks);

                stopwatch.Stop();
                return $"{parameters.Records.Count()} files generated in {stopwatch.Elapsed}";
            }
            catch (Exception ex)
            {
                return $"An error occurred when running the job. {ex.Message}";
            }
        }

        public async Task<string> TearDownScenarioData(
            Configuration configuration)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var directory = GetFileShareDirectory(configuration);

                int fileCount = 0;
                var files = directory.GetFileNames();

                await Task.WhenAll(
                    files.Select(async fileName =>
                    {
                        await _semaphore.WaitAsync();

                        await directory.DeleteFile(fileName);
                        Interlocked.Increment(ref fileCount);

                        _semaphore.Release();
                    }));

                stopwatch.Stop();
                return $"Cleared {fileCount} files in {stopwatch.Elapsed}";
            }
            catch (Exception ex)
            {
                return $"An error occurred when running the job. {ex.Message}";
            }
        }

        public IEnumerable<string> GetScenarioFileNames(Configuration configuration)
        {
            var directory = GetFileShareDirectory(configuration);
            return directory.GetFileNames();
        }

        private AzureFileShareDirectory GetFileShareDirectory(Configuration configuration)
            => new AzureFileShareDirectory(configuration.FileShareDirectory);

        public class Configuration
        {
            public AzureFileShareDirectory.Configuration FileShareDirectory { get; set; }
        }

        public class Parameters
        {
            public IEnumerable<DocumentRecord> Records { get; set; }
        }
    }
}