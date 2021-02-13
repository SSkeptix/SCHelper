﻿using SCHelper.Dtos;
using System.Collections.Generic;

namespace SCHelper
{
    public static class Constants
    {
        public const string ConfigFilePath = "appsettings.json";

        private static readonly Dictionary<ModificationType, double?> SeedChipSample = new()
        {
            [ModificationType.Damage] = -18.5,
            [ModificationType.ElectromagneticDamage] = 78,
            [ModificationType.WeaponHitSpeed] = -46.8,
            [ModificationType.CriticalChance] = 14.6,
            [ModificationType.FireRate] = 92,
        };

        public static readonly ConfigModel DefaultConfigModel = new ConfigModel
        {
            OutputFile = "output.json",
            Ships = new ShipConfigModel[]
            {
                new ShipConfigModel
                {
                    Name = "Berserker",
                    WeaponCount = 2,
                    Bonuses = new Dictionary<ModificationType, double?>
                    {
                        [ModificationType.Damage] = 7,
                        [ModificationType.CriticalChance] = 25,
                    },
                },
                new ShipConfigModel
                {
                    Name = "Rockwell",
                    WeaponCount = 4,
                    Bonuses = Utils.GetEmptyDictionary<ModificationType, double?>()
                        .Override(new Dictionary<ModificationType, double?>
                        {
                            [ModificationType.Damage] = 10,
                            [ModificationType.CriticalDamage] = 20,
                        }),
                },
                new ShipConfigModel
                {
                    Name = "Black Dragon",
                    WeaponCount = 6,
                    Bonuses = Utils.GetEmptyDictionary<ModificationType, double?>(0)
                        .Override(new Dictionary<ModificationType, double?>
                        {
                            [ModificationType.Damage] = 7,
                            [ModificationType.CriticalChance] = 15,
                        }),
                },
            },
            Weapons = new WeaponConfigModel[]
            {
                new WeaponConfigModel
                {
                    Name = "PlasmagunTrasser",
                    DamageType = DamageType.Electromagnetic,
                    Damage = 231,
                    FireRate = 200,
                    CriticalChance = 5,
                    CriticalDamage = 75,
                    HitTime = 5.5,
                    CoolingTime = 1,
                    FireRange = 2350,
                    FireSpread = 0.25,
                    ProjectiveSpeed = 5000,
                },
                new WeaponConfigModel
                {
                    Name = "Ионний излучатель 17",
                    DamageType = DamageType.Termal,
                    Damage = 505,
                    FireRate = 60,
                    CriticalChance = 5,
                    CriticalDamage = 50,
                    HitTime = 11,
                    CoolingTime = 2,
                    FireRange = 3000,
                    DecreaseResistance = 80
                },
                new WeaponConfigModel
                {
                    Name = "WazDum",
                    DamageType = DamageType.Kinetic,
                    Damage = 276,
                    FireRate = 125,
                    CriticalChance = 7,
                    CriticalDamage = 50,
                    HitTime = 5,
                    CoolingTime = 1,
                    FireRange = 4000,
                    FireSpread = 1.1,
                    ProjectiveSpeed = 6000,
                },
            },
            SeedChips = new SeedChipConfigModel[]
            {
                new SeedChipConfigModel
                {
                    Name = "Seed Chip Name",
                    Parameters = Constants.SeedChipSample,
                },
                new SeedChipConfigModel
                {
                    Name = "Seed Chip Name",
                    Parameters = Utils.GetEmptyDictionary<ModificationType, double?>(),
                },
                new SeedChipConfigModel
                {
                    Name = "Seed Chip Name",
                    Parameters = Utils.GetEmptyDictionary<ModificationType, double?>().Override(Constants.SeedChipSample),
                },
                new SeedChipConfigModel
                {
                    Name = "Seed Chip Name",
                    Parameters = Utils.GetEmptyDictionary<ModificationType, double?>(0),
                },
                new SeedChipConfigModel
                {
                    Name = "Seed Chip Name",
                    Parameters = Utils.GetEmptyDictionary<ModificationType, double?>(0).Override(Constants.SeedChipSample),
                },
            },
        };
    }
}
