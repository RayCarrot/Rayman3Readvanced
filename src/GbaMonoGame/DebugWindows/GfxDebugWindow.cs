using System.Linq;
using System;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace GbaMonoGame;

public class GfxDebugWindow : DebugWindow
{
    public override string Name => "Gfx";

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        if (ImGui.BeginTabBar("GfxTabs"))
        {
            if (ImGui.BeginTabItem("General"))
            {
                ImGui.SeparatorText("Resolution");

                Point windowRes = Engine.GameWindow.GetResolution();
                Vector2 viewPortFullSize = Engine.GameViewPort.FullSize;
                Box viewPortRenderBox = Engine.GameViewPort.RenderBox;
                ImGui.Text($"Window resolution: {windowRes.X} x {windowRes.Y}");
                ImGui.Text($"Viewport full size: {viewPortFullSize.X} x {viewPortFullSize.Y}");
                ImGui.Text($"Viewport render box position: {viewPortRenderBox.Position.X} x {viewPortRenderBox.Position.X}");
                ImGui.Text($"Viewport render box size: {viewPortRenderBox.Size.X} x {viewPortRenderBox.Size.X}");
                ImGui.Text($"Original game resolution: {Rom.OriginalResolution.X} x {Rom.OriginalResolution.Y}");
                ImGui.Text($"Original scaled game resolution: {Rom.OriginalScaledGameRenderContext.Resolution.X} x {Rom.OriginalScaledGameRenderContext.Resolution.Y}");
                ImGui.Text($"Internal game resolution: {Engine.Config.InternalGameResolution.X} x {Engine.Config.InternalGameResolution.Y}");

                float resX = Engine.Config.InternalGameResolution.X;
                float resY = Engine.Config.InternalGameResolution.Y;
                if (ImGui.SliderFloat("Internal resolution X", ref resX, Single.Epsilon, 1000))
                {
                    Engine.Config.InternalGameResolution = new Vector2(resX, resY);
                    Engine.GameViewPort.UpdateRenderBox();
                }
                if (ImGui.SliderFloat("Internal resolution Y", ref resY, Single.Epsilon, 1000))
                {
                    Engine.Config.InternalGameResolution = new Vector2(resX, resY);
                    Engine.GameViewPort.UpdateRenderBox();
                }

                ImGui.Spacing();
                ImGui.SeparatorText("Fade");

                float fade = Gfx.Fade;
                ImGui.SliderFloat("Level", ref fade, 0, 1);
                Gfx.Fade = fade;

                for (int i = 0; i < 4; i++)
                {
                    if (i != 0)
                        ImGui.SameLine();

                    if (ImGui.RadioButton($"{(FadeMode)i}", Gfx.FadeControl.Mode == (FadeMode)i))
                        Gfx.FadeControl = Gfx.FadeControl with { Mode = (FadeMode)i };
                }

                ImGui.Spacing();
                ImGui.SeparatorText("Effects");

                ImGui.Text($"Screen effect: {Gfx.ScreenEffect?.GetType().Name}");
                ImGui.Text($"Color: {Gfx.Color}");

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Screens"))
            {
                ImGui.SeparatorText("Screens");

                if (ImGui.BeginTable("_screens", 9))
                {
                    ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Wrap", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Id", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Prio", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Offset", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Size", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Bpp", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Render context");
                    ImGui.TableSetupColumn("Renderer");
                    ImGui.TableHeadersRow();

                    foreach (GfxScreen screen in Gfx.Screens.OrderBy(x => x.Id))
                    {
                        ImGui.TableNextRow();

                        ImGui.TableNextColumn();
                        screen.IsEnabled = ImGuiExt.Checkbox($"##{screen.Id}_enabled", screen.IsEnabled);

                        ImGui.TableNextColumn();
                        screen.Wrap = ImGuiExt.Checkbox($"##{screen.Id}_wrap", screen.Wrap);

                        ImGui.TableNextColumn();
                        ImGui.Text($"{screen.Id}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{screen.Priority}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{screen.Offset.X:0.00} x {screen.Offset.Y:0.00}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{screen.Renderer?.GetSize(screen).X:0.00} x {screen.Renderer?.GetSize(screen).Y:0.00}");

                        ImGui.TableNextColumn();
                        ImGui.Text(screen.Is8Bit switch
                        {
                            null => String.Empty,
                            true => "8",
                            false => "4",
                        });

                        ImGui.TableNextColumn();
                        ImGui.Text($"{screen.RenderOptions.RenderContext.GetType().Name}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{screen.Renderer?.GetType().Name}");
                    }
                    ImGui.EndTable();
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Sprites"))
            {
                ImGui.SeparatorText("Sprites");

                if (ImGui.BeginTable("_sprites", 9))
                {
                    ImGui.TableSetupColumn("Prio", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Position", ImGuiTableColumnFlags.WidthFixed, 120);
                    ImGui.TableSetupColumn("Affine", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Palette", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Render context");
                    ImGui.TableHeadersRow();

                    foreach (Sprite sprite in Gfx.Sprites)
                    {
                        ImGui.TableNextRow();

                        ImGui.TableNextColumn();
                        ImGui.Text($"{sprite.Priority}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{sprite.Position.X:0.00} x {sprite.Position.Y:0.00}");

                        ImGui.TableNextColumn();
                        ImGui.Text(sprite.AffineMatrix.HasValue ? "true" : "false");

                        ImGui.TableNextColumn();
                        ImGui.Text(sprite.RenderOptions.PaletteTexture != null ? "true" : "false");

                        ImGui.TableNextColumn();
                        ImGui.Text(sprite.RenderOptions.RenderContext.GetType().Name);
                    }
                    ImGui.EndTable();
                }

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }
}