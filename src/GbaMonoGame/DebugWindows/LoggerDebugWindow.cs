using System;
using System.Collections.Generic;
using ImGuiNET;
using static GbaMonoGame.Logger;
using Vector4 = System.Numerics.Vector4;

namespace GbaMonoGame;

/// <summary>
/// Debug window which shows the logger output.
/// </summary>
public class LoggerDebugWindow : DebugWindow
{
    public LoggerDebugWindow()
    {
        Logger.Log += Logger_Log;
    }

    private readonly List<Log> _logs = [];

    private readonly Dictionary<LogType, Vector4> _colors = new()
    {
        [LogType.NotImplemented] = new Vector4(42 / 255f, 175 / 255f, 212 / 255f, 1.0f),
        [LogType.Debug] = new Vector4(214 / 255f, 157 / 255f, 132 / 255f, 1.0f),
        [LogType.Info] = new Vector4(45 / 255f, 157 / 255f, 163 / 255f, 1.0f),
        [LogType.Error] = new Vector4(245 / 255f, 91 / 255f, 101 / 255f, 1.0f),
    };

    private readonly LogType[] _logTypes = Enum.GetValues<LogType>();
    private LogType _activeLogTypes = LogType.NotImplemented | LogType.Info | LogType.Error;

    private void Logger_Log(object sender, LogEventArgs e)
    {
        string message = e.Args.Length == 0 ? e.Message : String.Format(e.Message, e.Args);
        Log log = new(message, e.Type, _colors[e.Type]);

        // Only add if not already logged. This is because some logs will be sent out every frame.
        if (!_logs.Contains(log))
            _logs.Add(log);
    }

    public override string Name => "Log Output";

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        for (int i = 0; i < _logTypes.Length; i++)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, _colors[_logTypes[i]]);

            bool isActive = (_activeLogTypes & _logTypes[i]) != 0;

            if (ImGui.Checkbox(_logTypes[i].ToString(), ref isActive))
            {
                if (isActive)
                    _activeLogTypes |= _logTypes[i];
                else
                    _activeLogTypes &= ~_logTypes[i];
            }

            if (i < _logTypes.Length - 1)
                ImGui.SameLine();

            ImGui.PopStyleColor();
        }

        if (ImGui.Button("Clear"))
            _logs.Clear();

        ImGui.Spacing();

        foreach (Log log in _logs)
        {
            if ((_activeLogTypes & log.Type) == 0)
                continue;

            ImGui.PushStyleColor(ImGuiCol.Text, log.Color);
            ImGui.TextWrapped(log.Message);
            ImGui.PopStyleColor();
        }
    }

    public override void Unload()
    {
        Logger.Log -= Logger_Log;
    }

    private record struct Log(string Message, LogType Type, Vector4 Color);
}