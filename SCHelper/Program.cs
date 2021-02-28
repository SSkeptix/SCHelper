using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SCHelper.Dtos;
using SCHelper.Exceptions;
using SCHelper.Services;
using SCHelper.Services.Impl;
using System;
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
                try
                {
                    var startup = serviceProvider.GetRequiredService<Startup>();
                    if (File.Exists(Constants.ConfigFilePath))
                        startup.Execute();
                    else
                        startup.GenerateDocumentation();
                }
                catch (Exception ex)
                {
                    var logger = serviceProvider.GetRequiredService<ILogger>();
                    logger.LogError(ex.Message);
                }
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
