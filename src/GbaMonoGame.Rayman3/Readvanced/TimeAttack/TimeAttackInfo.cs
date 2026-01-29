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

    private static TimeAttackSave Save { get; set; } // TODO: Need to clear this when quit game

    private static MapId? CurrentMapId { get; set; }
    private static int SavedTimer { get; set; }
    private static uint SavedRandomSeed { get; set; }
    private static List<GhostMapData> SavedRecordedGhostData { get; } = [];

    public static bool IsActive { get; set; }
    public static bool IsPaused { get; set; }
    public static MapId? LevelId { get; set; }
    public static TimeAttackMode Mode { get; set; }
    public static int Timer { get; set; }
    public static TimeAttackTime[] TargetTimes { get; set; }

    public static TimeAttackGhostType GhostType { get; set; }
    public static GhostMapData[] MapGhosts { get; set; }
    public static GhostRecorder GhostRecorder { get; set; }
    public static GhostPlayer GhostPlayer { get; set; }

    private static void EnsureSaveIsLoaded()
    {
        Save ??= SaveGameManager.LoadTimeAttackSave() ?? new TimeAttackSave();
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

        SavedRecordedGhostData.Clear();
        IsActive = true;
        IsPaused = false;
        Mode = TimeAttackMode.Init;
        Timer = 0;
        TargetTimes = [];
        MapGhosts = null;
        GhostRecorder = null;
        GhostPlayer = null;
    }

    public static void UnInit()
    {
        Engine.RestoreActiveConfig();

        IsActive = false;
        LevelId = null;
        Mode = TimeAttackMode.None;
        
        CurrentMapId = null;
        SavedTimer = 0;
        SavedRandomSeed = RandomSeed;
        SavedRecordedGhostData.Clear();

        MapGhosts = null;
        GhostRecorder = null;
        GhostPlayer = null;
    }

    public static void LoadLevel(MapId mapId, TimeAttackGhostType ghostType)
    {
        LevelId = mapId;

        GameInfo.PersistentInfo.LastPlayedLevel = (byte)mapId;
        GameInfo.PersistentInfo.LastCompletedLevel = (byte)(mapId switch
        {
            MapId.Bonus1 => MapId.SanctuaryOfBigTree_M2,
            MapId.Bonus2 => MapId.MarshAwakening2, // TODO: Needs blue lum power - or always give all powers?
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

        // Load the ghost data
        GhostType = ghostType;
        switch (ghostType)
        {
            default:
            case TimeAttackGhostType.None:
                MapGhosts = null;
                break;

            case TimeAttackGhostType.Record:
                MapGhosts = SaveGameManager.LoadTimeAttackGhost(mapId)?.MapGhosts;
                break;
            
            case TimeAttackGhostType.Guide:
                // TODO: Implement
                break;
            
            case TimeAttackGhostType.Developer:
                // TODO: Implement
                break;
        }

        FrameManager.SetNextFrame(LevelFactory.Create(mapId));
    }

    public static void InitLevel(MapId mapId)
    {
        // If this is a new map...
        if (CurrentMapId != mapId)
        {
            // Save the recorded ghost data for the last map
            if (CurrentMapId != null && GhostRecorder != null)
                SavedRecordedGhostData.Add(GhostRecorder.GetData(CurrentMapId.Value));

            // Set the map id
            CurrentMapId = mapId;

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

    public static void SaveTime()
    {
        if (LevelId == null || CurrentMapId == null)
            return;

        EnsureSaveIsLoaded();

        MapId mapId = LevelId.Value;

        // Save the time
        Save.Times[(int)mapId] = Timer;
        SaveGameManager.SaveTimeAttackSave(Save);

        // Save the ghost data
        if (GhostRecorder != null)
        {
            SavedRecordedGhostData.Add(GhostRecorder.GetData(CurrentMapId.Value));
            SaveGameManager.SaveTimeAttackGhost(new TimeAttackGhostSave
            {
                MapGhosts = SavedRecordedGhostData.ToArray(),
            }, mapId);
        }
    }

    public static void InitGhostRecorder(Scene2D scene)
    {
        GhostRecorder = new GhostRecorder(scene, 
        [
            ActorType.Rayman, 
            ActorType.RaymanBody,
            ActorType.RaymanMode7,
            ActorType.MissileMode7,
            ActorType.FlyingShell,
        ]);
    }

    public static void StepGhostRecorder()
    {
        GhostRecorder?.Step();
    }

    public static void InitGhostPlayer(Scene2D scene)
    {
        if (LevelId == null)
        {
            GhostPlayer = null;
            return;
        }

        GhostMapData mapGhost = MapGhosts?.FirstOrDefault(x => x.MapId == CurrentMapId);

        if (mapGhost == null)
        {
            GhostPlayer = null;
            return;
        }

        GhostPlayer = new GhostPlayer(scene, mapGhost.Frames);
    }

    public static void StepGhostPlayer()
    {
        GhostPlayer?.Step();
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