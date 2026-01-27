using BinarySerializer;

namespace GbaMonoGame.Rayman3.Readvanced;

public class TimeAttackSave : BaseReadvancedSave
{
    public TimeAttackSave()
    {
        Times = new int[TimesCount];
    }

    // NOTE: Increase this as needed
    private const int TimesCount = 48;

    public int[] Times { get; set; }

    public override void SerializeImpl(SerializerObject s)
    {
        base.SerializeImpl(s);
        s.SerializeMagicString("TIME", 4);

        Times = s.SerializeArray<int>(Times, TimesCount, name: nameof(Times));
    }
}