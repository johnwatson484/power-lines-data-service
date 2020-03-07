using System;
using PowerLinesDataService.Imports.Readers;
using PowerLinesDataService.Common;
using PowerLinesDataService.Messaging;

namespace PowerLinesDataService.Imports.Factory
{
    public class ImportFactory : IImportFactory
    {
        ISender sender;   

        MessageConfig messageConfig;

        public ImportFactory(MessageConfig messageConfig)
        {
            this.messageConfig = messageConfig;
            Console.WriteLine("{0} {1} {2} {3}", messageConfig.Host, messageConfig.Port, messageConfig.FixtureUsername, messageConfig.FixturePassword);
        }     

        public Import GetImport(ImportType importType)
        {
            Console.WriteLine("{0} {1} {2} {3}", messageConfig.Host, messageConfig.Port, messageConfig.FixtureUsername, messageConfig.FixturePassword);
            InitializeConnection();

            switch(importType)
            {
                case ImportType.Fixture:
                    return new FixtureImport("http://www.football-data.co.uk/fixtures.csv",
                        new File(string.Format("./ImportedFiles/Fixtures_{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss")), new FixtureReader()),
                        sender,
                        messageConfig
                        );
                case ImportType.Result:
                    return new ResultImport("http://www.football-data.co.uk/mmz4281/{0}/{1}.csv",
                        new File(string.Format("./ImportedFiles/Results_{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss")), new ResultReader()),
                        sender,
                        messageConfig);
                default:
                    throw new ArgumentException("Import type not found");
            }
        }

        private void InitializeConnection()
        {
            if(sender == null)
            {
                sender = new Sender();
            }
        }
    }
}
