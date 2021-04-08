using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public record CalculationCommand(
        string Name,
        double EnemyResistance,
        Ship Ship,
        Weapon Weapon,
        Dictionary<ModificationType, double[]> Implants,
        Dictionary<ModificationType, double[]> Modules,
        TargerPropertyModel[] Targets);
}
