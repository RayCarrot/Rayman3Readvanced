using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO: Add support for Mode7 levels
// TODO: Go through all actors
public static class TimeAttackInfo
{
    private const int RandomSeed = 0x12345678; // The value doesn't matter - just needs to be constant
    private const int MinTime = 0;
    private const int MaxTime = 356400; // 99:00:00

    public static bool IsActive { get; set; }
    public static bool IsPaused { get; set; }
    public static MapId MapId { get; set; }
    public static TimeAttackMode Mode { get; set; }
    public static int Timer { get; set; }
    public static TimeAttackTime[] TargetTimes { get; set; }

    public static void Init()
    {
        // TODO: Look more into which values to change, this is temporary
        // Save configs
        Engine.OverrideActiveConfig(new ActiveGameConfig(
            tweaks: Engine.LocalConfig.Tweaks with
            {
                InternalGameResolution = Resolution.Modern,
                UseExtendedBackgrounds = true,
                UseModernPauseDialog = true,
                CanSkipTextBoxes = true,
                FixBugs = true,
                AddProjectilesWhenNeeded = true,
#if RELEASE
                AllowPrototypeCheats = false,
#endif
            },
            difficulty: new DifficultyGameConfig
            {
                InfiniteLives = true,
                NoInstaKills = true,
                KeepLumsInRaces = false,
                NoCheckpoints = true,
                OneHitPoint = false
            },
            debug: Engine.LocalConfig.Debug with
            {
#if RELEASE
                DebugModeEnabled = false,
#endif
            }));

        // Update resolution
        if (Engine.InternalGameResolution != Engine.ActiveConfig.Tweaks.InternalGameResolution)
            Engine.SetInternalGameResolution(Engine.ActiveConfig.Tweaks.InternalGameResolution!.Value);

        // Mark all lums as collected
        GameInfo.PersistentInfo.Lums ??= new byte[125];
        Array.Fill(GameInfo.PersistentInfo.Lums, (byte)0);

        // Mark all cages as collected
        GameInfo.PersistentInfo.Cages ??= new byte[7];
        Array.Fill(GameInfo.PersistentInfo.Cages, (byte)0);

        // Set a constant seed so the randomization is the same
        Random.SetSeed(RandomSeed);

        IsActive = true;
        IsPaused = false;
        Mode = TimeAttackMode.Init;
        Timer = 0;
        TargetTimes = [];
    }

    // TODO: Make sure this gets called
    public static void UnInit()
    {
        Engine.RestoreActiveConfig();

        IsActive = false;
        MapId = default;
        Mode = TimeAttackMode.None;
    }

    public static void LoadLevel(MapId mapId)
    {
        MapId = mapId;

        GameInfo.PersistentInfo.LastPlayedLevel = (byte)mapId;
        GameInfo.PersistentInfo.LastCompletedLevel = (byte)(mapId switch
        {
            MapId.Bonus1 => MapId.SanctuaryOfBigTree_M2,
            MapId.Bonus2 => MapId.MarshAwakening2,
            MapId.Bonus3 => MapId.SanctuaryOfRockAndLava_M3,
            MapId.Bonus4 => MapId.BossFinal_M2,
            MapId._1000Lums => MapId.BossFinal_M2,
            MapId.ChallengeLy1 => MapId.MarshAwakening2,
            MapId.ChallengeLy2 => MapId.BossFinal_M2,
            MapId.ChallengeLyGCN => MapId.BossFinal_M2,
            _ => mapId
        });

        // Get the target times
        IEnumerable<TimeAttackTime> targetTimes = GetTargetTimes(mapId);
        if (GetRecordTime(mapId) is { } recordTime)
            targetTimes = targetTimes.Append(recordTime);
        targetTimes = targetTimes.OrderByDescending(x => x.Time);
        TargetTimes = targetTimes.ToArray();

        FrameManager.SetNextFrame(LevelFactory.Create(mapId));
    }

    public static TimeAttackTime[] GetTargetTimes(MapId mapId)
    {
        Dictionary<MapId, TimeAttackTime[]> dictionary = Rom.Platform switch
        {
            Platform.GBA => TimeAttackTimes.Gba,
            Platform.NGage => TimeAttackTimes.NGage,
            _ => throw new UnsupportedPlatformException()
        };

        if (dictionary.TryGetValue(mapId, out TimeAttackTime[] targetTimes))
            return targetTimes;
        else
            return [];
    }

    public static TimeAttackTime? GetRecordTime(MapId mapId)
    {
        // TODO: Dynamically load from persistent data
        return new TimeAttackTime(TimeAttackTimeType.Record, Random.GetNumber(2000));
    }

    public static void SetMode(TimeAttackMode mode)
    {
        Mode = mode;
    }

    public static void Pause()
    {
        IsPaused = true;
    }

    public static void Resume()
    {
        IsPaused = false;
    }

    public static void RemoveTime(int timeDelta)
    {
        if (IsPaused)
            return;

        Timer -= timeDelta;
        if (Timer < MinTime)
            Timer = MinTime;
    }

    public static void AddTime(int timeDelta)
    {
        if (IsPaused)
            return;

        Timer += timeDelta;
        if (Timer > MaxTime)
            Timer = MaxTime;
    }
}