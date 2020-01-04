using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using PowerLinesDataService.Common;
using PowerLinesDataService.Messaging;
using PowerLinesDataService.Models;

namespace PowerLinesDataService.Imports
{
    public class FixtureImport : Import
    {
        public FixtureImport(string source, IFile file, IConnection connection, MessageConfig messageConfig) : base(source, file, connection, messageConfig)
        {
        }

        public override void Load(string[] args)
        {
            Console.WriteLine("Importing fixtures");
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
                Console.WriteLine("Error importing fixtures: {0}", ex);
            }
            finally
            {
                file.DeleteFileIfExists();
            }

            Console.WriteLine("Import complete");
        }

        public override void CreateConnectionToQueue()
        {
            Task.Run(() => connection.CreateConnectionToQueue(new BrokerUrl(messageConfig.Host, messageConfig.Port, messageConfig.FixtureUsername, messageConfig.FixturePassword).ToString(), messageConfig.FixtureQueue)).Wait();
        }
    }
}
