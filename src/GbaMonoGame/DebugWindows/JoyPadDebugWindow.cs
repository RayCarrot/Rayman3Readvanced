using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using BinarySerializer;
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

        Engine.JoyPad.SetReplayData(inputs.ToArray());
    }

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        ImGui.SeparatorText("General");

        ImGui.Text($"KeyStatus: {Engine.JoyPad.Current.KeyStatus}");
        ImGui.Text($"KeyTriggers: {Engine.JoyPad.Current.KeyTriggers}");
        ImGui.Text($"Replay: {Engine.JoyPad.Current.IsInReplayMode}");

        if (ImGui.Button("Load BizHawk TAS (.bk2)"))
        {
            string filePath = Engine.FileDialog.OpenFile("Select the TAS file", new FileDialogManager.FileFilter("bk2", "BK2 files"));
            if (filePath != null)
                LoadBizHawkTas(filePath);
        }

        ImGui.SameLine();
        if (ImGui.Button("Load recording (.rec)"))
        {
            string filePath = Engine.FileDialog.OpenFile("Select the JoyPad recording file", new FileDialogManager.FileFilter("rec", "REC files"));

            if (filePath != null)
            {
                using Stream fileStream = File.OpenRead(filePath);
                using Reader reader = new(fileStream);

                List<GbaInput> inputs = [];
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                    inputs.Add((GbaInput)reader.ReadUInt16());
                Engine.JoyPad.SetReplayData(inputs.ToArray());
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("End replay"))
            Engine.JoyPad.Current.ReplayData = null;

        ImGui.Spacing();
        ImGui.SeparatorText("Record");

        if (!Engine.JoyPad.Current.IsInRecordMode)
        {
            if (ImGui.Button("Start"))
                Engine.JoyPad.Current.BeginRecording();
        }
        else
        {
            if (ImGui.Button("End"))
            {
                GbaInput[] recordedData = Engine.JoyPad.Current.EndRecording();
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

                // Save as text file
                File.WriteAllText("JoyPadRecording.txt", sb.ToString());

                // Save as binary file
                using Stream outputStream = File.Create("JoyPadRecording.rec");
                using Writer writer = new(outputStream);
                foreach (GbaInput input in recordedData.Append(GbaInput.None))
                    writer.Write((ushort)input);
            }
        }
    }
}