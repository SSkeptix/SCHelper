using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public record SeedChip(
        Dictionary<ModificationType, double> Parameters
    );
}
