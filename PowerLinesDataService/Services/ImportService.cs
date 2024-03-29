using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PowerLinesDataService.Common;
using PowerLinesDataService.Imports;
using PowerLinesDataService.Imports.Factory;
using PowerLinesDataService.Messaging;
using PowerLinesMessaging;

namespace PowerLinesDataService.Services;

public class ImportService : IImportService
{
    protected IFolder folder;
    protected IImportFactory factory;
    protected MessageConfig messageConfig;
    protected IConnection connection;

    public ImportService(IFolder folder, IImportFactory factory, MessageConfig messageConfig)
    {
        this.folder = folder;
        this.factory = factory;
        this.messageConfig = messageConfig;
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
            Host = messageConfig.Host,
            Port = messageConfig.Port,
            Username = messageConfig.Username,
            Password = messageConfig.Password
        };
        connection = new Connection(options);
    }

    protected void CloseConnection()
    {
        connection.CloseConnection();
    }
}
