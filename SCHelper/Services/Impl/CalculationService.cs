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
                            data: seedChipsList
                                .Where(x => x.Level <= cmd.Ship.Level
                                    && (cmd.Weapon.CriticalChance.HasValue || x.Parameters.GetValueOrDefault(ModificationType.CriticalChance) >= 0)
                                    && (cmd.Weapon.CriticalDamage.HasValue || x.Parameters.GetValueOrDefault(ModificationType.CriticalDamage) >= 0)
                                    && (cmd.Weapon.FireSpread.HasValue || x.Parameters.GetValueOrDefault(ModificationType.FireSpread) >= 0)
                                    && (cmd.Weapon.ProjectiveSpeed.HasValue || x.Parameters.GetValueOrDefault(ModificationType.ProjectiveSpeed) >= 0)
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
                            return CalcDps(cmd, seedChips);
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

        public static CalculationResult CalcDps(CalculationCommand command, SeedChip[] seedChips)
        {
            var modifications = CalcModifications(command, seedChips);
            var multipliers = CalcMultipliers(modifications);

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
            var fireRate = CutOverflow(command.Weapon.FireRate * multipliers[ModificationType.FireRate], 0, 10);
            var criticalDamageDpsMultiplier = command.Weapon.CriticalChance.HasValue
                ? (1 + multipliers[ModificationType.CriticalChance] * multipliers[ModificationType.CriticalDamage])
                : 1;
            var criticalDamageMultiplier = command.Weapon.CriticalChance.HasValue
                ? (1 + multipliers[ModificationType.CriticalDamage])
                : 1;
            var dps = damage * fireRate * criticalDamageDpsMultiplier;

            var hitTime = command.Weapon.HitTime * multipliers[ModificationType.HitTime];
            var coollingTime = command.Weapon.CoolingTime * multipliers[ModificationType.CoolingTime];

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

        public static Dictionary<ModificationType, IEnumerable<double>> CalcModifications(CalculationCommand command, SeedChip[] seedChips)
            => seedChips.Select(x => x.Parameters)
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

        public static Dictionary<ModificationType, double> CalcMultipliers(Dictionary<ModificationType, IEnumerable<double>> modifications)
            => Utils.GetEmptyDictionary<ModificationType, double>()
            .Override(modifications.ToDictionary(x => x.Key,
                x => x.Key switch
                {
                    ModificationType.Damage
                        => 1,
                    ModificationType.KineticDamage
                    or ModificationType.TermalDamage
                    or ModificationType.ElectromagneticDamage
                        => CalcMultiplier(x.Value.Concat(modifications[ModificationType.Damage])),
                    ModificationType.DestroyerDamage
                    or ModificationType.AlienDamage
                    or ModificationType.ElidiumDamage
                    or ModificationType.FireRange
                    or ModificationType.FireSpread
                    or ModificationType.ProjectiveSpeed
                    or ModificationType.ModuleReloadingSpeed
                        => CalcMultiplier(x.Value),
                    ModificationType.CoolingTime
                        => 1 / CalcMultiplier(x.Value.Select(x => -x)),
                    ModificationType.HitTime
                        => 1 / CalcMultiplier(x.Value.Concat(modifications[ModificationType.FireRate])),
                    ModificationType.FireRate
                        => CalcMultiplier(x.Value, min: 0, max: 10),
                    ModificationType.CriticalChance
                        => CalcMultiplier(x.Value, min: 1, max: 2) - 1,
                    ModificationType.CriticalDamage
                        => CalcMultiplier(x.Value, min: 1) - 1,
                    ModificationType.DecreaseResistance
                        => x.Value.Sum(),
                    _ => throw new NotImplementedException()
                })
            );

        public static double CalcMod(double x) => x < 0 ? x / (1 + x) : x;
        public static double CalcVal(double x) => x < 0 ? 1 / (1 - x) : 1 + x;
        public static double CutOverflow(double value, double min = double.MinValue, double max = double.MaxValue)
            => Math.Min(max, Math.Max(min, value));
        public static double CalcMultiplier(IEnumerable<double> modifications, double min = double.MinValue, double max = double.MaxValue)
            => CutOverflow(
                value: CalcVal(modifications.Select(CalcMod).Sum()),
                min: min,
                max: max
            );
    }
}
