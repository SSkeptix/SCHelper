namespace SCHelper.Dtos
{
    public record Weapon(
        string Name,
        WeaponDamageType WeaponDamageType,
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
