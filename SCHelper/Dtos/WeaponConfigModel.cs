using System;

namespace SCHelper.Dtos
{
    public class WeaponConfigModel
    {
        public string Name { get; set; }
        public DamageType DamageType { get; set; }
        public double Damage { get; set; }
        public double FireRate { get; set; }
        public double? CriticalChance { get; set; }
        public double? CriticalDamage { get; set; }
        public double HitTime { get; set; }
        public double CoolingTime { get; set; }
        public double FireRange { get; set; }
        public double? FireSpread { get; set; }
        public double? ProjectiveSpeed { get; set; }
        public double? DecreaseResistance { get; set; }
    }

    public static class WeaponConfigModelExtensions
    {
        public static Weapon ToDomainModel(this WeaponConfigModel data)
            => new Weapon(
                Name: data.Name,
                DamageType: data.DamageType,
                Damage: data.Damage,
                FireRate: data.FireRate / 60,
                CriticalChance: data.CriticalChance / 100,
                CriticalDamage: data.CriticalDamage / 100,
                HitTime: data.HitTime,
                CoolingTime: data.CoolingTime,
                FireRange: data.FireRange,
                FireSpread: data.FireSpread,
                ProjectiveSpeed: data.ProjectiveSpeed,
                DecreaseResistance: Math.Abs(data.DecreaseResistance ?? 0) / 100
            );
    }
}
