using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Service to scan a file using a Windows Defender agent on another machine (using HTTP).
    /// </summary>
    internal class WindowsDefenderAntivirus : BaseAntivirus
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggerAdapter<WindowsDefenderAntivirus> _logger;

        private readonly string _remoteUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsDefenderAntivirus"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP Client factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public WindowsDefenderAntivirus(IHttpClientFactory httpClientFactory, WindowsDefenderAntivirusConfiguration configuration, ILoggerAdapter<WindowsDefenderAntivirus> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _remoteUrl = configuration.Url;
            _logger = logger;
        }

        /// <inheritdoc/>
        protected override string AntivirusName => "Windows Defender (remote)";

        /// <inheritdoc/>
        protected override async Task<FileSafety> GetFileSafety(string fileName, Stream stream)
        {
            var maxAttempts = 3;
            var scanResult = await ScanAndRetryIfErrorOccurs(fileName, stream, maxAttempts);

            if (!scanResult.HasValue)
            {
                _logger.LogInformation($"File name {fileName} ran out of retries.");
                scanResult = FileSafety.InternalError;
            }
            else
            {
                _logger.LogInformation($"Windows Defender states that {fileName} is {scanResult.Value}.");
            }

            return scanResult.Value;
        }

        private async Task<FileSafety?> ScanAndRetryIfErrorOccurs(string fileName, Stream stream, int maxAttempts)
        {
            FileSafety? scanResult = null;

            var random = new Random();
            var currentAttempt = 1;

            while (currentAttempt <= maxAttempts && !scanResult.HasValue)
            {
                if (currentAttempt > 1)
                {
                    var millisecondsDelay = random.Next(0, 2) * 1000;
                    await Task.Delay(millisecondsDelay);
                }

                try
                {
                    scanResult = await SendFileToScan(fileName, stream);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Attempt number {currentAttempt}.");
                }

                currentAttempt++;
            }

            return scanResult;
        }

        private async Task<FileSafety> SendFileToScan(string fileName, Stream stream)
        {
            var fileUrl = _remoteUrl + fileName;

            _logger.LogInformation($"Started Scanning the file {fileName}");

            using (var content = new StreamContent(stream))
            {
                var response = await _httpClient.PostAsync(fileUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                return (FileSafety)Enum.Parse(typeof(FileSafety), responseString);
            }
        }
    }
}