using ImGuiNET;

namespace GbaMonoGame;

public class SoundDebugWindow : DebugWindow
{
    public override string Name => "Sound";

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    { 
        ImGui.SeparatorText("Songs");

        if (ImGui.Button("Stop all"))
            Engine.Sem.StopAllSongs();

        Engine.Sem.DrawDebugLayout();
    }
}