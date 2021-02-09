using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public class ConfigModel
    {
        public string OutputFile { get; set; }

        public CalculationCommandConfigModel[] Calculate { get; set; }

        public ShipConfigModel[] Ships { get; set; }
        
        public WeaponConfigModel[] Weapons { get; set; }

        public Dictionary<ModificationType, double?>[] SeedChips { get; set; }
    }
}
