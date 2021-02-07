using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public class ShipConfigModel
    {
        public string Name { get; set; }
        public int WeaponCount { get; set; }
        public Dictionary<ModificationType, double?> Bonuses { get; set; }
    }
}
