namespace GbaMonoGame.Rayman3.J2me;

public readonly struct ArchiveDefine
{
    public ArchiveDefine(string fileName, byte imageResourcesCount, byte dataResourcesCount)
    {
        FileName = fileName;
        ImageResourcesCount = imageResourcesCount;
        DataResourcesCount = dataResourcesCount;
    }

    public string FileName { get; }
    public byte ImageResourcesCount { get; }
    public byte DataResourcesCount { get; }
}