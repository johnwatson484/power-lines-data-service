using System;
using PowerLinesDataService.Imports.Readers;
using PowerLinesDataService.Common;

namespace PowerLinesDataService.Imports.Factory
{
    public class ImportFactory
    {
        public Import GetImport(ImportType importType)
        {
            switch(importType)
            {
                case ImportType.Fixture:
                    return new FixtureImport(new File("", new FixtureReader()),"");
                case ImportType.Result:
                    return new ResultImport(new File("", new ResultReader()),"");
                default:
                    throw new ArgumentException("Import type not found");
            }
        }
    }
}
