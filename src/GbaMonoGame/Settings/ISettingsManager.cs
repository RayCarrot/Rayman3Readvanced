namespace GbaMonoGame;

public interface ISettingsManager
{
    LocalGameSettings Local { get; }
    ActiveGameSettings Active { get; }
    bool IsOverriden { get; }

    void Load();
    void Save();
    void OverrideActive(ActiveGameSettings activeGameSettings);
    void RestoreActive();
}