using System;
using PowerLinesDataService.Imports;

namespace PowerLinesDataService.Imports.Factory
{
    public class ImportFactory
    {
        public IImport GetImport(ImportType importType)
        {
            switch(importType)
            {
                case ImportType.Fixture:
                    return new FixtureImport();
                case ImportType.Result:
                    return new ResultImport();
                default:
                    throw new ArgumentException("Import type not found");
            }
        }
    }
}
