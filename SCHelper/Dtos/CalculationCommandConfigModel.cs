using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public class CalculationCommandConfigModel
    {
        public ShipConfigModel Ship { get; set; }
        public string ShipName { get; set; }

        public WeaponConfigModel Weapon { get; set; }
        public string WeaponName { get; set; }

        public Dictionary<ModificationType, double?>[] SeedChips { get; set; }
    }
}
