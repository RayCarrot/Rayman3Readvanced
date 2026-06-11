using System;
using System.Linq;
using System.Text;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.J2me;
using GbaMonoGame.Rayman3.Readvanced;
using ImGuiNET;
using Game = GbaMonoGame.Rayman3.J2me.Game;

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
            if (ImGui.BeginTabItem("Game"))
            {
                ImGui.SeparatorText("General");

                ImGui.Text($"MapId: {Rayman3.GameInfo.MapId}");
                ImGui.Text($"LevelType: {Rayman3.GameInfo.LevelType}");
                ImGui.Text($"World: {Rayman3.GameInfo.WorldId}");

                ImGui.Spacing();
                ImGui.SeparatorText("Powers");

                for (int i = 0; i < 6; i++)
                {
                    if (i != 0)
                        ImGui.SameLine();

                    Power power = (Power)(1 << i);
                    bool hasPower = Rayman3.GameInfo.IsPowerEnabled(power);
                    if (ImGui.Checkbox(power.ToString(), ref hasPower))
                    {
                        if (hasPower)
                            Rayman3.GameInfo.EnablePower(power);
                        else
                            Rayman3.GameInfo.DisablePower(power);
                    }
                }

                ImGui.Spacing();
                ImGui.SeparatorText("Cheats");

                for (int i = 0; i < 3; i++)
                {
                    if (i != 0)
                        ImGui.SameLine();

                    Cheat cheat = (Cheat)(1 << i);
                    bool cheatEnabled = Rayman3.GameInfo.IsCheatEnabled(cheat);
                    if (ImGui.Checkbox(cheat.ToString(), ref cheatEnabled))
                    {
                        if (cheatEnabled)
                            Rayman3.GameInfo.Cheats |= cheat;
                        else
                            Rayman3.GameInfo.Cheats &= ~cheat;
                    }
                }

                ImGui.Spacing();
                ImGui.SeparatorText("Persistent Info");

                if (ImGui.Button("Load"))
                {
                    Rayman3.GameInfo.GotoLastSaveGame();
                    Rayman3.GameInfo.StartPlayTime();
                }

                ImGui.SameLine();
                if (ImGui.Button("New game"))
                {
                    Engine.FrameMngr.SetNextFrame(new Act1());
                    Rayman3.GameInfo.ResetPersistentInfo();
                }

                for (int i = 0; i < GameInfo.ModernSaveSlotsCount; i++)
                {
                    if (i != 0)
                        ImGui.SameLine();

                    if (ImGui.RadioButton($"Slot {i}", Rayman3.GameInfo.CurrentSlot == i))
                    {
                        Rayman3.GameInfo.Init();
                        Rayman3.GameInfo.CurrentSlot = i;

                        if (Rayman3.Save.SlotExists(i))
                            Rayman3.GameInfo.Load(i);
                    }
                }

                ImGui.Text($"Lums: {Rayman3.GameInfo.GetTotalDeadLums()} / 1000");
                ImGui.Text($"Cages: {Rayman3.GameInfo.GetTotalDeadCages()} / 50");
                ImGui.Text($"Last played level: {(MapId)Rayman3.GameInfo.PersistentInfo.LastPlayedLevel}");
                ImGui.Text($"Last completed level: {(MapId)Rayman3.GameInfo.PersistentInfo.LastCompletedLevel}");
                ImGui.Text($"Lives: {Rayman3.GameInfo.PersistentInfo.Lives}");

                Rayman3.GameInfo.PersistentInfo.FinishedLyChallenge1 = ImGuiExt.Checkbox("Finished Ly Challenge 1", Rayman3.GameInfo.PersistentInfo.FinishedLyChallenge1);
                ImGui.SameLine();
                Rayman3.GameInfo.PersistentInfo.FinishedLyChallenge2 = ImGuiExt.Checkbox("Finished Ly Challenge 2", Rayman3.GameInfo.PersistentInfo.FinishedLyChallenge2);
                ImGui.SameLine();
                Rayman3.GameInfo.PersistentInfo.FinishedLyChallengeGCN = ImGuiExt.Checkbox("Finished Ly Challenge GCN", Rayman3.GameInfo.PersistentInfo.FinishedLyChallengeGCN);

                Rayman3.GameInfo.PersistentInfo.UnlockedBonus1 = ImGuiExt.Checkbox("Unlocked bonus 1", Rayman3.GameInfo.PersistentInfo.UnlockedBonus1);
                ImGui.SameLine();
                Rayman3.GameInfo.PersistentInfo.UnlockedBonus2 = ImGuiExt.Checkbox("Unlocked bonus 2", Rayman3.GameInfo.PersistentInfo.UnlockedBonus2);
                ImGui.SameLine();
                Rayman3.GameInfo.PersistentInfo.UnlockedBonus3 = ImGuiExt.Checkbox("Unlocked bonus 3", Rayman3.GameInfo.PersistentInfo.UnlockedBonus3);
                ImGui.SameLine();
                Rayman3.GameInfo.PersistentInfo.UnlockedBonus4 = ImGuiExt.Checkbox("Unlocked bonus 4", Rayman3.GameInfo.PersistentInfo.UnlockedBonus4);

                Rayman3.GameInfo.PersistentInfo.UnlockedWorld2 = ImGuiExt.Checkbox("Unlocked world 2", Rayman3.GameInfo.PersistentInfo.UnlockedWorld2);
                ImGui.SameLine();
                Rayman3.GameInfo.PersistentInfo.UnlockedWorld3 = ImGuiExt.Checkbox("Unlocked world 3", Rayman3.GameInfo.PersistentInfo.UnlockedWorld3);
                ImGui.SameLine();
                Rayman3.GameInfo.PersistentInfo.UnlockedWorld4 = ImGuiExt.Checkbox("Unlocked world 4", Rayman3.GameInfo.PersistentInfo.UnlockedWorld4);
                ImGui.SameLine();
                Rayman3.GameInfo.PersistentInfo.UnlockedFinalBoss = ImGuiExt.Checkbox("Unlocked final boss", Rayman3.GameInfo.PersistentInfo.UnlockedFinalBoss);

                Rayman3.GameInfo.PersistentInfo.PlayedWorld2Unlock = ImGuiExt.Checkbox("Played world 2 unlock", Rayman3.GameInfo.PersistentInfo.PlayedWorld2Unlock);
                ImGui.SameLine();
                Rayman3.GameInfo.PersistentInfo.PlayedWorld3Unlock = ImGuiExt.Checkbox("Played world 3 unlock", Rayman3.GameInfo.PersistentInfo.PlayedWorld3Unlock);
                ImGui.SameLine();
                Rayman3.GameInfo.PersistentInfo.PlayedWorld4Unlock = ImGuiExt.Checkbox("Played world 4 unlock", Rayman3.GameInfo.PersistentInfo.PlayedWorld4Unlock);

                Rayman3.GameInfo.PersistentInfo.PlayedAct4 = ImGuiExt.Checkbox("Played act 4", Rayman3.GameInfo.PersistentInfo.PlayedAct4);
                ImGui.SameLine();
                Rayman3.GameInfo.PersistentInfo.PlayedMurfyWorldHelp = ImGuiExt.Checkbox("Played Murfy world help", Rayman3.GameInfo.PersistentInfo.PlayedMurfyWorldHelp);
                ImGui.SameLine();
                Rayman3.GameInfo.PersistentInfo.UnlockedLyChallengeGCN = ImGuiExt.Checkbox("Unlocked Ly Challenge GCN", Rayman3.GameInfo.PersistentInfo.UnlockedLyChallengeGCN);

                ImGui.Text($"Completed GCN bonus levels: {Rayman3.GameInfo.PersistentInfo.CompletedGCNBonusLevels}");

                if (ImGui.Button("Reset"))
                    Rayman3.GameInfo.ResetPersistentInfo();

                ImGui.SameLine();
                if (ImGui.Button("Unlock all levels"))
                    Rayman3.GameInfo.PersistentInfo.LastCompletedLevel = (byte)MapId.BossFinal_M2;

                ImGui.SameLine();
                if (ImGui.Button("Unlock all GCN levels"))
                    Rayman3.GameInfo.PersistentInfo.CompletedGCNBonusLevels = 10;

                ImGui.SameLine();
                if (ImGui.Button("All lums and cages"))
                {
                    Array.Clear(Rayman3.GameInfo.PersistentInfo.Lums);
                    Array.Clear(Rayman3.GameInfo.PersistentInfo.Cages);
                }

                if (ImGui.Button("Reload configs"))
                    Rayman3.LoadConfigs();

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Achievements"))
            {
                ImGui.SeparatorText("Tracking");

                ImGui.Text($"{nameof(ReadvancedSlot.DefeatedPirateTypes)}: {Rayman3.GameInfo.SaveSlot.DefeatedPirateTypes}");
                ImGui.Text($"{nameof(ReadvancedSlot.CollectedWhiteLums)}: {Rayman3.GameInfo.SaveSlot.CollectedWhiteLums.Length}");
                ImGui.Text($"{nameof(Rayman3Achievements.MarshAwakening1_HasMoved)}: {Rayman3Achievements.MarshAwakening1_HasMoved}");
                ImGui.Text($"{nameof(Rayman3Achievements.MissileRace1_HasStrafed)}: {Rayman3Achievements.MissileRace1_HasStrafed}");
                ImGui.Text($"{nameof(Rayman3Achievements.CaveBadDreamsM1_HitSkulls)}: {Rayman3Achievements.CaveBadDreamsM1_HitSkulls}");
                ImGui.Text($"{nameof(Rayman3Achievements.MenhirHills_HasDied)}: {Rayman3Achievements.MenhirHills_HasDied}");
                ImGui.Text($"{nameof(Rayman3Achievements.BossRockAndLava_HasUsedBlueLum)}: {Rayman3Achievements.BossRockAndLava_HasUsedBlueLum}");
                ImGui.Text($"{nameof(Rayman3Achievements.SanctuaryOfRockAndLava_HasKilledBlackLum)}: {Rayman3Achievements.SanctuaryOfRockAndLava_HasKilledBlackLum}");
                ImGui.Text($"{nameof(Rayman3Achievements.MissileRace2_HasTakenDamage)}: {Rayman3Achievements.MissileRace2_HasTakenDamage}");

                ImGui.SeparatorText("Achievements");

                foreach (AchievementInfo achievement in Rayman3.Achievements.GetAchievements())
                {
                    bool unlocked = Rayman3.Achievements.IsUnlocked(achievement.Id);
                    if (ImGui.Checkbox($"{achievement.Id}: {achievement.Title}", ref unlocked))
                    {
                        if (unlocked)
                            Rayman3.Achievements.Unlock(achievement.Id);
                        else
                            Rayman3.Achievements.Lock(achievement.Id);
                    }
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Time Attack"))
            {
                ImGui.SeparatorText("General");

                Rayman3.TimeAttack.IsActive = ImGuiExt.Checkbox("Active", Rayman3.TimeAttack.IsActive);
                Rayman3.TimeAttack.IsPaused = ImGuiExt.Checkbox("Paused", Rayman3.TimeAttack.IsPaused);
                ImGui.Text($"Mode: {Rayman3.TimeAttack.Mode}");
                ImGui.Text($"Timer: {Rayman3.TimeAttack.Timer}");

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

                        if (Engine.Input.IsMouseOnScreen(frame.Scene.RenderContext) && Engine.Input.IsMouseLeftButtonJustPressed())
                        {
                            Vector2 mousePos = Engine.Input.GetMousePosition(frame.Scene.RenderContext) + frame.Scene.Playfield.Camera.Position;
                            frame.Scene.KnotManager.AddActor(frame.Scene, new ActorResource
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
                                Model = TimeAttackActorModels.TimeFreezeItemActorModel,
                            }, GameObjectType.AlwaysActor);
                        }
                    }

                    if (ImGui.Button("Resurrect all"))
                    {
                        foreach (BaseActor actor in frame.Scene.Iterate<BaseActor>(IteratorFlags.AlwaysActor | IteratorFlags.Disabled, IteratorKnot.All))
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

                        bool first = true;
                        foreach (BaseActor actor in frame.Scene.Iterate<BaseActor>(IteratorFlags.AlwaysActor, IteratorKnot.All))
                        {
                            if ((ReadvancedActorType)actor.Type == ReadvancedActorType.TimeFreezeItem)
                            {
                                TimeFreezeItem timeFreezeItem = (TimeFreezeItem)actor;

                                int x = (short)timeFreezeItem.InitialPosition.X;
                                int y = (short)timeFreezeItem.InitialPosition.Y;
                                int time = timeFreezeItem.InitialAction == TimeFreezeItem.Action.Init_Decrease3 ? 3 : 5;

                                if (!first)
                                    sb.AppendLine();
                                sb.Append($"{{ \"time\": {time}, \"x\": {x}, \"y\": {y} }},");
                                first = false;
                            }
                        }

                        // Remove last comma
                        if (sb.Length > 0)
                            sb.Remove(sb.Length - 1, 1);

                        ImGui.SetClipboardText(sb.ToString());
                    }
                }

                ImGui.EndTabItem();
            }

            if (Frame.Current is GameMidlet &&
                ImGui.BeginTabItem("J2ME"))
            {
                Game game = GameMidlet.Instance_Game;

                if (ImGui.CollapsingHeader("Resource manager"))
                {
                    if (ImGui.Button("Dump all data"))
                    {
                        if (Engine.FileDialog.OpenFolder("Select output path") is { } outputPath)
                            game.RM.DumpAllData(outputPath);
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Dump all text"))
                    {
                        if (Engine.FileDialog.OpenFolder("Select output path") is { } outputPath)
                            game.RM.DumpAllText(outputPath);
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Dump all images"))
                    {
                        if (Engine.FileDialog.OpenFolder("Select output path") is { } outputPath)
                            game.RM.DumpAllImages(outputPath);
                    }

                    if (ImGui.BeginTable("_rm", 4))
                    {
                        ImGui.TableSetupColumn("Index", ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("Archive", ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("Id", ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableHeadersRow();

                        int globalIndex = 0;
                        for (int archiveIndex = 0; archiveIndex < ResourceManager.ARCHIVES_COUNT; archiveIndex++)
                        {
                            string name = game.RM.kArchive_Names[archiveIndex];
                            ArchiveInformation info = game.RM.Archive_Information[archiveIndex];

                            for (int i = 0; i < info.ImageResourcesCount + info.DataResourcesCount; i++)
                            {
                                bool isImage = i < info.ImageResourcesCount;

                                ImGui.TableNextRow();

                                uint color;
                                if ((game.RM.Resource_Status[archiveIndex][i] & RESOURCE_STATUS.LOADED) != 0)
                                    color = 0xff00ff40; // Green
                                else
                                    color = 0xffffffff; // White

                                ImGui.PushStyleColor(ImGuiCol.Text, color);

                                ImGui.TableNextColumn();
                                ImGui.Text($"{globalIndex}");

                                ImGui.TableNextColumn();
                                ImGui.Text($"{name}");

                                ImGui.TableNextColumn();
                                ImGui.Text(isImage ? "Image" : "Data");

                                ImGui.TableNextColumn();
                                ImGui.Text($"0x{ResourceId.Create(i, isImage ? RESOURCE_TYPE.IMAGE : RESOURCE_TYPE.DATA, archiveIndex):X8}");
                                
                                ImGui.PopStyleColor();

                                globalIndex++;
                            }
                        }

                        ImGui.EndTable();
                    }
                }

                if (ImGui.CollapsingHeader("General"))
                {
                    game.m_chGameState = ImGuiExt.EnumCombo("Game state", game.m_chGameState);
                }

                if (ImGui.CollapsingHeader("Camera"))
                {
                    ImGui.Text($"Focus actor: {game.pFocusActor?.objType.ToString() ?? "NULL"}");
                }

                if (ImGui.CollapsingHeader("GameFrame"))
                {
                    ImGui.Text($"Cages: {game.s_iCageOpened}/{game.s_iCageTotal}");
                    ImGui.Text($"Lums: {game.s_iLumsTaken}/{game.s_iLumsTotal}");
                    ImGui.Text($"Actor checkpoint: {game.pFocusActor?.objType.ToString() ?? "NULL"}");
                    game.m_gameFrame_prevState = ImGuiExt.EnumCombo("Previous state", game.m_gameFrame_prevState);
                    game.m_gameFrame_curState = ImGuiExt.EnumCombo("Current state", game.m_gameFrame_curState);
                    ImGui.Text($"State step: {game.m_gameStateStep}");
                    game.m_gameFrame_paused = ImGuiExt.Checkbox("Paused", game.m_gameFrame_paused);
                    ImGui.Text($"Previous level: {game.m_iPrevLevel}");
                    ImGui.Text($"Level: {game.m_gameFrame_curLevel}");
                    ImGui.Text($"Unlocked level: {game.m_gameFrame_unlockedLevel}");
                    ImGui.Text($"Levels count: {game.m_gameFrame_nbLevels}");
                    ImGui.Text($"Lives: {game.m_gameFrame_nLife}");
                    ImGui.Text($"Energy: {game.m_gameFrame_nEnergy}");
                }

                if (ImGui.CollapsingHeader("Input"))
                {
                    ImGui.Text($"Key delay: {game.lStartMillForKeyDelay}");
                    ImGui.Text($"Current key: {game.currentKey}");
                    ImGui.Text($"Pressed key: {game.pressedKey}");
                    ImGui.Text($"Released key: {game.releasedKey}");
                    ImGui.Text($"Check counter: {game.m_iKeyCheckCounter}");
                    ImGui.Text($"Real keycode: {game.realKeyCode}");
                    ImGui.Text($"Keys: {game.m_keys}");
                }

                if (ImGui.CollapsingHeader("Menu"))
                {
                    int volume = game.SoundVolume;
                    if (ImGui.SliderInt("Volume", ref volume, 0, 100))
                    {
                        game.SoundVolume = volume;
                        game.setSoundVolume(volume);
                    }
                    ImGui.Text($"Page: {game.m_gameMenu_idCurPage}");
                    ImGui.Text($"Selected item: {game.m_gameMenu_idCurSel}");
                    ImGui.Text($"Items count: {game.m_gameMenu_nItem}");
                }

                if (ImGui.CollapsingHeader("Playfield"))
                {
                    ImGui.Text($"Background size: {game.m_sBackgroundWidth}x{game.m_sBackgroundHeight}");
                    ImGui.Text($"Background position: {game.m_iBackgroundX}x{game.m_iBackgroundY}");
                    game.m_bBackgroundUsed = ImGuiExt.Checkbox("Background used", game.m_bBackgroundUsed);
                }

                if (ImGui.CollapsingHeader("Scene"))
                {
                    ImGui.Text($"Left to die: {game.s_iLeftToDie}");
                    ImGui.Text($"Active sectors: {String.Join(" ", game.m_sectors_activeSector)}");

                    if (ImGui.BeginTable("_actors", 6))
                    {
                        ImGui.TableSetupColumn("Index", ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("Position", ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("Speed", ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthFixed);
                        ImGui.TableSetupColumn("Mech model");
                        ImGui.TableHeadersRow();

                        for (int i = 0; i < game.actors.Length; i++)
                        {
                            Actor actor = game.actors[i];
                            ImGui.TableNextRow();

                            uint color;
                            if ((actor.stateFlag & ACTOR_STATE.DEAD) != 0)
                                color = 0xff0000ff; // Red
                            else if (game.m_sectors_activeSector.SelectMany(x => game.m_sectors_actorIds[x]).Contains((sbyte)i))
                                color = 0xff00ff40; // Green
                            else
                                color = 0xffffffff; // White

                            ImGui.PushStyleColor(ImGuiCol.Text, color);

                            ImGui.TableNextColumn();
                            ImGui.Text($"{i}");

                            ImGui.TableNextColumn();
                            ImGui.Text($"{actor.objType}");

                            ImGui.TableNextColumn();
                            ImGui.Text($"{actor.x / 256f:0.00} x {actor.y / 256f:0.00}");

                            ImGui.TableNextColumn();
                            ImGui.Text($"{actor.dx / 256f:0.00} x {actor.dy / 256f:0.00}");

                            ImGui.TableNextColumn();
                            ImGui.Text($"{actor.anim.curAction}");

                            ImGui.TableNextColumn();
                            ImGui.Text((actor.stateFlag & ACTOR_STATE.USE_MECH_MODEL) != 0 ? $"{actor.mmodel_type}" : String.Empty);

                            ImGui.PopStyleColor();
                        }

                        ImGui.EndTable();
                    }

                    ImGui.Text($"Always active: {game.m_actors_1stAlwaysActive}");
                }

                if (ImGui.CollapsingHeader("Sound"))
                {
                    if (ImGui.BeginTable("_songs", 2))
                    {
                        ImGui.TableSetupColumn("Sound");
                        ImGui.TableSetupColumn("State");
                        ImGui.TableHeadersRow();

                        foreach (MidiSoundInstance songInstance in game.SoundInstances)
                        {
                            ImGui.TableNextRow();

                            ImGui.TableNextColumn();
                            ImGui.Text($"{songInstance.SoundIndex}");

                            ImGui.TableNextColumn();
                            ImGui.Text($"{songInstance.State}");
                        }

                        ImGui.EndTable();
                    }
                }

                if (ImGui.CollapsingHeader("SysFrame"))
                {
                    game.curState = ImGuiExt.EnumCombo("State", game.curState);
                    ImGui.Text($"Loading state: {game.m_byMainLoadingState}");
                }

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }
}