using Microsoft.Extensions.Options;
using SCHelper.Dtos;
using SCHelper.Exceptions;
using System;
using System.Linq;

namespace SCHelper.Services.Impl
{
    public class DataProvider : IDataProvider
    {
        private readonly ConfigModel config;
        private readonly IFileReader fileReader;

        public DataProvider(IOptions<ConfigModel> configModel,
            IFileReader fileReader)
        {
            this.config = configModel.Value;
            this.fileReader = fileReader;
        }

        public Weapon[] GetWeapons()
            => (config.Weapons
                ?? (config.WeaponsFilePath != null
                    ? this.fileReader.Read<WeaponConfigModel>(config.WeaponsFilePath)
                    : Array.Empty<WeaponConfigModel>())
            )
            .Where(x => x != null)
            .Select(x => x.ToDomainModel())
            .ToArray();

        public Ship[] GetShips()
            => (this.config.Ships 
                ?? (this.config.ShipsFilePath != null 
                    ? this.fileReader.Read<ShipConfigModel>(config.ShipsFilePath)
                    : Array.Empty<ShipConfigModel>())
            )
            ?.Where(x => x != null)
            .Select(x => x.ToDomainModel())
            .ToArray();

        public SeedChip[] GetSeedChips()
            => (this.config.SeedChips
                ?? (this.config.SeedChipsFilePath != null
                    ? this.fileReader.Read<SeedChipConfigModel>(config.SeedChipsFilePath)
                    : Array.Empty<SeedChipConfigModel>())
            )
            ?.Where(x => x != null)
            .Select(x => x.ToDomainModel())
            .ToArray();

        public CalculationCommand[] GetCalculationCommands()
        {
            var ships = this.GetShips();
            var weapons = this.GetWeapons();

            return (config.Calculate ?? Array.Empty<CalculationCommandConfigModel>())
                .Select(cmd => new CalculationCommand(
                    Name: cmd.Name,
                    EnemyResistance: cmd.EnemyResistance ?? 0,
                    Ship: cmd.Ship?.ToDomainModel()
                        ?? ships.FirstOrDefault(x => x.Name == cmd.ShipName)
                        ?? throw new DataValidationException($"There are no ship for command. Ship name: '{cmd.ShipName}'"),
                    Weapon: weapons.FirstOrDefault(x => x.Name == cmd.WeaponName)
                        ?? throw new DataValidationException($"There are no weapon for command. Weapon name: '{cmd.WeaponName}'"),
                    Implants: (cmd.Implants ?? Utils.GetEmptyDictionary<ModificationType, double?[]>()).ToDomainModel(),
                    Modules: (cmd.Modules ?? Utils.GetEmptyDictionary<ModificationType, double?[]>()).ToDomainModel(),
                    Targets: (cmd.Targets ?? Array.Empty<TargetPropertyConfigModel>()).Select(x => x.ToDomainModel()).ToArray()
                ))
                .ToArray();
        }
    }
}
