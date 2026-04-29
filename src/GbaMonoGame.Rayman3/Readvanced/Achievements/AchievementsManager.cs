using System.Collections.Frozen;
using System.Collections.Generic;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class AchievementsManager
{
    static AchievementsManager()
    {
        Achievements = new Dictionary<AchievementId, AchievementInfo>()
        {
            // TODO: Implement all achievements and trigger them
            [AchievementId.CompleteWorld1] = new(
                id: AchievementId.CompleteWorld1,
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Unknown name",
                description: "Complete Forgotten Forests"),
            [AchievementId.CompleteWorld2] = new(
                id: AchievementId.CompleteWorld2,
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "A Bad Night's Sleep",
                description: "Complete Haunted Dreams"),
            [AchievementId.CompleteWorld3] = new(
                id: AchievementId.CompleteWorld3,
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "It's Getting Hot in Here",
                description: "Complete Magmacosm"),
            [AchievementId.CompleteWorld4] = new(
                id: AchievementId.CompleteWorld4,
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Storming the Stronghold",
                description: "Complete Pirate Stronghold"),
            [AchievementId.DefeatBossMachine] = new(
                id: AchievementId.DefeatBossMachine,
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Horrible Machine",
                description: "Defeat the machine"),
            [AchievementId.DefeatJano] = new(
                id: AchievementId.DefeatJano,
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Skull Bash",
                description: "Defeat Jano"),
            [AchievementId.DefeatRocky] = new(
                id: AchievementId.DefeatRocky,
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "And a Hard Place",
                description: "Defeat Rocky"),
            [AchievementId.DefeatScaleman] = new(
                id: AchievementId.DefeatScaleman,
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "Try that for Size!",
                description: "Defeat Scaleman"),
            [AchievementId.DefeatGrolgoth] = new(
                id: AchievementId.DefeatGrolgoth,
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_LockedTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_LockedTexture,
                title: "The Grand Finale",
                description: "Defeat Razorbeard"),
            [AchievementId.TimeAttackBronze] = new(
                id: AchievementId.TimeAttackBronze,
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_TimeAttackBronzeTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_TimeAttackBronzeTexture,
                title: "Unknown name",
                description: "Earn bronze in every time attack level"),
            [AchievementId.TimeAttackSilver] = new(
                id: AchievementId.TimeAttackSilver,
                isGold: false,
                smallIconTexturePath: Assets.AchievementIcon32px_TimeAttackSilverTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_TimeAttackSilverTexture,
                title: "Unknown name",
                description: "Earn silver in every time attack level"),
            [AchievementId.TimeAttackGold] = new(
                id: AchievementId.TimeAttackGold,
                isGold: true,
                smallIconTexturePath: Assets.AchievementIcon32px_TimeAttackGoldTexture,
                bigIconTexturePath: Assets.AchievementIcon64px_TimeAttackGoldTexture,
                title: "Champion",
                description: "Earn gold in every time attack level"),
        }.ToFrozenDictionary();
    }

    private static AchievementPopup Popup { get; set; }
    private static Queue<AchievementId> AchievementsPopupQueue { get; set; }

    public static FrozenDictionary<AchievementId, AchievementInfo> Achievements { get; }

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