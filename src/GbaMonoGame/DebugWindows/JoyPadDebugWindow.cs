using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using BinarySerializer.Ubisoft.GbaEngine;
using ImGuiNET;

namespace GbaMonoGame;

public class JoyPadDebugWindow : DebugWindow
{
    public override string Name => "JoyPad";

    public void LoadBizHawkTas(string filePath)
    {
        using FileStream fileStream = File.OpenRead(filePath);
        using ZipArchive bk2 = new(fileStream, ZipArchiveMode.Read);

        ZipArchiveEntry entry = bk2.GetEntry("Input Log.txt");

        if (entry == null)
            return;

        using Stream logFileStream = entry.Open();
        using StreamReader reader = new(logFileStream);

        List<GbaInput> inputs = new();
        const string prefix = "|    0,    0,    0,    0,";
        GbaInput[] inputSeq =
        [
            GbaInput.Up,
            GbaInput.Down,
            GbaInput.Left,
            GbaInput.Right,
            GbaInput.Start,
            GbaInput.Select,
            GbaInput.B,
            GbaInput.A,
            GbaInput.L,
            GbaInput.R
        ];

        while (reader.ReadLine() is { } line)
        {
            GbaInput input = GbaInput.Valid;

            if (line.StartsWith(prefix))
            {
                line = line[prefix.Length..];

                for (int i = 0; i < inputSeq.Length; i++)
                {
                    if (line[i] != '.')
                        input |= inputSeq[i];
                }
            }

            inputs.Add(input);
        }

        inputs.Add(GbaInput.None);

        JoyPad.SetReplayData(inputs.ToArray());
    }

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        ImGui.SeparatorText("General");

        ImGui.Text($"KeyStatus: {JoyPad.Current.KeyStatus}");
        ImGui.Text($"KeyTriggers: {JoyPad.Current.KeyTriggers}");
        ImGui.Text($"Replay: {JoyPad.Current.IsInReplayMode}");

        if (ImGui.Button("Load BizHawk TAS (.bk2)"))
        {
            string filePath = FileDialog.OpenFile("Select the TAS file", new FileDialog.FileFilter("bk2", "BK2 files"));
            if (filePath != null)
                LoadBizHawkTas(filePath);
        }

        ImGui.SameLine();
        if (ImGui.Button("End replay"))
            JoyPad.Current.ReplayData = null;

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