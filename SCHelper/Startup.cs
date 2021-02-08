using Microsoft.Extensions.Logging;
using SCHelper.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SCHelper
{
    public class Startup
    {
        private readonly ICalculationService calculationService;
        private readonly IDataProvider dataProvider;
        private readonly ILogger<Startup> logger;

        public Startup(
            ICalculationService calculationService,
            IDataProvider dataProvider,
            ILogger<Startup> logger)
        {
            this.calculationService = calculationService;
            this.dataProvider = dataProvider;
            this.logger = logger;
        }

        public Task Execute()
        {
            var weapon = this.dataProvider.GetWeapons().First();
            var ship = this.dataProvider.GetShips().First();
            var seedChips = this.dataProvider.GetSeedChips();

            var shipParameters = this.calculationService.CalcShipParameters(weapon, ship, seedChips);

            this.logger.LogDebug("Dps: " + shipParameters.Damage.Dps);

            return Task.CompletedTask;
        }
    }
}
