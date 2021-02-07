using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public class ConfigModel
    {
        public ShipConfigModel[] Ships { get; set; }
        
        public WeaponConfigModel[] Weapons { get; set; }

        public Dictionary<ModificationType, double?>[] SeedChips { get; set; }
    }
}
