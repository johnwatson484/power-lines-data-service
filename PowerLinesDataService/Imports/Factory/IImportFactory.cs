namespace PowerLinesDataService.Imports.Factory
{
    public interface IImportFactory
    {
        Import GetImport(ImportType importType);
    }
}
