using SCHelper.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCHelper.Services.Impl
{
    public class CalculationService : ICalculationService
    {
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

        public CalculationResult Calc(CalculationCommand command)
        {
            var modifications = this.CalcModifications(command);
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
            var dps = damage * fireRate * (1 + multipliers[ModificationType.CriticalChance] * multipliers[ModificationType.CriticalDamage]);

            var damageDescription = new DamageDescription(
                Damage: damage,
                CriticalDamage: damage * (1 + multipliers[ModificationType.CriticalDamage]),
                Dps: dps,
                DpsWithHit: 0);
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
                HitTime: 0,
                CoollingTime: 0,
                FireRange: command.Weapon.FireRange * multipliers[ModificationType.FireRange],
                FireSpread: command.Weapon.FireSpread * multipliers[ModificationType.FireSpread],
                ProjectiveSpeed: command.Weapon.ProjectiveSpeed * multipliers[ModificationType.ProjectiveSpeed],
                DecreaseHullResistance: command.Weapon.DecreaseHullResistance + multipliers[ModificationType.DecreaseResistance]);
        }

        public Dictionary<ModificationType, IEnumerable<double>> CalcModifications(CalculationCommand command)
        {
            return command.SeedChips.Select(x => x.Parameters)
                .Append(command.Ship.Bonuses)
                .Append(command.Implants)
                .Append(command.Modules)
                .Append(new Dictionary<ModificationType, double>
                {
                    [ModificationType.CriticalChance] = command.Weapon.CriticalChance,
                    [ModificationType.CriticalDamage] = command.Weapon.CriticalDamage,
                    [ModificationType.DecreaseResistance] = command.Weapon.DecreaseHullResistance,
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
                        case ModificationType.WeaponHitSpeed:
                        case ModificationType.WeaponCoolingSpeed:
                        case ModificationType.ModuleReloadingSpeed:
                            return this.CalcMultiplier(x.Value);

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
    }
}
