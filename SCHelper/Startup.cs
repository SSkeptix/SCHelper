using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SCHelper.Services;
using System.Threading.Tasks;

namespace SCHelper
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IDataProvider dataProvider;
        private readonly ILogger<Startup> logger;

        public Startup(
            IConfiguration configuration,
            IDataProvider dataProvider,
            ILogger<Startup> logger)
        {
            this.configuration = configuration;
            this.dataProvider = dataProvider;
            this.logger = logger;
        }

        public Task Execute()
        {
            this.logger.LogDebug("Test config data. Config value: " + this.configuration["testConfigName"]);

            return Task.CompletedTask;
        }
    }
}
