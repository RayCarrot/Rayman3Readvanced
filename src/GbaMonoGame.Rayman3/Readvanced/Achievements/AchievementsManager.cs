using System.Collections.Frozen;
using System.Collections.Generic;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class AchievementsManager
{
    static AchievementsManager()
    {
        Achievements = new Dictionary<AchievementId, AchievementInfo>()
        {
            // TODO: Implement all achievements and trigger them
            [AchievementId.TimeAttackBronze] = new(
                id: AchievementId.TimeAttackBronze,
                isGold: false,
                iconTexturePath: Assets.AchievementIcon32px_TimeAttackBronzeTexture,
                title: "?",
                description: "Earn bronze in every time attack level"),
            [AchievementId.TimeAttackSilver] = new(
                id: AchievementId.TimeAttackSilver,
                isGold: false,
                iconTexturePath: Assets.AchievementIcon32px_TimeAttackSilverTexture,
                title: "?",
                description: "Earn silver in every time attack level"),
            [AchievementId.TimeAttackGold] = new(
                id: AchievementId.TimeAttackGold,
                isGold: true,
                iconTexturePath: Assets.AchievementIcon32px_TimeAttackGoldTexture,
                title: "Champion",
                description: "Earn gold in every time attack level"),
        }.ToFrozenDictionary();
    }

    private static FrozenDictionary<AchievementId, AchievementInfo> Achievements { get; }

    private static AchievementPopup Popup { get; set; }
    private static Queue<AchievementId> AchievementsPopupQueue { get; set; }

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
            Popup.SetIcon(achievementInfo.IconTexturePath);
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