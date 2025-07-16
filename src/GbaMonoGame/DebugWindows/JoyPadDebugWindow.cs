using System;
using System.IO;
using System.Linq;
using System.Text;
using BinarySerializer.Ubisoft.GbaEngine;
using ImGuiNET;

namespace GbaMonoGame;

public class JoyPadDebugWindow : DebugWindow
{
    public override string Name => "JoyPad";

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        ImGui.SeparatorText("General");

        ImGui.Text($"KeyStatus: {JoyPad.Current.KeyStatus}");
        ImGui.Text($"KeyTriggers: {JoyPad.Current.KeyTriggers}");
        ImGui.Text($"Replay: {JoyPad.Current.IsInReplayMode}");

        ImGui.Spacing();
        ImGui.SeparatorText("Record");

        if (!JoyPad.Current.IsInRecordMode)
        {
            if (ImGui.Button("Start"))
                JoyPad.Current.BeginRecording();
        }
        else
        {
            if (ImGui.Button("End"))
            {
                GbaInput[] recordedData = JoyPad.Current.EndRecording();
                StringBuilder sb = new();

                sb.AppendLine("[");
                foreach (GbaInput input in recordedData.Append(GbaInput.None))
                {
                    sb.Append("    ");

                    bool first = true;
                    foreach (GbaInput gbaInputValue in Enum.GetValues<GbaInput>())
                    {
                        if ((gbaInputValue != GbaInput.None || input == GbaInput.None) && input.HasFlag(gbaInputValue))
                        {
                            if (!first)
                                sb.Append(" | ");

                            sb.Append($"{nameof(GbaInput)}.{gbaInputValue}");
                            first = false;
                        }
                    }

                    sb.Append(",");
                    sb.AppendLine();
                }
                sb.AppendLine("]");

                File.WriteAllText("JoyPadRecording.txt", sb.ToString());
            }
        }
    }
}