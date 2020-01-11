using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration.Binder;
using PowerLinesDataService.Services;
using PowerLinesDataService.Common;
using PowerLinesDataService.Imports.Factory;
using PowerLinesDataService.Messaging;

namespace PowerLinesDataService
{
    public static class Program
    {
        private static IServiceProvider serviceProvider;   
        private static IConfigurationRoot configuration;    

        public static void Main(string[] args)
        {   
            Console.WriteLine("Starting service");
            RegisterServices();            

            var importService = new ImportService(
                new Folder("./ImportedFiles"), 
                serviceProvider.GetService<IImportFactory>());

            importService.RunImports(args);
            
            DisposeServices();
            Console.WriteLine("Stopping service");
        }

        private static void RegisterServices()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
                

            configuration = builder.Build();

            var messageConfig = configuration.GetSection("Message").Get<MessageConfig>();

            var services = new ServiceCollection();
            services.AddSingleton(messageConfig);
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
