using System.Diagnostics;
using System.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PowerLinesDataService.Common;
using PowerLinesDataService.Imports;
using PowerLinesDataService.Imports.Readers;
using PowerLinesMessaging;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace PowerLinesDataService.Tests;

[Category("Integration")]
[NonParallelizable]
public class MessagingImportTests
{
    readonly RabbitMqContainer rabbit = new RabbitMqBuilder("rabbitmq:3-management-alpine").Build();

    [OneTimeSetUp]
    public async Task StartBroker()
    {
        try { await rabbit.StartAsync(); }
        catch { await rabbit.DisposeAsync(); throw; }
    }

    [OneTimeTearDown]
    public async Task StopBroker() => await rabbit.DisposeAsync();

    [Test]
    public async Task Development_cli_starts_with_validated_dependency_injection_and_exits_without_imports()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = directory, RedirectStandardOutput = true, RedirectStandardError = true
        };
        start.ArgumentList.Add(typeof(PowerLinesDataService.Options.MessageOptions).Assembly.Location);
        start.Environment["DOTNET_ENVIRONMENT"] = "Development";
        start.Environment["Message__Host"] = rabbit.Hostname;
        start.Environment["Message__Port"] = rabbit.GetMappedPublicPort(5672).ToString();
        start.Environment["Message__Username"] = "rabbitmq";
        start.Environment["Message__Password"] = "rabbitmq";
        start.Environment["Message__FixtureQueue"] = "fixtures";
        start.Environment["Message__ResultQueue"] = "results";
        using var process = Process.Start(start);
        try
        {
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            await process.WaitForExitAsync(timeout.Token);
            Assert.That(process.ExitCode, Is.Zero, await output + await error);
        }
        finally
        {
            if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
            Directory.Delete(directory, true);
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Import_publishes_records_and_completion_through_the_real_messaging_package(bool fixtures)
    {
        const string csv = "Div,Date,Time,HomeTeam,AwayTeam,FTHG,FTAG,FTR,AvgH,AvgD,AvgA\nE0,01/08/2026,15:00,A,B,1,0,H,2,3,4\n";
        using var sources = new ImportTests.LocalSources(new()
        {
            [fixtures ? "fixtures.csv" : "E0.csv"] = (200, csv)
        });
        var factory = new ConnectionFactory { Uri = new Uri(rabbit.GetConnectionString()) };
        using var subscriber = factory.CreateConnection();
        using var channel = subscriber.CreateModel();
        var exchange = fixtures ? "fixtures" : "results";
        channel.ExchangeDeclare(exchange, ExchangeType.Fanout, durable: true);
        channel.ExchangeDeclare("imports", ExchangeType.Fanout, durable: true);
        var recordsQueue = channel.QueueDeclare().QueueName;
        var statusQueue = channel.QueueDeclare().QueueName;
        channel.QueueBind(recordsQueue, exchange, "");
        channel.QueueBind(statusQueue, "imports", "");
        var publisher = new PowerLinesMessaging.Connection(new ConnectionOptions
        {
            Host = rabbit.Hostname, Port = rabbit.GetMappedPublicPort(5672),
            Username = "rabbitmq", Password = "rabbitmq"
        });
        var path = Path.GetTempFileName();
        try
        {
            Import import = fixtures
                ? new FixtureImport(sources.Url.Replace("{0}/{1}.csv", "fixtures.csv"),
                    new Common.File(path, new FixtureReader()), publisher, exchange)
                : new ResultImport(sources.Url, new Common.File(path, new ResultReader()), publisher, exchange);
            await import.Load(fixtures ? ["--fixtures"] : ["--results"]);
            var record = await Read(channel, recordsQueue);
            var status = await Read(channel, statusQueue);
            Assert.Multiple(() =>
            {
                Assert.That(record["Division"]?.Value<string>(), Is.EqualTo("E0"));
                Assert.That(record["HomeTeam"]?.Value<string>(), Is.EqualTo("A"));
                Assert.That(record["SourceFileHash"]?.Value<string>(), Is.Not.Empty);
                Assert.That(status["RecordType"]?.Value<string>(), Is.EqualTo("ImportStatus"));
                Assert.That(status["RowCount"]?.Value<int>(), Is.EqualTo(1));
                Assert.That(status["Status"]?.Value<string>(), Is.EqualTo("Completed"));
                Assert.That(status["SourceFileHash"]?.Value<string>(), Is.EqualTo(record["SourceFileHash"]?.Value<string>()));
                Assert.That(System.IO.File.Exists(path), Is.False);
            });
        }
        finally
        {
            publisher.CloseConnection();
            System.IO.File.Delete(path);
        }
    }

    static async Task<JObject> Read(IModel channel, string queue)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (true)
        {
            var delivery = channel.BasicGet(queue, autoAck: true);
            if (delivery != null) return JObject.Parse(Encoding.UTF8.GetString(delivery.Body.Span));
            await Task.Delay(50, timeout.Token);
        }
    }
}
