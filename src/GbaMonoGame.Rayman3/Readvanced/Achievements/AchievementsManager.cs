using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class AchievementsManager
{
    static AchievementsManager()
    {
        Achievements = new Dictionary<AchievementId, AchievementInfo>()
        {
            [AchievementId.CompleteWorld1] = new(
                id: AchievementId.CompleteWorld1, // TODO: Trigger
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Unknown name", // TODO: Name
                description: "Complete Forgotten Forests"),
            [AchievementId.CompleteWorld2] = new(
                id: AchievementId.CompleteWorld2, // TODO: Trigger
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "A Bad Night's Sleep",
                description: "Complete Haunted Dreams"),
            [AchievementId.CompleteWorld3] = new(
                id: AchievementId.CompleteWorld3, // TODO: Trigger
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "It's Getting Hot in Here",
                description: "Complete Magmacosm"),
            [AchievementId.CompleteWorld4] = new(
                id: AchievementId.CompleteWorld4, // TODO: Trigger
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Storming the Stronghold",
                description: "Complete Pirate Stronghold"),
            [AchievementId.DefeatBossMachine] = new(
                id: AchievementId.DefeatBossMachine, // TODO: Trigger
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Horrible Machine",
                description: "Defeat the machine"),
            [AchievementId.DefeatJano] = new(
                id: AchievementId.DefeatJano, // TODO: Trigger
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Skull Bash",
                description: "Defeat Jano"),
            [AchievementId.DefeatRocky] = new(
                id: AchievementId.DefeatRocky, // TODO: Trigger
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "And a Hard Place",
                description: "Defeat Rocky"),
            [AchievementId.DefeatScaleman] = new(
                id: AchievementId.DefeatScaleman, // TODO: Trigger
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Try that for Size!",
                description: "Defeat Scaleman"),
            [AchievementId.DefeatGrolgoth] = new(
                id: AchievementId.DefeatGrolgoth, // TODO: Trigger
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "The Grand Finale",
                description: "Defeat Razorbeard"),
            [AchievementId.CollectAllWorld1] = new(
                id: AchievementId.CollectAllWorld1, // TODO: Trigger
                isGold: true,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Forget Anyone?",
                description: "Collect all lums and cages in Forgotten Forests"),
            [AchievementId.CollectAllWorld2] = new(
                id: AchievementId.CollectAllWorld2, // TODO: Trigger
                isGold: true,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Lucid Dreams",
                description: "Collect all lums and cages in Haunted Dreams"),
            [AchievementId.CollectAllWorld3] = new(
                id: AchievementId.CollectAllWorld3, // TODO: Trigger
                isGold: true,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Through Fire and Flames",
                description: "Collect all lums and cages in Magmacosm"),
            [AchievementId.CollectAllWorld4] = new(
                id: AchievementId.CollectAllWorld4, // TODO: Trigger
                isGold: true,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Unknown name", // TODO: Name
                description: "Collect all lums and cages in Pirate Stronghold"),
            [AchievementId.Collect1000thLum] = new(
                id: AchievementId.Collect1000thLum, // TODO: Trigger
                isGold: true,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "How Did It Get Here?",
                description: "Collect the secret lum"),
            [AchievementId.CompleteGCNBonus] = new(
                id: AchievementId.CompleteGCNBonus, // TODO: Trigger
                isGold: true,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Square Eyes",
                description: "Complete all GameCube bonus levels",
                exclusivePlatform: Platform.GBA),
            [AchievementId.CollectAllLives] = new(
                id: AchievementId.CollectAllLives, // TODO: Trigger
                isGold: true,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture, // TODO: Texture
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Hide and Seek Master",
                description: "Find all hidden lives"),
            [AchievementId.TimeAttackBronze] = new(
                id: AchievementId.TimeAttackBronze, // TODO: Trigger
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_TimeAttackBronzeTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_TimeAttackBronzeTexture,
                title: "Unknown name", // TODO: Name
                description: "Earn bronze in every time attack level"),
            [AchievementId.TimeAttackSilver] = new(
                id: AchievementId.TimeAttackSilver, // TODO: Trigger
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_TimeAttackSilverTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_TimeAttackSilverTexture,
                title: "Unknown name", // TODO: Name
                description: "Earn silver in every time attack level"),
            [AchievementId.TimeAttackGold] = new(
                id: AchievementId.TimeAttackGold, // TODO: Trigger
                isGold: true,
                smallIconTexturePath: Assets.AchievementIcon32px_TimeAttackGoldTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_TimeAttackGoldTexture,
                title: "Champion",
                description: "Earn gold in every time attack level"),
        }.ToFrozenDictionary();
    }

    private static FrozenDictionary<AchievementId, AchievementInfo> Achievements { get; }
    private static AchievementPopup Popup { get; set; }
    private static Queue<AchievementId> AchievementsPopupQueue { get; set; }

    public static ImmutableArray<AchievementInfo> GetAchievements()
    {
        ImmutableArray<AchievementInfo>.Builder achievementsArrayBuilder = Achievements.Values.ToBuilder();
        achievementsArrayBuilder.RemoveAll(x => x.ExclusivePlatform != null && x.ExclusivePlatform != Rom.Platform);
        return achievementsArrayBuilder.ToImmutable();
    }

    public static void Init()
    {
        Popup = new AchievementPopup();
        Popup.Init();
        AchievementsPopupQueue = new Queue<AchievementId>();

        FrameManager.AddStepAction(Step);
    }

    public static void UnInit()
    {
        Popup = null;
        AchievementsPopupQueue = null;

        FrameManager.RemoveStepAction(Step);
    }

    public static void Step()
    {
        // Show next achievement from the queue
        if (!Popup.IsShowingPopup && AchievementsPopupQueue.TryDequeue(out AchievementId achievementId))
        {
            AchievementInfo achievementInfo = Achievements[achievementId];
            Popup.SetText(achievementInfo.Title);
            Popup.SetRank(achievementInfo.IsGold);
            Popup.SetIcon(achievementInfo.SmallIconTexturePath);
            Popup.MoveIn();
        }

        // Update the popup
        Popup.Step();
        Popup.Draw();
    }

    public static void Unlock(AchievementId achievementId)
    {
        // TODO: Check if already unlocked
        // TODO: Save achievement
        AchievementsPopupQueue.Enqueue(achievementId);
    }
}