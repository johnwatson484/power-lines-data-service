using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerLinesDataService.Services;
using PowerLinesDataService.Common;
using PowerLinesDataService.Imports.Factory;
using System.Globalization;
using Microsoft.Extensions.Hosting;
using PowerLinesDataService.Options;
using Microsoft.Extensions.Options;

SetCulture();

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<MessageOptions>(builder.Configuration.GetSection(key: "Message"));

builder.Services.AddScoped<IImportFactory, ImportFactory>();
builder.Services.AddScoped<IImportService, ImportService>();

IHost host = builder.Build();

await RunImports(host.Services, args);

static async Task RunImports(IServiceProvider serviceProvider, string[] args)
{
    ImportService importService = new(
    new Folder(Path.GetTempPath()),
    serviceProvider.GetService<IImportFactory>(),
    serviceProvider.GetService<IOptions<MessageOptions>>());

    await importService.RunImports(args);
}

static void SetCulture()
{
    CultureInfo culture = new("en-GB");
    CultureInfo.DefaultThreadCurrentCulture = culture;
}
