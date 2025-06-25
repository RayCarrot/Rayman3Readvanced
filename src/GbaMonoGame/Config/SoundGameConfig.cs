namespace GbaMonoGame;

public class SoundGameConfig : IniSectionObject
{
    public SoundGameConfig()
    {
        SfxVolume = 1;
        MusicVolume = 1;
    }

    public override string SectionKey => "Sound";

    public float MusicVolume { get; set; }
    public float SfxVolume { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        MusicVolume = serializer.Serialize<float>(MusicVolume, "MusicVolume");
        SfxVolume = serializer.Serialize<float>(SfxVolume, "SfxVolume");
    }
}