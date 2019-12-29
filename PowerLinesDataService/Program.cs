using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace PowerLinesDataService
{
    public static class Program
    {
        private static IServiceProvider serviceProvider;       

        public static void Main(string[] args)
        {
            RegisterServices();
            Console.WriteLine("Importing data");
            DisposeServices();
        }

        private static void RegisterServices()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var services = new ServiceCollection();
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
