using SCHelper.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCHelper.Services.Impl
{
    public class ConversionService : IConversionService
    {
        public Ship ToDomainModel(ShipConfigModel ship)
            => new Ship(
                Name: ship.Name,
                Level: ship.Level ?? 0,
                WeaponCount: ship.WeaponCount,
                MaxChipCount: ship.MaxChipCount ?? 5,
                Bonuses: this.ToDomainModel(ship.Bonuses ?? new Dictionary<ModificationType, double?>())
            );

        public Weapon ToDomainModel(WeaponConfigModel weapon)
            => new Weapon(
                Name: weapon.Name,
                DamageType: weapon.DamageType,
                Damage: weapon.Damage,
                FireRate: weapon.FireRate / 60,
                CriticalChance: weapon.CriticalChance / 100,
                CriticalDamage: weapon.CriticalDamage / 100,
                HitTime: weapon.HitTime,
                CoolingTime: weapon.CoolingTime,
                FireRange: weapon.FireRange,
                FireSpread: weapon.FireSpread,
                ProjectiveSpeed: weapon.ProjectiveSpeed,
                DecreaseResistance: Math.Abs(weapon.DecreaseResistance ?? 0) / 100
            );

        public SeedChip ToDomainModel(SeedChipConfigModel seedChip)
            => new SeedChip(
                Name: seedChip.Name,
                Level: seedChip.Level ?? 0,
                Parameters: this.ToDomainModel(seedChip.Parameters ?? new Dictionary<ModificationType, double?>())
            );

        public Dictionary<ModificationType, double> ToDomainModel(Dictionary<ModificationType, double?> modifications)
            => Utils.GetEmptyDictionary<ModificationType, double?>()
                .Override(modifications)
                .ToDictionary(x => x.Key, x =>
                {
                    switch (x.Key)
                    {
                        case ModificationType.DecreaseResistance:
                            return Math.Abs(x.Value ?? 0) / 100;
                        default:
                            return (x.Value ?? 0) / 100;
                    }
                });

        public CalculationResult ToUserDataModel(CalculationResult data)
            => data with
            {
                FireRate = 60 * data.FireRate,
                CriticalChance = 100 * data.CriticalChance,
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
