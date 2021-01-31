using PowerLinesMessaging;

namespace PowerLinesDataService.Imports.Factory
{
    public interface IImportFactory
    {
        Import GetImport(ImportType importType, IConnection connection);
    }
}
