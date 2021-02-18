using SCHelper.Dtos;
using System.Linq;

namespace SCHelper.Services.Impl
{
    public class ConversionService : IConversionService
    {
        public CalculationResult ToUserDataModel(CalculationResult data)
            => data with
            {
                FireRate = 60 * data.FireRate,
                CriticalChance = 100 * data.CriticalChance,
                DecreaseResistance = 100 * data.DecreaseResistance,
                SeedChips = data.SeedChips
                    .Select(x => this.ToUserDataModel(x))
                    .ToArray()
            };

        public SeedChip ToUserDataModel(SeedChip data)
            => data with
            {
                Parameters = data.Parameters
                    .Where(x => x.Value != 0)
                    .ToDictionary(x => x.Key, x => x.Value * 100)
            };
    }
}
