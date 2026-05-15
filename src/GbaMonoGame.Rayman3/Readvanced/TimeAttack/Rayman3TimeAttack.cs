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

    private static int GetTime(int minutes, int seconds, int centiseconds)
    {
        return (minutes * 60 * 60) + (seconds * 60) + (centiseconds * 60 / 100);
    }

    public static TimeAttackLevelInfo[] LevelInfos { get; } =
    [
        // Wanderwood Forest
        new TimeAttackLevelInfo(
            level: MapId.WoodLight_M1,
            world: World1,
            targetTimes: 
            [
                new(TimeAttackTimeType.Bronze, GetTime(00, 55, 00)),
                new(TimeAttackTimeType.Silver, GetTime(00, 47, 00)),
                new(TimeAttackTimeType.Gold,   GetTime(00, 42, 00)),
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
                new(TimeAttackTimeType.Bronze, GetTime(02, 02, 00)),
                new(TimeAttackTimeType.Silver, GetTime(01, 50, 00)),
                new(TimeAttackTimeType.Gold,   GetTime(01, 44, 00)),
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                [MapId.FairyGlade_M1] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease3, new(1101, 434)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(1621, 124)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(2435, 158)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(2053, 385)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(3181, 151)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(2894, 166)),
                ],
                [MapId.FairyGlade_M2] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease5, new(943, 278)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(2250, 347)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(3847, 378)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(5185, 348)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(6519, 336)),
                ]
            }.ToFrozenDictionary()),
        // Skipping Swamp of Bégoniax (GBA)
        // Ascension (N-Gage)
        new TimeAttackLevelInfo(
            level: MapId.MarshAwakening1,
            world: World1,
            targetTimes: 
            [
                new(TimeAttackTimeType.Bronze, GetTime(00, 52, 00)),
                new(TimeAttackTimeType.Silver, GetTime(00, 44, 00)),
                new(TimeAttackTimeType.Gold,   GetTime(00, 38, 00)),
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                [MapId.MarshAwakening1] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease3, new(616, 1464)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(133, 959)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(902, 456)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(334, 86)),
                ]
            }.ToFrozenDictionary(),
            exclusivePlatform: Platform.NGage),
        // Separating between GBA and N-Gage since the Y offsets are different
        // Garish Gears (GBA)
        new TimeAttackLevelInfo(
            level: MapId.BossMachine,
            world: World1,
            targetTimes: 
            [
                new(TimeAttackTimeType.Bronze, GetTime(01, 00, 00)),
                new(TimeAttackTimeType.Silver, GetTime(00, 45, 00)),
                new(TimeAttackTimeType.Gold,   GetTime(00, 40, 00)),
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                [MapId.BossMachine] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease3, new(378, 4)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(290, 4)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(202, 4)),
                ]
            }.ToFrozenDictionary(),
            exclusivePlatform: Platform.GBA),
        // Garish Gears (N-Gage)
        new TimeAttackLevelInfo(
            level: MapId.BossMachine,
            world: World1,
            targetTimes: 
            [
                new(TimeAttackTimeType.Bronze, GetTime(01, 00, 00)),
                new(TimeAttackTimeType.Silver, GetTime(00, 45, 00)),
                new(TimeAttackTimeType.Gold,   GetTime(00, 40, 00)),
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                [MapId.BossMachine] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease3, new(378, 4 + 48)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(290, 4 + 48)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(202, 4 + 48)),
                ]
            }.ToFrozenDictionary(),
            exclusivePlatform: Platform.NGage),
        // Hoodlum Hideout
        new TimeAttackLevelInfo(
            level: MapId.SanctuaryOfBigTree_M1,
            world: World1,
            targetTimes: 
            [
                new(TimeAttackTimeType.Bronze, GetTime(03, 00, 00)),
                new(TimeAttackTimeType.Silver, GetTime(02, 42, 00)),
                new(TimeAttackTimeType.Gold,   GetTime(02, 33, 00)),
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                [MapId.SanctuaryOfBigTree_M1] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease3, new(941, 347)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(1999, 72)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(3701, 94)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(3915, 123)),
                ],
                [MapId.SanctuaryOfBigTree_M2] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease5, new(52, 192)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(4180, 122)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(4512, 181)),
                ]
            }.ToFrozenDictionary()),
        // Magma Mayhem (GBA)
        new TimeAttackLevelInfo(
            level: MapId.MissileRace1,
            world: World2,
            targetTimes: 
            [
                new(TimeAttackTimeType.Bronze, GetTime(02, 00, 00)),
                new(TimeAttackTimeType.Silver, GetTime(01, 55, 00)),
                new(TimeAttackTimeType.Gold,   GetTime(01, 50, 00)),
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // None
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
                new(TimeAttackTimeType.Bronze, GetTime(03, 14, 00)),
                new(TimeAttackTimeType.Silver, GetTime(03, 02, 00)),
                new(TimeAttackTimeType.Gold,   GetTime(02, 55, 00)),
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                [MapId.EchoingCaves_M1] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease3, new(46, 502)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(2080, 510)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(2188, 510)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(3478, 450)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(21, 177)),
                ],
                [MapId.EchoingCaves_M2] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease5, new(1564, 82)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(4342, 77)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(5767, 65)),
                ]
            }.ToFrozenDictionary()),
        // Void of Bones
        new TimeAttackLevelInfo(
            level: MapId.CavesOfBadDreams_M1,
            world: World2,
            targetTimes:
            [
                new(TimeAttackTimeType.Bronze, GetTime(06, 15, 00)),
                new(TimeAttackTimeType.Silver, GetTime(05, 56, 00)),
                new(TimeAttackTimeType.Gold,   GetTime(05, 40, 00)),
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                [MapId.CavesOfBadDreams_M1] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease3, new(41, 8196)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(494, 7623)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(98, 7123)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(540, 5287)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(488, 4554)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(518, 4554)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(548, 4554)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(503, 4522)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(533, 4522)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(314, 2483)),
                ],
                [MapId.CavesOfBadDreams_M2] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease3, new(1106, 242)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(2205, 263)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(2813, 200)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(5164, 309)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(7636, 290)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(8774, 104)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(8945, 104)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(9989, 403)),
                    new(TimeFreezeItem.Action.Init_Decrease5, new(10794, 359)),
                ]
            }.ToFrozenDictionary()),
        // Jano's Nest
        new TimeAttackLevelInfo(
            level: MapId.BossBadDreams,
            world: World2,
            targetTimes:
            [
                new(TimeAttackTimeType.Bronze, GetTime(01, 36, 00)),
                new(TimeAttackTimeType.Silver, GetTime(01, 29, 00)),
                new(TimeAttackTimeType.Gold,   GetTime(01, 24, 00)),
            ],
            actors: new Dictionary<MapId, TimeFreezeItemResource[]>()
            {
                // None
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
                [MapId.MenhirHills_M1] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease3, new(1073, 91)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(3357, 28)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(7354, 21)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(10276, 94)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(11963, 21)),
                ],
                [MapId.MenhirHills_M2] =
                [
                    new(TimeFreezeItem.Action.Init_Decrease3, new(4763, 25)),
                    new(TimeFreezeItem.Action.Init_Decrease3, new(9571, 44)),
                ]
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