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
                .Select(x => x.ToDomainModel())
                .ToArray();

        public Ship[] GetShips()
            => this.config.Ships
                ?.Where(x => x != null)
                .Select(x => x.ToDomainModel())
                .ToArray()
            ?? (this.config.ShipsFilePath != null ? this.fileReader.Read<ShipCsvModel>(config.ShipsFilePath) : null)
                ?.Where(x => x != null)
                .Select(x => x.ToDomainModel())
                .ToArray()
            ?? Array.Empty<Ship>();

        public SeedChip[] GetSeedChips()
            => this.config.SeedChips
                ?.Where(x => x != null)
                .Select(x => x.ToDomainModel())
                .ToArray()
            ?? (this.config.SeedChipsFilePath != null ? this.fileReader.Read<SeedChipCsvModel>(config.SeedChipsFilePath) : null)
                ?.Where(x => x != null)
                .Select(x => x.ToDomainModel())
                .ToArray()
            ?? Array.Empty<SeedChip>();

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
                    Ship: cmd.Ship?.ToDomainModel()
                        ?? ships.FirstOrDefault(x => x.Name == cmd.ShipName),
                    Weapon: weapons.FirstOrDefault(x => x.Name == cmd.WeaponName),
                    SeedChips: 
                        cmd.SeedChips
                            ?.Where(x => x != null)
                            .Select(x => x.ToDomainModel())
                            .ToArray()
                        ?? (cmd.SeedChipsFilePath != null ? this.fileReader.Read<SeedChipCsvModel>(cmd.SeedChipsFilePath) : null)
                            ?.Where(x => x != null)
                            .Select(x => x.ToDomainModel())
                            .ToArray(),
                    Implants: (cmd.Implants ?? Utils.GetEmptyDictionary<ModificationType, double?>()).ToDomainModel(),
                    Modules: (cmd.Modules ?? Utils.GetEmptyDictionary<ModificationType, double?>()).ToDomainModel()
                ))
                .ToArray();
        }
    }
}
