namespace GbaMonoGame;

public class SoundGameConfig : IniSectionObject
{
    public SoundGameConfig()
    {
        SfxVolume = 1;
        MusicVolume = 1;
        PlayMusicWhenPaused = null;
        DisableLowHealthSound = false;
    }

    public override string SectionKey => "Sound";

    public float MusicVolume { get; set; }
    public float SfxVolume { get; set; }
    public bool? PlayMusicWhenPaused { get; set; } // null for original behavior
    public bool DisableLowHealthSound { get; set; }

    public override void Serialize(BaseIniSerializer serializer)
    {
        MusicVolume = serializer.Serialize<float>(MusicVolume, "MusicVolume");
        SfxVolume = serializer.Serialize<float>(SfxVolume, "SfxVolume");
        PlayMusicWhenPaused = serializer.Serialize<bool?>(PlayMusicWhenPaused, "PlayMusicWhenPaused");
        DisableLowHealthSound = serializer.Serialize<bool>(DisableLowHealthSound, "DisableLowHealthSound");
    }
}