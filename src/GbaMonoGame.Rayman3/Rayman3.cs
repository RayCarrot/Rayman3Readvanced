using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public static class Rayman3
{
    public static AchievementsManager Achievements { get; private set; }

    public static void Init()
    {
        Achievements = new AchievementsManager(Rayman3Achievements.Achievements);

        FrameManager.AddStepAction(Achievements.Step);
    }

    public static void UnInit()
    {
        FrameManager.RemoveStepAction(Achievements.Step);

        Achievements = null;
    }
}