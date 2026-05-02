using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class Rayman3Achievements
{
    public static AchievementInfo[] Achievements { get; } =
    [
        new AchievementInfo(
            id: AchievementId.CompleteWorld1,
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_CompleteWorld1Texture,
            bigIconTexturePath: Assets.AchievementIcon48px_CompleteWorld1Texture,
            title: "Unknown name", // TODO: Name
            description: "Complete Forgotten Forests"),
        new AchievementInfo(
            id: AchievementId.CompleteWorld2,
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_CompleteWorld2Texture,
            bigIconTexturePath: Assets.AchievementIcon48px_CompleteWorld2Texture,
            title: "A Bad Night's Sleep",
            description: "Complete Haunted Dreams"),
        new AchievementInfo(
            id: AchievementId.CompleteWorld3,
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_CompleteWorld3Texture,
            bigIconTexturePath: Assets.AchievementIcon48px_CompleteWorld3Texture,
            title: "It's Getting Hot in Here",
            description: "Complete Magmacosm"),
        new AchievementInfo(
            id: AchievementId.CompleteWorld4,
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_CompleteWorld4Texture,
            bigIconTexturePath: Assets.AchievementIcon48px_CompleteWorld4Texture,
            title: "Storming the Stronghold",
            description: "Complete Pirate Stronghold"),
        new AchievementInfo(
            id: AchievementId.DefeatBossMachine,
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Horrible Machine",
            description: "Defeat the machine"),
        new AchievementInfo(
            id: AchievementId.DefeatBossBadDreams,
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_DefeatJanoTexture,
            bigIconTexturePath: Assets.AchievementIcon48px_DefeatJanoTexture,
            title: "Skull Bash",
            description: "Defeat Jano"),
        new AchievementInfo(
            id: AchievementId.DefeatBossRockAndLava,
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "And a Hard Place",
            description: "Defeat Rocky"),
        new AchievementInfo(
            id: AchievementId.DefeatBossScaleMan,
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Try that for Size!",
            description: "Defeat Scaleman"),
        new AchievementInfo(
            id: AchievementId.DefeatBossFinal,
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_DefeatGrolgothTexture,
            bigIconTexturePath: Assets.AchievementIcon48px_DefeatGrolgothTexture,
            title: "The Grand Finale",
            description: "Defeat Razorbeard"),
        new AchievementInfo(
            id: AchievementId.CollectAllWorld1,
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Forget Anyone?",
            description: "Collect all lums and cages in Forgotten Forests"),
        new AchievementInfo(
            id: AchievementId.CollectAllWorld2,
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Lucid Dreams",
            description: "Collect all lums and cages in Haunted Dreams"),
        new AchievementInfo(
            id: AchievementId.CollectAllWorld3,
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Through Fire and Flames",
            description: "Collect all lums and cages in Magmacosm"),
        new AchievementInfo(
            id: AchievementId.CollectAllWorld4,
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Unknown name", // TODO: Name
            description: "Collect all lums and cages in Pirate Stronghold"),
        new AchievementInfo(
            id: AchievementId.Collect1000thLum,
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "How Did It Get Here?",
            description: "Collect the secret lum"),
        new AchievementInfo(
            id: AchievementId.CompleteGCNBonus, // TODO: Trigger
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Square Eyes",
            description: "Complete all GameCube bonus levels",
            exclusivePlatform: Platform.GBA),
        new AchievementInfo(
            id: AchievementId.CollectAllLives, // TODO: Trigger
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Hide and Seek Master",
            description: "Find all hidden lives"),
        new AchievementInfo(
            id: AchievementId.TimeAttackBronze, // TODO: Trigger
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_TimeAttackBronzeTexture,
            bigIconTexturePath: Assets.AchievementIcon48px_TimeAttackBronzeTexture,
            title: "Unknown name", // TODO: Name
            description: "Earn bronze in every time attack level"),
        new AchievementInfo(
            id: AchievementId.TimeAttackSilver, // TODO: Trigger
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_TimeAttackSilverTexture,
            bigIconTexturePath: Assets.AchievementIcon48px_TimeAttackSilverTexture,
            title: "Unknown name", // TODO: Name
            description: "Earn silver in every time attack level"),
        new AchievementInfo(
            id: AchievementId.TimeAttackGold, // TODO: Trigger
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_TimeAttackGoldTexture,
            bigIconTexturePath: Assets.AchievementIcon48px_TimeAttackGoldTexture,
            title: "Champion",
            description: "Earn gold in every time attack level"),
        new AchievementInfo(
            id: AchievementId.DefeatPirateWithKeg, // TODO: Trigger
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Return to Sender",
            description: "Defeat a pirate with a keg"),
        new AchievementInfo(
            id: AchievementId.CompleteMarshes1WithoutMoving, // TODO: Trigger
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Ssssam Forces",
            description: "Complete Swamp of Bégoniax without moving to the side",
            exclusivePlatform: Platform.GBA),
        new AchievementInfo(
            id: AchievementId.RideKegBackwards, // TODO: Trigger
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "geK gniylF",
            description: "Ride a flying keg backwards"),
        new AchievementInfo(
            id: AchievementId.CompleteCaveBadDreamsWithMaxSkullHits, // TODO: Trigger
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Let the Dead Rest",
            description: "Complete the first section of Void of Bones while hitting 17 or fewer skull platforms"),
        new AchievementInfo(
            id: AchievementId.CompleteMenhirHillsWithoutDying, // TODO: Trigger
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Don't Stop Me Now",
            description: "Complete Prickly Passage without dying"),
        new AchievementInfo(
            id: AchievementId.CompleteFreeFallingWithoutCheckpoint, // TODO: Trigger
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Going Down",
            description: "Complete Free Falling without taking the checkpoint",
            exclusivePlatform: Platform.NGage),
        new AchievementInfo(
            id: AchievementId.DefeatPirateWithLava, // TODO: Trigger
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Your Lava Bath Is Ready",
            description: "Defeat a pirate by knocking it into the lava"),
        new AchievementInfo(
            id: AchievementId.DefeatRockyWithoutBlueLum, // TODO: Trigger
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "No, Jump Good",
            description: "Defeat Rocky without using the blue lum"),
        new AchievementInfo(
            id: AchievementId.CompleteRockAndLavaWithoutDefeatingBlackLums, // TODO: Trigger
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Pacifist",
            description: "Complete Wicked Flow without defeating any black lums"),
        new AchievementInfo(
            id: AchievementId.CompleteMissileRace2WithoutDamage, // TODO: Trigger
            isGold: true,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Burning Rubber",
            description: "Complete Magma Mayhem 2 without taking any damage"),
        new AchievementInfo(
            id: AchievementId.DefeatPirateOfEveryRank, // TODO: Trigger
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Scrapped Metal",
            description: "Defeat a pirate of every rank"),
        new AchievementInfo(
            id: AchievementId.ViewOriginalMenu, // TODO: Trigger
            isGold: false,
            smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
            bigIconTexturePath: Assets.AchievementIcon48px_LockedTexture,
            title: "Retro",
            description: "View the original main menu"),
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

    public static void CheckProgressionBasedAchievements()
    {
        // Check all progression-based achievements here so that they can be retroactively unlocked if importing a save

        // Check for world completions
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.SanctuaryOfBigTree_M2)
            AchievementsManager.Unlock(AchievementId.CompleteWorld1);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.MarshAwakening2)
            AchievementsManager.Unlock(AchievementId.CompleteWorld2);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.SanctuaryOfRockAndLava_M3)
            AchievementsManager.Unlock(AchievementId.CompleteWorld3);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.PirateShip_M2)
            AchievementsManager.Unlock(AchievementId.CompleteWorld4);

        // Check for boss defeats
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossMachine)
            AchievementsManager.Unlock(AchievementId.DefeatBossMachine);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossBadDreams)
            AchievementsManager.Unlock(AchievementId.DefeatBossBadDreams);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossRockAndLava)
            AchievementsManager.Unlock(AchievementId.DefeatBossRockAndLava);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossScaleMan)
            AchievementsManager.Unlock(AchievementId.DefeatBossScaleMan);
        if (GameInfo.PersistentInfo.LastCompletedLevel >= (int)MapId.BossFinal_M2)
            AchievementsManager.Unlock(AchievementId.DefeatBossFinal);

        // Check for world collection completions
        if (AllLumsAndCagesInLevels(
            [
                MapId.WoodLight_M1, MapId.WoodLight_M2, 
                MapId.FairyGlade_M1, MapId.FairyGlade_M2, 
                MapId.MarshAwakening1, 
                MapId.SanctuaryOfBigTree_M1, MapId.SanctuaryOfBigTree_M2, 
                MapId.Bonus1
            ]))
            AchievementsManager.Unlock(AchievementId.CollectAllWorld1);
        if (AllLumsAndCagesInLevels(
            [
                MapId.MissileRace1,
                MapId.EchoingCaves_M1, MapId.EchoingCaves_M2,
                MapId.CavesOfBadDreams_M1, MapId.CavesOfBadDreams_M2,
                MapId.MenhirHills_M1, MapId.MenhirHills_M2,
                MapId.MarshAwakening2,
                MapId.Bonus2
            ]))
            AchievementsManager.Unlock(AchievementId.CollectAllWorld2);
        if (AllLumsAndCagesInLevels(
            [
                MapId.SanctuaryOfStoneAndFire_M1, MapId.SanctuaryOfStoneAndFire_M2, MapId.SanctuaryOfStoneAndFire_M3,
                MapId.BeneathTheSanctuary_M1, MapId.BeneathTheSanctuary_M2,
                MapId.ThePrecipice_M1, MapId.ThePrecipice_M2,
                MapId.TheCanopy_M1, MapId.TheCanopy_M2,
                MapId.SanctuaryOfRockAndLava_M1, MapId.SanctuaryOfRockAndLava_M2, MapId.SanctuaryOfRockAndLava_M3,
                MapId.Bonus3
            ]))
            AchievementsManager.Unlock(AchievementId.CollectAllWorld3);
        if (AllLumsAndCagesInLevels(
            [
                MapId.TombOfTheAncients_M1, MapId.TombOfTheAncients_M2,
                MapId.IronMountains_M1, MapId.IronMountains_M2,
                MapId.MissileRace2,
                MapId.PirateShip_M1, MapId.PirateShip_M2,
                MapId.Bonus4
            ]))
            AchievementsManager.Unlock(AchievementId.CollectAllWorld4);
        if (GameInfo.GetTotalDeadLums() == 1000)
            AchievementsManager.Unlock(AchievementId.Collect1000thLum);
    }
}