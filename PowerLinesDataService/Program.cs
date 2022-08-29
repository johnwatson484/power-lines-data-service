using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerLinesDataService.Services;
using PowerLinesDataService.Common;
using PowerLinesDataService.Imports.Factory;
using PowerLinesDataService.Messaging;
using System.Globalization;
using System.Threading.Tasks;

namespace PowerLinesDataService;

public static class Program
{
    private static IServiceProvider serviceProvider;

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting service");
        SetCulture();
        RegisterServices();

        var importService = new ImportService(
            new Folder(Path.GetTempPath()),
            serviceProvider.GetService<IImportFactory>(),
            serviceProvider.GetService<MessageConfig>());

        await importService.RunImports(args);

        DisposeServices();
        Console.WriteLine("Stopping service");
    }

    private static void RegisterServices()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        IConfigurationRoot configuration = builder.Build();

        var messageConfig = configuration.GetSection("Message").Get<MessageConfig>();

        var services = new ServiceCollection();
        services.AddSingleton(messageConfig);
        services.AddScoped<IImportFactory, ImportFactory>();
        services.AddScoped<IImportService, ImportService>();
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

    private static void SetCulture()
    {
        CultureInfo culture = new CultureInfo("en-GB");
        CultureInfo.DefaultThreadCurrentCulture = culture;
    }
}
