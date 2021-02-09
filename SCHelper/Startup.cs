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
        private readonly IConversionService conversionService;
        private readonly IDataProvider dataProvider;
        private readonly IExportDataService exportDataService;
        private readonly ILogger<Startup> logger;
        private readonly ConfigModel config;

        public Startup(
            ICalculationService calculationService,
            IConversionService conversionService,
            IDataProvider dataProvider,
            IExportDataService exportDataService,
            ILogger<Startup> logger,
            IOptions<ConfigModel> configModel)
        {
            this.calculationService = calculationService;
            this.conversionService = conversionService;
            this.dataProvider = dataProvider;
            this.exportDataService = exportDataService;
            this.logger = logger;
            this.config = configModel.Value;
        }

        //TODO: Add ReadMe.txt file generation
        public Task GenerateDocumentation()
            => this.exportDataService.Export(Constants.ConfigFilePath, Constants.DefaultConfigModel);

        public Task Execute()
        {
            var calculationCommands = this.dataProvider.GetCalculationCommands();

            ShipParameters[] userData = calculationCommands
                .Select(cmd => this.calculationService.CalcShipParameters(cmd))
                .Select(shipDetails => this.conversionService.ToUserDataModel(shipDetails))
                .ToArray();

            return this.exportDataService.Export(config.OutputFile, userData);
        }
    }
}
