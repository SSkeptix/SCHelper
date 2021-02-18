using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public class ShipCsvModel
    {
        public string Name { get; set; }
        public int? Level { get; set; }
        public int WeaponCount { get; set; }
        public int? MaxChipCount { get; set; }

        public double? Damage { get; set; }
        public double? CriticalChance { get; set; }
        public double? CriticalDamage { get; set; }
        public double? FireRate { get; set; }
        public double? FireRange { get; set; }
        public double? FireSpread { get; set; }
        public double? ProjectiveSpeed { get; set; }
        public double? WeaponHitSpeed { get; set; }
        public double? WeaponCoolingSpeed { get; set; }
        public double? ModuleReloadingSpeed { get; set; }
    }

    public static class ShipCsvModelExtensions
    {
        public static Ship ToDomainModel(this ShipCsvModel data)
            => new Ship(
                Name: data.Name,
                Level: data.Level ?? 0,
                WeaponCount: data.WeaponCount,
                MaxChipCount: data.MaxChipCount ?? 5,
                Bonuses: new Dictionary<ModificationType, double?>
                {
                    [ModificationType.Damage] = data.Damage,
                    [ModificationType.CriticalDamage] = data.CriticalDamage,
                    [ModificationType.CriticalChance] = data.CriticalChance,
                    [ModificationType.FireRate] = data.FireRate,
                    [ModificationType.FireRange] = data.FireRange,
                    [ModificationType.FireSpread] = data.FireSpread,
                    [ModificationType.ProjectiveSpeed] = data.ProjectiveSpeed,
                    [ModificationType.WeaponHitSpeed] = data.WeaponHitSpeed,
                    [ModificationType.WeaponCoolingSpeed] = data.WeaponCoolingSpeed,
                    [ModificationType.ModuleReloadingSpeed] = data.ModuleReloadingSpeed,
                }.ToDomainModel()
            );

        public static ShipCsvModel ToCsvModel(this Ship data)
        {
            var bonuces = data.Bonuses.ToUserModel();

            return new ShipCsvModel
            {
                Name = data.Name,
                Level = data.Level,
                WeaponCount = data.WeaponCount,
                MaxChipCount = data.MaxChipCount,
                Damage = bonuces[ModificationType.Damage],
                CriticalChance = bonuces[ModificationType.CriticalChance],
                CriticalDamage = bonuces[ModificationType.CriticalDamage],
                FireRate = bonuces[ModificationType.FireRate],
                FireRange = bonuces[ModificationType.FireRange],
                FireSpread = bonuces[ModificationType.FireSpread],
                ProjectiveSpeed = bonuces[ModificationType.ProjectiveSpeed],
                WeaponHitSpeed = bonuces[ModificationType.WeaponHitSpeed],
                WeaponCoolingSpeed = bonuces[ModificationType.WeaponCoolingSpeed],
                ModuleReloadingSpeed = bonuces[ModificationType.ModuleReloadingSpeed],
            };
        }
    }
}
