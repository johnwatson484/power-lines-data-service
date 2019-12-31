using System;
using PowerLinesDataService.Imports.Readers;
using PowerLinesDataService.Common;

namespace PowerLinesDataService.Imports.Factory
{
    public class ImportFactory : IImportFactory
    {
        public Import GetImport(ImportType importType)
        {
            switch(importType)
            {
                case ImportType.Fixture:
                    return new FixtureImport(new File(string.Format("./ImportedFiles/Fixtures_{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss")),
                    new FixtureReader()),
                    "http://www.football-data.co.uk/fixtures.csv");
                case ImportType.Result:
                    return new ResultImport(new File(string.Format("./ImportedFiles/Results_{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss")),
                    new ResultReader()),
                    "http://www.football-data.co.uk/mmz4281/{0}/{1}.csv");
                default:
                    throw new ArgumentException("Import type not found");
            }
        }
    }
}
