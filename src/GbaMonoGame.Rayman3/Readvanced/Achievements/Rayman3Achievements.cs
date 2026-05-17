using System;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class Rayman3Achievements
{
    public static AchievementInfo[] Achievements { get; } =
    [
        new AchievementInfo(
            id: AchievementId.CompleteWorld1,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_CompleteWorld1,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_CompleteWorld1,
            title: "First Steps",
            description: "Complete Forgotten Forests"),
        new AchievementInfo(
            id: AchievementId.CompleteWorld2,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_CompleteWorld2,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_CompleteWorld2,
            title: "A Bad Night's Sleep",
            description: "Complete Haunted Dreams"),
        new AchievementInfo(
            id: AchievementId.CompleteWorld3,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_CompleteWorld3,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_CompleteWorld3,
            title: "It's Getting Hot in Here",
            description: "Complete Magmacosm"),
        new AchievementInfo(
            id: AchievementId.CompleteWorld4,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_CompleteWorld4,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_CompleteWorld4,
            title: "Storming the Stronghold",
            description: "Complete Pirate Stronghold"),
        new AchievementInfo(
            id: AchievementId.DefeatBossMachine,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_DefeatBossMachine,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_DefeatBossMachine,
            title: "Horrible Machine",
            description: "Defeat the machine"),
        new AchievementInfo(
            id: AchievementId.DefeatBossBadDreams,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_DefeatBossBadDreams,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_DefeatBossBadDreams,
            title: "Skull Bash",
            description: "Defeat Jano"),
        new AchievementInfo(
            id: AchievementId.DefeatBossRockAndLava,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_DefeatBossRockAndLava,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_DefeatBossRockAndLava,
            title: "And a Hard Place",
            description: "Defeat Rocky"),
        new AchievementInfo(
            id: AchievementId.DefeatBossScaleMan,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_DefeatBossScaleMan,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_DefeatBossScaleMan,
            title: "Try that for Size!",
            description: "Defeat Scaleman"),
        new AchievementInfo(
            id: AchievementId.DefeatBossFinal,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_DefeatBossFinal,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_DefeatBossFinal,
            title: "The Grand Finale",
            description: "Defeat Razorbeard"),
        new AchievementInfo(
            id: AchievementId.CollectAllWorld1,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_CollectAllWorld1,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_CollectAllWorld1,
            title: "Forget Anyone?",
            description: "Collect all lums and cages in Forgotten Forests"),
        new AchievementInfo(
            id: AchievementId.CollectAllWorld2,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_CollectAllWorld2,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_CollectAllWorld2,
            title: "Lucid Dreams",
            description: "Collect all lums and cages in Haunted Dreams"),
        new AchievementInfo(
            id: AchievementId.CollectAllWorld3,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_CollectAllWorld3,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_CollectAllWorld3,
            title: "Through Fire and Flames",
            description: "Collect all lums and cages in Magmacosm"),
        new AchievementInfo(
            id: AchievementId.CollectAllWorld4,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_CollectAllWorld4,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_CollectAllWorld4,
            title: "Quest for Booty",
            description: "Collect all lums and cages in Pirate Stronghold"),
        new AchievementInfo(
            id: AchievementId.Collect1000thLum,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_Locked, // TODO: Texture
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_Locked,
            title: "Omniscient",
            description: "Collect the secret lum"),
        new AchievementInfo(
            id: AchievementId.CompleteGCNBonus,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_CompleteGCNBonus,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_CompleteGCNBonus,
            title: "Square Eyes",
            description: "Complete all GameCube bonus levels",
            exclusivePlatform: Platform.GBA),
        new AchievementInfo(
            id: AchievementId.CollectAllLives,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_CollectAllLives,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_CollectAllLives,
            title: "Hide and Seek Master",
            description: "Find all hidden lives"),
        new AchievementInfo(
            id: AchievementId.TimeAttackBronze,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_TimeAttackBronze,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_TimeAttackBronze,
            title: "Gotta Go Fast",
            description: "Earn bronze in every time attack level"),
        new AchievementInfo(
            id: AchievementId.TimeAttackSilver,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_TimeAttackSilver,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_TimeAttackSilver,
            title: "Speedster",
            description: "Earn silver in every time attack level"),
        new AchievementInfo(
            id: AchievementId.TimeAttackGold,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_TimeAttackGold,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_TimeAttackGold,
            title: "Champion",
            description: "Earn gold in every time attack level"),
        new AchievementInfo(
            id: AchievementId.DefeatPirateWithKeg,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_DefeatPirateWithKeg,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_DefeatPirateWithKeg,
            title: "Return to Sender",
            description: "Defeat a pirate with a keg"),
        new AchievementInfo(
            id: AchievementId.CompleteMarshAwakening1WithoutMoving,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_Locked, // TODO: Texture
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_Locked,
            title: "Ssssam Forces",
            description: "Complete Swamp of Bégoniax without moving to the side",
            exclusivePlatform: Platform.GBA),
        new AchievementInfo(
            id: AchievementId.CompleteMissileRace1WithoutStrafing,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_Locked, // TODO: Texture
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_Locked,
            title: "Call Me Old-Fashioned!",
            description: "Complete Magma Mayhem without moving sideways",
            exclusivePlatform: Platform.GBA),
        new AchievementInfo(
            id: AchievementId.RideKegBackwards,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_RideKegBackwards,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_RideKegBackwards,
            title: "geK gniylF",
            description: "Ride a flying keg backwards"),
        new AchievementInfo(
            id: AchievementId.CompleteCaveBadDreamsWithMaxSkullHits,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_Locked, // TODO: Texture
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_Locked,
            title: "Let the Dead Rest",
            description: "Complete the first section of Void of Bones while hitting 17 or fewer skull platforms"),
        new AchievementInfo(
            id: AchievementId.CompleteMenhirHillsWithoutDying,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_Locked, // TODO: Texture
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_Locked,
            title: "Don't Stop Me Now",
            description: "Complete Prickly Passage without dying"),
        new AchievementInfo(
            id: AchievementId.CompleteFreeFallingWithoutCheckpoint,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_Locked, // TODO: Texture
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_Locked,
            title: "Going Down",
            description: "Complete Free Falling without taking the checkpoint",
            exclusivePlatform: Platform.NGage),
        new AchievementInfo(
            id: AchievementId.DefeatPirateWithLava,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_DefeatPirateWithLava,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_DefeatPirateWithLava,
            title: "Your Lava Bath Is Ready",
            description: "Defeat a pirate by knocking it into the lava"),
        new AchievementInfo(
            id: AchievementId.DefeatRockyWithoutBlueLum,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_Locked, // TODO: Texture
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_Locked,
            title: "No, Jump Good",
            description: "Defeat Rocky without using the blue lum"),
        new AchievementInfo(
            id: AchievementId.CompleteRockAndLavaWithoutDefeatingBlackLums,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_CompleteRockAndLavaWithoutDefeatingBlackLums,
            bigIconTexturePath: Assets.Achievements. AchievementIcon48px_CompleteRockAndLavaWithoutDefeatingBlackLums,
            title: "Pacifist",
            description: "Complete Wicked Flow without defeating any black lums"),
        new AchievementInfo(
            id: AchievementId.CompleteMissileRace2WithoutDamage,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_Locked, // TODO: Texture
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_Locked,
            title: "Burning Rubber",
            description: "Complete Magma Mayhem 2 without taking any damage"),
        new AchievementInfo(
            id: AchievementId.DefeatEveryPirateType,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_DefeatEveryPirateType,
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_DefeatEveryPirateType,
            title: "Scrapped Metal",
            description: "Defeat a pirate of every rank"),
        new AchievementInfo(
            id: AchievementId.MultiplayerWin, // TODO: Trigger
            isGold: true,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_Locked, // TODO: Texture
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_Locked,
            title: "Top of the World",
            description: "Win a Multiplayer Game"),
        new AchievementInfo(
            id: AchievementId.ViewOriginalMenu,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.AchievementIcon32px_Locked, // TODO: Texture
            bigIconTexturePath: Assets.Achievements.AchievementIcon48px_Locked,
            title: "Retro",
            description: "View the original main menu"),
    ];

    // Tracking achievement progress
    public static bool MarshAwakening1_HasMoved { get; set; }
    public static bool MissileRace1_HasStrafed { get; set; }
    public static int CaveBadDreamsM1_HitSkulls { get; set; }
    public static bool MenhirHills_HasDied { get; set; }
    public static bool BossRockAndLava_HasUsedBlueLum { get; set; }
    public static bool SanctuaryOfRockAndLava_HasKilledBlackLum { get; set; }
    public static bool MissileRace2_HasTakenDamage { get; set; }

    // Doesn't include bosses since they don't have cages or lums
    private static MapId[] World1Maps { get; } =
    [
        MapId.WoodLight_M1, MapId.WoodLight_M2,
        MapId.FairyGlade_M1, MapId.FairyGlade_M2,
        MapId.MarshAwakening1,
        MapId.SanctuaryOfBigTree_M1, MapId.SanctuaryOfBigTree_M2,
        MapId.Bonus1
    ];
    private static MapId[] World2Maps { get; } =
    [
        MapId.MissileRace1,
        MapId.EchoingCaves_M1, MapId.EchoingCaves_M2,
        MapId.CavesOfBadDreams_M1, MapId.CavesOfBadDreams_M2,
        MapId.MenhirHills_M1, MapId.MenhirHills_M2,
        MapId.MarshAwakening2,
        MapId.Bonus2
    ];
    private static MapId[] World3Maps { get; } =
    [
        MapId.SanctuaryOfStoneAndFire_M1, MapId.SanctuaryOfStoneAndFire_M2, MapId.SanctuaryOfStoneAndFire_M3,
        MapId.BeneathTheSanctuary_M1, MapId.BeneathTheSanctuary_M2,
        MapId.ThePrecipice_M1, MapId.ThePrecipice_M2,
        MapId.TheCanopy_M1, MapId.TheCanopy_M2,
        MapId.SanctuaryOfRockAndLava_M1, MapId.SanctuaryOfRockAndLava_M2, MapId.SanctuaryOfRockAndLava_M3,
        MapId.Bonus3
    ];
    private static MapId[] World4Maps { get; } =
    [
        MapId.TombOfTheAncients_M1, MapId.TombOfTheAncients_M2,
        MapId.IronMountains_M1, MapId.IronMountains_M2,
        MapId.MissileRace2,
        MapId.PirateShip_M1, MapId.PirateShip_M2,
        MapId.Bonus4
    ];

    private static bool AllLumsAndCagesInLevels(MapId[] maps)
    {
        int collectedLums = 0;
        int totalLums = 0;
        int collectedCages = 0;
        int totalCages = 0;
        foreach (MapId mapId in maps)
        {
            collectedLums += GameInfo.GetDeadLumsForCurrentMap(mapId);
            totalLums += GameInfo.Levels[(int)mapId].LumsCount;
            collectedCages += GameInfo.GetDeadCagesForCurrentMap(mapId);
            totalCages += GameInfo.Levels[(int)mapId].CagesCount;
        }

        return collectedLums == totalLums && collectedCages == totalCages;
    }

    private static int GetTotalLivesCount()
    {
        // FairyGlade_M1
        // FairyGlade_M2
        // MarshAwakening1 (N-Gage only)
        // SanctuaryOfBigTree_M1
        // SanctuaryOfBigTree_M2
        // EchoingCaves_M1
        // EchoingCaves_M2
        // CavesOfBadDreams_M1
        // CavesOfBadDreams_M2
        // SanctuaryOfStoneAndFire_M3
        // BeneathTheSanctuary_M1
        // ThePrecipice_M1
        // ThePrecipice_M2
        // ThePrecipice_M2
        // TheCanopy_M1
        // TheCanopy_M2
        // SanctuaryOfRockAndLava_M1
        // SanctuaryOfRockAndLava_M2
        // SanctuaryOfRockAndLava_M3
        // IronMountains_M2
        // PirateShip_M2
        // Bonus1
        // Bonus2
        // Bonus3
        // ChallengeLy1

        return Rom.Platform switch
        {
            Platform.GBA => 24,
            Platform.NGage => 25,
            _ => throw new UnsupportedPlatformException()
        };
    }

    public static void CheckProgressionBasedAchievements()
    {
        // Check all progression-based achievements here so that they can be retroactively unlocked if importing a save

        // Check for world completions
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.SanctuaryOfBigTree_M2)
            Rayman3.Achievements.Unlock(AchievementId.CompleteWorld1);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.MarshAwakening2)
            Rayman3.Achievements.Unlock(AchievementId.CompleteWorld2);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.SanctuaryOfRockAndLava_M3)
            Rayman3.Achievements.Unlock(AchievementId.CompleteWorld3);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.PirateShip_M2)
            Rayman3.Achievements.Unlock(AchievementId.CompleteWorld4);

        // Check for boss defeats
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossMachine)
            Rayman3.Achievements.Unlock(AchievementId.DefeatBossMachine);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossBadDreams)
            Rayman3.Achievements.Unlock(AchievementId.DefeatBossBadDreams);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossRockAndLava)
            Rayman3.Achievements.Unlock(AchievementId.DefeatBossRockAndLava);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossScaleMan)
            Rayman3.Achievements.Unlock(AchievementId.DefeatBossScaleMan);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossFinal_M2)
            Rayman3.Achievements.Unlock(AchievementId.DefeatBossFinal);

        // Check for world collection completions
        if (AllLumsAndCagesInLevels(World1Maps))
            Rayman3.Achievements.Unlock(AchievementId.CollectAllWorld1);
        if (AllLumsAndCagesInLevels(World2Maps))
            Rayman3.Achievements.Unlock(AchievementId.CollectAllWorld2);
        if (AllLumsAndCagesInLevels(World3Maps))
            Rayman3.Achievements.Unlock(AchievementId.CollectAllWorld3);
        if (AllLumsAndCagesInLevels(World4Maps))
            Rayman3.Achievements.Unlock(AchievementId.CollectAllWorld4);
        if (GameInfo.GetTotalDeadLums() == 1000)
            Rayman3.Achievements.Unlock(AchievementId.Collect1000thLum);

        // Check for GCN bonus completion
        if (Rom.Platform == Platform.GBA && GameInfo.PersistentInfo.CompletedGCNBonusLevels == 10)
            Rayman3.Achievements.Unlock(AchievementId.CompleteGCNBonus);
    }

    public static void CheckTimeAttackAchievements()
    {
        TimeAttackInfo.GetTotalEarnedMedals(
            out int earnedBronze, out int earnedSilver, out int earnedGold,
            out int totalBronze, out int totalSilver, out int totalGold);

        if (earnedBronze == totalBronze)
            Rayman3.Achievements.Unlock(AchievementId.TimeAttackBronze);
        if (earnedSilver == totalSilver)
            Rayman3.Achievements.Unlock(AchievementId.TimeAttackSilver);
        if (earnedGold == totalGold)
            Rayman3.Achievements.Unlock(AchievementId.TimeAttackGold);
    }

    public static void DefeatPirateType(PirateType pirateType)
    {
        if (!RSMultiplayer.IsActive && !TimeAttackInfo.IsActive)
        {
            GameInfo.SaveSlot.DefeatedPirateTypes |= pirateType;

            if (GameInfo.SaveSlot.DefeatedPirateTypes == PirateType.All)
                Rayman3.Achievements.Unlock(AchievementId.DefeatEveryPirateType);
        }
    }

    public static void CollectWhiteLum(Lums lum)
    {
        if (!RSMultiplayer.IsActive && !TimeAttackInfo.IsActive)
        {
            byte mapId = (byte)GameInfo.MapId;
            int instanceId = lum.InstanceId;

            if (!GameInfo.SaveSlot.CollectedWhiteLums.Any(x => x.MapId == mapId && x.InstanceId == instanceId))
            {
                CollectedWhiteLum[] collectedWhiteLums = new CollectedWhiteLum[GameInfo.SaveSlot.CollectedWhiteLums.Length + 1];
                Array.Copy(GameInfo.SaveSlot.CollectedWhiteLums, collectedWhiteLums, GameInfo.SaveSlot.CollectedWhiteLums.Length);
                collectedWhiteLums[^1] = new CollectedWhiteLum(mapId, instanceId);
                GameInfo.SaveSlot.CollectedWhiteLums = collectedWhiteLums;

                if (GameInfo.SaveSlot.CollectedWhiteLums.Length == GetTotalLivesCount())
                    Rayman3.Achievements.Unlock(AchievementId.CollectAllLives);
            }
        }
    }
}