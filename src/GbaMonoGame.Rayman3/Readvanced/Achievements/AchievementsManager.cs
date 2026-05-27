using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GbaMonoGame.Rayman3.Readvanced;

public class AchievementsManager
{
    public AchievementsManager(AchievementInfo[] achievements)
    {
        AchievementsDictionary = achievements.
            Where(static x => x.ExclusivePlatform == null || x.ExclusivePlatform == Rom.Platform).
            ToFrozenDictionary(x => x.Id);
        ImmutableArray<AchievementInfo>.Builder achievementsArrayBuilder = ImmutableArray.CreateBuilder<AchievementInfo>();
        achievementsArrayBuilder.AddRange(achievements);
        achievementsArrayBuilder.RemoveAll(static x => x.ExclusivePlatform != null && x.ExclusivePlatform != Rom.Platform);
        AchievementsArray = achievementsArrayBuilder.ToImmutable();

        Popup = new AchievementPopup();
        Popup.Init();
        AchievementsPopupQueue = new Queue<AchievementId>();

        Save = Rayman3.Save.LoadAchievementsSave() ?? new AchievementsSave();
    }

    private FrozenDictionary<AchievementId, AchievementInfo> AchievementsDictionary { get; }
    private ImmutableArray<AchievementInfo> AchievementsArray { get; }
    private AchievementPopup Popup { get; }
    private Queue<AchievementId> AchievementsPopupQueue { get; }
    private AchievementsSave Save { get; }

    public AchievementInfo GetAchievement(AchievementId achievementId) => AchievementsDictionary[achievementId];
    public ImmutableArray<AchievementInfo> GetAchievements() => AchievementsArray;

    public void Step()
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

    public void GetTotalEarnedAchievements(out int earnedAchievements, out int totalAchievements)
    {
        earnedAchievements = 0;
        totalAchievements = 0;
        foreach (AchievementInfo achievementInfo in GetAchievements())
        {
            if (IsUnlocked(achievementInfo.Id))
                earnedAchievements++;
            totalAchievements++;
        }
    }

    public bool IsUnlocked(AchievementId achievementId)
    {
        return Save.UnlockedAchievements[(int)achievementId];
    }

    public void Unlock(AchievementId achievementId)
    {
        int id = (int)achievementId;

        // Check if already unlocked
        if (Save.UnlockedAchievements[id])
            return;

        // Save
        Save.UnlockedAchievements[id] = true;
        Rayman3.Save.SaveAchievementsSave(Save);

        // Show popup
        if (Engine.Settings.Local.Display.ShowAchievementPopups)
            AchievementsPopupQueue.Enqueue(achievementId);
    }

    public void Lock(AchievementId achievementId)
    {
        int id = (int)achievementId;

        // Check if already locked
        if (!Save.UnlockedAchievements[id])
            return;

        // Save
        Save.UnlockedAchievements[id] = false;
        Rayman3.Save.SaveAchievementsSave(Save);
    }
}