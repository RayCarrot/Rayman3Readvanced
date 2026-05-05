using System.Collections.Frozen;
using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class Rayman3TimeAttack
{
    private const string GhostNameLy = "LY";
    private const string GhostNameClark = "CLARK";
    private const string GhostNameGlobox = "GLOBOX";
    private const string GhostNameMurfy = "MURFY";

    private const int World1 = 0;
    private const int World2 = 1;
    private const int World3 = 2;
    private const int World4 = 3;
    private const int WorldBonus = 4;

    public static TimeAttackLevelInfo[] LevelInfos { get; } =
    [
        // Wanderwood Forest
        new TimeAttackLevelInfo(
            level: MapId.WoodLight_M1,
            world: World1,
            targetTimes: 
            [
                new(TimeAttackTimeType.Bronze, 60 * 60),
                new(TimeAttackTimeType.Silver, 50 * 60),
                new(TimeAttackTimeType.Gold, 45 * 60),
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                [MapId.WoodLight_M1] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease3, new(300, 197)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(998, 158)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(1637, 252)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(3046, 44)),
                ],
                [MapId.WoodLight_M2] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease3, new(428, 232)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(1983, 342)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(2610, 125)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(2795, 130)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(3631, 54)),
                ],
            }.ToFrozenDictionary()),
        // Shining Glade
        new TimeAttackLevelInfo(
            level: MapId.FairyGlade_M1,
            world: World1,
            targetTimes: 
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Skipping Swamp of Bégoniax (GBA)
        // Ascension (N-Gage)
        new TimeAttackLevelInfo(
            level: MapId.MarshAwakening1,
            world: World1,
            targetTimes: 
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary(),
            exclusivePlatform: Platform.NGage),
        // Garish Gears
        new TimeAttackLevelInfo(
            level: MapId.BossMachine,
            world: World1,
            targetTimes: 
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Hoodlum Hideout
        new TimeAttackLevelInfo(
            level: MapId.SanctuaryOfBigTree_M1,
            world: World1,
            targetTimes: 
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Magma Mayhem (GBA)
        new TimeAttackLevelInfo(
            level: MapId.MissileRace1,
            world: World2,
            targetTimes: 
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary(),
            exclusivePlatform: Platform.GBA),
        // Ly's Punch Challenge 1 (N-Gage)
        new TimeAttackLevelInfo(
            level: MapId.MissileRace1,
            world: World2,
            targetTimes: 
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary(),
            exclusivePlatform: Platform.NGage),
        // Vertigo Wastes
        new TimeAttackLevelInfo(
            level: MapId.EchoingCaves_M1,
            world: World2,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Void of Bones
        new TimeAttackLevelInfo(
            level: MapId.CavesOfBadDreams_M1,
            world: World2,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Jano's Nest
        new TimeAttackLevelInfo(
            level: MapId.BossBadDreams,
            world: World2,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Prickly Passage
        new TimeAttackLevelInfo(
            level: MapId.MenhirHills_M1,
            world: World2,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Skipping Swamp of Bégoniax 2 (GBA)
        // Free Falling (N-Gage)
        new TimeAttackLevelInfo(
            level: MapId.MarshAwakening2,
            world: World2,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary(),
            exclusivePlatform: Platform.NGage),
        // River of Fire
        new TimeAttackLevelInfo(
            level: MapId.SanctuaryOfStoneAndFire_M1,
            world: World3,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // The Underlands
        new TimeAttackLevelInfo(
            level: MapId.BeneathTheSanctuary_M1,
            world: World3,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Boulder Brink
        new TimeAttackLevelInfo(
            level: MapId.ThePrecipice_M1,
            world: World3,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Den of Rocky
        new TimeAttackLevelInfo(
            level: MapId.BossRockAndLava,
            world: World3,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Wretched Ruins
        new TimeAttackLevelInfo(
            level: MapId.TheCanopy_M1,
            world: World3,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Wicked Flow
        new TimeAttackLevelInfo(
            level: MapId.SanctuaryOfRockAndLava_M1,
            world: World3,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Creeping Chaos
        new TimeAttackLevelInfo(
            level: MapId.TombOfTheAncients_M1,
            world: World4,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Scaleman's Keep
        new TimeAttackLevelInfo(
            level: MapId.BossScaleMan,
            world: World4,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // The Mettleworks
        new TimeAttackLevelInfo(
            level: MapId.IronMountains_M1,
            world: World4,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Magma Mayhem 2 (GBA)
        new TimeAttackLevelInfo(
            level: MapId.MissileRace2,
            world: World4,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary(),
            exclusivePlatform: Platform.GBA),
        // Falling Down (N-Gage)
        new TimeAttackLevelInfo(
            level: MapId.MissileRace2,
            world: World4,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary(),
            exclusivePlatform: Platform.NGage),
        // Razor Slide
        new TimeAttackLevelInfo(
            level: MapId.PirateShip_M1,
            world: World4,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Heart of the Ancients
        new TimeAttackLevelInfo(
            level: MapId.BossFinal_M1,
            world: World4,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Mega Havoc 1
        new TimeAttackLevelInfo(
            level: MapId.Bonus1,
            world: WorldBonus,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Mega Havoc 2
        new TimeAttackLevelInfo(
            level: MapId.Bonus2,
            world: WorldBonus,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Mega Havoc 3
        new TimeAttackLevelInfo(
            level: MapId.Bonus3,
            world: WorldBonus,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Mega Havoc 4
        new TimeAttackLevelInfo(
            level: MapId.Bonus4,
            world: WorldBonus,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
        // Lum Challenge
        new TimeAttackLevelInfo(
            level: MapId._1000Lums,
            world: WorldBonus,
            targetTimes:
            [
                // TODO: Define times
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // TODO: Define actors
            }.ToFrozenDictionary()),
    ];
}