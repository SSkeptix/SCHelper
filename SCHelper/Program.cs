using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SCHelper.Services;
using SCHelper.Services.Impl;
using System.Threading.Tasks;

namespace SCHelper
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", false, true)
                .Build();

            using (var serviceProvider = new ServiceCollection()
                .AddSingleton<IDataProvider, DataProvider>()
                .AddSingleton<Startup, Startup>()
                .AddSingleton<IConfiguration>(configuration)
                .AddLogging(loggingBuilder => loggingBuilder
                    .ClearProviders()
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Trace))
                .BuildServiceProvider())
            {
                var startup = serviceProvider.GetService<Startup>();
                return startup.Execute();
            };
        }
    }
}
