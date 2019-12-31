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
                    return new FixtureImport(new File(string.Format("./ImportedFiles/Fixtures_{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss")), new FixtureReader()),"http://www.football-data.co.uk/fixtures.csv");
                case ImportType.Result:
                    return new ResultImport(new File("", new ResultReader()),"");
                default:
                    throw new ArgumentException("Import type not found");
            }
        }
    }
}
