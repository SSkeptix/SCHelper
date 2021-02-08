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
    }
}
