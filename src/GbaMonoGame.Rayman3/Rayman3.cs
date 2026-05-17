using System.IO;
using System.Text.Json;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public static class Rayman3
{
    private static readonly JsonSerializerOptions _configJsonOptions = new() { ReadCommentHandling = JsonCommentHandling.Skip };

    public static LocalizationManager Loc { get; private set; }
    public static AchievementsManager Achievements { get; private set; }
    public static TimeAttackManager TimeAttack { get; private set; }

    private static T DeserializeConfig<T>(string configName)
    {
        string filePath = Path.Combine(Paths.AssetsDirectoryName, "Rayman3", "Config", $"{configName}.jsonc");
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<T>(json, _configJsonOptions);
    }

    public static void Init()
    {
        // Load configs
        TimeAttackLevelInfo[] timeAttackLevelInfos = DeserializeConfig<TimeAttackLevelInfo[]>("TimeAttackConfig");

        // Create services
        Loc = new LocalizationManager();
        Achievements = new AchievementsManager(Rayman3Achievements.Achievements);
        TimeAttack = new TimeAttackManager(timeAttackLevelInfos);

        // Initialize services
        Loc.SetLanguage(Engine.LocalConfig.Display.Language);
        FrameManager.AddStepAction(Achievements.Step);
    }

    public static void UnInit()
    {
        // Uninitialize services
        if (Achievements != null)
            FrameManager.RemoveStepAction(Achievements.Step);

        // Remove services
        Loc = null;
        Achievements = null;
        TimeAttack = null;
    }
}