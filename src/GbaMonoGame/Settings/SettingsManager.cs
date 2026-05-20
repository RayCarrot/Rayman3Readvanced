using System;

namespace GbaMonoGame;

public class SettingsManager : ISettingsManager
{
    /// <summary>
    /// The full, local, game settings. Avoid using this to read the settings as <see cref="Active"/>
    /// may be overriden and temporarily contain a different settings.
    /// </summary>
    public LocalGameSettings Local { get; set; }

    /// <summary>
    /// The currently active game settings. This is either the same as <see cref="Local"/> or a
    /// temporarily overriden settings.
    /// </summary>
    public ActiveGameSettings Active { get; set; }

    /// <summary>
    /// Indicates if the game settings has been overriden.
    /// </summary>
    public bool IsOverriden { get; set; }

    private void UpdateInternalGameResolution()
    {
        if (Engine.ViewPort.InternalGameResolution != Engine.Settings.Active.Tweaks.InternalGameResolution)
            Engine.ViewPort.SetInternalGameResolution(Engine.Settings.Active.Tweaks.InternalGameResolution!.Value);
    }

    public void Load()
    {
        string filePath = Engine.UserData.GetFile(Paths.SettingsFileName);
        LocalGameSettings settings = new();

        try
        {
            settings.Serialize(new IniDeserializer(filePath));
        }
        catch (Exception ex)
        {
            // Recreate and serialize without a file source to reset to default values
            settings = new LocalGameSettings();
            settings.Serialize(new IniDeserializer(null));

            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: $"An error occurred when reading the saved game options.{Environment.NewLine}All options will be reset to their default values.",
                header: "Error reading game options");
        }

        Local = settings;

        Active = new ActiveGameSettings(Local.Tweaks, Local.Difficulty, Local.Debug);
        IsOverriden = false;
    }

    public void Save()
    {
        string filePath = Engine.UserData.GetFile(Paths.SettingsFileName);
        IniSerializer serializer = new();
        Local.Serialize(serializer);

        try
        {
            serializer.Save(filePath);
        }
        catch (Exception ex)
        {
            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when saving the game options.",
                header: "Error reading game options");
        }
    }

    public void OverrideActive(ActiveGameSettings activeGameSettings)
    {
        Active = activeGameSettings;
        IsOverriden = true;
        UpdateInternalGameResolution();
    }

    public void RestoreActive()
    {
        Active = new ActiveGameSettings(Local.Tweaks, Local.Difficulty, Local.Debug);
        IsOverriden = false;
        UpdateInternalGameResolution();
    }
}