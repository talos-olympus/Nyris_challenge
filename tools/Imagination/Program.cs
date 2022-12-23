using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Imagination
{
    internal static class Program
    {
        internal static readonly ActivitySource Telemetry = new ("Imagination");

        private static Task Main(string[] args)
        {
            OpenTelemetry.Sdk.SetDefaultTextMapPropagator(new B3Propagator());

            var builder = new HostBuilder()
                .ConfigureAppConfiguration(configure => configure
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables()
                    .AddCommandLine(args))
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(config =>
                    {
                        config.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                        config.AddSimpleConsole(options =>
                        {
                            options.IncludeScopes = true;
                        });
                    });

                    services.AddOpenTelemetryTracing(builder => builder
                        .SetResourceBuilder(ResourceBuilder
                            .CreateDefault()
                            .AddEnvironmentVariableDetector()
                            .AddTelemetrySdk()
                            .AddService("Imagination"))
                        .AddHttpClientInstrumentation()
                        .AddJaegerExporter()
                        .AddSource(Telemetry.Name));

                    services.AddHostedService<QueryService>();
                    services.AddHttpClient<ProcessPictureClient>(client =>
                    {
                        client.BaseAddress = hostContext.Configuration.GetValue<Uri>("Endpoint");
                        client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
                        client.DefaultRequestVersion = HttpVersion.Version11;
                        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                        {
                            NoCache = true,
                            NoStore = true,
                            NoTransform = true
                        };
                    });

                    services.AddOptions<TestFileOptions>()
                        .Bind(hostContext.Configuration.GetSection("TestFiles"))
                        .Validate(options => Directory.Exists(options.BaseDirectory));
                });

            return builder.RunConsoleAsync();
        }
    }
}
