namespace SCHelper.Dtos
{
    public class WeaponConfigModel
    {
        public string Name { get; set; }
        public DamageType DamageType { get; set; }
        public double Damage { get; set; }
        public double FireRate { get; set; }
        public double CriticalChance { get; set; }
        public double CriticalDamage { get; set; }
        public double HitTime { get; set; }
        public double CoollingTime { get; set; }
        public double FireRange { get; set; }
        public double? FireSpread { get; set; }
        public double? ProjectiveSpeed { get; set; }
        public double DecreaseResistance { get; set; }
    }
}
