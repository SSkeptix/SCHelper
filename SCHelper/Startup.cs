using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SCHelper.Dtos;
using SCHelper.Services;
using System.Collections.Generic;
using System.Linq;

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

        public void Execute()
        {
            var calculationCommands = this.dataProvider.GetCalculationCommands();

            object[] userData = calculationCommands
                .Select(cmd =>
                {
                    var errors = new List<string>();
                    if (cmd.Ship == null)
                        errors.Add("There is no ship. Property Ship is null and ShipName is invalid.");
                    if (cmd.Weapon == null)
                        errors.Add("There is no weapon. Property Weapon is null and WeaponName is invalid.");

                    if (errors.Any())
                    {
                        return new
                        {
                            Name = cmd.Name,
                            Errors = errors.ToArray()
                        };
                    }
                    else
                    {
                        CalculationResult result = this.calculationService.Calc(cmd);
                        return (object)this.conversionService.ToUserDataModel(result);
                    }
                })
                .ToArray();
            this.exportDataService.ExportJson(config.OutputFile, userData);
        }
    }
}
