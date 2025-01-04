using System;
using ImGuiNET;

namespace GbaMonoGame;

public class MultiplayerDebugWindow : DebugWindow
{
    public override string Name => "Multiplayer";

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        ImGui.SeparatorText("General");

        RSMultiplayer.IsActive = ImGuiExt.Checkbox("IsActive", RSMultiplayer.IsActive);

        ImGui.BeginDisabled(!RSMultiplayer.IsActive);

        if (ImGui.BeginCombo("State", RSMultiplayer.MubState.ToString()))
        {
            foreach (MubState state in Enum.GetValues<MubState>())
            {
                if (ImGui.Selectable(state.ToString(), RSMultiplayer.MubState == state))
                    RSMultiplayer.MubState = state;
            }

            ImGui.EndCombo();
        }

        ImGui.Spacing();

        ImGui.Text("PlayersCount:");
        for (int i = 0; i < RSMultiplayer.MaxPlayersCount + 1; i++)
        {
            ImGui.SameLine();
            if (ImGui.RadioButton($"{i}##PlayersCount", RSMultiplayer.PlayersCount == i))
            {
                RSMultiplayer.PlayersCount = i;
                MultiplayerManager.PlayersCount = i;

                if (RSMultiplayer.MachineId >= i)
                    RSMultiplayer.MachineId = 0;
            }
        }

        ImGui.Text("MachineId:");
        for (int i = 0; i < Math.Max(RSMultiplayer.PlayersCount, 1); i++)
        {
            ImGui.SameLine();
            if (ImGui.RadioButton($"{i}##MachineId", RSMultiplayer.MachineId == i))
            {
                RSMultiplayer.MachineId = i;
                MultiplayerManager.MachineId = i;
            }
        }

        ImGui.EndDisabled();
    }
}