using BinarySerializer.Gameloft.J2me;

namespace GbaMonoGame.Rayman3.J2me;

public readonly struct TutorialEntry
{
    public TutorialEntry(StringId firstStringId, int stringsCount)
    {
        FirstStringId = firstStringId;
        StringsCount = stringsCount;
    }

    public StringId FirstStringId { get; }
    public int StringsCount { get; }
}