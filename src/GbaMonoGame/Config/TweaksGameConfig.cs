namespace GbaMonoGame;

public class TweaksGameConfig : IniSectionObject
{
    public TweaksGameConfig()
    {
        Vector2 defaultResolution = Resolution.Modern;

        InternalGameResolution = defaultResolution;
        UseExtendedBackgrounds = true;
        UseGbaEffectsOnNGage = true;
        UseModernPauseDialog = true;
        UseReadvancedLogo = true;
        CanSkipTextBoxes = true;
        FixBugs = true;
        AddProjectilesWhenNeeded = true;
        ShowMode7Walls = true;
    }

    public override string SectionKey => "Tweaks";

    public Vector2? InternalGameResolution { get; set; } // Null to use original resolution
    public bool UseExtendedBackgrounds { get; set; }
    public bool UseGbaEffectsOnNGage { get; set; }
    public bool UseModernPauseDialog { get; set; }
    public bool UseReadvancedLogo { get; set; }
    public bool CanSkipTextBoxes { get; set; }
    public bool FixBugs { get; set; }
    public bool AddProjectilesWhenNeeded { get; set; }
    public bool ShowMode7Walls { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        InternalGameResolution = serializer.Serialize<Vector2?>(InternalGameResolution, "InternalGameResolution");
        UseExtendedBackgrounds = serializer.Serialize<bool>(UseExtendedBackgrounds, "UseExtendedBackgrounds");
        UseGbaEffectsOnNGage = serializer.Serialize<bool>(UseGbaEffectsOnNGage, "UseGbaEffectsOnNGage");
        UseModernPauseDialog = serializer.Serialize<bool>(UseModernPauseDialog, "UseModernPauseDialog");
        UseReadvancedLogo = serializer.Serialize<bool>(UseReadvancedLogo, "UseReadvancedLogo");
        CanSkipTextBoxes = serializer.Serialize<bool>(CanSkipTextBoxes, "CanSkipTextBoxes");
        FixBugs = serializer.Serialize<bool>(FixBugs, "FixBugs");
        AddProjectilesWhenNeeded = serializer.Serialize<bool>(AddProjectilesWhenNeeded, "AddProjectilesWhenNeeded");
        ShowMode7Walls = serializer.Serialize<bool>(ShowMode7Walls, "ShowMode7Walls");
    }
}