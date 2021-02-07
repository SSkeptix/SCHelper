using Microsoft.Extensions.Options;
using SCHelper.Dtos;
using System.Linq;

namespace SCHelper.Services.Impl
{
    public class DataProvider : IDataProvider
    {
        private readonly ConfigModel configModel;
        private readonly IConversionService conversionService;

        public DataProvider(IOptions<ConfigModel> configModel,
            IConversionService conversionService)
        {
            this.configModel = configModel.Value;
            this.conversionService = conversionService;
        }

        public Weapon[] GetWeapons()
            => configModel.Weapons
                .Select(x => this.conversionService.ToDomainModel(x))
                .ToArray();

        public Ship[] GetShips()
            => configModel.Ships
                .Select(x => this.conversionService.ToDomainModel(x))
                .ToArray();

        public SeedChip[] GetSeedChips()
            => configModel.SeedChips
                .Select(x => new SeedChip(
                    Parameters: this.conversionService.ToDomainModel(x)
                ))
                .ToArray();
    }
}
