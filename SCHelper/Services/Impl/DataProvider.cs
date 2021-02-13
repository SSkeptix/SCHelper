using Microsoft.Extensions.Options;
using SCHelper.Dtos;
using System;
using System.Linq;

namespace SCHelper.Services.Impl
{
    public class DataProvider : IDataProvider
    {
        private readonly ConfigModel config;
        private readonly IConversionService conversionService;
        private readonly IFileReader fileReader;

        public DataProvider(IOptions<ConfigModel> configModel,
            IConversionService conversionService,
            IFileReader fileReader)
        {
            this.config = configModel.Value;
            this.conversionService = conversionService;
            this.fileReader = fileReader;
        }

        public Weapon[] GetWeapons()
            => (config.Weapons
                    ?? (config.WeaponsFilePath != null
                        ? this.fileReader.Read<WeaponConfigModel>(config.WeaponsFilePath)
                        : null)
                    ?? Array.Empty<WeaponConfigModel>()
                )
                .Where(x => x != null)
                .Select(x => this.conversionService.ToDomainModel(x))
                .ToArray();

        public Ship[] GetShips()
            => (config.Ships
                    ?? (config.ShipsFilePath != null
                        ? this.fileReader.Read<ShipConfigModel>(config.ShipsFilePath)
                        : null)
                    ?? Array.Empty<ShipConfigModel>()
                )
                .Where(x => x != null)
                .Select(x => this.conversionService.ToDomainModel(x))
                .ToArray();

        public SeedChip[] GetSeedChips()
            => (config.SeedChips
                    ?? (config.SeedChipsFilePath != null 
                        ? this.fileReader.Read<SeedChipConfigModel>(config.SeedChipsFilePath)
                        : null)
                    ?? Array.Empty<SeedChipConfigModel>()
                )
                .Where(x => x != null)
                .Select(x => this.conversionService.ToDomainModel(x))
                .ToArray();

        public CalculationCommand[] GetCalculationCommands()
        {
            var ships = this.GetShips();
            var weapons = this.GetWeapons();
            var seedChips = this.GetSeedChips();

            return (config.Calculate ?? Array.Empty<CalculationCommandConfigModel>())
                .Select(cmd => new CalculationCommand(
                    Name: cmd.Name,
                    DamageTarget: cmd.DamageTarget ?? DamageTarget.Normal,
                    EnemyResistance: cmd.EnemyResistance ?? 0,
                    Ship: cmd.Ship != null
                        ? this.conversionService.ToDomainModel(cmd.Ship)
                        : ships.FirstOrDefault(x => x.Name == cmd.ShipName),
                    Weapon: weapons.FirstOrDefault(x => x.Name == cmd.WeaponName),
                    SeedChips: (cmd.SeedChips
                            ?? (cmd.SeedChipsFilePath != null
                                ? this.fileReader.Read<SeedChipConfigModel>(cmd.SeedChipsFilePath)
                                : null)
                        )
                        ?.Where(x => x != null)
                        .Select(x => this.conversionService.ToDomainModel(x))
                        .ToArray()
                        ?? seedChips,
                    Implants: this.conversionService.ToDomainModel(cmd.Implants ?? Utils.GetEmptyDictionary<ModificationType, double?>()),
                    Modules: this.conversionService.ToDomainModel(cmd.Modules ?? Utils.GetEmptyDictionary<ModificationType, double?>())
                ))
                .ToArray();
        }

    }
}
