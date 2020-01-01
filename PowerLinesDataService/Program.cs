using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerLinesDataService.Services;
using PowerLinesDataService.Common;
using PowerLinesDataService.Imports.Factory;

namespace PowerLinesDataService
{
    public static class Program
    {
        private static IServiceProvider serviceProvider;       

        public static void Main(string[] args)
        {
            RegisterServices();

            var importService = new ImportService(
                new Folder("./ImportedFiles"), 
                serviceProvider.GetService<IImportFactory>());

            importService.RunImports(args);
            
            DisposeServices();
        }

        private static void RegisterServices()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var services = new ServiceCollection();
            services.AddScoped<IImportService, ImportService>();
            services.AddScoped<IImportFactory, ImportFactory>();
            serviceProvider = services.BuildServiceProvider();
        }

        private static void DisposeServices()
        {
            if (serviceProvider == null)
            {
                return;
            }
            if (serviceProvider is IDisposable)
            {
                ((IDisposable)serviceProvider).Dispose();
            }
        }
    }
}
