using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public class SeedChipConfigModel
    {
        public string Name { get; set; }
        public int? Level { get; set; }
        public Dictionary<ModificationType, double?> Parameters { get; set; }
    }

    public static class SeedChipConfigModelExtensions
    {
        public static SeedChip ToDomainModel(this SeedChipConfigModel data)
            => new SeedChip(
                Name: data.Name,
                Level: data.Level ?? 0,
                Parameters: (data.Parameters ?? new Dictionary<ModificationType, double?>()).ToDomainModel()
            );
    }
}
