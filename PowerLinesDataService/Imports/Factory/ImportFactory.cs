using System;
using PowerLinesDataService.Imports.Readers;
using PowerLinesDataService.Common;
using PowerLinesDataService.Messaging;
using PowerLinesMessaging;

namespace PowerLinesDataService.Imports.Factory
{
    public class ImportFactory : IImportFactory
    {
        readonly MessageConfig messageConfig;

        public ImportFactory(MessageConfig messageConfig)
        {
            this.messageConfig = messageConfig;
        }

        public Import GetImport(ImportType importType, IConnection connection)
        {
            switch(importType)
            {
                case ImportType.Fixture:
                    return new FixtureImport("https://www.football-data.co.uk/fixtures.csv",
                        new File(string.Format("{0}Fixtures_{1}.csv", System.IO.Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmss")), new FixtureReader()),
                        connection,
                        messageConfig.FixtureQueue
                        );
                case ImportType.Result:
                    return new ResultImport("https://www.football-data.co.uk/mmz4281/{0}/{1}.csv",
                        new File(string.Format("{0}Results_{1}.csv", System.IO.Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmss")), new ResultReader()),
                        connection,
                        messageConfig.ResultQueue);
                default:
                    throw new ArgumentException("Import type not found");
            }
        }
    }
}
