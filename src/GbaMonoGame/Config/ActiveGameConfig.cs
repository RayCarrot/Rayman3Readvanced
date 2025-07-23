namespace GbaMonoGame;

public class ActiveGameConfig
{
    public ActiveGameConfig(TweaksGameConfig tweaks, DifficultyGameConfig difficulty, DebugGameConfig debug)
    {
        Tweaks = tweaks;
        Difficulty = difficulty;
        Debug = debug;
    }

    public TweaksGameConfig Tweaks { get; }
    public DifficultyGameConfig Difficulty { get; }
    public DebugGameConfig Debug { get; }
}