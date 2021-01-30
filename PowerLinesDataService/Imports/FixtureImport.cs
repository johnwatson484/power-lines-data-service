using System;
using System.Threading.Tasks;
using PowerLinesDataService.Common;
using PowerLinesDataService.Messaging;
using PowerLinesMessaging;

namespace PowerLinesDataService.Imports
{
    public class FixtureImport : Import
    {
        public FixtureImport(string source, IFile file, ISender sender, MessageConfig messageConfig) : base(source, file, sender, messageConfig)
        {
        }

        public override void Load(string[] args)
        {
            Console.WriteLine("Importing fixtures");
            base.Load(args);
        }

        public override void CreateConnectionToQueue()
        {
            var options = new SenderOptions
            {
                Host = messageConfig.Host,
                Port = messageConfig.Port,
                Username = messageConfig.FixtureUsername,
                Password = messageConfig.FixturePassword,
                QueueName = messageConfig.FixtureQueue,
                QueueType = QueueType.ExchangeFanout
            };

            Task.Run(() =>
                sender.CreateConnectionToQueue(options))
            .Wait();
        }
    }
}
