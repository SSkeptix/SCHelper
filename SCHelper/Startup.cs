using Microsoft.Extensions.Logging;
using SCHelper.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SCHelper
{
    public class Startup
    {
        private readonly IDataProvider dataProvider;
        private readonly ILogger<Startup> logger;

        public Startup(
            IDataProvider dataProvider,
            ILogger<Startup> logger)
        {
            this.dataProvider = dataProvider;
            this.logger = logger;
        }

        public Task Execute()
        {
            this.logger.LogDebug("Test config data. Config value: " + this.dataProvider.GetWeapons().FirstOrDefault());

            return Task.CompletedTask;
        }
    }
}
