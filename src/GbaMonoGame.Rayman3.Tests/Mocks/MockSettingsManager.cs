namespace GbaMonoGame.Rayman3.Tests;

public class MockSettingsManager : ISettingsManager
{
    public MockSettingsManager(LocalGameSettings settings)
    {
        _settings = settings;
    }

    private readonly SettingsManager _settingsManager = new();
    private readonly LocalGameSettings _settings;

    public LocalGameSettings Local => _settingsManager.Local;
    public ActiveGameSettings Active => _settingsManager.Active;
    public bool IsOverriden => _settingsManager.IsOverriden;

    public void Load()
    {
        // Load from provided settings rather than from file
        _settingsManager.Local = _settings;
        _settingsManager.Active = new ActiveGameSettings(Local.Tweaks, Local.Difficulty, Local.Debug);
        _settingsManager.IsOverriden = false;
    }

    public void Save()
    {
        // Do nothing
    }

    public void OverrideActive(ActiveGameSettings activeGameSettings) => _settingsManager.OverrideActive(activeGameSettings);
    public void RestoreActive() => _settingsManager.RestoreActive();
}