using Microsoft.Extensions.Logging;
using SCHelper.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCHelper.Services.Impl
{
    public class CalculationService : ICalculationService
    {
        private readonly ILogger<CalculationService> logger;

        public CalculationService(ILogger<CalculationService> logger)
        {
            this.logger = logger;
        }

        public Task<CalculationResult[]> Calc(IEnumerable<CalculationCommand> commands, IEnumerable<SeedChip> seedChips)
        {
            string trackShipName = "Initialization";
            int trackIter = -1;
            int trackIterCount = -1;

            var logProgress = new Action(() => this.logger.LogInformation($"{trackShipName} - {trackIter} / {trackIterCount}"));

            var calcTask = Task.Run(() =>
            {
                var commandsList = commands.ToList();
                var seedChipsList = seedChips.ToList();
                var usedSeedChips = new List<SeedChip>();

                var results = new List<CalculationResult>();
                foreach (var cmd in commandsList)
                {
                    var seedChipCombinations = Utils.GetAllCombinations(
                            data: seedChips
                                .Where(x => x.Level <= cmd.Ship.Level
                                    && (cmd.Weapon.CriticalChance.HasValue || x.Parameters[ModificationType.CriticalChance] >= 0)
                                    && (cmd.Weapon.CriticalDamage.HasValue || x.Parameters[ModificationType.CriticalDamage] >= 0)
                                    && (cmd.Weapon.FireSpread.HasValue || x.Parameters[ModificationType.FireSpread] >= 0)
                                    && (cmd.Weapon.ProjectiveSpeed.HasValue || x.Parameters[ModificationType.ProjectiveSpeed] >= 0)
                                )
                                .ToArray(),
                            count: cmd.Ship.MaxChipCount)
                        .ToArray();

                    trackShipName = cmd.Ship.Name;
                    trackIter = 0;
                    trackIterCount = seedChipCombinations.Length;
                    logProgress();

                    CalculationResult result = seedChipCombinations
                        .Select((seedChips, iter) =>
                        {
                            trackIter = iter;
                            return this.CalcDps(cmd, seedChips);
                        })
                        .OrderByDescending(x => x.DamageTarget[cmd.DamageTarget].Dps)
                        .First();

                    foreach (var seedChip in result.SeedChips)
                        seedChipsList.Remove(seedChip);
                    usedSeedChips.AddRange(result.SeedChips);
                    results.Add(result);

                    logProgress();
                }
                return results.ToArray();
            });

            return Utils.RepeatOnBackground(calcTask, logProgress, TimeSpan.FromSeconds(1));
        }

        public CalculationResult CalcDps(CalculationCommand command, SeedChip[] seedChips)
        {
            var modifications = this.CalcModifications(command, seedChips);
            var multipliers = this.CalcMultipliers(modifications);

            var damage = command.Ship.WeaponCount *
                command.Weapon.Damage *
                multipliers[ModificationType.Damage] *
                command.Weapon.DamageType switch
                {
                    DamageType.Electromagnetic => multipliers[ModificationType.ElectromagneticDamage],
                    DamageType.Kinetic => multipliers[ModificationType.KineticDamage],
                    DamageType.Termal => multipliers[ModificationType.TermalDamage],
                    _ => throw new NotImplementedException()
                };
            var fireRate = command.Weapon.FireRate * multipliers[ModificationType.FireRate];
            var criticalDamageDpsMultiplier = command.Weapon.CriticalChance.HasValue
                ? (1 + multipliers[ModificationType.CriticalChance] * multipliers[ModificationType.CriticalDamage])
                : 1;
            var criticalDamageMultiplier = command.Weapon.CriticalChance.HasValue
                ? (1 + multipliers[ModificationType.CriticalDamage])
                : 1;
            var dps = damage * fireRate * criticalDamageDpsMultiplier;

            var hitTime = command.Weapon.HitTime / multipliers[ModificationType.WeaponHitSpeed];
            var coollingTime = command.Weapon.CoolingTime / multipliers[ModificationType.WeaponCoolingSpeed];

            var damageDescription = new DamageDescription(
                Damage: damage,
                CriticalDamage: damage * criticalDamageMultiplier,
                Dps: dps,
                DpsWithHit: dps * hitTime / (hitTime + coollingTime),
                DpsWithResistance: dps * (1 + multipliers[ModificationType.DecreaseResistance] - command.EnemyResistance));
            var destroyerDamageDescription = damageDescription.Multiply(multipliers[ModificationType.DestroyerDamage]);

            return new CalculationResult(
                Name: command.Name,
                ShipName: command.Ship.Name,
                WeaponName: command.Weapon.Name,
                DamageTarget: new Dictionary<DamageTarget, DamageDescription>
                {
                    [DamageTarget.Normal] = damageDescription,
                    [DamageTarget.Destroyer] = destroyerDamageDescription,
                    [DamageTarget.Alien] = damageDescription.Multiply(multipliers[ModificationType.AlienDamage]),
                    [DamageTarget.Elidium] = damageDescription.Multiply(multipliers[ModificationType.ElidiumDamage]),
                    [DamageTarget.DestroyerAlien] = destroyerDamageDescription.Multiply(multipliers[ModificationType.AlienDamage]),
                    [DamageTarget.DestroyerElidium] = destroyerDamageDescription.Multiply(multipliers[ModificationType.ElidiumDamage]),
                },
                DamageType: command.Weapon.DamageType,
                FireRate: fireRate,
                CriticalChance: multipliers[ModificationType.CriticalChance],
                HitTime: hitTime,
                CoollingTime: coollingTime,
                FireRange: command.Weapon.FireRange * multipliers[ModificationType.FireRange],
                FireSpread: command.Weapon.FireSpread.HasValue
                    ? command.Weapon.FireSpread.Value * multipliers[ModificationType.FireSpread]
                    : null,
                ProjectiveSpeed: command.Weapon.ProjectiveSpeed.HasValue
                    ? command.Weapon.ProjectiveSpeed.Value * multipliers[ModificationType.ProjectiveSpeed]
                    : null,
                DecreaseResistance: multipliers[ModificationType.DecreaseResistance],
                SeedChips: seedChips);
        }

        public Dictionary<ModificationType, IEnumerable<double>> CalcModifications(CalculationCommand command, SeedChip[] seedChips)
        {
            return seedChips.Select(x => x.Parameters)
                .Append(command.Ship.Bonuses)
                .Append(command.Implants)
                .Append(command.Modules)
                .Append(new Dictionary<ModificationType, double>
                {
                    [ModificationType.CriticalChance] = command.Weapon.CriticalChance ?? 0,
                    [ModificationType.CriticalDamage] = command.Weapon.CriticalDamage ?? 0,
                    [ModificationType.DecreaseResistance] = command.Weapon.DecreaseResistance,
                })
                .SelectMany(x => x)
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Value).Where(x => x != 0));
        }

        public Dictionary<ModificationType, double> CalcMultipliers(Dictionary<ModificationType, IEnumerable<double>> modifications)
        {
            return modifications.ToDictionary(x => x.Key,
                x =>
                {
                    switch (x.Key)
                    {
                        case ModificationType.Damage:
                            return 1;

                        case ModificationType.KineticDamage:
                        case ModificationType.TermalDamage:
                        case ModificationType.ElectromagneticDamage:
                            return this.CalcMultiplier(x.Value.Concat(modifications[ModificationType.Damage]));

                        case ModificationType.DestroyerDamage:
                        case ModificationType.AlienDamage:
                        case ModificationType.ElidiumDamage:
                        case ModificationType.FireRange:
                        case ModificationType.FireSpread:
                        case ModificationType.ProjectiveSpeed:
                        case ModificationType.WeaponCoolingSpeed:
                        case ModificationType.ModuleReloadingSpeed:
                            return this.CalcMultiplier(x.Value);

                        case ModificationType.WeaponHitSpeed:
                            return this.CalcMultiplier(x.Value.Concat(modifications[ModificationType.FireRate]));

                        case ModificationType.FireRate:
                            return this.CalcMultiplier(x.Value, min: 0, max: 10);

                        case ModificationType.CriticalChance:
                            return this.CalcMultiplier(x.Value, min: 1, max: 2) - 1;

                        case ModificationType.CriticalDamage:
                            return this.CalcMultiplier(x.Value, min: 1) - 1;

                        case ModificationType.DecreaseResistance:
                            return x.Value.Sum();

                        default:
                            throw new NotImplementedException();
                    };
                });
        }

        public double CalcMod(double x) => x < 0 ? x / (1 + x) : x;
        public double CalcVal(double x) => x < 0 ? 1 / (1 - x) : 1 + x;
        public double CutOverflow(double value, double min = double.MinValue, double max = double.MaxValue)
            => Math.Min(max, Math.Max(min, value));
        public double CalcMultiplier(IEnumerable<double> modifications, double min = double.MinValue, double max = double.MaxValue)
            => this.CutOverflow(
                value: this.CalcVal(modifications.Select(this.CalcMod).Sum()),
                min: min,
                max: max
            );
    }
}
