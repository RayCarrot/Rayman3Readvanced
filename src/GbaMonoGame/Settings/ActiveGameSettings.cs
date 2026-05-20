namespace GbaMonoGame;

public class ActiveGameSettings
{
    public ActiveGameSettings(TweaksGameSettings tweaks, DifficultyGameSettings difficulty, DebugGameSettings debug)
    {
        Tweaks = tweaks;
        Difficulty = difficulty;
        Debug = debug;
    }

    public TweaksGameSettings Tweaks { get; }
    public DifficultyGameSettings Difficulty { get; }
    public DebugGameSettings Debug { get; }
}