namespace SCHelper.Dtos
{
    public class ConfigModel
    {
        public string OutputFile { get; set; }

        public CalculationCommandConfigModel[] Calculate { get; set; }

        public ShipConfigModel[] Ships { get; set; }
        public string ShipsFilePath { get; set; }

        public WeaponConfigModel[] Weapons { get; set; }
        public string WeaponsFilePath { get; set; }

        public SeedChipConfigModel[] SeedChips { get; set; }

        public string SeedChipsFilePath { get; set; }
    }
}
