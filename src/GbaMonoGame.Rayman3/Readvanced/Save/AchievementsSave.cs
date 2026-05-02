using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

public class AchievementsSave : BaseReadvancedSave
{
    public AchievementsSave()
    {
        UnlockedAchievements = new bool[(int)AchievementId.Count];
    }

    public bool[] UnlockedAchievements { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        base.SerializeImpl(s);
        s.SerializeMagicString("ACHV", 4);

        UnlockedAchievements = s.SerializeArray<bool>(UnlockedAchievements, (int)AchievementId.Count, name: nameof(UnlockedAchievements));
    }
}