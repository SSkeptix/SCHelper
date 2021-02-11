using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public record Ship(
        string Name,
        int Level,
        int WeaponCount,
        int MaxChipCount,
        Dictionary<ModificationType, double> Bonuses
    );
}
