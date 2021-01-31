using System.Collections.Generic;
using System.Linq;
using PowerLinesDataService.Common;
using PowerLinesDataService.Imports;
using PowerLinesDataService.Imports.Factory;
using PowerLinesDataService.Messaging;
using PowerLinesMessaging;

namespace PowerLinesDataService.Services
{
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

        public void RunImports(string[] args)
        {
            folder.CreateFolderIfNotExists();
            List<Import> imports = new List<Import>();            

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
                import.Load(args);
            }

            CloseConnection();
        }

        protected void CreateConnection()
        {
            var options = new ConnectionOptions
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
}
