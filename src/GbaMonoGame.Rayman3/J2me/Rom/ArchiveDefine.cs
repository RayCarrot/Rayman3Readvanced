namespace GbaMonoGame.Rayman3.J2me;

public readonly struct ArchiveDefine
{
    public ArchiveDefine(string fileName, sbyte imageResourcesCount, sbyte dataResourcesCount)
    {
        FileName = fileName;
        ImageResourcesCount = imageResourcesCount;
        DataResourcesCount = dataResourcesCount;
    }

    public string FileName { get; }
    public sbyte ImageResourcesCount { get; }
    public sbyte DataResourcesCount { get; }
}