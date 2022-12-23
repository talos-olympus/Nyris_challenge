using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace Imagination
{
    internal sealed class ProcessPictureClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<ProcessPictureClient> _log;

        public ProcessPictureClient(HttpClient client, ILogger<ProcessPictureClient> log)
        {
            _client = client;
            _log = log;
        }

        public async Task<FileStream> ProcessPictureAsync(Stream picture, CancellationToken cancellationToken)
        {
            using var activity = Program.Telemetry.StartActivity(nameof(ProcessPictureAsync));
            try
            {
                activity?.AddEvent(new ActivityEvent("Sending request"));
                using var response = await _client.PostAsync((Uri?)null, new StreamContent(picture), cancellationToken);

                activity?.AddEvent(new ActivityEvent("Response received"));
                response.EnsureSuccessStatusCode();

                await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                activity?.AddEvent(new ActivityEvent("Stream received"));

                return await StreamToDiskAsync(responseStream, cancellationToken);
            }
            catch (Exception e)
            {
                activity?.SetStatus(Status.Error);
                activity?.AddEvent(new ActivityEvent(e.Message));
                throw;
            }
        }

        private async Task<FileStream> StreamToDiskAsync(Stream responseStream, CancellationToken cancellationToken)
        {
            var fileName = Path.GetTempFileName();
            Activity.Current?.AddTag("output.file", fileName);

            _log.LogInformation("Reading response stream into {TemporaryFile}", fileName);

            var outStream = new FileStream(fileName, FileMode.Truncate, FileAccess.ReadWrite,
                FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            await responseStream.CopyToAsync(outStream, cancellationToken);
            await outStream.FlushAsync(cancellationToken);
            Activity.Current?.AddEvent(new ActivityEvent("Response flushed to disk"));

            outStream.Seek(0, SeekOrigin.Begin);
            return outStream;
        }
    }
}
