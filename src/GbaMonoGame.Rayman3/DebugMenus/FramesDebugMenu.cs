using System;
using System.Linq;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Editor;
using GbaMonoGame.Rayman3.Readvanced;
using ImGuiNET;

namespace GbaMonoGame.Rayman3;

public class FramesDebugMenu : DebugMenu
{
    private FrameMenuItem[] Menu { get; } =
    [
        new("Intro", () => new Intro()),
        new("GameCubeMenu", () => new GameCubeMenu()),
        new("Game Over", () => new GameOver()),
        new("Level Select", () => new LevelSelect()),
        new("Animation Viewer", () => new AnimationViewer()),
        new("Credits", () => new Credits(false)),
        new("Modern Menu", null,
        [
            new("Language", () => new ModernMenuAll(InitialMenuPage.Language)),
            new("Game Mode", () => new ModernMenuAll(InitialMenuPage.GameMode)),
            new("Options", () => new ModernMenuAll(InitialMenuPage.Options)),
            new("Multiplayer", () => new ModernMenuAll(InitialMenuPage.Multiplayer)),
            new("Multiplayer Lost Connection", () => new ModernMenuAll(InitialMenuPage.MultiplayerLostConnection)),
            new("N-Gage First Page", () => new ModernMenuAll(InitialMenuPage.NGage_FirstPage)),
            new("Time Attack", () => new ModernMenuAll(InitialMenuPage.TimeAttack)),
        ]),
        new("Menu", null, 
        [
            new("Language", () => new MenuAll(InitialMenuPage.Language)),
            new("Game Mode", () => new MenuAll(InitialMenuPage.GameMode)),
            new("Options", () => new MenuAll(InitialMenuPage.Options)),
            new("Multiplayer", () => new MenuAll(InitialMenuPage.Multiplayer)),
            new("Multiplayer Lost Connection", () => new MenuAll(InitialMenuPage.MultiplayerLostConnection)),
            new("N-Gage First Page", () => new MenuAll(InitialMenuPage.NGage_FirstPage))
        ]),
        new("Story", null, 
        [
            new("NGage Splash Screens", () => new NGageSplashScreensAct()),
            new("Act #1", () => new Act1()),
            new("Act #2", () => new Act2()),
            new("Act #3", () => new Act3()),
            new("Act #4", () => new Act4()),
            new("Act #5", () => new Act5()),
            new("Act #6", () => new Act6())
        ]),
        new("Levels", null, 
            Enumerable.Range(0, (int)(MapId.WorldMap + 1)).
            Select(i => new FrameMenuItem(((MapId)i).ToString(), () =>
            {
                MapId mapId = (MapId)i;

                // New power levels have to have the previous map id set before loading
                GameInfo.MapId = mapId switch
                {
                    MapId.Power1 => MapId.WoodLight_M2,
                    MapId.Power2 => MapId.BossMachine,
                    MapId.Power3 => MapId.EchoingCaves_M2,
                    MapId.Power4 => MapId.BossRockAndLava,
                    MapId.Power5 => MapId.SanctuaryOfStoneAndFire_M3,
                    MapId.Power6 => MapId.BossScaleMan,
                    _ => GameInfo.MapId
                };

                // Create the level frame
                Frame frame = LevelFactory.Create(mapId);

                // Set the powers
                GameInfo.SetPowerBasedOnMap(mapId);

                return frame;
            }, EndWithSeparator: (MapId)i switch
            {
                MapId.SanctuaryOfBigTree_M2 => true,
                MapId.MarshAwakening2 => true,
                MapId.SanctuaryOfRockAndLava_M3 => true,
                MapId.BossFinal_M2 => true,
                MapId._1000Lums => true,
                MapId.ChallengeLyGCN => true,
                MapId.Power6 => true,
                _ => false
            })).
            ToArray()),
        new("Multiplayer", null, 
            Enumerable.Range(59, Rom.Platform == Platform.NGage ? 10 : 6).
            Select(i => new FrameMenuItem((MapId)i switch
            {
                // GBA
                MapId.GbaMulti_MissileRace when Rom.Platform == Platform.GBA => "Bumper Race",
                MapId.GbaMulti_MissileArena when Rom.Platform == Platform.GBA => "Bumper Arena",
                MapId.GbaMulti_TagWeb when Rom.Platform == Platform.GBA => "Web Tag",
                MapId.GbaMulti_TagSlide when Rom.Platform == Platform.GBA => "Slide Tag",
                MapId.GbaMulti_CatAndMouseSlide when Rom.Platform == Platform.GBA => "Steal n' Slide",
                MapId.GbaMulti_CatAndMouseSpider when Rom.Platform == Platform.GBA => "Steal n' Spider",

                // N-Gage
                MapId.NGageMulti_CaptureTheFlagMiddleGround when Rom.Platform == Platform.NGage => "Middle Ground",
                MapId.NGageMulti_CaptureTheFlagFloors when Rom.Platform == Platform.NGage => "Floors",
                MapId.NGageMulti_CaptureTheFlagOneForAll when Rom.Platform == Platform.NGage => "One for All",
                MapId.NGageMulti_CaptureTheFlagAllForOne when Rom.Platform == Platform.NGage => "All for One",
                MapId.NGageMulti_CaptureTheFlagTeamWork when Rom.Platform == Platform.NGage => "Team Work",
                MapId.NGageMulti_CaptureTheFlagTeamPlayer when Rom.Platform == Platform.NGage => "Team Player",
                MapId.NGageMulti_TagWeb when Rom.Platform == Platform.NGage => "Web Tag",
                MapId.NGageMulti_TagSlide when Rom.Platform == Platform.NGage => "Slide Tag",
                MapId.NGageMulti_CatAndMouseSlide when Rom.Platform == Platform.NGage => "Steal n' Slide",
                MapId.NGageMulti_CatAndMouseSpider when Rom.Platform == Platform.NGage => "Steal n' Spider",
                
                _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
            }, () =>
            {
                MapId mapId = (MapId)i;

                // Initialize multiplayer
                RSMultiplayer.UnInit();
                RSMultiplayer.Init();
                RSMultiplayer.MachineId = 0;

                if (Rom.Platform == Platform.NGage && mapId is MapId.NGageMulti_CaptureTheFlagMiddleGround or MapId.NGageMulti_CaptureTheFlagFloors)
                    RSMultiplayer.PlayersCount = 2;
                else
                    RSMultiplayer.PlayersCount = 4;

                RSMultiplayer.MubState = MubState.Connected;
                MultiplayerInfo.Init();

                MultiplayerInfo.MapId = (i - 1) % 2; // Hack

                // Set the game type
                MultiplayerInfo.SetGameType(mapId switch
                {
                    // GBA
                    MapId.GbaMulti_MissileRace when Rom.Platform == Platform.GBA => MultiplayerGameType.Missile,
                    MapId.GbaMulti_MissileArena when Rom.Platform == Platform.GBA => MultiplayerGameType.Missile,
                    MapId.GbaMulti_TagWeb when Rom.Platform == Platform.GBA => MultiplayerGameType.RayTag,
                    MapId.GbaMulti_TagSlide when Rom.Platform == Platform.GBA => MultiplayerGameType.RayTag,
                    MapId.GbaMulti_CatAndMouseSlide when Rom.Platform == Platform.GBA => MultiplayerGameType.CatAndMouse,
                    MapId.GbaMulti_CatAndMouseSpider when Rom.Platform == Platform.GBA => MultiplayerGameType.CatAndMouse,

                    // N-Gage
                    MapId.NGageMulti_CaptureTheFlagMiddleGround when Rom.Platform == Platform.NGage => MultiplayerGameType.CaptureTheFlag,
                    MapId.NGageMulti_CaptureTheFlagFloors when Rom.Platform == Platform.NGage => MultiplayerGameType.CaptureTheFlag,
                    MapId.NGageMulti_CaptureTheFlagOneForAll when Rom.Platform == Platform.NGage => MultiplayerGameType.CaptureTheFlag,
                    MapId.NGageMulti_CaptureTheFlagAllForOne when Rom.Platform == Platform.NGage => MultiplayerGameType.CaptureTheFlag,
                    MapId.NGageMulti_CaptureTheFlagTeamWork when Rom.Platform == Platform.NGage => MultiplayerGameType.CaptureTheFlag,
                    MapId.NGageMulti_CaptureTheFlagTeamPlayer when Rom.Platform == Platform.NGage => MultiplayerGameType.CaptureTheFlag,
                    MapId.NGageMulti_TagWeb when Rom.Platform == Platform.NGage => MultiplayerGameType.RayTag,
                    MapId.NGageMulti_TagSlide when Rom.Platform == Platform.NGage => MultiplayerGameType.RayTag,
                    MapId.NGageMulti_CatAndMouseSlide when Rom.Platform == Platform.NGage => MultiplayerGameType.CatAndMouse,
                    MapId.NGageMulti_CatAndMouseSpider when Rom.Platform == Platform.NGage => MultiplayerGameType.CatAndMouse,

                    _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
                });

                // Create the level frame
                Frame frame = LevelFactory.Create(mapId);

                if (MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
                {
                    MultiplayerInfo.CaptureTheFlagMode = mapId is MapId.NGageMulti_CaptureTheFlagTeamWork or MapId.NGageMulti_CaptureTheFlagTeamPlayer
                        ? CaptureTheFlagMode.Teams
                        : CaptureTheFlagMode.Solo;
                    ((FrameMultiCaptureTheFlag)frame).InitNewGame(
                        remainingTime: 300, 
                        targetFlagsCount: 5, 
                        mode: MultiplayerInfo.CaptureTheFlagMode);
                }

                // Set all powers
                GameInfo.EnablePower(Power.All);

                return frame;
            })).
            ToArray()),
        new("Level Editor", null, 
            GameInfo.Levels.
            Select((_, i) => new FrameMenuItem(((MapId)i).ToString(), () => new LevelEditor(i), EndWithSeparator: (MapId)i switch
            {
                MapId.SanctuaryOfBigTree_M2 => true,
                MapId.MarshAwakening2 => true,
                MapId.SanctuaryOfRockAndLava_M3 => true,
                MapId.BossFinal_M2 => true,
                MapId._1000Lums => true,
                MapId.ChallengeLyGCN => true,
                MapId.Power6 => true,
                MapId.WorldMap => true,
                _ => false
            })).
            ToArray())
    ];

    public override string Name => "Frames";

    private void DrawMenu(FrameMenuItem[] items)
    {
        foreach (FrameMenuItem menuItem in items)
        {
            if (menuItem.SubMenu != null)
            {
                if (ImGui.BeginMenu(menuItem.Name))
                {
                    DrawMenu(menuItem.SubMenu);
                    ImGui.EndMenu();
                }
            }
            else if (ImGui.MenuItem(menuItem.Name))
            {
                if (SoundEventsManager.IsLoaded)
                    SoundEventsManager.StopAllSongs();

                FrameManager.SetNextFrame(menuItem.CreateFrame());
            }

            if (menuItem.EndWithSeparator)
                ImGui.Separator();
        }
    }

    public override void Draw(DebugLayout debugLayout, DebugLayoutTextureManager textureManager)
    {
        DrawMenu(Menu);
    }

    private record FrameMenuItem(string Name, Func<Frame> CreateFrame, FrameMenuItem[] SubMenu = null, bool EndWithSeparator = false);
}