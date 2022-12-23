using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;

namespace Imagination
{
    internal sealed class QueryService : BackgroundService
    {
        private readonly ProcessPictureClient _client;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<QueryService> _log;
        private readonly TestFileOptions _options;

        public QueryService(ProcessPictureClient client, IHostApplicationLifetime lifetime, ILogger<QueryService> log, IOptions<TestFileOptions> options)
        {
            _client = client;
            _lifetime = lifetime;
            _log = log;
            _options = options.Value;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var activity = Program.Telemetry.StartActivity("Processing Files");
            _log.LogInformation("Now processing files");

            await ProcessAllFilesAsync(stoppingToken);

            _log.LogInformation("Done processing files");
            _lifetime.StopApplication();
        }

        private async Task ProcessAllFilesAsync(CancellationToken stoppingToken)
        {
            foreach (var file in Directory.EnumerateFiles(_options.BaseDirectory))
            {
                await ProcessFileAsync(file, stoppingToken);
            }
        }

        private async Task ProcessFileAsync(string file, CancellationToken stoppingToken)
        {
            using var activity = Program.Telemetry.StartActivity("Processing File")?
                .AddTag("file.input", file);
            using var scope = _log.BeginScope("File: {File}", file);

            try
            {
                await using var inputStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);

                var result = await _client.ProcessPictureAsync(inputStream, stoppingToken);
                _log.LogInformation("Processing succeeded, output file is {OutputFileName}", result.Name);
                return;
            }
            catch (HttpRequestException e)
            {
                _log.LogError("Processing failed with status {HttpStatus}", e.StatusCode);
            }
            catch (Exception e)
            {
                _log.LogCritical(e, "An unhandled exception occurred");
            }

            activity?.SetStatus(Status.Error);
        }
    }
}
