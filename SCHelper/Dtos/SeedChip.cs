using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public record SeedChip(
        string Name,
        int Level,
        Dictionary<ModificationType, double> Parameters
    );
}
