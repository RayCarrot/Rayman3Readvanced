using System;
using System.Linq;
using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

public class AchievementsSave : BaseReadvancedSave
{
    public AchievementsSave()
    {
        UnlockedAchievements = new bool[AchievementsCount];
    }

    public static int AchievementsCount { get; } = Enum.GetValuesAsUnderlyingType<AchievementId>().Cast<int>().Max() + 1;

    public override int Version => 1;
    public override string Id => "ACHV";

    public bool[] UnlockedAchievements { get; set; }

    protected override void SerializeSave(SerializerObject s, int version)
    {
        if (version == 0)
        {
            UnlockedAchievements = s.SerializeArray<bool>(UnlockedAchievements, 33, name: nameof(UnlockedAchievements));
        }
        else
        {
            UnlockedAchievements = s.SerializeArraySize<bool, int>(UnlockedAchievements, name: nameof(UnlockedAchievements));
            UnlockedAchievements = s.SerializeArray<bool>(UnlockedAchievements, UnlockedAchievements.Length, name: nameof(UnlockedAchievements));
        }

        if (UnlockedAchievements.Length < AchievementsCount)
        {
            bool[] array = UnlockedAchievements;
            Array.Resize(ref array, AchievementsCount);
            UnlockedAchievements = array;
        }
    }
}