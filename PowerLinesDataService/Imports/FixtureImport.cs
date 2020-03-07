using System;
using System.Threading.Tasks;
using PowerLinesDataService.Common;
using PowerLinesDataService.Messaging;
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
            Task.Run(() =>
                sender.CreateConnectionToQueue(new BrokerUrl(messageConfig.Host, messageConfig.Port, messageConfig.FixtureUsername, messageConfig.FixturePassword).ToString(),
                    messageConfig.FixtureQueue))
            .Wait();
        }
    }
}
