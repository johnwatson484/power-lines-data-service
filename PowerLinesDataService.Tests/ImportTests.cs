using System.Net;
using System.Net.Sockets;
using System.Text;
using NUnit.Framework;
using PowerLinesDataService.Imports;
using PowerLinesDataService.Imports.Readers;
using PowerLinesDataService.Models;
using PowerLinesMessaging;

namespace PowerLinesDataService.Tests;

public class ImportTests
{
    const string ValidCsv = "Div,Date,HomeTeam,AwayTeam,FTHG,FTAG,FTR\nE0,01/08/2026,A,B,1,0,H\n";

    [Test]
    public async Task Full_import_uses_four_digit_seasons_across_the_millennium()
    {
        using var sources = new LocalSources(new());
        var path = Path.GetTempFileName();
        try
        {
            await new RecordingImport(sources.Url, path).Load(["--results", "--all"]);
            Assert.That(sources.Paths, Does.Contain("/9394/E0.csv"));
            Assert.That(sources.Paths, Does.Contain("/9900/E0.csv"));
            Assert.That(sources.Paths, Does.Not.Contain("/99100/E0.csv"));
        }
        finally { System.IO.File.Delete(path); }
    }

    [Test]
    public async Task Unavailable_sources_do_not_fail_import_and_status_sender_is_reused()
    {
        using var sources = new LocalSources(new()
        {
            ["E0.csv"] = (200, ValidCsv), ["E1.csv"] = (200, ValidCsv),
            ["E2.csv"] = (300, "Multiple Choices"), ["E3.csv"] = (503, "Service Unavailable")
        });
        var path = Path.GetTempFileName();
        var import = new RecordingImport(sources.Url, path);
        try
        {
            await import.Load(["--results"]);
            Assert.That(sources.RequestCount, Is.EqualTo(22));
            Assert.That(import.Results.Messages.Count, Is.EqualTo(2));
            Assert.That(import.StatusSenderCreations, Is.EqualTo(1));
            Assert.That(import.Statuses.Messages.Cast<ImportStatus>().Select(x => x.RowCount), Is.EqualTo(new[] { 1, 1 }));
            Assert.That(System.IO.File.Exists(path), Is.False);
        }
        finally { System.IO.File.Delete(path); }
    }

    [Test]
    public void Parse_failures_remain_fatal_after_other_sources_are_imported()
    {
        using var sources = new LocalSources(new()
        {
            ["E0.csv"] = (200, ValidCsv), ["E1.csv"] = (200, "Div,Date\nE1,01/08/2026\n"),
            ["E2.csv"] = (200, ValidCsv)
        });
        var path = Path.GetTempFileName();
        var import = new RecordingImport(sources.Url, path);
        try
        {
            var error = Assert.ThrowsAsync<InvalidOperationException>(() => import.Load(["--results"]));
            Assert.That(error.Message, Does.Contain("/E1.csv"));
            Assert.That(sources.RequestCount, Is.EqualTo(22));
            Assert.That(import.Results.Messages.Count, Is.EqualTo(2));
            Assert.That(System.IO.File.Exists(path), Is.False);
        }
        finally { System.IO.File.Delete(path); }
    }

    [Test]
    public void Publishing_failures_are_fatal_and_do_not_report_completion()
    {
        using var sources = new LocalSources(new() { ["E0.csv"] = (200, ValidCsv) });
        var path = Path.GetTempFileName();
        var import = new RecordingImport(sources.Url, path);
        import.Results.Fail = true;
        try
        {
            var error = Assert.ThrowsAsync<InvalidOperationException>(() => import.Load(["--results"]));
            Assert.That(error.Message, Does.Contain("/E0.csv"));
            Assert.That(sources.RequestCount, Is.EqualTo(22));
            Assert.That(import.Statuses.Messages, Is.Empty);
        }
        finally { System.IO.File.Delete(path); }
    }

    [Test]
    public void Duplicate_completed_match_is_rejected_before_publishing()
    {
        using var sources = new LocalSources(new() { ["E0.csv"] = (200, ValidCsv + "E0,01/08/2026,A,B,1,0,H\n") });
        var path = Path.GetTempFileName();
        var import = new RecordingImport(sources.Url, path);
        try
        {
            Assert.ThrowsAsync<InvalidOperationException>(() => import.Load(["--results"]));
            Assert.That(import.Results.Messages, Is.Empty);
            Assert.That(import.Statuses.Messages, Is.Empty);
        }
        finally { System.IO.File.Delete(path); }
    }

    sealed class RecordingSender : ISender
    {
        public List<object> Messages { get; } = [];
        public bool Fail { get; set; }
        public void SendMessage(object message, string routingKey = "")
        {
            if (Fail) throw new InvalidOperationException("Simulated publishing failure");
            Messages.Add(message);
        }
    }

    sealed class RecordingImport(string source, string path) : ResultImport(source,
        new PowerLinesDataService.Common.File(path, new ResultReader()), null, "results")
    {
        public RecordingSender Results => (RecordingSender)sender;
        public RecordingSender Statuses { get; } = new();
        public int StatusSenderCreations { get; private set; }
        protected override void CreateSender() => sender = new RecordingSender();
        protected override ISender CreateStatusSender() { StatusSenderCreations++; return Statuses; }
    }

    internal sealed class LocalSources : IDisposable
    {
        readonly HttpListener listener = new();
        readonly Task server;
        int requestCount;
        public int RequestCount => requestCount;
        public List<string> Paths { get; } = [];
        public string Url { get; }

        public LocalSources(Dictionary<string, (int Status, string Body)> responses)
        {
            using var port = new TcpListener(IPAddress.Loopback, 0);
            port.Start();
            var address = $"http://127.0.0.1:{((IPEndPoint)port.LocalEndpoint).Port}/";
            port.Stop();
            Url = address + "{0}/{1}.csv";
            listener.Prefixes.Add(address);
            listener.Start();
            server = Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        var context = await listener.GetContextAsync();
                        Interlocked.Increment(ref requestCount);
                        Paths.Add(context.Request.Url.AbsolutePath);
                        var response = responses.GetValueOrDefault(Path.GetFileName(context.Request.Url.AbsolutePath), (404, "Not Found"));
                        context.Response.StatusCode = response.Item1;
                        var body = Encoding.UTF8.GetBytes(response.Item2);
                        context.Response.ContentLength64 = body.Length;
                        await context.Response.OutputStream.WriteAsync(body);
                        context.Response.Close();
                    }
                }
                catch (HttpListenerException) { }
                catch (ObjectDisposedException) { }
            });
        }

        public void Dispose() { listener.Close(); server.GetAwaiter().GetResult(); }
    }
}
