namespace GbaMonoGame;

public class GameConfig
{
    public GeneralGameConfig General { get; set; } = new();
    public DisplayGameConfig Display { get; set; } = new();
    public ControlsGameConfig Controls { get; set; } = new();
    public SoundGameConfig Sound { get; set; } = new();
    public TweaksGameConfig Tweaks { get; set; } = new();
    public DifficultyGameConfig Difficulty { get; set; } = new();
    public DebugGameConfig Debug { get; set; } = new(); // Can only be manually modified

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