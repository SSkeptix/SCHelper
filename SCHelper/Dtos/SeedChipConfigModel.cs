using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public class SeedChipConfigModel
    {
        public string Name { get; set; }
        public int? Level { get; set; }
        public Dictionary<ModificationType, double?> Parameters { get; set; }
    }
}
