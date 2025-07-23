using System;
using System.IO;
using System.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame;

public class GfxDebugWindow : DebugWindow
{
    public override string Name => "Gfx";

    // NOTE: This seems to have some issue where the screen is shifted one pixel down on the texture - not sure why
    private void ExportScreen(GfxScreen screen)
    {
        GraphicsDevice graphicsDevice = Engine.GraphicsDevice;
        Vector2 size = screen.Renderer.GetSize(screen);

        using RenderTarget2D renderTarget = new(
            graphicsDevice: graphicsDevice,
            width: (int)size.X,
            height: (int)size.Y,
            mipMap: false,
            preferredFormat: graphicsDevice.PresentationParameters.BackBufferFormat,
            preferredDepthFormat: DepthFormat.Depth24);

        graphicsDevice.SetRenderTarget(renderTarget);
        graphicsDevice.DepthStencilState = DepthStencilState.Default;

        Vector2 oldResolution = Engine.InternalGameResolution;
        Vector2 oldOffset = screen.Offset;
        RenderContext oldRenderContext = screen.RenderOptions.RenderContext;
        BlendMode oldBlendMode = screen.RenderOptions.BlendMode;

        Engine.SetInternalGameResolution(size);
        screen.Offset = Vector2.Zero;
        screen.RenderOptions.RenderContext = new FixedResolutionRenderContext(size);
        screen.RenderOptions.BlendMode = BlendMode.None;

        Engine.GameViewPort.Resize(size);

        graphicsDevice.Clear(Color.Transparent);

        GfxRenderer renderer = new(graphicsDevice);
        
        screen.Draw(renderer, Color.White);
        renderer.EndRender();

        Engine.SetInternalGameResolution(oldResolution);
        screen.Offset = oldOffset;
        screen.RenderOptions.RenderContext = oldRenderContext;
        screen.RenderOptions.BlendMode = oldBlendMode;

        const string outputDir = "Screens";
        Directory.CreateDirectory(outputDir);
        using (Stream stream = File.Create(Path.Combine(outputDir, $"{screen.Id}.png")))
            renderTarget.SaveAsPng(stream, renderTarget.Width, renderTarget.Height);

        graphicsDevice.SetRenderTarget(null);
    }

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
                ImGui.Text($"Internal game resolution: {Engine.InternalGameResolution.X} x {Engine.InternalGameResolution.Y}");

                float resX = Engine.InternalGameResolution.X;
                float resY = Engine.InternalGameResolution.Y;
                if (ImGui.SliderFloat("Internal resolution X", ref resX, Single.Epsilon, 1000))
                {
                    Engine.SetInternalGameResolution(new Vector2(resX, resY));
                    Engine.LocalConfig.Tweaks.InternalGameResolution = new Vector2(resX, resY);
                }
                if (ImGui.SliderFloat("Internal resolution Y", ref resY, Single.Epsilon, 1000))
                {
                    Engine.SetInternalGameResolution(new Vector2(resX, resY));
                    Engine.LocalConfig.Tweaks.InternalGameResolution = new Vector2(resX, resY);
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

                if (ImGui.BeginTable("_screens", 11))
                {
                    ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Wrap", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Id", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Prio", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Offset", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Size", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Wrap", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Bpp", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Render context");
                    ImGui.TableSetupColumn("Renderer");
                    ImGui.TableSetupColumn("Actions");
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
                        ImGui.Text($"{screen.CurrentWrapX}x{screen.CurrentWrapY}");

                        ImGui.TableNextColumn();
                        ImGui.Text(screen.Is8Bit switch
                        {
                            null => String.Empty,
                            true => "8",
                            false => "4",
                        });

                        ImGui.TableNextColumn();
                        ImGui.Text($"{screen.RenderOptions.RenderContext.GetType().Name.Replace("RenderContext", String.Empty)}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{screen.Renderer?.GetType().Name.Replace("ScreenRenderer", String.Empty)}");

                        ImGui.TableNextColumn();
                        if (screen.Renderer != null && ImGui.Button($"Export##{screen.Id}"))
                            ExportScreen(screen);
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