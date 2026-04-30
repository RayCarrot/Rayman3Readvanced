using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GbaMonoGame.Rayman3.Readvanced;

public static class AchievementsManager
{
    private static FrozenDictionary<AchievementId, AchievementInfo> AchievementsDictionary { get; set; }
    private static ImmutableArray<AchievementInfo> AchievementsArray { get; set; }
    private static AchievementPopup Popup { get; set; }
    private static Queue<AchievementId> AchievementsPopupQueue { get; set; }

    public static AchievementInfo GetAchievement(AchievementId achievementId) => AchievementsDictionary[achievementId];
    public static ImmutableArray<AchievementInfo> GetAchievements() => AchievementsArray;

    public static void Init(AchievementInfo[] achievements)
    {
        AchievementsDictionary = achievements.ToFrozenDictionary(x => x.Id);
        ImmutableArray<AchievementInfo>.Builder achievementsArrayBuilder = ImmutableArray.CreateBuilder<AchievementInfo>();
        achievementsArrayBuilder.AddRange(achievements);
        achievementsArrayBuilder.RemoveAll(x => x.ExclusivePlatform != null && x.ExclusivePlatform != Rom.Platform);
        AchievementsArray = achievementsArrayBuilder.ToImmutable();

        Popup = new AchievementPopup();
        Popup.Init();
        AchievementsPopupQueue = new Queue<AchievementId>();

        FrameManager.AddStepAction(Step);
    }

    public static void UnInit()
    {
        AchievementsDictionary = null;
        AchievementsArray = default;

        Popup = null;
        AchievementsPopupQueue = null;

        FrameManager.RemoveStepAction(Step);
    }

    public static void Step()
    {
        // Show next achievement from the queue
        if (!Popup.IsShowingPopup && AchievementsPopupQueue.TryDequeue(out AchievementId achievementId))
        {
            AchievementInfo achievement = GetAchievement(achievementId);
            Popup.SetText(achievement.Title);
            Popup.SetRank(achievement.IsGold);
            Popup.SetIcon(achievement.SmallIconTexturePath);
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
        if (Engine.LocalConfig.Display.ShowAchievementPopups)
            AchievementsPopupQueue.Enqueue(achievementId);
    }
}