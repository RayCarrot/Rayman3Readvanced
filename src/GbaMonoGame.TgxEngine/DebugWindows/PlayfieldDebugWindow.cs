using ImGuiNET;
using System.Linq;
using System;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public class PlayfieldDebugWindow : DebugWindow
{
    public override string Name => "Playfield";

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        if (Frame.Current is not IHasPlayfield { Playfield: TgxPlayfield2D playfield2D }) 
            return;

        Vector2 pos = playfield2D.Camera.Position;

        ImGui.SeparatorText("Camera");

        bool modifiedX = ImGui.SliderFloat("Camera X", ref pos.X, 0, playfield2D.Camera.GetMainCluster().MaxPosition.X);
        bool modifiedY = ImGui.SliderFloat("Camera Y", ref pos.Y, 0, playfield2D.Camera.GetMainCluster().MaxPosition.Y);

        if (modifiedX || modifiedY)
            playfield2D.Camera.Position = pos;

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.SeparatorText("Clusters");

        if (ImGui.BeginTable("_clusters", 7))
        {
            ImGui.TableSetupColumn("Id", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Layers", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Position");
            ImGui.TableSetupColumn("Max position");
            ImGui.TableSetupColumn("Scroll factor");
            ImGui.TableSetupColumn("Type");
            ImGui.TableSetupColumn("Render context");
            ImGui.TableHeadersRow();

            int i = 0;
            foreach (TgxCluster cluster in playfield2D.Camera.GetClusters(true))
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text($"{(i == 0 ? "Main" : $"{i}")}");

                ImGui.TableNextColumn();
                ImGui.Text($"{String.Join(", ", cluster.Layers.Where(x => x is TgxTileLayer).Select(x => ((TgxTileLayer)x).LayerId))}");

                ImGui.TableNextColumn();
                ImGui.Text($"{cluster.Position.X:0.00} x {cluster.Position.Y:0.00}");

                ImGui.TableNextColumn();
                ImGui.Text($"{cluster.MaxPosition.X:0.00} x {cluster.MaxPosition.Y:0.00}");

                ImGui.TableNextColumn();
                ImGui.Text($"{cluster.ScrollFactor.X:0.00} x {cluster.ScrollFactor.Y:0.00}");

                ImGui.TableNextColumn();
                ImGui.Text($"{(cluster.Stationary ? "Stationary" : "Scrollable")}");

                ImGui.TableNextColumn();
                ImGui.Text($"{cluster.RenderContext.GetType().Name}");

                i++;
            }
            ImGui.EndTable();
        }
    }
}