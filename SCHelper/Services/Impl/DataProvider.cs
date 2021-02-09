using Microsoft.Extensions.Options;
using SCHelper.Dtos;
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
            => config.Weapons
                .Select(x => this.conversionService.ToDomainModel(x))
                .ToArray();

        public Ship[] GetShips()
            => config.Ships
                .Select(x => this.conversionService.ToDomainModel(x))
                .ToArray();

        public SeedChip[] GetSeedChips()
            => config.SeedChips
                .Select(x => new SeedChip(
                    Parameters: this.conversionService.ToDomainModel(x)
                ))
                .ToArray();

        public CalculationCommand[] GetCalculationCommands()
        {
            var ships = this.GetShips();
            var weapons = this.GetWeapons();
            var seedChips = this.GetSeedChips();

            return config.Calculate
                .Select(cmd => new CalculationCommand(
                    Ship: cmd.Ship != null
                        ? this.conversionService.ToDomainModel(cmd.Ship)
                        : ships.FirstOrDefault(x => x.Name == cmd.ShipName),
                    Weapon: cmd.Weapon != null
                        ? this.conversionService.ToDomainModel(cmd.Weapon)
                        : weapons.FirstOrDefault(x => x.Name == cmd.WeaponName),
                    SeedChips: cmd.SeedChips
                        ?.Select(x => new SeedChip(
                            Parameters: this.conversionService.ToDomainModel(x)))
                        .ToArray()
                        ?? seedChips
                ))
                .ToArray();
        }

    }
}
