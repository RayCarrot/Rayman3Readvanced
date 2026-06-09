using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

public class TimeAttackSave : BaseReadvancedSave
{
    public TimeAttackSave()
    {
        Times = new int[TimesCount];
    }

    public override int Version => 0;
    public override string Id => "TIME";

    // NOTE: Increase this as needed
    private const int TimesCount = 48;

    public int[] Times { get; set; }

    protected override void SerializeSave(SerializerObject s, int version)
    {
        Times = s.SerializeArray<int>(Times, TimesCount, name: nameof(Times));
    }
}