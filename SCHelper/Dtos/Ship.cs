﻿using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public record Ship(
        string Name,
        int WeaponCount,
        Dictionary<ModificationType, double> Bonuses
    );
}