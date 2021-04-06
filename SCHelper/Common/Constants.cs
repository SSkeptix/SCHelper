using SCHelper.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace SCHelper
{
    public static class Constants
    {
        public const string ConfigFilePath = "appsettings.json";

        private static readonly Dictionary<ModificationType, double?> SeedChipSample = new()
        {
            [ModificationType.Damage] = -18.5,
            [ModificationType.ElectromagneticDamage] = 78,
            [ModificationType.HitTime] = -46.8,
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

    public static class SampleData
    {
        public static ConfigModel AppSettings = new ConfigModel
        {
            OutputFile = "output.json",
            Calculate = new CalculationCommandConfigModel[]
            {
                new CalculationCommandConfigModel
                {
                    Name = "Berserker Asteroid",
                    ShipName = "Berserker",
                    WeaponName = "Nosovoi EM lazer 17",
                    DamageTarget = DamageTarget.Normal,
                    EnemyResistance = 0,
                    Implants = new()
                    {
                        [ModificationType.Damage] = new double?[]{ 3 },
                        [ModificationType.CriticalDamage] = new double?[]{ 30 },
                        [ModificationType.CriticalChance] = new double?[]{ 10 },
                        [ModificationType.DecreaseResistance] = new double?[]{ 10 },
                    },
                    Modules = new()
                    {
                        [ModificationType.CriticalChance] = new double?[]{ 11 },
                    }
                },
                new CalculationCommandConfigModel
                {
                    Name = "Jaguar SO",
                    ShipName = "Jaguar",
                    WeaponName = "IonGun 17",
                    DamageTarget = DamageTarget.Alien,
                    EnemyResistance = 0,
                    Implants = new()
                    {
                        [ModificationType.Damage] = new double?[]{ 3 },
                        [ModificationType.CriticalDamage] = new double?[]{ 30 },
                        [ModificationType.CriticalChance] = new double?[]{ 10 },
                        [ModificationType.DecreaseResistance] = new double?[]{ 10 },
                    },
                    Modules = new()
                    {
                        [ModificationType.CriticalChance] = new double?[]{ 10.5 },
                        [ModificationType.Damage] = new double?[]{ -6.5 },
                        [ModificationType.FireRange] = new double?[]{ 40.1 },
                    }
                },
            },
            ShipsFilePath = "Ships.csv",
            WeaponsFilePath = "Weapons.csv",
            SeedChipsFilePath = "SeedChips.csv",
        };

        public static ShipConfigModel[] ShipConfigModels = new ShipConfigModel[]
        {
            new ShipConfigModel
            {
                Name = "Berserker",
                Level = 15,
                WeaponCount = 2,
                MaxChipCount = 5,
                Bonuses = new()
                {
                    [ModificationType.Damage] = 7,
                    [ModificationType.CriticalChance] = 25,
                },
            },
            new ShipConfigModel
            {
                Name = "Jaguar",
                Level = 15,
                WeaponCount = 4,
                MaxChipCount = 5,
                Bonuses = new()
                {
                    [ModificationType.Damage] = 10,
                },
            }
        };

        public static ShipCsvModel[] ShipCsvModels = ShipConfigModels.Select(x => x.ToDomainModel().ToCsvModel()).ToArray();

        public static SeedChipConfigModel[] SeedChipConfigModels = new SeedChipConfigModel[]
        {
            new SeedChipConfigModel
            {
                Name = "Подталкивающий чип молнии 15",
                Level = 15,
                Parameters = new()
                {
                    [ModificationType.FireRate] = 92,
                    [ModificationType.Damage] = -18.5,
                    [ModificationType.HitTime] = -46.8,
                    [ModificationType.ElectromagneticDamage] = 78,
                    [ModificationType.CriticalChance] = 14.6,
                }
            },
            new SeedChipConfigModel
            {
                Name = "Таранний чип вспишки 13",
                Level = 13,
                Parameters = new()
                {
                    [ModificationType.CriticalDamage] = 49.3,
                    [ModificationType.ElectromagneticDamage] = 56,
                    [ModificationType.CriticalChance] = 9.4,
                }
            },
            new SeedChipConfigModel
            {
                Name = "Подталкивающий чип Вознесения 13",
                Level = 13,
                Parameters = new()
                {
                    [ModificationType.FireRate] = 78,
                    [ModificationType.Damage]= -19.2,
                    [ModificationType.FireSpread] = -32.9,
                    [ModificationType.CoolingTime ]= 75,
                }
            },
            new SeedChipConfigModel
            {
                Name = "Лечебний чип безжалости 13",
                Level = 13,
                Parameters = new()
                {
                    [ModificationType.Damage] = 56,
                    [ModificationType.CriticalChance] = 9.6,
                }
            },
            new SeedChipConfigModel
            {
                Name = "Ураганний чип вспишки 13",
                Level = 13,
                Parameters = new()
                {
                    [ModificationType.HitTime] = -61,
                    [ModificationType.ElectromagneticDamage] = 56,
                    [ModificationType.Damage] = 59,
                }
            },
            new SeedChipConfigModel
            {
                Name = "Шквальний чип жгучести 15",
                Level = 15,
                Parameters = new()
                {
                    [ModificationType.TermalDamage] = 71,
                    [ModificationType.CriticalDamage] = 36.1,
                    [ModificationType.FireRate] = 31.1,
                    [ModificationType.Damage] = 44.4,
                }
            },
            new SeedChipConfigModel
            {
                Name = "Убистряющий чип Сжатия 15",
                Level = 15,
                Parameters = new()
                {
                    [ModificationType.FireRate] = 102,
                    [ModificationType.Damage] = -15.1,
                    [ModificationType.CriticalDamage] = 38.1,
                }
            },
            new SeedChipConfigModel
            {
                Name = "Ураганний чип счастья 13",
                Level = 13,
                Parameters = new()
                {
                    [ModificationType.CriticalChance] = 23.2,
                    [ModificationType.HitTime] = -31,
                    [ModificationType.Damage] = 66,
                    [ModificationType.CriticalDamage] = 35.2,
                }
            },
            new SeedChipConfigModel
            {
                Name = "Ураганний чип прорива 13",
                Level = 13,
                Parameters = new()
                {
                    [ModificationType.Damage] = 59,
                    [ModificationType.CriticalDamage] = 33.7,
                    [ModificationType.ProjectiveSpeed] = 17.6,
                    [ModificationType.CriticalChance] = 18.2,
                }
            },
            new SeedChipConfigModel
            {
                Name = "Буревой чип счастья 13",
                Level = 13,
                Parameters = new()
                {
                    [ModificationType.ModuleReloadingSpeed] = -19.4,
                    [ModificationType.Damage] = 29.3,
                    [ModificationType.FireRate] = 23.6,
                    [ModificationType.CriticalChance] = 17.5,
                }
            }
        };  

        public static SeedChipCsvModel[] SeedChipCsvModels = SeedChipConfigModels.Select(x => x.ToDomainModel().ToCsvModel()).ToArray();

        public static WeaponConfigModel[] WeaponConfigModels = new WeaponConfigModel[]
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
                DecreaseResistance = null,
            },
            new WeaponConfigModel
            {
                Name = "Nosovoi EM lazer 17",
                DamageType = DamageType.Electromagnetic,
                Damage = 496,
                FireRate = 180,
                CriticalChance = 5,
                CriticalDamage = 50,
                HitTime = 6,
                CoolingTime = 1.5,
                FireRange = 2650,
                FireSpread = null,
                ProjectiveSpeed = null,
                DecreaseResistance = null,
            },
            new WeaponConfigModel
            {
                Name = "IonGun 17",
                DamageType = DamageType.Termal,
                Damage = 505,
                FireRate = 60,
                CriticalChance = 5,
                CriticalDamage = 50,
                HitTime = 11,
                CoolingTime = 2,
                FireRange = 3000,
                FireSpread = null,
                ProjectiveSpeed = null,
                DecreaseResistance = 80,
            },
        };
    }
}
