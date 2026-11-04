using PowerLinesDataService.Common;
using PowerLinesDataService.Models;
using PowerLinesMessaging;

namespace PowerLinesDataService.Imports;

public abstract class Import
{
    protected string source;
    protected IFile file;
    protected IConnection connection;
    protected ISender sender;
    protected string queueName;

    protected Import(string source, IFile file, IConnection connection, string queueName)
    {
        this.source = source;
        this.file = file;
        this.connection = connection;
        this.queueName = queueName;
        CreateSender();
    }

    protected void CreateSender()
    {
        var options = new SenderOptions
        {
            Name = queueName,
            QueueType = QueueType.ExchangeFanout,
            QueueName = queueName
        };

        sender = connection.CreateSenderChannel(options);
    }

    public virtual async Task Load(string[] args)
    {
        try
        {
            using (HttpClient httpClient = new())
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, source);
                using Stream contentStream = await (await GetResponse(httpClient, request)).Content.ReadAsStreamAsync(),
                stream = new FileStream(file.Filepath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                await contentStream.CopyToAsync(stream);
            }
            SendSource(file.ReadFileToList(), source);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error importing: {0}", ex);
            throw;
        }
        finally
        {
            file.DeleteFileIfExists();
        }

        Console.WriteLine("Import complete");
    }

    protected static async Task<HttpResponseMessage> GetResponse(HttpClient client, HttpRequestMessage request)
    {
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return response;
    }

    protected void SendSource(IList<object> items, string url)
    {
        var ambiguous = items.OfType<SourceRecord>().GroupBy(x => x.SourceMatchId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (ambiguous.Count > 0) throw new InvalidOperationException("Ambiguous repeated seasonal pairings; stage and resolve source identity before import: " + string.Join(",", ambiguous));
        foreach (var item in items.OfType<SourceRecord>()) item.SourceUrl = url;
        SendToQueue(items);
        var statusSender = connection.CreateSenderChannel(new SenderOptions { Name = "imports", QueueType = QueueType.ExchangeFanout, QueueName = "imports" });
        foreach (var group in items.OfType<SourceRecord>().GroupBy(x => new { x.SourceSeason, x.SourceFileHash, Division = x is Result result ? result.Division : ((Fixture)x).Division }))
        {
            var division = group.Key.Division;
            statusSender.SendMessage(new ImportStatus { SourceUrl = url, Division = division, SourceSeason = group.Key.SourceSeason, SourceFileHash = group.Key.SourceFileHash, RowCount = group.Count(), CompletedAt = DateTime.UtcNow });
        }
        Console.WriteLine($"Successful import {DateTime.UtcNow:O}: {url}, rows={items.Count}");
    }

    public virtual void SendToQueue(IList<object> items)
    {
        foreach (var item in items)
        {
            sender.SendMessage(item);
        }
    }
}
