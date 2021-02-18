using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SCHelper.Dtos;
using SCHelper.Services;
using SCHelper.Services.Impl;
using System.IO;

namespace SCHelper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Constants.ConfigFilePath, true, true)
                .Build();

            using (var serviceProvider = GetServiceProvider(configuration))
            {
                var startup = serviceProvider.GetService<Startup>();
                if (File.Exists(Constants.ConfigFilePath))
                    startup.Execute();
                else
                    startup.GenerateDocumentation();
            };
        }

        private static ServiceProvider GetServiceProvider(IConfiguration configuration)
        {
            return new ServiceCollection()
                .AddSingleton<ICalculationService, CalculationService>()
                .AddSingleton<IConversionService, ConversionService>()
                .AddSingleton<IDataProvider, DataProvider>()
                .AddSingleton<IExportDataService, ExportDataService>()
                .AddSingleton<IFileReader, FileReader>()
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
