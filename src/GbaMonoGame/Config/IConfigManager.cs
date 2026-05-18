namespace GbaMonoGame;

public interface IConfigManager
{
    LocalGameConfig Local { get; }
    ActiveGameConfig Active { get; }
    bool IsOverriden { get; }

    void Save();
    void OverrideActive(ActiveGameConfig activeGameConfig);
    void RestoreActive();
}