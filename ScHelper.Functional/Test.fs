// Learn more about F# at http://fsharp.org
namespace Test

open System
open System.Collections.Generic


type DamageType =
    | Kinetic = 1
    | Termal = 2
    | Electromagnetic = 3

type DamageTarget =
    | Normal = 1
    | Destroyer = 2
    | Alien = 3
    | Elidium = 4
    | DestroyerAlien = 5
    | DestroyerElidium = 6

type ModificationType =
    | Damage = 1
    | KineticDamage = 2
    | TermalDamage = 3
    | ElectromagneticDamage = 4
    | DestroyerDamage = 5
    | AlienDamage = 6
    | ElidiumDamage = 7
    | CriticalDamage = 8
    | CriticalChance = 9
    | FireRate = 10
    | FireRange = 11
    | FireSpread = 12
    | ProjectiveSpeed = 13
    | WeaponHitSpeed = 14
    | WeaponCoolingSpeed = 15
    | DecreaseResistance = 16
    | ModuleReloadingSpeed = 17

type SeedChip = {Name: string; Level: int; Parameters: IDictionary<ModificationType, double>}


module Utils =
    let getEnumValues<'T> =
        typeof<'T>
        |> Enum.GetValues
        :?> ('T [])

    let createEmptyDictionary<'T, 'U when 'T: equality> defaultValue =
        getEnumValues<'T>
        |> Array.map (fun x -> (x, defaultValue))
        |> dict

    let getEmptyDictionary2<'T, 'U when 'T: equality> =
        getEnumValues<'T>
        |> Array.map (fun x -> (x, Unchecked.defaultof<'U>))
        |> dict

    let overrideDict (newData: IDictionary<'Key,'Value>) (source: IDictionary<'Key,'Value>) =
        source.Keys
            |> Seq.map (fun x -> (x, if newData.ContainsKey x then newData.[x] else source.[x]))
            |> dict

module Calculation =
    let calcMod x = if x < 0. then x / (1. + x) else x

    let calcVal x = if x < 0. then 1. / (1. - x) else 1. + x

    let calcOverflow (x: float, min, max) = Math.Min(max, Math.Max(min, x))

    let cutTop max (x: float) = Math.Min(max, x)
    let cutBot min (x: float) = Math.Max(min, x)
    let cutBoth min max (x: float) = Math.Min(max, Math.Max(min, x))

    let calcMultiplier modifications : float =
        modifications |> List.map calcMod |> List.sum |> calcVal

    let groupModifications (seedChip: List<Dictionary<ModificationType, float>>) =
        seedChip
            |> Seq.collect (fun x -> x)
            |> Seq.groupBy (fun x -> x.Key)
            |> Seq.map (fun (key, value) -> (key, value
                |> Seq.map (fun x -> x.Value)
                |> Seq.where (fun x -> x <> 0.)
            ))
        


    let calcMultipliers (modifications: IDictionary<ModificationType,list<float>>) =
        let calcMultiplier (x: KeyValuePair<ModificationType, list<float>>) =
            match x.Key with
            | ModificationType.Damage
                -> 1.
            | ModificationType.KineticDamage
            | ModificationType.TermalDamage
            | ModificationType.ElectromagneticDamage
                -> x.Value @ modifications.[ModificationType.Damage] |> calcMultiplier
            | ModificationType.DestroyerDamage
            | ModificationType.AlienDamage
            | ModificationType.ElidiumDamage
            | ModificationType.FireRange
            | ModificationType.FireSpread
            | ModificationType.ProjectiveSpeed
            | ModificationType.WeaponCoolingSpeed
            | ModificationType.ModuleReloadingSpeed
            | ModificationType.FireRate
                -> x.Value |> calcMultiplier
            | ModificationType.WeaponHitSpeed
                -> x.Value @ modifications.[ModificationType.FireRate] |> calcMultiplier
            | ModificationType.CriticalChance
                -> x.Value |> calcMultiplier |> cutBoth 1. 2. |> (fun x -> x - 1.)
            | ModificationType.CriticalDamage
                -> x.Value |> calcMultiplier |> cutBot 1. |> (fun x -> x - 1.)
            | ModificationType.DecreaseResistance
                -> x.Value |> List.sum 
            | _ -> raise (System.NotImplementedException())
       
        let newDict = modifications |> Seq.map (fun x -> (x.Key, calcMultiplier x)) |> dict
        Utils.createEmptyDictionary<ModificationType, float> 0.
            |> Utils.overrideDict newDict        

