using Pds.DocumentExchange.Data.Populator.Models;
using Pds.DocumentExchange.Data.Populator.Storage;
using Pds.DocumentExchange.Data.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Scenarios
{
    public abstract class DocumentsUploadedScenario : IPopulatorScenario<
        DocumentsUploadedScenario.Configuration,
        DocumentsUploadedScenario.Parameters,
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

                await LocalFileHelper.LoadExampleDocuments();
                var blobContainer = new AzureBlobContainer(configuration.BlobContainer);

                using (var client = new DocumentsCosmosDb(configuration.CosmosDb))
                {
                    var tasks = new List<Task>();

                    foreach (var currentRecord in parameters.Records)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            await _semaphore.WaitAsync();

                            var batch = CreateBatch(currentRecord);
                            var createDocumentMetadataTask = client.CreateDocument(batch);

                            var fileName = batch.Files.FirstOrDefault()?.FileName;
                            var document = LocalFileHelper.GetExampleDocumentBySize(currentRecord.FileSize);

                            var createBlobFileTask = blobContainer.CreateFile(fileName, document);

                            await Task.WhenAll(createDocumentMetadataTask, createBlobFileTask);

                            _semaphore.Release();
                        }));
                    }

                    await Task.WhenAll(tasks);
                }

                stopwatch.Stop();
                return $"Generated {parameters.Records.Count()} cosmos documents and blobs in {stopwatch.Elapsed}";
            }
            catch (Exception ex)
            {
                return $"An error occurred when running the job. {ex.Message}";
            }
        }

        public async Task<string> TearDownScenarioData(Configuration configuration)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var cosmosDeletedCount = -1;

                using (var client = new DocumentsCosmosDb(configuration.CosmosDb))
                {
                    cosmosDeletedCount = await client.DeleteAllDocuments();
                }

                var blobDeletedCount = 0;

                var blobContainer = new AzureBlobContainer(configuration.BlobContainer);
                var files = blobContainer.GetFileNames();

                await Task.WhenAll(
                    files.Select(async file =>
                    {
                        await _semaphore.WaitAsync();

                        await blobContainer.DeleteFile(file);
                        Interlocked.Increment(ref blobDeletedCount);

                        _semaphore.Release();
                    }));

                stopwatch.Stop();
                return $"Deleted {cosmosDeletedCount} cosmos documents and {blobDeletedCount} blobs in {stopwatch.Elapsed}";
            }
            catch (Exception ex)
            {
                return $"An error occurred when running the job. {ex.Message}";
            }
        }

        protected abstract BatchMetadata CreateBatch(DocumentRecord record);

        public class Configuration
        {
            public DocumentsCosmosDb.Configuration CosmosDb { get; set; }

            public AzureBlobContainer.Configuration BlobContainer { get; set; }
        }

        public class Parameters
        {
            public IEnumerable<DocumentRecord> Records { get; set; }
        }
    }
}