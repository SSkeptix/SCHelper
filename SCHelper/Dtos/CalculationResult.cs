using System.Collections.Generic;

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
        double Dps,
        double DpsWithHit,
        double DpsWithResistance);

    public static class DamageDescriptionExtension
    {
        public static DamageDescription Multiply(this DamageDescription source, double multiplier)
            => new DamageDescription(
                Damage: source.Damage * multiplier,
                CriticalDamage: source.CriticalDamage * multiplier,
                Dps: source.Dps * multiplier,
                DpsWithHit: source.DpsWithHit * multiplier,
                DpsWithResistance: source.DpsWithResistance * multiplier);
    }
}
