﻿using System.Collections.Generic;

namespace SCHelper.Dtos
{
    public class CalculationCommandConfigModel
    {
        public string Name { get; set; }

        public ShipConfigModel Ship { get; set; }

        public string ShipName { get; set; }

        public string WeaponName { get; set; }

        public double? EnemyResistance { get; set; }

        public Dictionary<ModificationType, double?[]> Implants { get; set; }

        public Dictionary<ModificationType, double?[]> Modules { get; set; }

        public TargetPropertyConfigModel[] Targets { get; set; }
    }
}
