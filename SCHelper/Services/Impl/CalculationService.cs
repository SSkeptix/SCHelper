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

        public Task<CalculationResult[]> Calc(CalculationCommand[] commands, SeedChip[] seedChips)
        {
            string trackShipName = "Initialization";
            int trackIter = -1;
            int trackIterCount = -1;

            var logProgress = new Action(() => this.logger.LogInformation($"{trackShipName} - {trackIter} / {trackIterCount}"));

            var allScores = new List<(SeedChip chip, double score)>();

            var calcTask = Task.Run(() =>
            {
                var seedChipsList = seedChips.ToList();
                var usedSeedChips = new List<SeedChip>();

                var results = new List<CalculationResult>();
                foreach (var cmd in commands)
                {
                    var suitableSeedChips = GetSuitableSeedChips(cmd, seedChipsList);
                    var seedChipCombinations = Utils.GetAllCombinations(
                            data: suitableSeedChips,
                            count: cmd.Ship.MaxChipCount)
                        .ToArray();

                    trackShipName = cmd.Ship.Name;
                    trackIter = 0;
                    trackIterCount = seedChipCombinations.Length;
                    logProgress();



                    var scores = CalcChipScore(cmd, seedChips);
                    var maxScore = scores.Max(x => x.score);
                    scores = scores.Select(x =>
                    {
                        x.score /= maxScore;
                        return x;
                    }).ToArray();
                    allScores.AddRange(scores);



                    CalculationResult result = seedChipCombinations
                        .AsParallel()
                        .Select(seedChips =>
                        {
                            trackIter++;
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




                var seedChipNameLength = seedChips.Max(x => x.Name.Length);
                var scores1 = allScores
                    .GroupBy(x => x.chip)
                    .Select(x => (Chip: x.Key, Score: x.Max(y => y.score)))
                    .OrderByDescending(x => x.Score)
                    .ToArray();

                scores1.OrderByDescending(x => x.Score).ToList()
                    .ForEach(x => this.logger.LogInformation(x.Chip.Name.PadRight(seedChipNameLength, ' ') + $": {x.Score:F2}"));

                this.logger.LogInformation("===================================");

                scores1.OrderBy(x => x.Chip.Name).ToList()
                    .ForEach(x => this.logger.LogInformation(x.Chip.Name.PadRight(seedChipNameLength, ' ') + $": {x.Score:F2}"));



                return results.ToArray();
            });

            return Utils.RepeatOnBackground(calcTask, logProgress, TimeSpan.FromSeconds(1));
        }

        private static SeedChip[] GetSuitableSeedChips(CalculationCommand command, IEnumerable<SeedChip> seedChips)
            => seedChips
            .Where(x => x.Level <= command.Ship.Level
                && (command.Weapon.CriticalChance.HasValue || x.Parameters.GetValueOrDefault(ModificationType.CriticalChance) >= 0)
                && (command.Weapon.CriticalDamage.HasValue || x.Parameters.GetValueOrDefault(ModificationType.CriticalDamage) >= 0)
                && (command.Weapon.FireSpread.HasValue || x.Parameters.GetValueOrDefault(ModificationType.FireSpread) >= 0)
                && (command.Weapon.ProjectiveSpeed.HasValue || x.Parameters.GetValueOrDefault(ModificationType.ProjectiveSpeed) >= 0)
            )
            .ToArray();

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
                    or ModificationType.ProjectiveSpeed
                        => CalcMultiplier(x.Value),
                    ModificationType.FireSpread
                    or ModificationType.ModuleReloadingSpeed
                        => CalcMultiplier(x.Value.Select(x => -x)),
                    ModificationType.CoolingTime
                        => 1 / CalcMultiplier(x.Value),
                    ModificationType.HitTime
                        => 1 / CalcMultiplier(x.Value.Select(x => -x).Concat(modifications.GetValueOrDefault(ModificationType.FireRate, Enumerable.Empty<double>()))),
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

        public void CalcChipScores(CalculationCommand[] commands, SeedChip[] seedChips)
        {
            var seedChipNameLength = seedChips.Max(x => x.Name.Length);

            var scores = commands
                .SelectMany(command =>
                {
                    var scores = CalcChipScore(command, seedChips)
                        .OrderByDescending(x => x.score)
                        .ToArray();
                    var maxScore = scores.Max(x => x.score);
                    return scores.Select(x =>
                    {
                        x.score /= maxScore;
                        return x;
                    }).ToArray();
                })
                .GroupBy(x => x.chip)
                .Select(x => (Chip: x.Key, Score: x.Max(y => y.score)))
                .OrderByDescending(x => x.Score)
                .ToArray();

            scores.OrderByDescending(x => x.Score).ToList()
                .ForEach(x => this.logger.LogInformation(x.Chip.Name.PadRight(seedChipNameLength, ' ') + $": {x.Score:F2}"));

            this.logger.LogInformation("===================================");

            scores.OrderBy(x => x.Chip.Name).ToList()
                .ForEach(x => this.logger.LogInformation(x.Chip.Name.PadRight(seedChipNameLength, ' ') + $": {x.Score:F2}"));
        }

        public (SeedChip chip, double score)[] CalcChipScore(CalculationCommand command, SeedChip[] seedChips)
        {
            var multipliers = CalcMultipliers(CalcModifications(command, Array.Empty<SeedChip>()));
            var fireRateMaxMutiplier = 10 / command.Weapon.FireRate - multipliers.GetValueOrDefault(ModificationType.FireRate);
            var critChanceMaxMutiplier = 1 - multipliers.GetValueOrDefault(ModificationType.CriticalChance);

            var maxMods = seedChips.Where(x => x.Parameters.All(y => y.Value >= 0))
                .SelectMany(x => x.Parameters)
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Max(y => y.Value));

            var fileRateEfficiency = CutOverflow(
                value: fireRateMaxMutiplier / (maxMods.GetValueOrDefault(ModificationType.FireRate, 1) * command.Ship.MaxChipCount),
                min: 0,
                max: 1);

            var critChanceEfficiency = command.Weapon.CriticalChance.HasValue
                ? CutOverflow(
                    value: critChanceMaxMutiplier / (maxMods[ModificationType.CriticalChance] * command.Ship.MaxChipCount),
                    min: 0,
                    max: 1)
                : 0;

            var seedChipNameLength = seedChips.Max(x => x.Name.Length);

            return seedChips.Select(chip =>
                {
                    double score = 0;
                    foreach (var mod in chip.Parameters)
                    {
                        var val = CalcMod(mod.Value);
                        score += mod.Key switch
                        {
                            ModificationType.Damage
                            or ModificationType.CriticalDamage
                                => val / maxMods[mod.Key],
                            ModificationType.FireRate
                                => (val >= 0 ? fileRateEfficiency : 1) * val / maxMods[mod.Key],
                            ModificationType.CriticalChance
                                => (val >= 0 ? critChanceEfficiency : 1) * val / maxMods[mod.Key],
                            ModificationType.KineticDamage
                                => command.Weapon.DamageType == DamageType.Kinetic ? val / maxMods[mod.Key] : 0,
                            ModificationType.TermalDamage
                                => command.Weapon.DamageType == DamageType.Termal ? val / maxMods[mod.Key] : 0,
                            ModificationType.ElectromagneticDamage
                                => command.Weapon.DamageType == DamageType.Electromagnetic ? val / maxMods[mod.Key] : 0,
                            ModificationType.AlienDamage
                                => command.DamageTarget == DamageTarget.Alien || command.DamageTarget == DamageTarget.DestroyerAlien ? val / maxMods[mod.Key] : 0,
                            ModificationType.DestroyerDamage
                                => command.DamageTarget == DamageTarget.Destroyer || command.DamageTarget == DamageTarget.DestroyerAlien ? val / maxMods[mod.Key] : 0,
                            _ => 0
                        };
                    }

                    return (chip, score);
                })
                .ToArray();
        }
    }
}
