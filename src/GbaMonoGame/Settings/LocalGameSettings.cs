namespace GbaMonoGame;

public class LocalGameSettings
{
    public GeneralGameSettings General { get; set; } = new();
    public DisplayGameSettings Display { get; set; } = new();
    public ControlsGameSettings Controls { get; set; } = new();
    public SoundGameSettings Sound { get; set; } = new();
    public TweaksGameSettings Tweaks { get; set; } = new();
    public DifficultyGameSettings Difficulty { get; set; } = new();
    public DebugGameSettings Debug { get; set; } = new(); // Can only be manually modified

    public void Serialize(BaseIniSerializer serializer)
    {
        General = serializer.SerializeSectionObject(General);
        Display = serializer.SerializeSectionObject(Display);
        Controls = serializer.SerializeSectionObject(Controls);
        Sound = serializer.SerializeSectionObject(Sound);
        Tweaks = serializer.SerializeSectionObject(Tweaks);
        Difficulty = serializer.SerializeSectionObject(Difficulty);
        Debug = serializer.SerializeSectionObject(Debug);
    }
}