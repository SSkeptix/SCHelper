using SCHelper.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace SCHelper.Services.Impl
{
    public class ConversionService : IConversionService
    {
        public Ship ToDomainModel(ShipConfigModel ship)
            => new Ship(
                Name: ship.Name,
                WeaponCount: ship.WeaponCount,
                Bonuses: this.ToDomainModel(ship.Bonuses)
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
                CoollingTime: weapon.CoollingTime,
                FireRange: weapon.FireRange,
                FireSpread: weapon.FireSpread,
                ProjectiveSpeed: weapon.ProjectiveSpeed,
                DecreaseHullResistance: weapon.DecreaseHullResistance / 100
            );

        public Dictionary<ModificationType, double> ToDomainModel(Dictionary<ModificationType, double?> modifications)
        {
            var result = modifications.ToDictionary(x => x.Key, x => (x.Value ?? 0) / 100);

            foreach (var modificationType in Utils.GetEnumValues<ModificationType>())
            {
                if (!result.ContainsKey(modificationType))
                    result[modificationType] = 0;
            }

            return result;
        }
    }
}
