using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public record CalculationCommand(
        string Name,
        DamageTarget DamageTarget,
        Ship Ship,
        Weapon Weapon,
        SeedChip[] SeedChips,
        Dictionary<ModificationType, double> Implants,
        Dictionary<ModificationType, double> Modules);
}
