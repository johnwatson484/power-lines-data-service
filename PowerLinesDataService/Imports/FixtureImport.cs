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
            base.Load(args);
        }

        public override void CreateConnectionToQueue()
        {
            Task.Run(() =>
                connection.CreateConnectionToQueue(new BrokerUrl(messageConfig.Host, messageConfig.Port, messageConfig.FixtureUsername, messageConfig.FixturePassword).ToString(),
                messageConfig.FixtureQueue))
            .Wait();
        }
    }
}
