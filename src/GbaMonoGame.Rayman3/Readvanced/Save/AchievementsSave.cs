using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

public class AchievementsSave : BaseReadvancedSave
{
    public AchievementsSave()
    {
        UnlockedAchievements = new bool[(int)AchievementId.Count];
    }

    public override int Version => 0;
    public override string Id => "ACHV";

    public bool[] UnlockedAchievements { get; set; }

    protected override void SerializeSave(SerializerObject s, int version)
    {
        UnlockedAchievements = s.SerializeArray<bool>(UnlockedAchievements, (int)AchievementId.Count, name: nameof(UnlockedAchievements));
    }
}