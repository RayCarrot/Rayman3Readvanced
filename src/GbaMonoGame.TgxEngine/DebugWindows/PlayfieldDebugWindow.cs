using ImGuiNET;
using System.Linq;
using System;
using Microsoft.Xna.Framework;

namespace GbaMonoGame.TgxEngine;

public class PlayfieldDebugWindow : DebugWindow
{
    public override string Name => "Playfield";

    private void DrawPlayfield2D(TgxPlayfield2D playfield2D)
    {
        Vector2 pos = playfield2D.Camera.Position;

        ImGui.SeparatorText("Camera");

        Vector2 minPos = playfield2D.Camera.GetMainCluster().MinPosition;
        Vector2 maxPos = playfield2D.Camera.GetMainCluster().MaxPosition;

        bool modifiedX = ImGui.SliderFloat("Camera X", ref pos.X, minPos.X, maxPos.X);
        bool modifiedY = ImGui.SliderFloat("Camera Y", ref pos.Y, minPos.Y, maxPos.Y);

        if (modifiedX || modifiedY)
            playfield2D.Camera.Position = pos;

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.SeparatorText("Clusters");

        if (ImGui.BeginTable("_clusters", 9))
        {
            ImGui.TableSetupColumn("Id", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Layers", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Position");
            ImGui.TableSetupColumn("Size");
            ImGui.TableSetupColumn("Min position");
            ImGui.TableSetupColumn("Max position");
            ImGui.TableSetupColumn("Scroll factor");
            ImGui.TableSetupColumn("Scrolls", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Render context");
            ImGui.TableHeadersRow();

            int i = 0;
            foreach (TgxCluster cluster in playfield2D.Camera.GetClusters(true))
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text($"{(i == 0 ? "Main" : $"{i}")}");

                ImGui.TableNextColumn();
                ImGui.Text($"{String.Join(", ", cluster.Layers.Where(x => x is TgxTileLayer).Select(x => (int)((TgxTileLayer)x).LayerId).Concat(cluster.Layers.Where(x => x is TgxTextureLayer).Select(x => ((TgxTextureLayer)x).Screen.Id)))}");

                ImGui.TableNextColumn();
                ImGui.Text($"{cluster.Position.X:0.00} x {cluster.Position.Y:0.00}");

                ImGui.TableNextColumn();
                ImGui.Text($"{cluster.Size.X:0.} x {cluster.Size.Y:0.}");

                ImGui.TableNextColumn();
                ImGui.Text($"{cluster.MinPosition.X:0.00} x {cluster.MinPosition.Y:0.00}");

                ImGui.TableNextColumn();
                ImGui.Text($"{cluster.MaxPosition.X:0.00} x {cluster.MaxPosition.Y:0.00}");

                ImGui.TableNextColumn();
                ImGui.Text($"{cluster.ScrollFactor.X:0.00} x {cluster.ScrollFactor.Y:0.00}");

                ImGui.TableNextColumn();
                ImGui.Text($"{(cluster.Stationary ? "No" : "Yes")}");

                ImGui.TableNextColumn();
                ImGui.Text($"{cluster.RenderContext.GetType().Name.Replace("RenderContext", String.Empty)}");

                i++;
            }
            ImGui.EndTable();
        }
    }

    private void DrawPlayfieldMode7(TgxPlayfieldMode7 playfieldMode7)
    {
        Vector2 pos = playfieldMode7.Camera.Position;

        ImGui.SeparatorText("Camera");

        bool modifiedX = ImGui.SliderFloat("Camera X", ref pos.X, 0, 2048);
        bool modifiedY = ImGui.SliderFloat("Camera Y", ref pos.Y, 0, 2048);

        if (modifiedX || modifiedY)
            playfieldMode7.Camera.Position = pos;

        float direction = playfieldMode7.Camera.Direction;
        if (ImGui.SliderFloat("Direction", ref direction, 0, 256))
            playfieldMode7.Camera.Direction = direction;

        float horizon = playfieldMode7.Camera.Horizon;
        if (ImGui.SliderFloat("Horizon", ref horizon, 0, 256))
            playfieldMode7.Camera.Horizon = horizon;

        float cameraFieldOfView = playfieldMode7.Camera.CameraFieldOfView;
        if (ImGui.SliderFloat("Camera FOV", ref cameraFieldOfView, 0, MathF.PI))
            playfieldMode7.Camera.CameraFieldOfView = cameraFieldOfView;

        float cameraDistance = playfieldMode7.Camera.CameraDistance;
        if (ImGui.SliderFloat("Camera Distance", ref cameraDistance, 0, 1000))
            playfieldMode7.Camera.CameraDistance = cameraDistance;

        float cameraHeight = playfieldMode7.Camera.CameraHeight;
        if (ImGui.SliderFloat("Camera Height", ref cameraHeight, -100, 200))
            playfieldMode7.Camera.CameraHeight = cameraHeight;

        float cameraTargetHeight = playfieldMode7.Camera.CameraTargetHeight;
        if (ImGui.SliderFloat("Camera Target Height", ref cameraTargetHeight, -100, 100))
            playfieldMode7.Camera.CameraTargetHeight = cameraTargetHeight;

        float cameraFar = playfieldMode7.Camera.CameraFar;
        if (ImGui.SliderFloat("Camera Far", ref cameraFar, 0, 800))
            playfieldMode7.Camera.CameraFar = cameraFar;
    }

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        if (Frame.Current is IHasPlayfield { Playfield: TgxPlayfield2D playfield2D }) 
            DrawPlayfield2D(playfield2D);
        else if (Frame.Current is IHasPlayfield { Playfield: TgxPlayfieldMode7 playfieldMode7 })
            DrawPlayfieldMode7(playfieldMode7);
    }
}