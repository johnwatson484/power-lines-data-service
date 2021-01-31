using System;
using System.Collections.Generic;
using System.Net;
using PowerLinesDataService.Common;
using PowerLinesMessaging;

namespace PowerLinesDataService.Imports
{
    public abstract class Import
    {
        protected string source;
        protected IFile file;
        protected IConnection connection;
        protected ISender sender;
        protected string queueName;

        public Import(string source, IFile file, IConnection connection, string queueName)
        {
            this.source = source;
            this.file = file;
            this.connection = connection;
            this.queueName = queueName;
            CreateSender();
        }

        protected void CreateSender()
        {
            var options =  new SenderOptions
            {
                Name = queueName,
                QueueType = QueueType.ExchangeFanout,
                QueueName = queueName
            };

            sender = connection.CreateSenderChannel(options);
        }

        public virtual void Load(string[] args)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(string.Format(source), file.Filepath);
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
}
