namespace SCHelper.Dtos
{
    public record ShipParameters(
        DamageDescription Damage,
        DamageDescription DestroyerDamage,
        DamageDescription AlienDamage,
        DamageDescription ElidiumDamage,
        DamageDescription DestroyerAlienDamage,
        DamageDescription DestroyerElidiumDamage,

        string Name,
        DamageType DamageType,
        double FireRate,
        double CriticalChance,
        double HitTime,
        double CoollingTime,
        double FireRange,
        double FireSpread,
        double ProjectiveSpeed,
        double DecreaseHullResistance);

    public record DamageDescription(
        double Damage,
        double CriticalDamage,
        double Dps,
        double DpsWithHit);

    public static class ShipParameterExtension
    {
        public static DamageDescription Multiply(this DamageDescription source, double multiplier)
            => new DamageDescription(
                Damage: source.Damage * multiplier,
                CriticalDamage: source.CriticalDamage * multiplier,
                Dps: source.Dps * multiplier,
                DpsWithHit: source.DpsWithHit * multiplier);
    }
}
