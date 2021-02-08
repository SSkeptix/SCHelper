using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SCHelper.Dtos;
using SCHelper.Services;
using SCHelper.Services.Impl;
using System.IO;
using System.Threading.Tasks;

namespace SCHelper
{
    public class Program
    {
        private const string DefaultConfigFileName = "appsettings.json";

        public static Task Main(string[] args)
        {
            var configuration = GetConfiguration();
            if (configuration == null)
                return Task.CompletedTask;

            using (var serviceProvider = GetServiceProvider(configuration))
            {
                var startup = serviceProvider.GetService<Startup>();
                return startup.Execute();
            };
        }

        private static IConfiguration GetConfiguration()
        {
            if (File.Exists(DefaultConfigFileName))
            {
                return new ConfigurationBuilder()
                    .AddJsonFile(DefaultConfigFileName, false, true)
                    .Build();
            }
            else
            {
                // Create sample file
                // Create ReadMe.txt
                return null;
            }            
        }

        private static ServiceProvider GetServiceProvider(IConfiguration configuration)
        {
            return new ServiceCollection()
                .AddSingleton<ICalculationService, CalculationService>()
                .AddSingleton<IConversionService, ConversionService>()
                .AddSingleton<IDataProvider, DataProvider>()
                .AddSingleton<IExportDataService, ExportDataService>()
                .AddSingleton<Startup, Startup>()
                .Configure<ConfigModel>(configuration)
                .AddLogging(loggingBuilder => loggingBuilder
                    .ClearProviders()
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Trace))
                .BuildServiceProvider();
        }
    }
}
