using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using PowerLinesDataService.Common;
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
            using (var httpClient = new HttpClient())
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, source);
                using Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                stream = new FileStream(file.Filepath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                await contentStream.CopyToAsync(stream);
            }
            SendToQueue(file.ReadFileToList());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error importing: {0}", ex);
        }
        finally
        {
            file.DeleteFileIfExists();
        }

        Console.WriteLine("Import complete");
    }

    public virtual void SendToQueue(IList<object> items)
    {
        foreach (var item in items)
        {
            sender.SendMessage(item);
        }
    }
}
