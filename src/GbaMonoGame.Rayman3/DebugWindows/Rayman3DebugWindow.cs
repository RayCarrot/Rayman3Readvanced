using System;
using System.Text;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;
using ImGuiNET;

namespace GbaMonoGame.Rayman3;

public class Rayman3DebugWindow : DebugWindow
{
    public override string Name => "Rayman 3";

    public bool PlaceTimeFreezeItems { get; set; }
    public TimeFreezeItem.Action TimeFreezeItemAction { get; set; }

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        if (ImGui.BeginTabBar("Rayman3Tabs"))
        {
            if (ImGui.BeginTabItem("Game Info"))
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
                    bool hasPower = GameInfo.IsPowerEnabled(power);
                    if (ImGui.Checkbox(power.ToString(), ref hasPower))
                    {
                        if (hasPower)
                            GameInfo.EnablePower(power);
                        else
                            GameInfo.DisablePower(power);
                    }
                }

                ImGui.Spacing();
                ImGui.SeparatorText("Cheats");

                for (int i = 0; i < 3; i++)
                {
                    if (i != 0)
                        ImGui.SameLine();

                    Cheat cheat = (Cheat)(1 << i);
                    bool cheatEnabled = GameInfo.IsCheatEnabled(cheat);
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

                        if (SaveGameManager.SlotExists(i))
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

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Time Attack Info"))
            {
                ImGui.SeparatorText("General");

                TimeAttackInfo.IsActive = ImGuiExt.Checkbox("Active", TimeAttackInfo.IsActive);
                TimeAttackInfo.IsPaused = ImGuiExt.Checkbox("Paused", TimeAttackInfo.IsPaused);
                ImGui.Text($"Mode: {TimeAttackInfo.Mode}");
                ImGui.Text($"Timer: {TimeAttackInfo.Timer}");

                if (Frame.Current is IHasScene frame)
                {
                    ImGui.SeparatorText("Time Freeze Items");

                    PlaceTimeFreezeItems = ImGuiExt.Checkbox("Place Items", PlaceTimeFreezeItems);

                    if (PlaceTimeFreezeItems)
                    {
                        if (ImGui.RadioButton("Decrease 3", TimeFreezeItemAction == TimeFreezeItem.Action.Init_Decrease3))
                            TimeFreezeItemAction = TimeFreezeItem.Action.Init_Decrease3;

                        if (ImGui.RadioButton("Decrease 5", TimeFreezeItemAction == TimeFreezeItem.Action.Init_Decrease5))
                            TimeFreezeItemAction = TimeFreezeItem.Action.Init_Decrease5;

                        if (InputManager.IsMouseOnScreen(frame.Scene.RenderContext) && InputManager.IsMouseLeftButtonJustPressed())
                        {
                            Vector2 mousePos = InputManager.GetMousePosition(frame.Scene.RenderContext) + frame.Scene.Playfield.Camera.Position;
                            frame.Scene.KnotManager.AddAlwaysActor(frame.Scene, new ActorResource
                            {
                                Pos = new BinarySerializer.Ubisoft.GbaEngine.Vector2((short)mousePos.X, (short)mousePos.Y),
                                IsEnabled = true,
                                IsAwake = true,
                                IsAnimatedObjectDynamic = false,
                                IsProjectile = false,
                                ResurrectsImmediately = false,
                                ResurrectsLater = false,
                                Type = (byte)ReadvancedActorType.TimeFreezeItem,
                                FirstActionId = (byte)TimeFreezeItemAction,
                                Links = [0xFF, 0xFF, 0xFF, 0xFF],
                                Model = ReadvancedResources.TimeFreezeItemActorModel,
                            });
                        }
                    }

                    if (ImGui.Button("Resurrect all"))
                    {
                        foreach (BaseActor actor in new DisabledAlwaysActorIterator(frame.Scene))
                        {
                            if ((ReadvancedActorType)actor.Type == ReadvancedActorType.TimeFreezeItem)
                            {
                                ((ActionActor)actor).HitPoints = 1;
                                actor.ProcessMessage(this, Message.Resurrect);
                            }
                        }
                    }

                    if (ImGui.Button("Copy to clipboard"))
                    {
                        StringBuilder sb = new();

                        foreach (BaseActor actor in new AlwaysActorIterator(frame.Scene))
                        {
                            if ((ReadvancedActorType)actor.Type == ReadvancedActorType.TimeFreezeItem)
                            {
                                TimeFreezeItem timeFreezeItem = (TimeFreezeItem)actor;
                                sb.AppendLine($"new({nameof(TimeFreezeItem)}.{nameof(TimeFreezeItem.Action)}.{timeFreezeItem.InitialAction}, " +
                                              $"new BinarySerializer.Ubisoft.GbaEngine.Vector2({(short)timeFreezeItem.InitialPosition.X}, {(short)timeFreezeItem.InitialPosition.Y})),");
                            }
                        }

                        ImGui.SetClipboardText(sb.ToString());
                    }
                }

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }
}