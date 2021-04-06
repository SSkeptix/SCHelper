namespace SCHelper
{
    public enum DamageType
    {
        Kinetic,
        Termal,
        Electromagnetic,
    }

    public enum DpsType
    {
        Normal,
        WithHit,
        WithResistance,
        WithHitAndResistance,
    }

    public enum DamageTarget
    {
        Normal,
        Destroyer,
        Alien,
        Elidium,
        DestroyerAlien,
        DestroyerElidium
    }

    public enum ModificationType
    {
        Damage,
        KineticDamage,
        TermalDamage,
        ElectromagneticDamage,
        DestroyerDamage,
        AlienDamage,
        ElidiumDamage,
        CriticalDamage,
        CriticalChance,
        FireRate,
        FireRange,
        FireSpread,
        ProjectiveSpeed,
        HitTime,
        CoolingTime,
        DecreaseResistance,
        ModuleReloadingSpeed,
    }
}
