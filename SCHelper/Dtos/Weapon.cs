namespace SCHelper.Dtos
{
    public record Weapon(
        string Name,
        DamageType DamageType,
        double Damage,
        double FireRate,
        double CriticalChance,
        double CriticalDamage,
        double HitTime,
        double CoollingTime,
        double FireRange,
        double FireSpread,
        double ProjectiveSpeed,
        double DecreaseHullResistance
    );
}
