using Microsoft.Extensions.Logging;
using SCHelper.Dtos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SCHelper.Services.Impl
{
    public class CalculationService : ICalculationService
    {
        private readonly object BestResultLock = new object();
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

            var calcTask = Task.Run(() =>
            {
                var seedChipsList = seedChips.ToList();
                var usedSeedChips = new List<SeedChip>();

                var results = new List<CalculationResult>();
                foreach (var cmd in commands)
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

                    var calcEfficiency = CreateCalcEfficiencyFunc(cmd.Targets);
                    var cmdMods = CalcCommandMods(cmd);

                    double bestScore = 0;
                    CalculationResult result = null;

                    Parallel.ForEach(seedChipCombinations, seedChips =>
                    {
                        trackIter++;
                        var multipliers = CalcMultipliers(cmdMods, CalcSeedChipMods(seedChips), cmd.Weapon.DamageType);
                        var currentResult = CalcShipProperties(cmd, seedChips, multipliers);
                        var score = calcEfficiency(currentResult);

                        if (score > bestScore)
                        {
                            lock (BestResultLock)
                            {
                                if (score > bestScore)
                                {
                                    bestScore = score;
                                    result = currentResult;
                                }
                            }
                        }
                    });

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

        public static Func<CalculationResult, double> CreateCalcEfficiencyFunc(TargerPropertyModel[] targets)
        {
            if (targets.Length == 0) { return new Func<CalculationResult, double>((obj) => obj.DamageTarget[DamageTarget.Normal].Dps[DpsType.Normal]); }

            var calculateTargetImpactFuncs = targets.Select(target =>
                {
                    var getPropertyValue = CreateGetMethod<CalculationResult, double>(target.Property);
                    return new Func<CalculationResult, double>((obj) 
                        => 1 - target.Impact + target.Impact * CutOverflow((getPropertyValue(obj) - target.Min) / (target.Best - target.Min), 0, 1));
                })
                .ToArray();

            return new Func<CalculationResult, double>((obj) =>
            {
                double result = 1;
                foreach (var calculateTargetImpact in calculateTargetImpactFuncs)
                    result *= calculateTargetImpact(obj);
                return result;
            });
        }

        public static Func<T, TResult> CreateGetMethod<T, TResult>(string propertyFullName)
        {
            var sourceType = typeof(T);
            var getters = new List<Func<object, object>>();

            var propertyNames = propertyFullName.Split('.');
            foreach (var propertyName in propertyNames)
            {
                if (typeof(IDictionary).IsAssignableFrom(sourceType))
                {
                    var key = Enum.Parse(sourceType.GenericTypeArguments[0], propertyName);
                    getters.Add(new Func<object, object>((obj) => ((IDictionary)obj)[key]));
                    sourceType = sourceType.GenericTypeArguments[1];
                }
                else
                {
                    var propertyInfo = sourceType.GetProperty(propertyName);
                    var method = propertyInfo.GetGetMethod();

                    ParameterExpression obj = Expression.Parameter(typeof(object), "obj");
                    Expression<Func<object, object>> expr =
                        Expression.Lambda<Func<object, object>>(
                            Expression.Convert(
                                Expression.Call(
                                    Expression.Convert(obj, method.DeclaringType),
                                    method),
                                typeof(object)),
                            obj);

                    getters.Add(expr.Compile());
                    sourceType = propertyInfo.PropertyType;
                }
            }

            return new Func<T, TResult>((obj) => {
                object result = obj;
                foreach (var func in getters) { result = func(result); }
                return (TResult)result;
            });
        }

        public static CalculationResult CalcShipProperties(CalculationCommand command, SeedChip[] seedChips, Dictionary<ModificationType, double> multipliers)
        {
            var damage = command.Ship.WeaponCount *
                command.Weapon.Damage *
                multipliers[ModificationType.Damage];

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
            var dpsWithHitMultiplier = hitTime / (hitTime + coollingTime);

            var dpsWithResistanseMultiplier = 1 + multipliers[ModificationType.DecreaseResistance] - command.EnemyResistance;

            var damageDescription = new DamageDescription(
                Damage: damage,
                CriticalDamage: damage * criticalDamageMultiplier,
                Dps: new Dictionary<DpsType, double>
                {
                    [DpsType.Normal] = dps,
                    [DpsType.WithHit] = dps * dpsWithHitMultiplier,
                    [DpsType.WithResistance] = dps * dpsWithResistanseMultiplier,
                    [DpsType.WithHitAndResistance] = dps * dpsWithHitMultiplier * dpsWithResistanseMultiplier,
                });
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

        public static Dictionary<ModificationType, double> CalcMultipliers(Dictionary<ModificationType, double> cmdMods, Dictionary<ModificationType, double> chipMods, DamageType damageType)
        {
            var dict = Utils.GetEnumValues<ModificationType>()
                .ToDictionary(x => x, x => cmdMods.GetValueOrDefault(x) + chipMods.GetValueOrDefault(x));
            return dict.ToDictionary(x => x.Key,
                x => x.Key switch
                {
                    ModificationType.Damage
                        => CalcVal(x.Value + damageType switch
                        {
                            DamageType.Electromagnetic => dict[ModificationType.ElectromagneticDamage],
                            DamageType.Kinetic => dict[ModificationType.KineticDamage],
                            DamageType.Termal => dict[ModificationType.TermalDamage],
                            _ => throw new NotImplementedException()
                        }),
                    ModificationType.KineticDamage
                    or ModificationType.TermalDamage
                    or ModificationType.ElectromagneticDamage
                        => 1,
                    ModificationType.DestroyerDamage
                    or ModificationType.AlienDamage
                    or ModificationType.ElidiumDamage
                    or ModificationType.FireRange
                    or ModificationType.ProjectiveSpeed
                    or ModificationType.FireSpread
                    or ModificationType.ModuleReloadingSpeed
                        => CalcVal(x.Value),
                    ModificationType.CoolingTime
                        => 1 / CalcVal(x.Value),
                    ModificationType.HitTime
                        => 1 / CalcVal(x.Value + dict[ModificationType.FireRate]),
                    ModificationType.FireRate
                        => CutOverflow(CalcVal(x.Value), min: 0, max: 10),
                    ModificationType.CriticalChance
                        => CutOverflow(CalcVal(x.Value), min: 1, max: 2) - 1,
                    ModificationType.CriticalDamage
                        => CutOverflow(CalcVal(x.Value), min: 1) - 1,
                    ModificationType.DecreaseResistance
                        => x.Value,
                    _ => throw new NotImplementedException()
                });
        }

        public static Dictionary<ModificationType, double> CalcCommandMods(CalculationCommand command)
            => CalcMods(Enumerable.Empty<Dictionary<ModificationType, double>>()
            .Append(command.Ship.Bonuses)
            .Append(command.Implants.SelectMany(x => x.Value.Select(y => new KeyValuePair<ModificationType, double>(x.Key, y))))
            .Append(command.Modules.SelectMany(x => x.Value.Select(y => new KeyValuePair<ModificationType, double>(x.Key, y))))
            .Append(new Dictionary<ModificationType, double>
            {
                [ModificationType.CriticalChance] = command.Weapon.CriticalChance ?? 0,
                [ModificationType.CriticalDamage] = command.Weapon.CriticalDamage ?? 0,
                [ModificationType.DecreaseResistance] = command.Weapon.DecreaseResistance,
            })
            .SelectMany(x => x));

        public static Dictionary<ModificationType, double> CalcSeedChipMods(SeedChip[] seedChips)
            => CalcMods(seedChips.SelectMany(x => x.Parameters));

        public static Dictionary<ModificationType, double> CalcMods(IEnumerable<KeyValuePair<ModificationType, double>> modifications)
            => modifications.Select(x => new KeyValuePair<ModificationType, double>(x.Key, x.Key switch
            {
                ModificationType.FireSpread
                or ModificationType.ModuleReloadingSpeed
                or ModificationType.HitTime
                    => CalcMod(-x.Value),
                ModificationType.DecreaseResistance
                    => x.Value,
                _ => CalcMod(x.Value)
            }))
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.Value));

        public static double CalcMod(double x) => x < 0 ? x / (1 + x) : x;
        public static double CalcVal(double x) => x < 0 ? 1 / (1 - x) : 1 + x;
        public static double CutOverflow(double value, double min = double.MinValue, double max = double.MaxValue)
            => Math.Min(max, Math.Max(min, value));
    }
}
