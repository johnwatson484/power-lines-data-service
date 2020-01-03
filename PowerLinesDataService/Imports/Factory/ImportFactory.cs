using System;
using PowerLinesDataService.Imports.Readers;
using PowerLinesDataService.Common;
using PowerLinesDataService.Messaging;

namespace PowerLinesDataService.Imports.Factory
{
    public class ImportFactory : IImportFactory
    {
        IConnection connection;   

        MessageConfig messageConfig;

        public ImportFactory(MessageConfig messageConfig)
        {
            this.messageConfig = messageConfig;
        }     

        public Import GetImport(ImportType importType)
        {
            InitializeConnection();

            switch(importType)
            {
                case ImportType.Fixture:
                    return new FixtureImport("http://www.football-data.co.uk/fixtures.csv",
                        new File(string.Format("./ImportedFiles/Fixtures_{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss")), new FixtureReader()),
                        connection,
                        messageConfig
                        );
                case ImportType.Result:
                    return new ResultImport("http://www.football-data.co.uk/mmz4281/{0}/{1}.csv",
                        new File(string.Format("./ImportedFiles/Results_{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss")), new ResultReader()),
                        connection,
                        messageConfig);
                default:
                    throw new ArgumentException("Import type not found");
            }
        }

        private void InitializeConnection()
        {
            if(connection == null)
            {
                connection = new AmqpConnection();
            }
        }
    }
}
