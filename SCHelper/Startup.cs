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
        private readonly IExportDataService exportDataService;
        private readonly ILogger<Startup> logger;

        public Startup(
            ICalculationService calculationService,
            IDataProvider dataProvider,
            IExportDataService exportDataService,
            ILogger<Startup> logger)
        {
            this.calculationService = calculationService;
            this.dataProvider = dataProvider;
            this.exportDataService = exportDataService;
            this.logger = logger;
        }

        public Task Execute()
        {
            var weapon = this.dataProvider.GetWeapons().First();
            var ship = this.dataProvider.GetShips().First();
            var seedChips = this.dataProvider.GetSeedChips();
            var shipParameters = this.calculationService.CalcShipParameters(weapon, ship, seedChips);
            return this.exportDataService.Export(shipParameters);
        }
    }
}
