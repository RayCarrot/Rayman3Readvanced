using System;
using GbaMonoGame.Rayman3;
using ImGuiNET;

namespace GbaMonoGame.TgxEngine;

public class GameInfoDebugWindow : DebugWindow
{
    public override string Name => "Game Info";

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        ImGui.SeparatorText("General");

        ImGui.Text($"MapId: {GameInfo.MapId}");
        ImGui.Text($"LevelType: {GameInfo.LevelType}");
        ImGui.Text($"World: {GameInfo.WorldId}");

        ImGui.Spacing();
        ImGui.SeparatorText("Powers");

        for (int i = 0; i < 6; i++)
        {
            if (i != 0)
                ImGui.SameLine();

            Power power = (Power)(1 << i);
            bool hasPower = (GameInfo.Powers & power) != 0;
            if (ImGui.Checkbox(power.ToString(), ref hasPower))
            {
                if (hasPower)
                    GameInfo.Powers |= power;
                else
                    GameInfo.Powers &= ~power;
            }
        }

        ImGui.Spacing();
        ImGui.SeparatorText("Cheats");

        for (int i = 0; i < 3; i++)
        {
            if (i != 0)
                ImGui.SameLine();

            Cheat cheat = (Cheat)(1 << i);
            bool cheatEnabled = (GameInfo.Cheats & cheat) != 0;
            if (ImGui.Checkbox(cheat.ToString(), ref cheatEnabled))
            {
                if (cheatEnabled)
                    GameInfo.Cheats |= cheat;
                else
                    GameInfo.Cheats &= ~cheat;
            }
        }

        ImGui.Spacing();
        ImGui.SeparatorText("Persistent Info");

        if (ImGui.Button("Load"))
        {
            GameInfo.GotoLastSaveGame();
        }

        ImGui.SameLine();
        if (ImGui.Button("New game"))
        {
            FrameManager.SetNextFrame(new Act1());
            GameInfo.ResetPersistentInfo();
        }

        for (int i = 0; i < GameInfo.ModernSaveSlotsCount; i++)
        {
            if (i != 0)
                ImGui.SameLine();

            if (ImGui.RadioButton($"Slot {i}", GameInfo.CurrentSlot == i))
            {
                GameInfo.Init();
                GameInfo.CurrentSlot = i;
                GameInfo.Load(i);
            }
        }

        ImGui.Text($"Lums: {GameInfo.GetTotalDeadLums()} / 1000");
        ImGui.Text($"Cages: {GameInfo.GetTotalDeadCages()} / 50");
        ImGui.Text($"Last played level: {(MapId)GameInfo.PersistentInfo.LastPlayedLevel}");
        ImGui.Text($"Last completed level: {(MapId)GameInfo.PersistentInfo.LastCompletedLevel}");
        ImGui.Text($"Lives: {GameInfo.PersistentInfo.Lives}");

        GameInfo.PersistentInfo.FinishedLyChallenge1 = ImGuiExt.Checkbox("Finished Ly Challenge 1", GameInfo.PersistentInfo.FinishedLyChallenge1);
        ImGui.SameLine();
        GameInfo.PersistentInfo.FinishedLyChallenge2 = ImGuiExt.Checkbox("Finished Ly Challenge 2", GameInfo.PersistentInfo.FinishedLyChallenge2);
        ImGui.SameLine();
        GameInfo.PersistentInfo.FinishedLyChallengeGCN = ImGuiExt.Checkbox("Finished Ly Challenge GCN", GameInfo.PersistentInfo.FinishedLyChallengeGCN);

        GameInfo.PersistentInfo.UnlockedBonus1 = ImGuiExt.Checkbox("Unlocked bonus 1", GameInfo.PersistentInfo.UnlockedBonus1);
        ImGui.SameLine();
        GameInfo.PersistentInfo.UnlockedBonus2 = ImGuiExt.Checkbox("Unlocked bonus 2", GameInfo.PersistentInfo.UnlockedBonus2);
        ImGui.SameLine();
        GameInfo.PersistentInfo.UnlockedBonus3 = ImGuiExt.Checkbox("Unlocked bonus 3", GameInfo.PersistentInfo.UnlockedBonus3);
        ImGui.SameLine();
        GameInfo.PersistentInfo.UnlockedBonus4 = ImGuiExt.Checkbox("Unlocked bonus 4", GameInfo.PersistentInfo.UnlockedBonus4);

        GameInfo.PersistentInfo.UnlockedWorld2 = ImGuiExt.Checkbox("Unlocked world 2", GameInfo.PersistentInfo.UnlockedWorld2);
        ImGui.SameLine();
        GameInfo.PersistentInfo.UnlockedWorld3 = ImGuiExt.Checkbox("Unlocked world 3", GameInfo.PersistentInfo.UnlockedWorld3);
        ImGui.SameLine();
        GameInfo.PersistentInfo.UnlockedWorld4 = ImGuiExt.Checkbox("Unlocked world 4", GameInfo.PersistentInfo.UnlockedWorld4);
        ImGui.SameLine();
        GameInfo.PersistentInfo.UnlockedFinalBoss = ImGuiExt.Checkbox("Unlocked final boss", GameInfo.PersistentInfo.UnlockedFinalBoss);

        GameInfo.PersistentInfo.PlayedWorld2Unlock = ImGuiExt.Checkbox("Played world 2 unlock", GameInfo.PersistentInfo.PlayedWorld2Unlock);
        ImGui.SameLine();
        GameInfo.PersistentInfo.PlayedWorld3Unlock = ImGuiExt.Checkbox("Played world 3 unlock", GameInfo.PersistentInfo.PlayedWorld3Unlock);
        ImGui.SameLine();
        GameInfo.PersistentInfo.PlayedWorld4Unlock = ImGuiExt.Checkbox("Played world 4 unlock", GameInfo.PersistentInfo.PlayedWorld4Unlock);
        
        GameInfo.PersistentInfo.PlayedAct4 = ImGuiExt.Checkbox("Played act 4", GameInfo.PersistentInfo.PlayedAct4);
        ImGui.SameLine();
        GameInfo.PersistentInfo.PlayedMurfyWorldHelp = ImGuiExt.Checkbox("Played Murfy world help", GameInfo.PersistentInfo.PlayedMurfyWorldHelp);
        ImGui.SameLine();
        GameInfo.PersistentInfo.UnlockedLyChallengeGCN = ImGuiExt.Checkbox("Unlocked Ly Challenge GCN", GameInfo.PersistentInfo.UnlockedLyChallengeGCN);

        ImGui.Text($"Completed GCN bonus levels: {GameInfo.PersistentInfo.CompletedGCNBonusLevels}");

        if (ImGui.Button("Reset"))
            GameInfo.ResetPersistentInfo();

        ImGui.SameLine();
        if (ImGui.Button("Unlock all levels"))
            GameInfo.PersistentInfo.LastCompletedLevel = (byte)MapId.BossFinal_M2;

        ImGui.SameLine();
        if (ImGui.Button("Unlock all GCN levels"))
            GameInfo.PersistentInfo.CompletedGCNBonusLevels = 10;

        ImGui.SameLine();
        if (ImGui.Button("All lums and cages"))
        {
            Array.Clear(GameInfo.PersistentInfo.Lums);
            Array.Clear(GameInfo.PersistentInfo.Cages);
        }
    }
}