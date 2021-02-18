using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public class ShipConfigModel
    {
        public string Name { get; set; }
        public int? Level { get; set; }
        public int WeaponCount { get; set; }
        public int? MaxChipCount { get; set; }
        public Dictionary<ModificationType, double?> Bonuses { get; set; }
    }

    public static class ShipConfigModelExtensions
    {
        public static Ship ToDomainModel(this ShipConfigModel data)
            => new Ship(
                Name: data.Name,
                Level: data.Level ?? 0,
                WeaponCount: data.WeaponCount,
                MaxChipCount: data.MaxChipCount ?? 5,
                Bonuses: (data.Bonuses ?? new Dictionary<ModificationType, double?>()).ToDomainModel()
            );
    }
}
