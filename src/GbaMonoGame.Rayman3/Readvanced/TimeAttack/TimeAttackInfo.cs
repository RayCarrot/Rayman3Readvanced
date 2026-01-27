using System;
using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO: Add support for Mode7 levels
public static class TimeAttackInfo
{
    private const int RandomSeed = 0x12345678; // The value doesn't matter - just needs to be constant
    private const int MinTime = 0;
    private const int MaxTime = 356400; // 99:00:00

    private static TimeAttackSave Save { get; set; }

    private static MapId? LastMapId { get; set; }
    private static int SavedTimer { get; set; }
    private static uint SavedRandomSeed { get; set; }

    public static bool IsActive { get; set; }
    public static bool IsPaused { get; set; }
    public static MapId? LevelId { get; set; }
    public static TimeAttackMode Mode { get; set; }
    public static int Timer { get; set; }
    public static TimeAttackTime[] TargetTimes { get; set; }

    private static void EnsureSaveIsLoaded()
    {
        Save ??= SaveGameManager.LoadTimeAttackSave();
    }

    public static void Init()
    {
        // TODO: Look more into which values to change, this is temporary. The visual options might affect Random seed. However Random only matters for gameplay for Grolgoth, Jano, Rocky. None of them use the visual effects.
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
                AllowCheatMenu = false,
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

    public static void UnInit()
    {
        Engine.RestoreActiveConfig();

        IsActive = false;
        LevelId = null;
        Mode = TimeAttackMode.None;
        
        LastMapId = null;
        SavedTimer = 0;
        SavedRandomSeed = RandomSeed;
    }

    public static void LoadLevel(MapId mapId)
    {
        LevelId = mapId;

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

    public static void InitLevel(MapId mapId)
    {
        // If this is a new map...
        if (LastMapId != mapId)
        {
            // Set the map id
            LastMapId = mapId;

            // Save state
            SavedTimer = Timer;
            SavedRandomSeed = Random.GetSeed();
        }
        // Reloading same map as before...
        else
        {
            // Restore state
            Timer = SavedTimer;
            Random.SetSeed(SavedRandomSeed);

            // Reset game info
            GameInfo.SetNextMapId(mapId);
        }

        IsPaused = false;
        Mode = TimeAttackMode.Init;
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
        EnsureSaveIsLoaded();

        int time = Save.Times[(int)mapId];

        if (time <= 0)
            return null;
        else
            return new TimeAttackTime(TimeAttackTimeType.Record, time);
    }

    public static void SaveRecordTime(MapId mapId, int time)
    {
        EnsureSaveIsLoaded();

        Save.Times[(int)mapId] = time;

        SaveGameManager.SaveTimeAttackSave(Save);
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
        if (IsPaused || Mode != TimeAttackMode.Play)
            return;

        Timer -= timeDelta;
        if (Timer < MinTime)
            Timer = MinTime;
    }

    public static void AddTime(int timeDelta)
    {
        if (IsPaused || Mode != TimeAttackMode.Play)
            return;

        Timer += timeDelta;
        if (Timer > MaxTime)
            Timer = MaxTime;
    }
}