using Microsoft.Extensions.Options;
using SCHelper.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCHelper.Services.Impl
{
    public class DataProvider : IDataProvider
    {
        private readonly ConfigModel config;
        private readonly IConversionService conversionService;

        public DataProvider(IOptions<ConfigModel> configModel,
            IConversionService conversionService)
        {
            this.config = configModel.Value;
            this.conversionService = conversionService;
        }

        public Weapon[] GetWeapons()
            => (config.Weapons ?? Array.Empty<WeaponConfigModel>())
                .Where(x => x != null)
                .Select(x => this.conversionService.ToDomainModel(x))
                .ToArray();

        public Ship[] GetShips()
            => (config.Ships ?? Array.Empty<ShipConfigModel>())
                .Where(x => x != null)
                .Select(x => this.conversionService.ToDomainModel(x))
                .ToArray();

        public SeedChip[] GetSeedChips()
            => (config.SeedChips ?? Array.Empty<Dictionary<ModificationType, double?>>())
                .Where(x => x != null)
                .Select(x => new SeedChip(
                    Parameters: this.conversionService.ToDomainModel(x)
                ))
                .ToArray();

        public CalculationCommand[] GetCalculationCommands()
        {
            var ships = this.GetShips();
            var weapons = this.GetWeapons();
            var seedChips = this.GetSeedChips();

            return (config.Calculate ?? Array.Empty<CalculationCommandConfigModel>())
                .Select(cmd => new CalculationCommand(
                    Name: cmd.Name,
                    Ship: cmd.Ship != null
                        ? this.conversionService.ToDomainModel(cmd.Ship)
                        : ships.FirstOrDefault(x => x.Name == cmd.ShipName),
                    Weapon: cmd.Weapon != null
                        ? this.conversionService.ToDomainModel(cmd.Weapon)
                        : weapons.FirstOrDefault(x => x.Name == cmd.WeaponName),
                    SeedChips: cmd.SeedChips
                        ?.Where(x => x != null)
                        .Select(x => new SeedChip(
                            Parameters: this.conversionService.ToDomainModel(x)))
                        .ToArray()
                        ?? seedChips,
                    Implants: this.conversionService.ToDomainModel(cmd.Implants ?? Utils.GetEmptyDictionary<ModificationType, double?>()),
                    Modules: this.conversionService.ToDomainModel(cmd.Modules ?? Utils.GetEmptyDictionary<ModificationType, double?>())
                ))
                .ToArray();
        }

    }
}
