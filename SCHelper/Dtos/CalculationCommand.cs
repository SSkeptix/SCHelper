using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public record CalculationCommand(
        string Name,
        Ship Ship,
        Weapon Weapon,
        SeedChip[] SeedChips,
        Dictionary<ModificationType, double> Implants,
        Dictionary<ModificationType, double> Modules);
}
