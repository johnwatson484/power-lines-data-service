using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.IO;
using System.Linq;
using PowerLinesDataService.Data;

namespace PowerLinesDataService
{
    public static class Program
    {
        private static IServiceProvider serviceProvider;
        private static IConfigurationRoot configuration;
       

        public static void Main(string[] args)
        {
            RegisterServices();
            Console.WriteLine("Importing data");
            DisposeServices();
        }

        public static IConfigurationRoot GetConfiguration()
        {
            if(configuration != null)
            {
                return configuration;
            }
            RegisterServices();
            
            return configuration;
        }

        private static void RegisterServices()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            configuration = builder.Build();

            var services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>();

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
