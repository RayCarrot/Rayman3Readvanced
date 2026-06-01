namespace GbaMonoGame.Rayman3.J2ME;

public class ArchiveInformation
{
    public sbyte ImageResourcesCount { get; init; }
    public sbyte DataResourcesCount { get; init; }

    public sbyte PendingFree { get; set; }
    public sbyte PendingLoad { get; set; }
}