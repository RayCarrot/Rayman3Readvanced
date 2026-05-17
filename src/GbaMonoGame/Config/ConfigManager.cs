using System;

namespace GbaMonoGame;

public class ConfigManager
{
    public ConfigManager()
    {
        string filePath = FileManager.GetDataFile(Paths.ConfigFileName);
        LocalGameConfig config = new();

        try
        {
            config.Serialize(new IniDeserializer(filePath));
        }
        catch (Exception ex)
        {
            // Recreate and serialize without a file source to reset to default values
            config = new LocalGameConfig();
            config.Serialize(new IniDeserializer(null));

            Engine.MessageManager.EnqueueExceptionMessage(
                ex: ex,
                text: $"An error occurred when reading the saved game options.{Environment.NewLine}All options will be reset to their default values.",
                header: "Error reading game options");
        }

        Local = config;

        Active = new ActiveGameConfig(Local.Tweaks, Local.Difficulty, Local.Debug);
        IsOverrided = false;
    }

    /// <summary>
    /// The full, local, game config. Avoid using this to read the config as <see cref="Active"/>
    /// may be overriden and temporarily contain a different config.
    /// </summary>
    public LocalGameConfig Local { get; }

    /// <summary>
    /// The currently active game config. This is either the same as <see cref="Local"/> or a
    /// temporarily overriden config.
    /// </summary>
    public ActiveGameConfig Active { get; private set; }

    /// <summary>
    /// Indicates if the game config has been overriden.
    /// </summary>
    public bool IsOverrided { get; private set; }

    public void Save()
    {
        string filePath = FileManager.GetDataFile(Paths.ConfigFileName);
        IniSerializer serializer = new();
        Local.Serialize(serializer);

        try
        {
            serializer.Save(filePath);
        }
        catch (Exception ex)
        {
            Engine.MessageManager.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when saving the game options.",
                header: "Error reading game options");
        }
    }

    public void OverrideActive(ActiveGameConfig activeGameConfig)
    {
        Active = activeGameConfig;
        IsOverrided = true;
        Engine.UpdateInternalGameResolution();
    }

    public void RestoreActive()
    {
        Active = new ActiveGameConfig(Local.Tweaks, Local.Difficulty, Local.Debug);
        IsOverrided = false;
        Engine.UpdateInternalGameResolution();
    }
}