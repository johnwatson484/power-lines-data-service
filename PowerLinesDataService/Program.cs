using PowerLinesDataService.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PowerLinesDataService.Services;
using PowerLinesDataService.Common;
using PowerLinesDataService.Imports.Factory;
using System.Globalization;
using Microsoft.Extensions.Hosting;
using PowerLinesDataService.Options;

LocalEnvironment.Load();

SetCulture();

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<MessageOptions>(builder.Configuration.GetSection(key: "Message"));

builder.Services.AddScoped<IFolder>(_ => new Folder(Path.GetTempPath()));
builder.Services.AddScoped<IImportFactory, ImportFactory>();
builder.Services.AddScoped<IImportService, ImportService>();

using IHost host = builder.Build();
using var scope = host.Services.CreateScope();
await scope.ServiceProvider.GetRequiredService<IImportService>().RunImports(args);

static void SetCulture()
{
    CultureInfo culture = new("en-GB");
    CultureInfo.DefaultThreadCurrentCulture = culture;
}
