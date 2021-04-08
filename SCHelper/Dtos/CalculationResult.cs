using System.Collections.Generic;
using System.Linq;

namespace SCHelper.Dtos
{
    public record CalculationResult(
        string Name,
        string ShipName,
        string WeaponName,
        Dictionary<DamageTarget, DamageDescription> DamageTarget,
        DamageType DamageType,
        double FireRate,
        double CriticalChance,
        double HitTime,
        double CoollingTime,
        double FireRange,
        double? FireSpread,
        double? ProjectiveSpeed,
        double DecreaseResistance,
        SeedChip[] SeedChips);

    public record DamageDescription(
        double Damage,
        double CriticalDamage,
        Dictionary<DpsType, double> Dps);

    public static class DamageDescriptionExtension
    {
        public static DamageDescription Multiply(this DamageDescription source, double multiplier)
            => multiplier == 1 
            ? source
            : new DamageDescription(
                Damage: source.Damage * multiplier,
                CriticalDamage: source.CriticalDamage * multiplier,
                Dps: source.Dps.ToDictionary(x => x.Key, x => x.Value * multiplier));
    }
}
