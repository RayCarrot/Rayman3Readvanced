using System;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class Rayman3Achievements
{
    public static AchievementInfo[] Achievements { get; } =
    [
        new AchievementInfo(
            id: AchievementId.CompleteWorld1,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.CompleteWorld1_32px,
            bigIconTexturePath: Assets.Achievements.CompleteWorld1_48px,
            title: "First Steps",
            description: "Complete Forgotten Forests"),
        new AchievementInfo(
            id: AchievementId.CompleteWorld2,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.CompleteWorld2_32px,
            bigIconTexturePath: Assets.Achievements.CompleteWorld2_48px,
            title: "A Bad Night's Sleep",
            description: "Complete Haunted Dreams"),
        new AchievementInfo(
            id: AchievementId.CompleteWorld3,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.CompleteWorld3_32px,
            bigIconTexturePath: Assets.Achievements.CompleteWorld3_48px,
            title: "It's Getting Hot in Here",
            description: "Complete Magmacosm"),
        new AchievementInfo(
            id: AchievementId.CompleteWorld4,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.CompleteWorld4_32px,
            bigIconTexturePath: Assets.Achievements.CompleteWorld4_48px,
            title: "Storming the Stronghold",
            description: "Complete Pirate Stronghold"),
        new AchievementInfo(
            id: AchievementId.DefeatBossMachine,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.DefeatBossMachine_32px,
            bigIconTexturePath: Assets.Achievements.DefeatBossMachine_48px,
            title: "Horrible Machine",
            description: "Defeat the machine"),
        new AchievementInfo(
            id: AchievementId.DefeatBossBadDreams,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.DefeatBossBadDreams_32px,
            bigIconTexturePath: Assets.Achievements.DefeatBossBadDreams_48px,
            title: "Skull Bash",
            description: "Defeat Jano"),
        new AchievementInfo(
            id: AchievementId.DefeatBossRockAndLava,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.DefeatBossRockAndLava_32px,
            bigIconTexturePath: Assets.Achievements.DefeatBossRockAndLava_48px,
            title: "And a Hard Place",
            description: "Defeat Rocky"),
        new AchievementInfo(
            id: AchievementId.DefeatBossScaleMan,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.DefeatBossScaleMan_32px,
            bigIconTexturePath: Assets.Achievements.DefeatBossScaleMan_48px,
            title: "Try that for Size!",
            description: "Defeat Scaleman"),
        new AchievementInfo(
            id: AchievementId.DefeatBossFinal,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.DefeatBossFinal_32px,
            bigIconTexturePath: Assets.Achievements.DefeatBossFinal_48px,
            title: "The Grand Finale",
            description: "Defeat Razorbeard"),
        new AchievementInfo(
            id: AchievementId.CollectAllWorld1,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CollectAllWorld1_32px,
            bigIconTexturePath: Assets.Achievements.CollectAllWorld1_48px,
            title: "Forget Anyone?",
            description: "Collect all lums and cages in Forgotten Forests"),
        new AchievementInfo(
            id: AchievementId.CollectAllWorld2,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CollectAllWorld2_32px,
            bigIconTexturePath: Assets.Achievements.CollectAllWorld2_48px,
            title: "Lucid Dreams",
            description: "Collect all lums and cages in Haunted Dreams"),
        new AchievementInfo(
            id: AchievementId.CollectAllWorld3,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CollectAllWorld3_32px,
            bigIconTexturePath: Assets.Achievements.CollectAllWorld3_48px,
            title: "Through Fire and Flames",
            description: "Collect all lums and cages in Magmacosm"),
        new AchievementInfo(
            id: AchievementId.CollectAllWorld4,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CollectAllWorld4_32px,
            bigIconTexturePath: Assets.Achievements.CollectAllWorld4_48px,
            title: "Quest for Booty",
            description: "Collect all lums and cages in Pirate Stronghold"),
        new AchievementInfo(
            id: AchievementId.Collect1000thLum,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.Collect1000thLum_32px,
            bigIconTexturePath: Assets.Achievements.Collect1000thLum_48px,
            title: "Omniscient",
            description: "Collect the secret lum"),
        new AchievementInfo(
            id: AchievementId.CompleteGCNBonus,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CompleteGCNBonus_32px,
            bigIconTexturePath: Assets.Achievements.CompleteGCNBonus_48px,
            title: "Square Eyes",
            description: "Complete all GameCube bonus levels",
            exclusivePlatform: Platform.GBA),
        new AchievementInfo(
            id: AchievementId.CollectAllLives,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CollectAllLives_32px,
            bigIconTexturePath: Assets.Achievements.CollectAllLives_48px,
            title: "Hide and Seek Master",
            description: "Find all hidden lives"),
        new AchievementInfo(
            id: AchievementId.TimeAttackBronze,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.TimeAttackBronze_32px,
            bigIconTexturePath: Assets.Achievements.TimeAttackBronze_48px,
            title: "Gotta Go Fast",
            description: "Earn bronze in every time attack level"),
        new AchievementInfo(
            id: AchievementId.TimeAttackSilver,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.TimeAttackSilver_32px,
            bigIconTexturePath: Assets.Achievements.TimeAttackSilver_48px,
            title: "Speedster",
            description: "Earn silver in every time attack level"),
        new AchievementInfo(
            id: AchievementId.TimeAttackGold,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.TimeAttackGold_32px,
            bigIconTexturePath: Assets.Achievements.TimeAttackGold_48px,
            title: "Champion",
            description: "Earn gold in every time attack level"),
        new AchievementInfo(
            id: AchievementId.CompleteJ2me,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.CompleteJ2me_32px,
            bigIconTexturePath: Assets.Achievements.CompleteJ2me_48px,
            title: "Smooth Operator",
            description: "Complete the mobile version"),
        new AchievementInfo(
            id: AchievementId.UnlockBonusLevelJ2me,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.UnlockBonusLevelJ2me_32px,
            bigIconTexturePath: Assets.Achievements.UnlockBonusLevelJ2me_48px,
            title: "Booty Call",
            description: "Unlock the bonus level in the mobile version"),
        new AchievementInfo(
            id: AchievementId.CollectAllJ2me,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CollectAllJ2me_32px,
            bigIconTexturePath: Assets.Achievements.CollectAllJ2me_48px,
            title: "Full Bars",
            description: "Collect all lums and cages in the mobile version"),
        new AchievementInfo(
            id: AchievementId.DefeatPirateWithKeg,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.DefeatPirateWithKeg_32px,
            bigIconTexturePath: Assets.Achievements.DefeatPirateWithKeg_48px,
            title: "Return to Sender",
            description: "Defeat a pirate with a keg"),
        new AchievementInfo(
            id: AchievementId.CompleteMarshAwakening1WithoutMoving,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CompleteMarshAwakening1WithoutMoving_32px,
            bigIconTexturePath: Assets.Achievements.CompleteMarshAwakening1WithoutMoving_48px,
            title: "Ssssam Forces",
            description: "Complete Swamp of Bégoniax without moving to the side",
            exclusivePlatform: Platform.GBA),
        new AchievementInfo(
            id: AchievementId.CompleteMissileRace1WithoutStrafing,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CompleteMissileRace1WithoutStrafing_32px,
            bigIconTexturePath: Assets.Achievements.CompleteMissileRace1WithoutStrafing_48px,
            title: "Call Me Old-Fashioned!",
            description: "Complete Magma Mayhem without moving sideways",
            exclusivePlatform: Platform.GBA),
        new AchievementInfo(
            id: AchievementId.RideKegBackwards,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.RideKegBackwards_32px,
            bigIconTexturePath: Assets.Achievements.RideKegBackwards_48px,
            title: "geK gniylF",
            description: "Ride a flying keg backwards"),
        new AchievementInfo(
            id: AchievementId.CompleteCaveBadDreamsWithMaxSkullHits,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CompleteCaveBadDreamsWithMaxSkullHits_32px,
            bigIconTexturePath: Assets.Achievements.CompleteCaveBadDreamsWithMaxSkullHits_48px,
            title: "Let the Dead Rest",
            description: "Complete the first section of Void of Bones while hitting 17 or fewer skull platforms"),
        new AchievementInfo(
            id: AchievementId.CompleteMenhirHillsWithoutDying,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CompleteMenhirHillsWithoutDying_32px,
            bigIconTexturePath: Assets.Achievements.CompleteMenhirHillsWithoutDying_48px,
            title: "Don't Stop Me Now",
            description: "Complete Prickly Passage without dying"),
        new AchievementInfo(
            id: AchievementId.CompleteFreeFallingWithoutCheckpoint,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CompleteFreeFallingWithoutCheckpoint_32px,
            bigIconTexturePath: Assets.Achievements.CompleteFreeFallingWithoutCheckpoint_48px,
            title: "Going Down",
            description: "Complete Free Falling without taking the checkpoint",
            exclusivePlatform: Platform.NGage),
        new AchievementInfo(
            id: AchievementId.DefeatPirateWithLava,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.DefeatPirateWithLava_32px,
            bigIconTexturePath: Assets.Achievements.DefeatPirateWithLava_48px,
            title: "Your Lava Bath Is Ready",
            description: "Defeat a pirate by knocking it into the lava"),
        new AchievementInfo(
            id: AchievementId.DefeatRockyWithoutBlueLum,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.DefeatRockyWithoutBlueLum_32px,
            bigIconTexturePath: Assets.Achievements.DefeatRockyWithoutBlueLum_48px,
            title: "No, Jump Good",
            description: "Defeat Rocky without using the blue lum"),
        new AchievementInfo(
            id: AchievementId.CompleteRockAndLavaWithoutDefeatingBlackLums,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CompleteRockAndLavaWithoutDefeatingBlackLums_32px,
            bigIconTexturePath: Assets.Achievements.CompleteRockAndLavaWithoutDefeatingBlackLums_48px,
            title: "Pacifist",
            description: "Complete Wicked Flow without defeating any black lums"),
        new AchievementInfo(
            id: AchievementId.CompleteMissileRace2WithoutDamage,
            isGold: true,
            smallIconTexturePath: Assets.Achievements.CompleteMissileRace2WithoutDamage_32px,
            bigIconTexturePath: Assets.Achievements.CompleteMissileRace2WithoutDamage_48px,
            title: "Burning Rubber",
            description: "Complete Magma Mayhem 2 without taking any damage"),
        new AchievementInfo(
            id: AchievementId.DefeatEveryPirateType,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.DefeatEveryPirateType_32px,
            bigIconTexturePath: Assets.Achievements.DefeatEveryPirateType_48px,
            title: "Scrapped Metal",
            description: "Defeat a pirate of every rank"),
        new AchievementInfo(
            id: AchievementId.MultiplayerWin, // TODO: Trigger
            isGold: false,
            smallIconTexturePath: Assets.Achievements.MultiplayerWin_32px,
            bigIconTexturePath: Assets.Achievements.MultiplayerWin_48px,
            title: "Top of the World",
            description: "Win a Multiplayer Game"),
        new AchievementInfo(
            id: AchievementId.CollectSecretLifeJ2me,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.CollectSecretLifeJ2me_32px,
            bigIconTexturePath: Assets.Achievements.CollectSecretLifeJ2me_48px,
            title: "No Service",
            description: "Collect the hidden life in the mobile version"),
        new AchievementInfo(
            id: AchievementId.ViewOriginalMenu,
            isGold: false,
            smallIconTexturePath: Assets.Achievements.ViewOriginalMenu_32px,
            bigIconTexturePath: Assets.Achievements.ViewOriginalMenu_48px,
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
            collectedLums += Rayman3.GameInfo.GetDeadLumsForCurrentMap(mapId);
            totalLums += Rayman3.GameInfo.Levels[(int)mapId].LumsCount;
            collectedCages += Rayman3.GameInfo.GetDeadCagesForCurrentMap(mapId);
            totalCages += Rayman3.GameInfo.Levels[(int)mapId].CagesCount;
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
        if (RSMultiplayer.IsActive || Rayman3.TimeAttack.IsActive)
            return;

        // Check all progression-based achievements here so that they can be retroactively unlocked if importing a save

        // Check for world completions
        if (Rayman3.GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.SanctuaryOfBigTree_M2)
            Rayman3.Achievements.Unlock(AchievementId.CompleteWorld1);
        if (Rayman3.GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.MarshAwakening2)
            Rayman3.Achievements.Unlock(AchievementId.CompleteWorld2);
        if (Rayman3.GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.SanctuaryOfRockAndLava_M3)
            Rayman3.Achievements.Unlock(AchievementId.CompleteWorld3);
        if (Rayman3.GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.PirateShip_M2)
            Rayman3.Achievements.Unlock(AchievementId.CompleteWorld4);

        // Check for boss defeats
        if (Rayman3.GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossMachine)
            Rayman3.Achievements.Unlock(AchievementId.DefeatBossMachine);
        if (Rayman3.GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossBadDreams)
            Rayman3.Achievements.Unlock(AchievementId.DefeatBossBadDreams);
        if (Rayman3.GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossRockAndLava)
            Rayman3.Achievements.Unlock(AchievementId.DefeatBossRockAndLava);
        if (Rayman3.GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossScaleMan)
            Rayman3.Achievements.Unlock(AchievementId.DefeatBossScaleMan);
        if (Rayman3.GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossFinal_M2)
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
        if (Rayman3.GameInfo.GetTotalDeadLums() == 1000)
            Rayman3.Achievements.Unlock(AchievementId.Collect1000thLum);

        // Check for GCN bonus completion
        if (Rom.Platform == Platform.GBA && Rayman3.GameInfo.PersistentInfo.CompletedGCNBonusLevels == 10)
            Rayman3.Achievements.Unlock(AchievementId.CompleteGCNBonus);
    }

    public static void CheckTimeAttackAchievements()
    {
        Rayman3.TimeAttack.GetTotalEarnedMedals(
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
        if (RSMultiplayer.IsActive || Rayman3.TimeAttack.IsActive) 
            return;
        
        Rayman3.GameInfo.SaveSlot.DefeatedPirateTypes |= pirateType;

        if (Rayman3.GameInfo.SaveSlot.DefeatedPirateTypes == PirateType.All)
            Rayman3.Achievements.Unlock(AchievementId.DefeatEveryPirateType);
    }

    public static void CollectWhiteLum(Lums lum)
    {
        if (RSMultiplayer.IsActive || Rayman3.TimeAttack.IsActive) 
            return;
        
        byte mapId = (byte)Rayman3.GameInfo.MapId;
        int instanceId = lum.InstanceId;

        bool hasCollected = false;
        foreach (CollectedWhiteLum x in Rayman3.GameInfo.SaveSlot.CollectedWhiteLums)
        {
            if (x.MapId == mapId && x.InstanceId == instanceId)
            {
                hasCollected = true;
                break;
            }
        }

        if (!hasCollected)
        {
            CollectedWhiteLum[] collectedWhiteLums = new CollectedWhiteLum[Rayman3.GameInfo.SaveSlot.CollectedWhiteLums.Length + 1];
            Array.Copy(Rayman3.GameInfo.SaveSlot.CollectedWhiteLums, collectedWhiteLums, Rayman3.GameInfo.SaveSlot.CollectedWhiteLums.Length);
            collectedWhiteLums[^1] = new CollectedWhiteLum(mapId, instanceId);
            Rayman3.GameInfo.SaveSlot.CollectedWhiteLums = collectedWhiteLums;

            if (Rayman3.GameInfo.SaveSlot.CollectedWhiteLums.Length == GetTotalLivesCount())
                Rayman3.Achievements.Unlock(AchievementId.CollectAllLives);
        }
    }
}