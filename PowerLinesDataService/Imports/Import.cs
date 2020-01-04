using System;
using System.Collections.Generic;
using System.Net;
using PowerLinesDataService.Common;
using PowerLinesDataService.Messaging;

namespace PowerLinesDataService.Imports
{
    public abstract class Import
    {
        protected string source;

        protected IFile file;

        protected IConnection connection;

        protected MessageConfig messageConfig;

        public Import(string source, IFile file, IConnection connection, MessageConfig messageConfig)
        {
            this.source = source;
            this.file = file;
            this.connection = connection;
            this.messageConfig = messageConfig;
        }

        public virtual void Load(string[] args)
        {            
            CreateConnectionToQueue();

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

        public abstract void CreateConnectionToQueue();

        public virtual void SendToQueue(IList<object> items)
        {
            foreach (var item in items)
            {
                connection.SendMessage(item);
            }
        }
    }
}
