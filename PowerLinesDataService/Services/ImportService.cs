using System.Collections.Generic;
using System.Linq;
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

        public void RunImports(string[] args)
        {
            folder.CreateFolderIfNotExists();

            List<Import> imports = new List<Import>();

            if(args.Contains("-fixtures"))
            {
                imports.Add(factory.GetImport(ImportType.Fixture));
            }

            if(args.Contains("-results"))
            {
                imports.Add(factory.GetImport(ImportType.Result));
            }

            foreach(var import in imports)
            {
                import.Load(args);
            }
        }
    }
}
