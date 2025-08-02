using System;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO: Add support for Mode7 levels
// TODO: Go through all actors
public static class TimeAttackInfo
{
    private const int RandomSeed = 0x12345678;

    public static bool IsActive { get; set; }
    public static TimeAttackMode Mode { get; set; }

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
                AllowPrototypeCheats = false,
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
        Mode = TimeAttackMode.Init;
    }

    // TODO: Make sure this gets called
    public static void UnInit()
    {
        Engine.RestoreActiveConfig();

        IsActive = false;
        Mode = TimeAttackMode.None;
    }

    public static void LoadLevel(MapId mapId)
    {
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

        FrameManager.SetNextFrame(LevelFactory.Create(mapId));
    }
}