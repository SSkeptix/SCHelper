using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SCHelper.Dtos;
using SCHelper.Services;
using System.Collections.Generic;
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

        public void GenerateDocumentation()
        {
            this.exportDataService.ExportJson(Constants.ConfigFilePath, SampleData.AppSettings);
            this.exportDataService.ExportCsv(SampleData.AppSettings.ShipsFilePath, SampleData.ShipCsvModels);
            this.exportDataService.ExportCsv(SampleData.AppSettings.WeaponsFilePath, SampleData.WeaponConfigModels);
            this.exportDataService.ExportCsv(SampleData.AppSettings.SeedChipsFilePath, SampleData.SeedChipCsvModels);
            //TODO: Add ReadMe.txt file generation
        }

        public async Task Execute()
        {
            var seedChips = this.dataProvider.GetSeedChips().ToList();
            var usedSeedChips = new List<SeedChip>();
            var calculationCommands = this.dataProvider.GetCalculationCommands();

            var results = (await this.calculationService.Calc(calculationCommands, seedChips))
                .Select(x => this.conversionService.ToUserDataModel(x))
                .ToArray();

            this.exportDataService.ExportJson(config.OutputFile, results);
        }
    }
}
