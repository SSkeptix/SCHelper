using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SCHelper.Dtos;
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
        private readonly ConfigModel config;

        public Startup(
            ICalculationService calculationService,
            IDataProvider dataProvider,
            IExportDataService exportDataService,
            ILogger<Startup> logger,
            IOptions<ConfigModel> configModel)
        {
            this.calculationService = calculationService;
            this.dataProvider = dataProvider;
            this.exportDataService = exportDataService;
            this.logger = logger;
            this.config = configModel.Value;
        }

        //TODO: Add ReadMe.txt file generation
        public Task GenerateDocumentation()
            => this.exportDataService.ExportData(Constants.ConfigFilePath, Constants.DefaultConfigModel);

        public Task Execute()
        {
            var weapon = this.dataProvider.GetWeapons().First();
            var ship = this.dataProvider.GetShips().First();
            var seedChips = this.dataProvider.GetSeedChips();
            var shipParameters = this.calculationService.CalcShipParameters(weapon, ship, seedChips);
            return this.exportDataService.Export(config.OutputFile, shipParameters);
        }
    }
}
