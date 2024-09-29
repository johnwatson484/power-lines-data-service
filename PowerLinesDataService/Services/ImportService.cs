using Microsoft.Extensions.Options;
using PowerLinesDataService.Common;
using PowerLinesDataService.Imports;
using PowerLinesDataService.Imports.Factory;
using PowerLinesDataService.Options;
using PowerLinesMessaging;

namespace PowerLinesDataService.Services;

public class ImportService : IImportService
{
    protected IFolder folder;
    protected IImportFactory factory;
    protected MessageOptions messageOptions;
    protected IConnection connection;

    public ImportService(IFolder folder, IImportFactory factory, IOptions<MessageOptions> messageOptions)
    {
        this.folder = folder;
        this.factory = factory;
        this.messageOptions = messageOptions.Value;
        CreateConnection();
    }

    public async Task RunImports(string[] args)
    {
        folder.CreateFolderIfNotExists();
        List<Import> imports = new();

        if (args.Contains("--results"))
        {
            imports.Add(factory.GetImport(ImportType.Result, connection));
        }

        if (args.Contains("--fixtures"))
        {
            imports.Add(factory.GetImport(ImportType.Fixture, connection));
        }

        foreach (var import in imports)
        {
            await import.Load(args);
        }

        CloseConnection();
    }

    protected void CreateConnection()
    {
        ConnectionOptions options = new()
        {
            Host = messageOptions.Host,
            Port = messageOptions.Port,
            Username = messageOptions.Username,
            Password = messageOptions.Password
        };
        connection = new Connection(options);
    }

    protected void CloseConnection()
    {
        connection.CloseConnection();
    }
}
