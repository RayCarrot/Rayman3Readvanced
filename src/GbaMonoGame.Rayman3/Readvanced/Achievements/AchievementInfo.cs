using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public struct AchievementInfo
{
    public AchievementInfo(AchievementId id, bool isGold, string smallIconTexturePath, string bigIconTexturePath, string title, string description, Platform? exclusivePlatform = null)
    {
        Id = id;
        IsGold = isGold;
        SmallIconTexturePath = smallIconTexturePath;
        BigIconTexturePath = bigIconTexturePath;
        Title = title;
        Description = description;
        ExclusivePlatform = exclusivePlatform;
    }

    public AchievementId Id { get; }
    public bool IsGold { get; }
    public string SmallIconTexturePath { get; }
    public string BigIconTexturePath { get; }
    public string Title { get; }
    public string Description { get; }
    public Platform? ExclusivePlatform { get; }
}