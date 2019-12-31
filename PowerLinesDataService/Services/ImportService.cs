using System.Collections.Generic;
using PowerLinesDataService.Common;
using PowerLinesDataService.Imports;
using PowerLinesDataService.Imports.Factory;

namespace PowerLinesDataService.Services
{
    public class ImportService : IImportService
    {
        protected IFolder folder;

        protected IImportFactory factory;

        public ImportService(IFolder folder, IImportFactory factory)
        {
            this.folder = folder;
            this.factory = factory;
        }

        public void RunImports()
        {
            folder.CreateFolderIfNotExists();

            List<Import> imports = new List<Import>();
            imports.Add(factory.GetImport(ImportType.Fixture));
            // imports.Add(factory.GetImport(ImportType.Result));

            foreach(var import in imports)
            {
                import.Load();
            }
        }
    }
}
