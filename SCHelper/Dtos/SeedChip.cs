using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public record SeedChip(
        string Name,
        Dictionary<ModificationType, double> Parameters
    );
}
