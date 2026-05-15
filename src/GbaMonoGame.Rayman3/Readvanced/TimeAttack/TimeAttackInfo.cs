using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class TimeAttackInfo
{
    private const int RandomSeed = 0x12345678; // The value doesn't matter - just needs to be constant
    private const int MinTime = 0;
    private const int MaxTime = 356400; // 99:00:00

    private static FrozenDictionary<MapId, TimeAttackLevelInfo> LevelInfosDictionary { get; set; }
    private static ImmutableArray<TimeAttackLevelInfo> LevelInfosArray { get; set; }

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

    public static TimeAttackLevelInfo GetLevelInfo(MapId mapId) => LevelInfosDictionary[mapId];
    public static ImmutableArray<TimeAttackLevelInfo> GetLevelInfos() => LevelInfosArray;

    public static void Init(TimeAttackLevelInfo[] levelInfos)
    {
        LevelInfosDictionary = levelInfos.
            Where(x => x.ExclusivePlatform == null || x.ExclusivePlatform == Rom.Platform).
            ToFrozenDictionary(x => x.Level);
        ImmutableArray<TimeAttackLevelInfo>.Builder levelInfosArrayBuilder = ImmutableArray.CreateBuilder<TimeAttackLevelInfo>();
        levelInfosArrayBuilder.AddRange(levelInfos);
        levelInfosArrayBuilder.RemoveAll(x => x.ExclusivePlatform != null && x.ExclusivePlatform != Rom.Platform);
        LevelInfosArray = levelInfosArrayBuilder.ToImmutable();
    }

    public static void UnInit()
    {
        LevelInfosDictionary = null;
        LevelInfosArray = default;
    }

    public static void Start()
    {
        // TODO: In GameOptions each option has a TimeAttackValue. If null then don't override. Otherwise we lock it.
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
        Mode = TimeAttackMode.Start;
        Timer = 0;
        TargetTimes = [];
        MapGhosts = null;
        GhostRecorder = null;
        GhostPlayer = null;
    }

    public static void End()
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

        // Set last level completed in order to give the correct powers
        GameInfo.PersistentInfo.LastCompletedLevel = (byte)(mapId switch
        {
            // All powers for the bonus levels
            MapId.Bonus1 => MapId.BossFinal_M2,
            MapId.Bonus2 => MapId.BossFinal_M2,
            MapId.Bonus3 => MapId.BossFinal_M2,
            MapId.Bonus4 => MapId.BossFinal_M2,
            MapId._1000Lums => MapId.BossFinal_M2,
            
            // Powers based on last level in the world for the Ly levels
            MapId.ChallengeLy1 => MapId.MarshAwakening2,
            MapId.ChallengeLy2 => MapId.BossFinal_M2,
            MapId.ChallengeLyGCN => MapId.BossFinal_M2,

            // Otherwise based on the map itself
            _ => mapId
        });

        // Get the target times
        IEnumerable<TimeAttackTime> targetTimes = GetTargetTimes(mapId);
        if (TimeAttackDataManager.GetRecordTime(mapId) is { } recordTime)
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
        Mode = TimeAttackMode.Start;
    }

    public static IReadOnlyCollection<ActorResource> GetActors()
    {
        if (LevelId == null || CurrentMapId == null)
            return [];

        TimeAttackLevelInfo levelInfo = GetLevelInfo(LevelId.Value);

        // Add the time freeze items
        List<ActorResource> actors = [];
        foreach (TimeFreezeItemInstance timeFreezeItem in levelInfo.Actors.GetValueOrDefault(CurrentMapId.Value, []))
        {
            actors.Add(new ActorResource()
            {
                Pos = new BinarySerializer.Ubisoft.GbaEngine.Vector2(timeFreezeItem.X, timeFreezeItem.Y),
                IsEnabled = true,
                IsAwake = true,
                IsAnimatedObjectDynamic = false,
                IsProjectile = false,
                ResurrectsImmediately = false,
                ResurrectsLater = false,
                Type = (byte)ReadvancedActorType.TimeFreezeItem,
                Idx_ActorModel = 0xFF,
                FirstActionId = (byte)(timeFreezeItem.Time switch
                {
                    3 => TimeFreezeItem.Action.Init_Decrease3,
                    5 => TimeFreezeItem.Action.Init_Decrease5,
                    _ => throw new Exception($"Invalid time value {timeFreezeItem.Time}")
                }),
                Links = [0xFF, 0xFF, 0xFF, 0xFF],
                Model = TimeAttackActorModels.TimeFreezeItemActorModel,
            });
        }

        // Add max 5 projectile actors
        int projectilesCount = Math.Min(actors.Count, 5);

        // TODO: Update model to have a bigger viewbox
        // Load Power1 scene to get the sparkles model from it
        Scene2DResource sceneResource = Rom.LoadResource<Scene2DResource>((int)MapId.Power1);
        ActorModel sparklesModel = sceneResource.AlwaysActors.First(x => (ActorType)x.Type == ActorType.ChainedSparkles).Model;

        for (int i = 0; i < projectilesCount; i++)
        {
            // Add sparkles
            actors.Add(new ActorResource
            {
                Pos = new BinarySerializer.Ubisoft.GbaEngine.Vector2(0, 0),
                IsEnabled = false,
                IsAwake = false,
                IsAnimatedObjectDynamic = false,
                IsProjectile = true,
                ResurrectsImmediately = false,
                ResurrectsLater = false,
                Type = (byte)ReadvancedActorType.TimeFreezeItemSparkles,
                Model = sparklesModel,
            });

            // Add time decrease
            actors.Add(new ActorResource
            {
                Pos = new BinarySerializer.Ubisoft.GbaEngine.Vector2(0, 0),
                IsEnabled = false,
                IsAwake = false,
                IsAnimatedObjectDynamic = false,
                IsProjectile = true,
                ResurrectsImmediately = false,
                ResurrectsLater = false,
                Type = (byte)ReadvancedActorType.TimeDecrease,
                Model = TimeAttackActorModels.TimeDecreaseActorModel,
            });
        }

        return actors;
    }

    public static TimeAttackTime[] GetTargetTimes(MapId mapId)
    {
        return GetLevelInfo(mapId).TargetTimes;
    }

    public static void GetTotalEarnedMedals(
        out int earnedBronzeModels, out int earnedSilverModels, out int earnedGoldModels,
        out int totalBronzeModels, out int totalSilverMedals, out int totalGoldMedal)
    {
        earnedBronzeModels = 0;
        earnedSilverModels = 0;
        earnedGoldModels = 0;
        totalBronzeModels = 0;
        totalSilverMedals = 0;
        totalGoldMedal = 0;
        foreach (TimeAttackLevelInfo levelInfo in GetLevelInfos())
        {
            TimeAttackTime? recordTime = TimeAttackDataManager.GetRecordTime(levelInfo.Level);

            foreach (TimeAttackTime targetTime in levelInfo.TargetTimes)
            {
                bool earned = recordTime?.Time <= targetTime.Time;

                switch (targetTime.Type)
                {
                    case TimeAttackTimeType.Bronze:
                        if (earned) 
                            earnedBronzeModels++;
                        totalBronzeModels++;
                        break;
                    case TimeAttackTimeType.Silver:
                        if (earned) 
                            earnedSilverModels++;
                        totalSilverMedals++;
                        break;
                    case TimeAttackTimeType.Gold:
                        if (earned) 
                            earnedGoldModels++;
                        totalGoldMedal++;
                        break;
                }
            }
        }
    }

    public static void SaveTime()
    {
        if (LevelId == null || CurrentMapId == null)
            return;

        TimeAttackGhostSave ghostSave = null;
        if (GhostRecorder != null)
        {
            SavedRecordedGhostData.Add(GhostRecorder.GetData(CurrentMapId.Value));
            ghostSave = new TimeAttackGhostSave
            {
                MapGhosts = SavedRecordedGhostData.ToArray(),
            };
        }

        TimeAttackDataManager.SaveRecordTime(LevelId.Value, Timer, ghostSave);

        Rayman3Achievements.CheckTimeAttackAchievements();
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

        GhostPlayer = new GhostPlayer(scene, mapGhost);
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