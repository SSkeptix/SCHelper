using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public class SeedChipCsvModel
    {
        public string Name { get; set; }
        public int? Level { get; set; }

        public double? Damage { get; set; }
        public double? KineticDamage { get; set; }
        public double? TermalDamage { get; set; }
        public double? ElectromagneticDamage { get; set; }
        public double? DestroyerDamage { get; set; }
        public double? AlienDamage { get; set; }
        public double? ElidiumDamage { get; set; }
        public double? CriticalDamage { get; set; }
        public double? CriticalChance { get; set; }
        public double? FireRate { get; set; }
        public double? FireRange { get; set; }
        public double? FireSpread { get; set; }
        public double? ProjectiveSpeed { get; set; }
        public double? HitTime { get; set; }
        public double? CoolingTime { get; set; }
        public double? DecreaseResistance { get; set; }
        public double? ModuleReloadingSpeed { get; set; }
    }

    public static class SeedChipCsvModelExtensions
    {
        public static SeedChip ToDomainModel(this SeedChipCsvModel data)
            => new SeedChip(
                Name: data.Name,
                Level: data.Level ?? 0,
                Parameters: new Dictionary<ModificationType, double?>
                {
                    [ModificationType.Damage] = data.Damage,
                    [ModificationType.KineticDamage] = data.KineticDamage,
                    [ModificationType.TermalDamage] = data.TermalDamage,
                    [ModificationType.ElectromagneticDamage] = data.ElectromagneticDamage,
                    [ModificationType.DestroyerDamage] = data.DestroyerDamage,
                    [ModificationType.AlienDamage] = data.AlienDamage,
                    [ModificationType.ElidiumDamage] = data.ElidiumDamage,
                    [ModificationType.CriticalDamage] = data.CriticalDamage,
                    [ModificationType.CriticalChance] = data.CriticalChance,
                    [ModificationType.FireRate] = data.FireRate,
                    [ModificationType.FireRange] = data.FireRange,
                    [ModificationType.FireSpread] = data.FireSpread,
                    [ModificationType.ProjectiveSpeed] = data.ProjectiveSpeed,
                    [ModificationType.HitTime] = data.HitTime,
                    [ModificationType.CoolingTime] = data.CoolingTime,
                    [ModificationType.DecreaseResistance] = data.DecreaseResistance,
                    [ModificationType.ModuleReloadingSpeed] = data.ModuleReloadingSpeed,
                }.ToDomainModel()
            );

        public static SeedChipCsvModel ToCsvModel(this SeedChip data)
        {
            var parameters = data.Parameters.ToUserModel();
            return new SeedChipCsvModel
            {
                Name = data.Name,
                Level = data.Level,
                Damage = parameters[ModificationType.Damage],
                KineticDamage = parameters[ModificationType.KineticDamage],
                TermalDamage = parameters[ModificationType.TermalDamage],
                ElectromagneticDamage = parameters[ModificationType.ElectromagneticDamage],
                DestroyerDamage = parameters[ModificationType.DestroyerDamage],
                AlienDamage = parameters[ModificationType.AlienDamage],
                ElidiumDamage = parameters[ModificationType.ElidiumDamage],
                CriticalDamage = parameters[ModificationType.CriticalDamage],
                CriticalChance = parameters[ModificationType.CriticalChance],
                FireRate = parameters[ModificationType.FireRate],
                FireRange = parameters[ModificationType.FireRange],
                FireSpread = parameters[ModificationType.FireSpread],
                ProjectiveSpeed = parameters[ModificationType.ProjectiveSpeed],
                HitTime = parameters[ModificationType.HitTime],
                CoolingTime = parameters[ModificationType.CoolingTime],
                DecreaseResistance = parameters[ModificationType.DecreaseResistance],
                ModuleReloadingSpeed = parameters[ModificationType.ModuleReloadingSpeed],
            };
        }
    }
}
