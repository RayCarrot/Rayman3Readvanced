using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public partial class MenuAll
{
    #region Constant Fields

    private const ushort MultiPakConnectedMessage = 0xace;

    #endregion

    #region Properties

    // Multi-pak
    public uint MultiplayerInititialGameTime { get; set; }
    public int MultiplayerPlayersOffsetY { get; set; }
    public bool ReturningFromMultiplayerGame { get; set; }
    public bool? IsMultiplayerConnected { get; set; }
    public byte MultiplayerConnectionTimer { get; set; }
    public byte MultiplayerLostConnectionTimer { get; set; }
    public uint LastConnectionTime { get; set; }
    public byte MultiplayerMapHighlightValue { get; set; }
    public bool HasProcessedPackets { get; set; }
    public int MultiplayerType { get; set; } // Int instead of enum since the order is different for GBA and N-Gage
    public int MultiplayerMapId { get; set; }

    public int MultiplayerTypeAnimationsCount => Rom.Platform switch
    {
        Platform.GBA => 3,
        Platform.NGage => 4,
        _ => throw new UnsupportedPlatformException()
    };

    // N-Gage
    public int SelectedHost { get; set; }
    public int HostsCount { get; set; }
    public bool FinishedSearchingForHosts { get; set; }
    public int CaptureTheFlagTargetFlagsCount { get; set; }
    public int CaptureTheFlagTargetTime { get; set; }
    public CaptureTheFlagMode CaptureTheFlagMode { get; set; }
    public int CaptureTheFlagSoloMode { get; set; }
    public int[] ArrowYPositions { get; } = [74, 107, 138, 140];

    // Single-pak
    public int SinglePakPlayersOffsetY { get; set; }
    public byte MultiplayerSinglePakConnectionTimer { get; set; }
    public byte field_0xe1 { get; set; } // TODO: Name

    #endregion

    #region Private Methods

    private void CheckForStartGame()
    {
        if (Rom.Platform == Platform.GBA)
        {
            for (int id = 0; id < RSMultiplayer.PlayersCount; id++)
            {
                if (RSMultiplayer.IsPacketPending(id))
                {
                    if (id != RSMultiplayer.MachineId)
                    {
                        ushort packet = RSMultiplayer.ReadPacket(id)[0];

                        if (packet == MultiPakConnectedMessage)
                        {
                            LastConnectionTime = GameTime.ElapsedFrames;
                        }
                        else if ((packet & 0xf000) == 0xd000)
                        {
                            MultiplayerInfo.InitialGameTime = (uint)(packet & 0x1ff);
                            MultiplayerManager.CacheData();

                            FinishedLyChallenge1 = (packet & 0x200) != 0;
                            FinishedLyChallenge2 = (packet & 0x400) != 0;
                            HasAllCages = (packet & 0x800) != 0;

                            NextStepAction = Step_InitializeTransitionToMultiplayerTypeSelection;
                            CurrentStepAction = Step_TransitionOutOfMultiplayerPlayerSelection;
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                            HasProcessedPackets = true;
                            SelectOption(0, false);
                        }
                    }

                    RSMultiplayer.ReleasePacket(id);
                }
            }

            IsMultiplayerConnected = true;
        }
        else if (Rom.Platform == Platform.NGage)
        {
            ushort packet = 0; // NOTE: Hardcoded for now

            if ((packet & 0xf000) == 0xd000)
            {
                MultiplayerInfo.InitialGameTime = (uint)(packet & 0x1ff);
                NextStepAction = Step_InitializeTransitionToMultiplayerTypeSelection;
                CurrentStepAction = Step_TransitionOutOfMultiplayerJoinedGamePlayerSelection;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                SelectOption(0, false);
            }
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    private void AnimateSelectedMultiplayerMapPalette()
    {
        // TODO: Implement
    }

    private void StartMultiplayerGame()
    {
        MultiplayerInfo.MapId = MultiplayerMapId;
        Random.SetSeed(MultiplayerInfo.InitialGameTime);

        switch (MultiplayerType)
        {
            case 0 when Rom.Platform == Platform.GBA:
            case 1 when Rom.Platform == Platform.NGage:
                MultiplayerInfo.SetGameType(MultiplayerGameType.RayTag);

                if (MultiplayerMapId == 0)
                    FrameManager.SetNextFrame(new FrameMultiTag(Rom.Platform switch
                    {
                        Platform.GBA => MapId.GbaMulti_TagWeb,
                        Platform.NGage => MapId.NGageMulti_TagWeb,
                        _ => throw new UnsupportedPlatformException()
                    }));
                else if (MultiplayerMapId == 1)
                    FrameManager.SetNextFrame(new FrameMultiTag(Rom.Platform switch
                    {
                        Platform.GBA => MapId.GbaMulti_TagSlide,
                        Platform.NGage => MapId.NGageMulti_TagSlide,
                        _ => throw new UnsupportedPlatformException()
                    }));
                break;

            case 1 when Rom.Platform == Platform.GBA:
            case 2 when Rom.Platform == Platform.NGage:
                MultiplayerInfo.SetGameType(MultiplayerGameType.CatAndMouse);

                if (MultiplayerMapId == 0)
                    FrameManager.SetNextFrame(new FrameMultiCatAndMouse(Rom.Platform switch
                    {
                        Platform.GBA => MapId.GbaMulti_CatAndMouseSlide,
                        Platform.NGage => MapId.NGageMulti_CatAndMouseSlide,
                        _ => throw new UnsupportedPlatformException()
                    }));
                else if (MultiplayerMapId == 1)
                    FrameManager.SetNextFrame(new FrameMultiCatAndMouse(Rom.Platform switch
                    {
                        Platform.GBA => MapId.GbaMulti_CatAndMouseSpider,
                        Platform.NGage => MapId.NGageMulti_CatAndMouseSpider,
                        _ => throw new UnsupportedPlatformException()
                    }));
                break;

            case 2 when Rom.Platform == Platform.GBA:
                MultiplayerInfo.SetGameType(MultiplayerGameType.Missile);

                throw new NotImplementedException("Not implemented loading multiplayer missile maps");
                break;

            case 0 when Rom.Platform == Platform.NGage:
                MultiplayerInfo.SetGameType(MultiplayerGameType.CaptureTheFlag);
                MultiplayerInfo.CaptureTheFlagMode = CaptureTheFlagMode;

                MapId mapId = MapId.NGageMulti_CaptureTheFlagMiddleGround + CaptureTheFlagSoloMode * 2 + (int)CaptureTheFlagMode * 4 + MultiplayerMapId;
                FrameManager.SetNextFrame(new FrameMultiCaptureTheFlag(mapId));
                break;
        }
    }

    #endregion

    #region Mode Selection Steps (GBA)

    private void Step_InitializeTransitionToMultiplayerModeSelection()
    {
        Anims.MultiplayerModeSelection.CurrentAnimation = Localization.LanguageUiIndex * 2;

        // Center sprites if English
        if (Localization.LanguageId == 0)
            Anims.MultiplayerModeSelection.ScreenPos = Anims.MultiplayerModeSelection.ScreenPos with { X = 86 };

        CurrentStepAction = Step_TransitionToMultiplayerModeSelection;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        
        IsLoadingMultiplayerMap = true;

        ResetStem();
        SetBackgroundPalette(1);
    }

    private void Step_TransitionToMultiplayerModeSelection()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, 8);

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_MultiplayerModeSelection;
        }

        AnimationPlayer.Play(Anims.MultiplayerModeSelection);
    }

    private void Step_MultiplayerModeSelection()
    {
        if (JoyPad.IsButtonJustPressed(GbaInput.Up) || JoyPad.IsButtonJustPressed(GbaInput.Down))
        {
            SelectOption(SelectedOption == 0 ? 1 : 0, true);
            Anims.MultiplayerModeSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + SelectedOption;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.B))
        {
            NextStepAction = Step_InitializeTransitionToGameMode;
            CurrentStepAction = Step_TransitionOutOfMultiplayerModeSelection;

            TransitionOutCursorAndStem();
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.A))
        {
            Anims.Cursor.CurrentAnimation = 16;

            NextStepAction = SelectedOption switch
            {
                0 => Step_InitializeTransitionToMultiplayerPlayerSelection,
                1 => Step_InitializeTransitionToMultiplayerSinglePak,
                _ => throw new Exception("Invalid multiplayer mode")
            };

            CurrentStepAction = Step_TransitionOutOfMultiplayerModeSelection;

            TransitionOutCursorAndStem();
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        }

        AnimationPlayer.Play(Anims.MultiplayerModeSelection);
    }

    private void Step_TransitionOutOfMultiplayerModeSelection()
    {
        TransitionValue += 4;

        if (TransitionValue <= 160)
        {
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, -4);
        }
        else if (TransitionValue >= 220)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        AnimationPlayer.Play(Anims.MultiplayerModeSelection);
    }

    #endregion

    #region Connection Selection (N-Gage)

    private void Step_InitializeTransitionToMultiplayerConnectionSelection()
    {
        Anims.MultiplayerConnectionSelection.CurrentAnimation = Localization.LanguageUiIndex * 2;

        // Center sprites if English
        if (Localization.LanguageId == 0)
            Anims.MultiplayerConnectionSelection.ScreenPos = Anims.MultiplayerConnectionSelection.ScreenPos with { X = 58 };

        CurrentStepAction = Step_TransitionToMultiplayerConnectionSelection;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);

        IsLoadingMultiplayerMap = true;

        ResetStem();
        SetBackgroundPalette(1);
    }

    private void Step_TransitionToMultiplayerConnectionSelection()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, 8);

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_MultiplayerConnectionSelection;
        }

        AnimationPlayer.Play(Anims.MultiplayerConnectionSelection);
    }

    private void Step_MultiplayerConnectionSelection()
    {
        if (JoyPad.IsButtonJustPressed(GbaInput.Up) || JoyPad.IsButtonJustPressed(GbaInput.Down))
        {
            SelectOption(SelectedOption == 0 ? 2 : 0, true);
            Anims.MultiplayerConnectionSelection.CurrentAnimation = Localization.LanguageUiIndex * 2 + SelectedOption / 2;
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.B))
        {
            NextStepAction = Step_InitializeTransitionToGameMode;
            CurrentStepAction = Step_TransitionOutOfMultiplayerConnectionSelection;

            TransitionOutCursorAndStem();
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.A))
        {
            Anims.Cursor.CurrentAnimation = 16;

            NextStepAction = SelectedOption switch
            {
                0 => Step_InitializeTransitionToMultiplayerHostedGamePlayerSelection,
                2 => Step_InitializeTransitionToMultiplayerJoinGame,
                _ => throw new Exception("Invalid multiplayer mode")
            };

            CurrentStepAction = Step_TransitionOutOfMultiplayerConnectionSelection;

            TransitionOutCursorAndStem();
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        }

        AnimationPlayer.Play(Anims.MultiplayerConnectionSelection);
    }

    private void Step_TransitionOutOfMultiplayerConnectionSelection()
    {
        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, -4);
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        AnimationPlayer.Play(Anims.MultiplayerConnectionSelection);
    }

    #endregion

    #region Join Game (N-Gage)

    private void Step_InitializeTransitionToMultiplayerJoinGame()
    {
        Anims.ArrowLeft.CurrentAnimation = 1;
        Anims.ArrowRight.CurrentAnimation = 0;

        CurrentStepAction = Step_TransitionToMultiplayerJoinGame;

        // NOTE: The game sets some global value here

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        SetBackgroundPalette(2);
        GameTime.Resume();

        // NOTE: The game inits searching for hosts here

        SelectedHost = -1;
        HostsCount = 0;
        FinishedSearchingForHosts = false;
        
        NGageSetMenuText(27, false, null, 0); // Looking for potential hosts
    }

    private void Step_TransitionToMultiplayerJoinGame()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, 8);

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_MultiplayerJoinGame;

            foreach (SpriteTextObject textObj in Anims.Texts)
                textObj.BgPriority = 0;
        }

        DrawText(false);
    }

    private void Step_MultiplayerJoinGame()
    {
        int hostsCount = 2; // NOTE: Hardcoded for now - the game gets this from a function call
        int selectedHost = SelectedHost;
        bool finishedSearchingForHosts = false; // NOTE: Hardcoded for now - the game gets this from a function call

        if (selectedHost == -1 && hostsCount > 0)
            SelectedHost = 0;

        if (JoyPad.IsButtonJustPressed(GbaInput.Left))
        {
            if (SelectedHost != -1)
            {
                SelectedHost--;

                if (SelectedHost < 0)
                    SelectedHost = 0;
            }
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Right))
        {
            if (SelectedHost != -1)
            {
                SelectedHost++;
                if (SelectedHost >= hostsCount)
                    SelectedHost = hostsCount - 1;
            }
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.A))
        {
            if (SelectedHost != -1)
            {
                // NOTE: The game sets the host to the selected one here
                NextStepAction = Step_InitializeTransitionToMultiplayerJoinedGamePlayerSelection;
                CurrentStepAction = Step_TransitionOutOfMultiplayerJoinGame;

                foreach (SpriteTextObject textObj in Anims.Texts)
                    textObj.BgPriority = 3;

                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                SelectOption(0, false);
            }
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.B))
        {
            // NOTE: The game deinits the connection here

            SelectOption(0, false);

            NextStepAction = Step_InitializeTransitionToMultiplayerConnectionSelection;
            CurrentStepAction = Step_TransitionOutOfMultiplayerJoinGame;

            foreach (SpriteTextObject textObj in Anims.Texts)
                textObj.BgPriority = 3;

            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        }

        // Update text
        if (SelectedHost != selectedHost || hostsCount != HostsCount || finishedSearchingForHosts && !FinishedSearchingForHosts)
        {
            if (hostsCount == 0)
            {
                NGageSetMenuText(30, false, null, 0); // No host found
            }
            else
            {
                string selectedHostName = $"Host {SelectedHost + 1}"; // NOTE: Hardcoded for now - the game gets this from a function call
                int textId = finishedSearchingForHosts 
                    ? 29  // %i host(s) found Select a host %s
                    : 28; // Looking for potential hosts %i host(s) found Select a host %s
                NGageSetMenuText(textId, false, null, 80, hostsCount, selectedHostName);
                HostsCount = hostsCount;
                FinishedSearchingForHosts = finishedSearchingForHosts;
            }
        }

        // This shouldn't be here - leftover?
        Anims.MultiplayerPlayerSelection.ScreenPos = Anims.MultiplayerPlayerSelection.ScreenPos with { Y = 60 - MultiplayerPlayersOffsetY };
        Anims.MultiplayerPlayerNumberIcons.ScreenPos = Anims.MultiplayerPlayerNumberIcons.ScreenPos with { Y = 42 - MultiplayerPlayersOffsetY };
        Anims.MultiplayerPlayerSelectionHighlight.ScreenPos = Anims.MultiplayerPlayerSelectionHighlight.ScreenPos with { Y = 46 - MultiplayerPlayersOffsetY };
        foreach (AnimatedObject obj in Anims.MultiplayerPlayerSelectionIcons)
            obj.ScreenPos = obj.ScreenPos with { Y = 69 - MultiplayerPlayersOffsetY };

        int stringsCount;
        if (finishedSearchingForHosts)
            stringsCount = Localization.GetText(TextBankId.Connectivity, 29).Length;
        else
            stringsCount = Localization.GetText(TextBankId.Connectivity, 28).Length;

        float arrowYPos = 58 + (96 + stringsCount * -16) / 2f + (stringsCount - 1) * 16;

        if (SelectedHost < 1)
            Anims.ArrowLeft.ScreenPos = Anims.ArrowLeft.ScreenPos with { X = 300 };
        else
            Anims.ArrowLeft.ScreenPos = new Vector2(68, arrowYPos);

        if (SelectedHost >= hostsCount - 1)
            Anims.ArrowRight.ScreenPos = Anims.ArrowRight.ScreenPos with { X = 300 };
        else
            Anims.ArrowRight.ScreenPos = new Vector2(152, arrowYPos);

        AnimationPlayer.Play(Anims.ArrowLeft);
        AnimationPlayer.Play(Anims.ArrowRight);

        DrawText(true);
    }

    private void Step_TransitionOutOfMultiplayerJoinGame()
    {
        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, -4);
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        DrawText(false);
    }

    #endregion

    #region Player Selection Steps (GBA)

    private void Step_InitializeTransitionToMultiplayerPlayerSelection()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerPlayersAnimations);

        Anims.MultiplayerPlayerSelection = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 32,
            ScreenPos = new Vector2(145, 40 - MultiplayerPlayersOffsetY),
            CurrentAnimation = 0,
            RenderContext = Playfield.RenderContext,
        };

        Anims.MultiplayerPlayerNumberIcons = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(102, 22 - MultiplayerPlayersOffsetY),
            CurrentAnimation = 4,
            RenderContext = Playfield.RenderContext,
        };

        Anims.MultiplayerPlayerSelectionIcons = new AnimatedObject[4];
        for (int i = 0; i < Anims.MultiplayerPlayerSelectionIcons.Length; i++)
        {
            Anims.MultiplayerPlayerSelectionIcons[i] = new AnimatedObject(resource, false)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 16,
                ScreenPos = new Vector2(104 + 24 * i, 49 - MultiplayerPlayersOffsetY),
                CurrentAnimation = 8,
                RenderContext = Playfield.RenderContext,
            };
        }

        Anims.MultiplayerPlayerSelectionHighlight = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(104, 26 - MultiplayerPlayersOffsetY),
            CurrentAnimation = 10,
            RenderContext = Playfield.RenderContext,
        };

        if (InitialPage == InitialMenuPage.Multiplayer)
        {
            for (int i = 0; i < 5; i++)
                Anims.Texts[i].Text = "";

            CurrentStepAction = Step_MultiplayerPlayerSelection;
            InitialPage = InitialMenuPage.Language;
            MultiplayerConnectionTimer = 30;
            LastConnectionTime = GameTime.ElapsedFrames;
            ReturningFromMultiplayerGame = true;
        }
        else
        {
            SetMenuText(0, false);
            CurrentStepAction = Step_TransitionToMultiplayerPlayerSelection;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
            ReturningFromMultiplayerGame = false;
        }

        SetBackgroundPalette(2);
        MultiplayerManager.Init();
        GameTime.Resume();

        MultiplayerType = 0;
        MultiplayerMapId = 0;
        PreviousTextId = 0;
    }

    private void Step_TransitionToMultiplayerPlayerSelection()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, 8);

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_MultiplayerPlayerSelection;
        }

        if (RSMultiplayer.MubState == MubState.Connected)
        {
            if (RSMultiplayer.PlayersCount > 1)
            {
                if (RSMultiplayer.IsMaster)
                    SetMenuText(2, true); // Press START
                else
                    SetMenuText(3, false); // Please Wait...

                Anims.MultiplayerPlayerNumberIcons.CurrentAnimation = 3 + RSMultiplayer.PlayersCount;

                Anims.MultiplayerPlayerSelectionHighlight.ScreenPos = Anims.MultiplayerPlayerSelectionHighlight.ScreenPos with { X = 104 + RSMultiplayer.MachineId * 24 };

                Anims.MultiplayerPlayerSelection.CurrentAnimation = RSMultiplayer.MachineId;
            }
            
            MultiplayerPlayersOffsetY -= 4;

            if (MultiplayerPlayersOffsetY < 0)
                MultiplayerPlayersOffsetY = 0;

            MultiplayerConnectionTimer = 30;
            IsMultiplayerConnected = true;
            LastConnectionTime = GameTime.ElapsedFrames;
        }
        else
        {
            if (MultiplayerPlayersOffsetY <= 70)
                MultiplayerPlayersOffsetY += 4;
            else
                MultiplayerPlayersOffsetY = 70;

            MultiplayerConnectionTimer = 0;
            IsMultiplayerConnected = null;
        }

        Anims.MultiplayerPlayerSelection.ScreenPos = Anims.MultiplayerPlayerSelection.ScreenPos with { Y = 40 - MultiplayerPlayersOffsetY };
        Anims.MultiplayerPlayerNumberIcons.ScreenPos = Anims.MultiplayerPlayerNumberIcons.ScreenPos with { Y = 22 - MultiplayerPlayersOffsetY };
        Anims.MultiplayerPlayerSelectionHighlight.ScreenPos = Anims.MultiplayerPlayerSelectionHighlight.ScreenPos with { Y = 26 - MultiplayerPlayersOffsetY };

        foreach (AnimatedObject obj in Anims.MultiplayerPlayerSelectionIcons)
            obj.ScreenPos = obj.ScreenPos with { Y = 49 - MultiplayerPlayersOffsetY };

        DrawText(false);
        AnimationPlayer.Play(Anims.MultiplayerPlayerSelection);
        AnimationPlayer.Play(Anims.MultiplayerPlayerNumberIcons);

        for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
            AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[i]);

        AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionHighlight);
    }

    private void Step_MultiplayerPlayerSelection()
    {
        RSMultiplayer.CheckForLostConnection();

        // Disconnected
        if (IsMultiplayerConnected == null)
        {
            MultiplayerLostConnectionTimer = 0;

            if (ReturningFromMultiplayerGame)
            {
                if (MultiplayerConnectionTimer == 20)
                {
                    if (PreviousTextId != 1)
                        SetMenuText(0, false);

                    PreviousTextId = 1;
                    MultiplayerConnectionTimer++;
                }
                else if (MultiplayerConnectionTimer > 20)
                {
                    if (MultiplayerPlayersOffsetY < 70)
                        MultiplayerPlayersOffsetY += 4;
                    else
                        MultiplayerPlayersOffsetY = 70;
                }
                else
                {
                    MultiplayerConnectionTimer++;
                }
            }
            else
            {
                if (MultiplayerConnectionTimer == 10)
                {
                    if (PreviousTextId != 1)
                        SetMenuText(0, false);

                    PreviousTextId = 1;
                    MultiplayerConnectionTimer++;
                }
                else if (MultiplayerConnectionTimer > 10)
                {
                    if (MultiplayerPlayersOffsetY < 70)
                        MultiplayerPlayersOffsetY += 4;
                    else
                        MultiplayerPlayersOffsetY = 70;
                }
                else
                {
                    MultiplayerConnectionTimer++;
                }
            }
        }
        // Lost connection
        else if (IsMultiplayerConnected == false)
        {
            if (MultiplayerLostConnectionTimer < 10)
            {
                MultiplayerLostConnectionTimer++;
            }
            else
            {
                IsMultiplayerConnected = null;
                MultiplayerConnectionTimer = 0;
                MultiplayerLostConnectionTimer = 0;
                RSMultiplayer.Reset();
                MultiplayerInititialGameTime = GameTime.ElapsedFrames;
            }
        }
        // Connected
        else if (RSMultiplayer.MubState == MubState.Connected)
        {
            MultiplayerLostConnectionTimer = 0;

            if (RSMultiplayer.PlayersCount > 1)
            {
                if (MultiplayerConnectionTimer < 30)
                {
                    MultiplayerConnectionTimer++;
                }
                else
                {
                    if (RSMultiplayer.IsMaster)
                    {
                        if (PreviousTextId != 2)
                            SetMenuText(2, true); // Press START

                        PreviousTextId = 2;
                    }
                    else
                    {
                        if (PreviousTextId != 3)
                            SetMenuText(3, false); // Please Wait...

                        PreviousTextId = 3;
                    }

                    Anims.MultiplayerPlayerNumberIcons.CurrentAnimation = 3 + RSMultiplayer.PlayersCount;

                    Anims.MultiplayerPlayerSelectionHighlight.ScreenPos = Anims.MultiplayerPlayerSelectionHighlight.ScreenPos with { X = 104 + RSMultiplayer.MachineId * 24 };

                    Anims.MultiplayerPlayerSelection.CurrentAnimation = RSMultiplayer.MachineId;

                    MultiplayerPlayersOffsetY -= 4;

                    if (MultiplayerPlayersOffsetY < 0)
                        MultiplayerPlayersOffsetY = 0;
                }
            }
        }
        else if (RSMultiplayer.MubState < MubState.Connected)
        {
            if (MultiplayerPlayersOffsetY < 70)
                MultiplayerPlayersOffsetY += 4;
            else
                MultiplayerPlayersOffsetY = 70;

            MultiplayerConnectionTimer = 30;
        }
        else if (RSMultiplayer.MubState > MubState.Connected)
        {
            if (MultiplayerPlayersOffsetY < 70)
                MultiplayerPlayersOffsetY += 4;
            else
                MultiplayerPlayersOffsetY = 70;
        }

        // Master
        if (RSMultiplayer.IsMaster)
        {
            if (RSMultiplayer.MubState == MubState.Connected)
            {
                CheckForStartGame();

                if (JoyPad.IsButtonJustPressed(GbaInput.Start))
                {
                    uint trimmedGameTime = GameTime.ElapsedFrames & 0x1ff;
                    
                    ushort packet = (ushort)trimmedGameTime;
                    packet |= 0xd000;

                    if (FinishedLyChallenge1)
                        packet |= 0x200; 
                    
                    if (FinishedLyChallenge2)
                        packet |= 0x400;

                    if (HasAllCages)
                        packet |= 0x800;

                    RSMultiplayer.SendPacket([packet]);
                    MultiplayerInfo.InitialGameTime = trimmedGameTime;
                    MultiplayerManager.CacheData();

                    NextStepAction = Step_InitializeTransitionToMultiplayerTypeSelection;
                    CurrentStepAction = Step_TransitionOutOfMultiplayerPlayerSelection;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                    HasProcessedPackets = true;
                    SelectOption(0, false);
                }
                else
                {
                    RSMultiplayer.SendPacket([MultiPakConnectedMessage]);
                }
            }
            else if (RSMultiplayer.MubState == MubState.EstablishConnections && RSMultiplayer.PlayersCount > 1)
            {
                RSMultiplayer.Connect();
                MultiplayerInititialGameTime = GameTime.ElapsedFrames;
            }
        }
        // Slave
        else if (RSMultiplayer.MachineId is >= 1 and <= 4) // TODO: Why is id 4 valid?
        {
            if (RSMultiplayer.MubState == MubState.Connected)
            {
                CheckForStartGame();
                RSMultiplayer.SendPacket([MultiPakConnectedMessage]);
            }
        }

        if (IsMultiplayerConnected == true && GameTime.ElapsedFrames - LastConnectionTime > 15)
            IsMultiplayerConnected = false;

        if (RSMultiplayer.MubState == MubState.EstablishConnections)
        {
            if ((!RSMultiplayer.IsSlave && GameTime.ElapsedFrames - MultiplayerInititialGameTime > 50) ||
                // TODO: Why is id 4 valid?
                (RSMultiplayer.MachineId is >= 1 and <= 4 && GameTime.ElapsedFrames - MultiplayerInititialGameTime > 55))
            {
                IsMultiplayerConnected = null;
                MultiplayerConnectionTimer = 0;
                MultiplayerLostConnectionTimer = 0;
                RSMultiplayer.Reset();
                MultiplayerInititialGameTime = GameTime.ElapsedFrames;
            }
        }
        else if (RSMultiplayer.MubState >= MubState.Error)
        {
            IsMultiplayerConnected = null;
            MultiplayerConnectionTimer = ReturningFromMultiplayerGame ? (byte)20 : (byte)10;
            MultiplayerLostConnectionTimer = 0;
            RSMultiplayer.Reset();
            MultiplayerInititialGameTime = GameTime.ElapsedFrames;
        }

        if (JoyPad.IsButtonJustPressed(GbaInput.B))
        {
            SelectOption(0, false);
            NextStepAction = Step_InitializeTransitionToMultiplayerModeSelection;
            CurrentStepAction = Step_TransitionOutOfMultiplayerPlayerSelection;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        }

        Anims.MultiplayerPlayerSelection.ScreenPos = Anims.MultiplayerPlayerSelection.ScreenPos with { Y = 40 - MultiplayerPlayersOffsetY };
        Anims.MultiplayerPlayerNumberIcons.ScreenPos = Anims.MultiplayerPlayerNumberIcons.ScreenPos with { Y = 22 - MultiplayerPlayersOffsetY };
        Anims.MultiplayerPlayerSelectionHighlight.ScreenPos = Anims.MultiplayerPlayerSelectionHighlight.ScreenPos with { Y = 26 - MultiplayerPlayersOffsetY };

        foreach (AnimatedObject obj in Anims.MultiplayerPlayerSelectionIcons)
            obj.ScreenPos = obj.ScreenPos with { Y = 49 - MultiplayerPlayersOffsetY };

        DrawText(false);
        AnimationPlayer.Play(Anims.MultiplayerPlayerSelection);
        AnimationPlayer.Play(Anims.MultiplayerPlayerNumberIcons);

        for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
            AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[i]);

        AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionHighlight);
    }

    private void Step_TransitionOutOfMultiplayerPlayerSelection()
    {
        TransitionValue += 4;

        if (TransitionValue <= 160)
        {
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, -4);
        }
        else if (TransitionValue >= 220)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        if (MultiplayerPlayersOffsetY <= 70)
            MultiplayerPlayersOffsetY += 4;
        else
            MultiplayerPlayersOffsetY = 70;

        Anims.MultiplayerPlayerSelection.ScreenPos = Anims.MultiplayerPlayerSelection.ScreenPos with { Y = 40 - MultiplayerPlayersOffsetY };
        Anims.MultiplayerPlayerNumberIcons.ScreenPos = Anims.MultiplayerPlayerNumberIcons.ScreenPos with { Y = 22 - MultiplayerPlayersOffsetY };

        foreach (AnimatedObject obj in Anims.MultiplayerPlayerSelectionIcons)
            obj.ScreenPos = obj.ScreenPos with { Y = 49 - MultiplayerPlayersOffsetY };

        AnimationPlayer.Play(Anims.MultiplayerPlayerSelection);
        AnimationPlayer.Play(Anims.MultiplayerPlayerNumberIcons);

        for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
            AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[i]);
    }

    #endregion

    #region Hosted Game Player Selection Steps (N-Gage)

    private void Step_InitializeTransitionToMultiplayerHostedGamePlayerSelection()
    {
        string hostName = "HostName"; // NOTE: Hardcoded for now

        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerPlayersAnimations);

        Anims.MultiplayerPlayerSelection = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 32,
            ScreenPos = new Vector2(113, 60 - MultiplayerPlayersOffsetY),
            CurrentAnimation = 0,
            RenderContext = Playfield.RenderContext,
        };

        Anims.MultiplayerPlayerNumberIcons = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(70, 42 - MultiplayerPlayersOffsetY),
            CurrentAnimation = 4,
            RenderContext = Playfield.RenderContext,
        };

        Anims.MultiplayerPlayerSelectionIcons = new AnimatedObject[4];
        for (int i = 0; i < Anims.MultiplayerPlayerSelectionIcons.Length; i++)
        {
            Anims.MultiplayerPlayerSelectionIcons[i] = new AnimatedObject(resource, false)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 16,
                ScreenPos = new Vector2(72 + 24 * i, 69 - MultiplayerPlayersOffsetY),
                CurrentAnimation = 8,
                RenderContext = Playfield.RenderContext,
            };
        }

        Anims.MultiplayerPlayerSelectionHighlight = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(72, 46 - MultiplayerPlayersOffsetY),
            CurrentAnimation = 10,
            RenderContext = Playfield.RenderContext,
        };

        NGageSetMenuText(25, false, 36, 256, hostName); // Please wait for connections on %s
        CurrentStepAction = Step_TransitionToMultiplayerHostedGamePlayerSelection;

        // NOTE: The game sets some global value here

        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        SetBackgroundPalette(2);

        MultiplayerManager.Init();

        // NOTE: The game does some connection stuff here

        GameTime.Resume();

        MultiplayerType = 0;
        MultiplayerMapId = 0;
        PreviousTextId = 0;
    }

    private void Step_TransitionToMultiplayerHostedGamePlayerSelection()
    {
        int[] playerConnectionStates = [4, 4, 4, 4]; // NOTE: Hardcoded for now

        TransitionValue += 4;

        if (TransitionValue <= 80)
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, 8);

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_MultiplayerHostedGamePlayerSelection;
        }

        if (playerConnectionStates[0] == 4)
        {
            Anims.MultiplayerPlayerNumberIcons.CurrentAnimation = 3 + RSMultiplayer.PlayersCount;

            Anims.MultiplayerPlayerSelectionHighlight.ScreenPos = Anims.MultiplayerPlayerSelectionHighlight.ScreenPos with { X = 72 + RSMultiplayer.MachineId * 24 };

            Anims.MultiplayerPlayerSelection.CurrentAnimation = RSMultiplayer.MachineId;
        }

        MultiplayerPlayersOffsetY -= 4;

        if (MultiplayerPlayersOffsetY < 0)
            MultiplayerPlayersOffsetY = 0;

        Anims.MultiplayerPlayerSelection.ScreenPos = Anims.MultiplayerPlayerSelection.ScreenPos with { Y = 60 - MultiplayerPlayersOffsetY };
        Anims.MultiplayerPlayerNumberIcons.ScreenPos = Anims.MultiplayerPlayerNumberIcons.ScreenPos with { Y = 42 - MultiplayerPlayersOffsetY };
        Anims.MultiplayerPlayerSelectionHighlight.ScreenPos = Anims.MultiplayerPlayerSelectionHighlight.ScreenPos with { Y = 46 - MultiplayerPlayersOffsetY };

        foreach (AnimatedObject obj in Anims.MultiplayerPlayerSelectionIcons)
            obj.ScreenPos = obj.ScreenPos with { Y = 69 - MultiplayerPlayersOffsetY };

        DrawText(false);

        AnimationPlayer.Play(Anims.MultiplayerPlayerSelection);
        AnimationPlayer.Play(Anims.MultiplayerPlayerNumberIcons);

        for (int id = 0; id < RSMultiplayer.PlayersCount; id++)
        {
            switch (playerConnectionStates[id])
            {
                case 1:
                    if ((GameTime.ElapsedFrames & 8) != 0)
                        AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[id]);
                    break;

                case 2:
                    if ((GameTime.ElapsedFrames & 4) != 0)
                        AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[id]);
                    break;

                case 3:
                case 4:
                    AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[id]);
                    break;
            }
        }

        AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionHighlight);
    }

    private void Step_MultiplayerHostedGamePlayerSelection()
    {
        int[] playerConnectionStates = [4, 4, 4, 4]; // NOTE: Hardcoded for now
        string hostName = "HostName"; // NOTE: Hardcoded for now

        if (playerConnectionStates[0] == 4)
        {
            Anims.MultiplayerPlayerNumberIcons.CurrentAnimation = 3 + RSMultiplayer.PlayersCount;

            Anims.MultiplayerPlayerSelectionHighlight.ScreenPos = Anims.MultiplayerPlayerSelectionHighlight.ScreenPos with { X = 72 + RSMultiplayer.MachineId * 24 };

            Anims.MultiplayerPlayerSelection.CurrentAnimation = RSMultiplayer.MachineId;
        }

        MultiplayerPlayersOffsetY -= 4;

        if (MultiplayerPlayersOffsetY < 0)
            MultiplayerPlayersOffsetY = 0;

        if (RSMultiplayer.PlayersCount > 1)
        {
            // NOTE: Hard-code conditions for now
            if (false)
            {
                NGageSetMenuText(26, false, 36, 100, hostName); // Press 5 when ready. Host name : %s

                if (JoyPad.IsButtonJustPressed(GbaInput.A))
                {
                    // NOTE: The game notifies other players that it's ready
                }
            }
            else if (false)
            {
                NGageSetMenuText(4, false, 36, 0); // Please Wait...
            }
            else if (true)
            {
                uint trimmedGameTime = GameTime.ElapsedFrames & 0x1FF;

                ushort packet = (ushort)trimmedGameTime;
                packet |= 0xd000;

                // NOTE: The game sends the packet

                MultiplayerInfo.InitialGameTime = trimmedGameTime;
                MultiplayerManager.CacheData();

                NextStepAction = Step_InitializeTransitionToMultiplayerTypeSelection;
                CurrentStepAction = Step_TransitionOutOfMultiplayerHostedGamePlayerSelection;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                HasProcessedPackets = true;
                SelectOption(0, false);
            }
        }

        if (JoyPad.IsButtonJustPressed(GbaInput.B))
        {
            // NOTE: The game deinits the connection here

            SelectOption(0, false);
            NextStepAction = Step_InitializeTransitionToMultiplayerConnectionSelection;
            CurrentStepAction = Step_TransitionOutOfMultiplayerHostedGamePlayerSelection;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        }
        else if (playerConnectionStates[0] != 4)
        {
            SelectOption(0, false);
            NextStepAction = Step_InitializeTransitionToMultiplayerConnectionSelection;
            CurrentStepAction = Step_TransitionOutOfMultiplayerHostedGamePlayerSelection;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        }

        Anims.MultiplayerPlayerSelection.ScreenPos = Anims.MultiplayerPlayerSelection.ScreenPos with { Y = 60 - MultiplayerPlayersOffsetY };
        Anims.MultiplayerPlayerNumberIcons.ScreenPos = Anims.MultiplayerPlayerNumberIcons.ScreenPos with { Y = 42 - MultiplayerPlayersOffsetY };
        Anims.MultiplayerPlayerSelectionHighlight.ScreenPos = Anims.MultiplayerPlayerSelectionHighlight.ScreenPos with { Y = 46 - MultiplayerPlayersOffsetY };

        foreach (AnimatedObject obj in Anims.MultiplayerPlayerSelectionIcons)
            obj.ScreenPos = obj.ScreenPos with { Y = 69 - MultiplayerPlayersOffsetY };

        DrawText(true);

        AnimationPlayer.Play(Anims.MultiplayerPlayerSelection);
        AnimationPlayer.Play(Anims.MultiplayerPlayerNumberIcons);

        for (int id = 0; id < RSMultiplayer.PlayersCount; id++)
        {
            switch (playerConnectionStates[id])
            {
                case 1:
                    if ((GameTime.ElapsedFrames & 8) != 0)
                        AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[id]);
                    break;

                case 2:
                    if ((GameTime.ElapsedFrames & 4) != 0)
                        AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[id]);
                    break;

                case 3:
                case 4:
                    AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[id]);
                    break;
            }
        }

        AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionHighlight);
    }

    private void Step_TransitionOutOfMultiplayerHostedGamePlayerSelection()
    {
        int[] playerConnectionStates = [4, 4, 4, 4]; // NOTE: Hardcoded for now

        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, -4);
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        if (MultiplayerPlayersOffsetY <= 100)
            MultiplayerPlayersOffsetY += 4;
        else
            MultiplayerPlayersOffsetY = 100;

        Anims.MultiplayerPlayerSelection.ScreenPos = Anims.MultiplayerPlayerSelection.ScreenPos with { Y = 60 - MultiplayerPlayersOffsetY };
        Anims.MultiplayerPlayerNumberIcons.ScreenPos = Anims.MultiplayerPlayerNumberIcons.ScreenPos with { Y = 42 - MultiplayerPlayersOffsetY };

        foreach (AnimatedObject obj in Anims.MultiplayerPlayerSelectionIcons)
            obj.ScreenPos = obj.ScreenPos with { Y = 69 - MultiplayerPlayersOffsetY };

        AnimationPlayer.Play(Anims.MultiplayerPlayerSelection);
        AnimationPlayer.Play(Anims.MultiplayerPlayerNumberIcons);

        for (int id = 0; id < RSMultiplayer.PlayersCount; id++)
        {
            switch (playerConnectionStates[id])
            {
                case 1:
                    if ((GameTime.ElapsedFrames & 8) != 0)
                        AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[id]);
                    break;

                case 2:
                    if ((GameTime.ElapsedFrames & 4) != 0)
                        AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[id]);
                    break;

                case 3:
                case 4:
                    AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[id]);
                    break;
            }
        }
    }

    #endregion

    #region Joined Game Player Selection Steps (N-Gage)

    private void Step_InitializeTransitionToMultiplayerJoinedGamePlayerSelection()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerPlayersAnimations);

        Anims.MultiplayerPlayerSelection = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 32,
            ScreenPos = new Vector2(113, 60 - MultiplayerPlayersOffsetY),
            CurrentAnimation = 0,
            RenderContext = Playfield.RenderContext,
        };

        Anims.MultiplayerPlayerNumberIcons = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(70, 42 - MultiplayerPlayersOffsetY),
            CurrentAnimation = 4,
            RenderContext = Playfield.RenderContext,
        };

        Anims.MultiplayerPlayerSelectionIcons = new AnimatedObject[4];
        for (int i = 0; i < Anims.MultiplayerPlayerSelectionIcons.Length; i++)
        {
            Anims.MultiplayerPlayerSelectionIcons[i] = new AnimatedObject(resource, false)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 16,
                ScreenPos = new Vector2(72 + 24 * i, 69 - MultiplayerPlayersOffsetY),
                CurrentAnimation = 8,
                RenderContext = Playfield.RenderContext,
            };
        }

        Anims.MultiplayerPlayerSelectionHighlight = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(72, 46 - MultiplayerPlayersOffsetY),
            CurrentAnimation = 10,
            RenderContext = Playfield.RenderContext,
        };

        NGageSetMenuText(31, false, null, 0); // Connecting
        CurrentStepAction = Step_TransitionToMultiplayerJoinedGamePlayerSelection;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        SetBackgroundPalette(2);

        MultiplayerManager.Init();
        GameTime.Resume();

        MultiplayerType = 0;
        MultiplayerMapId = 0;
        PreviousTextId = 0;
    }

    private void Step_TransitionToMultiplayerJoinedGamePlayerSelection()
    {
        int[] playerConnectionStates = [4, 4, 4, 4]; // NOTE: Hardcoded for now

        TransitionValue += 4;

        if (TransitionValue <= 80)
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, 8);

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_MultiplayerJoinedGamePlayerSelection;
        }

        if (playerConnectionStates[0] == 4)
        {
            NGageSetMenuText(33, false, null, 0); // Please wait

            if (RSMultiplayer.PlayersCount > 1)
            {
                NGageSetMenuText(4, false, null, 0); // Please Wait...

                Anims.MultiplayerPlayerNumberIcons.CurrentAnimation = 3 + RSMultiplayer.PlayersCount;

                Anims.MultiplayerPlayerSelectionHighlight.ScreenPos = Anims.MultiplayerPlayerSelectionHighlight.ScreenPos with { X = 72 + RSMultiplayer.MachineId * 24 };

                Anims.MultiplayerPlayerSelection.CurrentAnimation = RSMultiplayer.MachineId;
            }

            Anims.MultiplayerPlayerSelection.ScreenPos = Anims.MultiplayerPlayerSelection.ScreenPos with { Y = 60 - MultiplayerPlayersOffsetY };
            Anims.MultiplayerPlayerNumberIcons.ScreenPos = Anims.MultiplayerPlayerNumberIcons.ScreenPos with { Y = 42 - MultiplayerPlayersOffsetY };
            Anims.MultiplayerPlayerSelectionHighlight.ScreenPos = Anims.MultiplayerPlayerSelectionHighlight.ScreenPos with { Y = 46 - MultiplayerPlayersOffsetY };

            foreach (AnimatedObject obj in Anims.MultiplayerPlayerSelectionIcons)
                obj.ScreenPos = obj.ScreenPos with { Y = 69 - MultiplayerPlayersOffsetY };

            AnimationPlayer.Play(Anims.MultiplayerPlayerSelection);
            AnimationPlayer.Play(Anims.MultiplayerPlayerNumberIcons);

            for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
                AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[i]);

            AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionHighlight);

            MultiplayerPlayersOffsetY -= 4;

            if (MultiplayerPlayersOffsetY < 0)
                MultiplayerPlayersOffsetY = 0;
        }
        else
        {
            if (MultiplayerPlayersOffsetY <= 100)
                MultiplayerPlayersOffsetY += 4;
            else
                MultiplayerPlayersOffsetY = 100;
        }

        DrawText(false);
    }

    private void Step_MultiplayerJoinedGamePlayerSelection()
    {
        int[] playerConnectionStates = [4, 4, 4, 4]; // NOTE: Hardcoded for now

        switch (playerConnectionStates[0])
        {
            case 1:
                NGageSetMenuText(32, false, null, 0); // Connected. Wait for host to start a game.
                break;

            case 2:
                NGageSetMenuText(31, false, null, 0); // Connecting
                break;

            case 3:
            case 4:
                NGageSetMenuText(33, false, null, 0); // Please wait
                break;
        }

        if (playerConnectionStates[0] == 4)
        {
            Anims.MultiplayerPlayerNumberIcons.CurrentAnimation = 3 + RSMultiplayer.PlayersCount;

            Anims.MultiplayerPlayerSelectionHighlight.ScreenPos = Anims.MultiplayerPlayerSelectionHighlight.ScreenPos with { X = 72 + RSMultiplayer.MachineId * 24 };

            Anims.MultiplayerPlayerSelection.CurrentAnimation = RSMultiplayer.MachineId;

            Anims.MultiplayerPlayerSelection.ScreenPos = Anims.MultiplayerPlayerSelection.ScreenPos with { Y = 60 - MultiplayerPlayersOffsetY };
            Anims.MultiplayerPlayerNumberIcons.ScreenPos = Anims.MultiplayerPlayerNumberIcons.ScreenPos with { Y = 42 - MultiplayerPlayersOffsetY };
            Anims.MultiplayerPlayerSelectionHighlight.ScreenPos = Anims.MultiplayerPlayerSelectionHighlight.ScreenPos with { Y = 46 - MultiplayerPlayersOffsetY };

            foreach (AnimatedObject obj in Anims.MultiplayerPlayerSelectionIcons)
                obj.ScreenPos = obj.ScreenPos with { Y = 69 - MultiplayerPlayersOffsetY };

            AnimationPlayer.Play(Anims.MultiplayerPlayerSelection);
            AnimationPlayer.Play(Anims.MultiplayerPlayerNumberIcons);

            for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
                AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[i]);

            AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionHighlight);

            MultiplayerPlayersOffsetY -= 4;

            if (MultiplayerPlayersOffsetY < 0)
                MultiplayerPlayersOffsetY = 0;

            CheckForStartGame();
        }
        else
        {
            if (MultiplayerPlayersOffsetY <= 100)
                MultiplayerPlayersOffsetY += 4;
            else
                MultiplayerPlayersOffsetY = 100;
        }

        // Go back
        if (JoyPad.IsButtonJustPressed(GbaInput.B))
        {
            // NOTE: The game deinits the connection here

            SelectOption(0, false);

            NextStepAction = Step_InitializeTransitionToMultiplayerConnectionSelection;
            CurrentStepAction = Step_TransitionOutOfMultiplayerJoinedGamePlayerSelection;

            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        }
        // Lost connection
        else if (playerConnectionStates[0] == 0)
        {
            SelectOption(0, false);

            NextStepAction = Step_InitializeTransitionToMultiplayerLostConnection;
            CurrentStepAction = Step_TransitionOutOfMultiplayerJoinedGamePlayerSelection;

            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        }

        DrawText(true);
    }

    private void Step_TransitionOutOfMultiplayerJoinedGamePlayerSelection()
    {
        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, -4);
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        if (MultiplayerPlayersOffsetY <= 100)
            MultiplayerPlayersOffsetY += 4;
        else
            MultiplayerPlayersOffsetY = 100;

        Anims.MultiplayerPlayerSelection.ScreenPos = Anims.MultiplayerPlayerSelection.ScreenPos with { Y = 60 - MultiplayerPlayersOffsetY };
        Anims.MultiplayerPlayerNumberIcons.ScreenPos = Anims.MultiplayerPlayerNumberIcons.ScreenPos with { Y = 42 - MultiplayerPlayersOffsetY };

        foreach (AnimatedObject obj in Anims.MultiplayerPlayerSelectionIcons)
            obj.ScreenPos = obj.ScreenPos with { Y = 69 - MultiplayerPlayersOffsetY };

        AnimationPlayer.Play(Anims.MultiplayerPlayerSelection);
        AnimationPlayer.Play(Anims.MultiplayerPlayerNumberIcons);

        for (int i = 0; i < RSMultiplayer.PlayersCount; i++)
            AnimationPlayer.Play(Anims.MultiplayerPlayerSelectionIcons[i]);
    }

    #endregion

    #region Type Selection Steps

    private void Step_InitializeTransitionToMultiplayerTypeSelection()
    {
        Anims.MultiplayerTypeName.CurrentAnimation = MultiplayerType + Localization.LanguageUiIndex * MultiplayerTypeAnimationsCount;
        Anims.MultiplayerTypeFrame.CurrentAnimation = 2;
        Anims.ArrowLeft.CurrentAnimation = 1;
        Anims.ArrowRight.CurrentAnimation = 0;
        Anims.MultiplayerTypeIcon.CurrentAnimation = MultiplayerType;

        if (Rom.Platform == Platform.NGage)
        {
            ShouldTextBlink = true;
            string text = Localization.GetText(TextBankId.Connectivity, 34)[0]; // Please wait
            int width = FontManager.GetStringWidth(FontSize.Font16, text);
            Anims.Texts[4].ScreenPos = new Vector2(108 - width / 2f, 110);
            Anims.Texts[4].Text = text;
        }

        CurrentStepAction = Step_TransitionToMultiplayerTypeSelection;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        MultiplayerPlayersOffsetY = 112;
        SetBackgroundPalette(0);
    }

    private void Step_TransitionToMultiplayerTypeSelection()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, 8);

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_MultiplayerTypeSelection;
            GameTime.Resume();
        }

        MultiplayerPlayersOffsetY -= 4;

        if (MultiplayerPlayersOffsetY < 0)
            MultiplayerPlayersOffsetY = 0;

        if (Rom.Platform == Platform.GBA)
        {
            Anims.MultiplayerTypeFrame.ScreenPos = Anims.MultiplayerTypeFrame.ScreenPos with { Y = 35 - MultiplayerPlayersOffsetY };
            Anims.ArrowLeft.ScreenPos = Anims.ArrowLeft.ScreenPos with { Y = 50 - MultiplayerPlayersOffsetY };
            Anims.ArrowRight.ScreenPos = Anims.ArrowRight.ScreenPos with { Y = 50 - MultiplayerPlayersOffsetY };
            Anims.MultiplayerTypeIcon.ScreenPos = Anims.MultiplayerTypeIcon.ScreenPos with { Y = 24 - MultiplayerPlayersOffsetY };
        }
        else if (Rom.Platform == Platform.NGage)
        {
            Anims.MultiplayerTypeFrame.ScreenPos = Anims.MultiplayerTypeFrame.ScreenPos with { Y = 65 - MultiplayerPlayersOffsetY };
            Anims.ArrowLeft.ScreenPos = Anims.ArrowLeft.ScreenPos with { Y = 80 - MultiplayerPlayersOffsetY };
            Anims.ArrowRight.ScreenPos = Anims.ArrowRight.ScreenPos with { Y = 80 - MultiplayerPlayersOffsetY };
            Anims.MultiplayerTypeIcon.ScreenPos = Anims.MultiplayerTypeIcon.ScreenPos with { Y = 54 - MultiplayerPlayersOffsetY };
        }
        else
        {
            throw new UnsupportedPlatformException();
        }

        if (Rom.Platform == Platform.GBA && TransitionValue == 152 && HasProcessedPackets)
        {
            MultiplayerManager.DiscardPendingPackets();
            HasProcessedPackets = false;
        }

        AnimationPlayer.Play(Anims.MultiplayerTypeName);
        
        Anims.MultiplayerTypeFrame.FrameChannelSprite();
        if (Rom.Platform == Platform.GBA)
            AnimationPlayer.Play(Anims.MultiplayerTypeFrame);

        int arrowLeftPosX = Rom.Platform switch
        {
            Platform.GBA => 100,
            Platform.NGage => 68,
            _ => throw new UnsupportedPlatformException()
        };
        if (MultiplayerType == 0)
            Anims.ArrowLeft.ScreenPos = Anims.ArrowLeft.ScreenPos with { X = 300 };
        else
            Anims.ArrowLeft.ScreenPos = Anims.ArrowLeft.ScreenPos with { X = arrowLeftPosX };

        int arrowRightPosX = Rom.Platform switch
        {
            Platform.GBA => 184,
            Platform.NGage => 152,
            _ => throw new UnsupportedPlatformException()
        };
        if (MultiplayerType == 2)
            Anims.ArrowRight.ScreenPos = Anims.ArrowRight.ScreenPos with { X = 300 };
        else
            Anims.ArrowRight.ScreenPos = Anims.ArrowRight.ScreenPos with { X = arrowRightPosX };

        AnimationPlayer.Play(Anims.ArrowLeft);
        AnimationPlayer.Play(Anims.ArrowRight);
        AnimationPlayer.Play(Anims.MultiplayerTypeIcon);

        // NOTE: Hard-code for now
        if (Rom.Platform == Platform.NGage && false)
        {
            if (!ShouldTextBlink || (GameTime.ElapsedFrames & 0x10) != 0)
                AnimationPlayer.Play(Anims.Texts[4]);
        }
    }

    private void Step_MultiplayerTypeSelection()
    {
        MubState state = MultiplayerManager.Step();

        if (state == MubState.Connected)
        {
            if (MultiplayerManager.HasReadJoyPads())
            {
                GameTime.Resume();

                if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.Left) && MultiplayerType != 0)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    MultiplayerType--;
                    Anims.MultiplayerTypeName.CurrentAnimation = MultiplayerType + Localization.LanguageUiIndex * MultiplayerTypeAnimationsCount;
                    Anims.MultiplayerTypeIcon.CurrentAnimation = MultiplayerType;
                }

                if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.Right) && MultiplayerType != 2)
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    MultiplayerType++;
                    Anims.MultiplayerTypeName.CurrentAnimation = MultiplayerType + Localization.LanguageUiIndex * MultiplayerTypeAnimationsCount;
                    Anims.MultiplayerTypeIcon.CurrentAnimation = MultiplayerType;
                }
                else if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.A))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);

                    if (Rom.Platform == Platform.NGage && MultiplayerType == 0)
                        NextStepAction = Step_InitializeTransitionToMultiplayerFlagOptions;
                    else
                        NextStepAction = Step_InitializeTransitionToMultiplayerMapSelection;

                    CurrentStepAction = Step_TransitionOutOfMultiplayerTypeSelection;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                }
                else if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.B))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);

                    if (Rom.Platform == Platform.GBA)
                        NextStepAction = Step_InitializeTransitionToMultiplayerPlayerSelection;
                    else if (Rom.Platform == Platform.NGage)
                        NextStepAction = Step_InitializeTransitionToMultiplayerConnectionSelection;
                    else
                        throw new UnsupportedPlatformException();
                    
                    CurrentStepAction = Step_TransitionOutOfMultiplayerTypeSelection;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                }

                MultiplayerManager.FrameProcessed();
            }
            else
            {
                GameTime.Pause();
            }

            AnimationPlayer.Play(Anims.MultiplayerTypeName);

            Anims.MultiplayerTypeFrame.FrameChannelSprite();
            if (Rom.Platform == Platform.GBA)
                AnimationPlayer.Play(Anims.MultiplayerTypeFrame);

            int arrowLeftPosX = Rom.Platform switch
            {
                Platform.GBA => 100,
                Platform.NGage => 68,
                _ => throw new UnsupportedPlatformException()
            };
            if (MultiplayerType == 0)
                Anims.ArrowLeft.ScreenPos = Anims.ArrowLeft.ScreenPos with { X = 300 };
            else
                Anims.ArrowLeft.ScreenPos = Anims.ArrowLeft.ScreenPos with { X = arrowLeftPosX };

            int arrowRightPosX = Rom.Platform switch
            {
                Platform.GBA => 184,
                Platform.NGage => 152,
                _ => throw new UnsupportedPlatformException()
            };
            if (MultiplayerType == 2)
                Anims.ArrowRight.ScreenPos = Anims.ArrowRight.ScreenPos with { X = 300 };
            else
                Anims.ArrowRight.ScreenPos = Anims.ArrowRight.ScreenPos with { X = arrowRightPosX };

            AnimationPlayer.Play(Anims.ArrowLeft);
            AnimationPlayer.Play(Anims.ArrowRight);
            AnimationPlayer.Play(Anims.MultiplayerTypeIcon);

            // NOTE: Hard-code for now
            if (Rom.Platform == Platform.NGage && false)
            {
                if (!ShouldTextBlink || (GameTime.ElapsedTotalFrames & 0x10) != 0)
                    AnimationPlayer.Play(Anims.Texts[4]);
            }

            // NOTE: Hard-code for now
            if (Rom.Platform == Platform.NGage && false)
            {
                NextStepAction = Step_InitializeTransitionToMultiplayerConnectionSelection;
                CurrentStepAction = Step_TransitionOutOfMultiplayerTypeSelection;
            }
        }
        else
        {
            if (Rom.Platform == Platform.GBA)
            {
                NextStepAction = Step_InitializeTransitionToMultiplayerPlayerSelection;
            }
            else if (Rom.Platform == Platform.NGage)
            {
                GameTime.Resume();
                NextStepAction = Step_InitializeTransitionToMultiplayerLostConnection;
            }
            else
            {
                throw new UnsupportedPlatformException();
            }

            CurrentStepAction = Step_TransitionOutOfMultiplayerTypeSelection;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        }
    }

    private void Step_TransitionOutOfMultiplayerTypeSelection()
    {
        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, -4);
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        MultiplayerPlayersOffsetY += 4;

        if (MultiplayerPlayersOffsetY > 112)
            MultiplayerPlayersOffsetY = 112;

        if (Rom.Platform == Platform.GBA)
        {
            Anims.MultiplayerTypeFrame.ScreenPos = Anims.MultiplayerTypeFrame.ScreenPos with { Y = 35 - MultiplayerPlayersOffsetY };
            Anims.ArrowLeft.ScreenPos = Anims.ArrowLeft.ScreenPos with { Y = 50 - MultiplayerPlayersOffsetY };
            Anims.ArrowRight.ScreenPos = Anims.ArrowRight.ScreenPos with { Y = 50 - MultiplayerPlayersOffsetY };
            Anims.MultiplayerTypeIcon.ScreenPos = Anims.MultiplayerTypeIcon.ScreenPos with { Y = 24 - MultiplayerPlayersOffsetY };
        }
        else if (Rom.Platform == Platform.NGage)
        {
            Anims.MultiplayerTypeFrame.ScreenPos = Anims.MultiplayerTypeFrame.ScreenPos with { Y = 65 - MultiplayerPlayersOffsetY };
            Anims.ArrowLeft.ScreenPos = Anims.ArrowLeft.ScreenPos with { Y = 80 - MultiplayerPlayersOffsetY };
            Anims.ArrowRight.ScreenPos = Anims.ArrowRight.ScreenPos with { Y = 80 - MultiplayerPlayersOffsetY };
            Anims.MultiplayerTypeIcon.ScreenPos = Anims.MultiplayerTypeIcon.ScreenPos with { Y = 54 - MultiplayerPlayersOffsetY };
        }
        else
        {
            throw new UnsupportedPlatformException();
        }

        AnimationPlayer.Play(Anims.MultiplayerTypeName);

        Anims.MultiplayerTypeFrame.FrameChannelSprite();
        if (Rom.Platform == Platform.GBA)
            AnimationPlayer.Play(Anims.MultiplayerTypeFrame);

        int arrowLeftPosX = Rom.Platform switch
        {
            Platform.GBA => 100,
            Platform.NGage => 68,
            _ => throw new UnsupportedPlatformException()
        };
        if (MultiplayerType == 0)
            Anims.ArrowLeft.ScreenPos = Anims.ArrowLeft.ScreenPos with { X = 300 };
        else
            Anims.ArrowLeft.ScreenPos = Anims.ArrowLeft.ScreenPos with { X = arrowLeftPosX };

        int arrowRightPosX = Rom.Platform switch
        {
            Platform.GBA => 184,
            Platform.NGage => 152,
            _ => throw new UnsupportedPlatformException()
        };
        if (MultiplayerType == 2)
            Anims.ArrowRight.ScreenPos = Anims.ArrowRight.ScreenPos with { X = 300 };
        else
            Anims.ArrowRight.ScreenPos = Anims.ArrowRight.ScreenPos with { X = arrowRightPosX };

        AnimationPlayer.Play(Anims.ArrowLeft);
        AnimationPlayer.Play(Anims.ArrowRight);
        AnimationPlayer.Play(Anims.MultiplayerTypeIcon);

        // NOTE: Hard-code for now
        if (Rom.Platform == Platform.NGage && false)
        {
            if (!ShouldTextBlink || (GameTime.ElapsedFrames & 0x10) != 0)
                AnimationPlayer.Play(Anims.Texts[4]);
        }
    }

    #endregion

    #region Map Selection Steps

    private void Step_InitializeTransitionToMultiplayerMapSelection()
    {
        MultiplayerMapId = 0;

        if (Rom.Platform == Platform.GBA)
        {
            Anims.MultiplayerMapSelection.CurrentAnimation = MultiplayerType;

            Anims.MultiplayerMapName1.Text = Localization.GetText(TextBankId.Connectivity, 9 + MultiplayerType * 2)[0];
            Anims.MultiplayerMapName1.ScreenPos = new Vector2(
                x: 140 - Anims.MultiplayerMapName1.GetStringWidth() / 2f,
                y: 56);

            Anims.MultiplayerMapName2.Text = Localization.GetText(TextBankId.Connectivity, 10 + MultiplayerType * 2)[0];
            Anims.MultiplayerMapName2.ScreenPos = new Vector2(
                x: 140 - Anims.MultiplayerMapName2.GetStringWidth() / 2f,
                y: 96);
        }
        else if (Rom.Platform == Platform.NGage)
        {
            if (MultiplayerType == 0)
                Anims.MultiplayerMapSelection.CurrentAnimation = 8 + (int)CaptureTheFlagMode * 4 + CaptureTheFlagSoloMode * 2;
            else
                Anims.MultiplayerMapSelection.CurrentAnimation = MultiplayerType - 1;

            if (MultiplayerType == 0)
            {
                Anims.MultiplayerMapName1.Text = Localization.GetText(TextBankId.Connectivity, 10 + (int)CaptureTheFlagMode * 4 + CaptureTheFlagSoloMode * 2)[0];
                Anims.MultiplayerMapName1.ScreenPos = new Vector2(
                    x: 108 - Anims.MultiplayerMapName1.GetStringWidth() / 2f,
                    y: 84);

                Anims.MultiplayerMapName2.Text = Localization.GetText(TextBankId.Connectivity, 11 + (int)CaptureTheFlagMode * 4 + CaptureTheFlagSoloMode * 2)[0];
                Anims.MultiplayerMapName2.ScreenPos = new Vector2(
                    x: 108 - Anims.MultiplayerMapName2.GetStringWidth() / 2f,
                    y: 124);
            }
            else
            {
                Anims.MultiplayerMapName1.Text = Localization.GetText(TextBankId.Connectivity, 14 + MultiplayerType * 2)[0];
                Anims.MultiplayerMapName1.ScreenPos = new Vector2(
                    x: 108 - Anims.MultiplayerMapName1.GetStringWidth() / 2f,
                    y: 80);

                Anims.MultiplayerMapName2.Text = Localization.GetText(TextBankId.Connectivity, 15 + MultiplayerType * 2)[0];
                Anims.MultiplayerMapName2.ScreenPos = new Vector2(
                    x: 108 - Anims.MultiplayerMapName2.GetStringWidth() / 2f,
                    y: 120);
            }

            ShouldTextBlink = true;
            string text = Localization.GetText(TextBankId.Connectivity, 34)[0]; // Please wait
            int width = FontManager.GetStringWidth(FontSize.Font16, text);
            Anims.Texts[4].ScreenPos = new Vector2(108 - width / 2f, 136);
            Anims.Texts[4].Text = text;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }

        CurrentStepAction = Step_TransitionToMultiplayerMapSelection;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        SetBackgroundPalette(2);
        MultiplayerPlayersOffsetY = 112;
        ResetStem();
        MultiplayerMapHighlightValue = 0;

        if (Rom.Platform == Platform.GBA)
        {
            if ((MultiplayerType == 0 && !FinishedLyChallenge1) ||
                (MultiplayerType == 1 && !FinishedLyChallenge2) ||
                (MultiplayerType == 2 && !HasAllCages))
            {
                // TODO: Remove second option
            }
        }
    }

    private void Step_TransitionToMultiplayerMapSelection()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, 8);

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;

            if (Rom.Platform == Platform.NGage)
            {
                MultiplayerMapId = 0;
                PrevSelectedOption = 0;
                SelectedOption = 0;

                foreach (SpriteTextObject textObj in Anims.Texts)
                    textObj.BgPriority = 0;
            }

            CurrentStepAction = Step_MultiplayerMapSelection;
            GameTime.Resume();
        }

        MultiplayerPlayersOffsetY -= 4;

        if (MultiplayerPlayersOffsetY < 0)
            MultiplayerPlayersOffsetY = 0;

        AnimateSelectedMultiplayerMapPalette();

        Anims.MultiplayerMapSelection.FrameChannelSprite();

        AnimationPlayer.Play(Anims.MultiplayerMapSelection);
        AnimationPlayer.Play(Anims.MultiplayerMapName1);
        AnimationPlayer.Play(Anims.MultiplayerMapName2);

        // NOTE: Hard-code for now
        if (Rom.Platform == Platform.NGage && false)
        {
            if (!ShouldTextBlink || (GameTime.ElapsedFrames & 0x10) != 0)
                AnimationPlayer.Play(Anims.Texts[4]);
        }
    }

    private void Step_MultiplayerMapSelection()
    {
        MubState state = MultiplayerManager.Step();

        if (state == MubState.Connected)
        {
            if (MultiplayerManager.HasReadJoyPads())
            {
                GameTime.Resume();

                if (IsStartingGame)
                {
                    if (Rom.Platform == Platform.NGage || TransitionsFX.IsFadeOutFinished)
                    {
                        StartMultiplayerGame();
                        IsStartingGame = false;
                    }
                }
                else
                {
                    if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.Up) && (Rom.Platform == Platform.NGage || StemMode == StemMode.Active))
                    {
                        if (MultiplayerMapId == 1)
                        {
                            MultiplayerMapId = 0;

                            if (Rom.Platform == Platform.GBA)
                            {
                                Anims.MultiplayerMapSelection.CurrentAnimation = MultiplayerType;
                            }
                            else if (Rom.Platform == Platform.NGage)
                            {
                                if (MultiplayerType == 0)
                                    Anims.MultiplayerMapSelection.CurrentAnimation = 8 + (int)CaptureTheFlagMode * 4 + CaptureTheFlagSoloMode * 2;
                                else
                                    Anims.MultiplayerMapSelection.CurrentAnimation = MultiplayerType - 1;
                            }
                            else
                            {
                                throw new UnsupportedPlatformException();
                            }

                            SelectOption(0, true);
                        }
                    }
                    else if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.Down) && (Rom.Platform == Platform.NGage || StemMode == StemMode.Active))
                    {
                        if (Rom.Platform == Platform.NGage || 
                            (MultiplayerType == 0 && FinishedLyChallenge1) ||
                            (MultiplayerType == 1 && FinishedLyChallenge2) ||
                            (MultiplayerType == 2 && HasAllCages))
                        {
                            if (MultiplayerMapId == 0)
                            {
                                MultiplayerMapId = 1;

                                if (Rom.Platform == Platform.GBA)
                                {
                                    Anims.MultiplayerMapSelection.CurrentAnimation = 3 + MultiplayerType;
                                }
                                else if (Rom.Platform == Platform.NGage)
                                {
                                    if (MultiplayerType == 0)
                                        Anims.MultiplayerMapSelection.CurrentAnimation = 9 + (int)CaptureTheFlagMode * 4 + CaptureTheFlagSoloMode * 2;
                                    else
                                        Anims.MultiplayerMapSelection.CurrentAnimation = 2 + MultiplayerType;
                                }
                                else
                                {
                                    throw new UnsupportedPlatformException();
                                }

                                SelectOption(2, true);
                            }
                        }
                    }
                    else if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.A))
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                        SoundEventsManager.ReplaceAllSongs(-1, 1);
                        IsStartingGame = true;

                        if (Rom.Platform == Platform.GBA)
                            MultiplayerManager.FUN_080ae49c();

                        Gfx.FadeControl = new FadeControl(FadeMode.None);
                        TransitionsFX.FadeOutInit(4 / 16f);
                    }
                    else if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.B))
                    {
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);

                        if (Rom.Platform == Platform.NGage && MultiplayerType == 0)
                            NextStepAction = Step_InitializeTransitionToMultiplayerFlagOptions;
                        else    
                            NextStepAction = Step_InitializeTransitionToMultiplayerTypeSelection;
                        CurrentStepAction = Step_TransitionOutOfMultiplayerMapSelection;

                        if (Rom.Platform == Platform.NGage)
                        {
                            foreach (SpriteTextObject textObj in Anims.Texts)
                                textObj.BgPriority = 3;
                        }

                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                        TransitionOutCursorAndStem();
                    }
                }

                MultiplayerManager.FrameProcessed();
            }
            else
            {
                GameTime.Pause();
            }

            AnimateSelectedMultiplayerMapPalette();

            Anims.MultiplayerMapSelection.FrameChannelSprite();

            AnimationPlayer.Play(Anims.MultiplayerMapSelection);
            AnimationPlayer.Play(Anims.MultiplayerMapName1);
            AnimationPlayer.Play(Anims.MultiplayerMapName2);

            // NOTE: Hard-code for now
            if (Rom.Platform == Platform.NGage && false)
            {
                if (!ShouldTextBlink || (GameTime.ElapsedTotalFrames & 0x10) != 0)
                    AnimationPlayer.Play(Anims.Texts[4]);
            }

            // NOTE: Hard-code for now
            if (Rom.Platform == Platform.NGage && false)
            {
                NextStepAction = Step_InitializeTransitionToMultiplayerConnectionSelection;
                CurrentStepAction = Step_TransitionOutOfMultiplayerTypeSelection; // NOTE: This seems to be wrong? Bug?
            }
        }
        else
        {
            if (Rom.Platform == Platform.GBA)
            {
                NextStepAction = Step_InitializeTransitionToMultiplayerPlayerSelection;
                CurrentStepAction = Step_TransitionOutOfMultiplayerMapSelection;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                TransitionOutCursorAndStem();
            }
            else if (Rom.Platform == Platform.NGage)
            {
                GameTime.Resume();
                NextStepAction = Step_InitializeTransitionToMultiplayerLostConnection;
                CurrentStepAction = Step_TransitionOutOfMultiplayerMapSelection;
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);

                foreach (SpriteTextObject textObj in Anims.Texts)
                    textObj.BgPriority = 3;

                TransitionOutCursorAndStem();
            }
            else
            {
                throw new UnsupportedPlatformException();
            }
        }
    }

    private void Step_TransitionOutOfMultiplayerMapSelection()
    {
        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, -4);
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
            Gfx.FadeControl = new FadeControl(FadeMode.None);
        }

        MultiplayerPlayersOffsetY += 4;

        if (MultiplayerPlayersOffsetY > 112)
            MultiplayerPlayersOffsetY = 112;

        AnimateSelectedMultiplayerMapPalette();

        Anims.MultiplayerMapSelection.FrameChannelSprite();

        AnimationPlayer.Play(Anims.MultiplayerMapSelection);
        AnimationPlayer.Play(Anims.MultiplayerMapName1);
        AnimationPlayer.Play(Anims.MultiplayerMapName2);

        // NOTE: Hard-code for now
        if (Rom.Platform == Platform.NGage && false)
        {
            if (!ShouldTextBlink || (GameTime.ElapsedFrames & 0x10) != 0)
                AnimationPlayer.Play(Anims.Texts[4]);
        }
    }

    #endregion

    #region Flag Options Steps (N-Gage)

    private void Step_InitializeTransitionToMultiplayerFlagOptions()
    {
        Anims.MultiplayerCaptureTheFlagOptions.CurrentAnimation = Localization.LanguageUiIndex;
        Anims.MultiplayerCaptureTheFlagModeName.CurrentAnimation = 5 + Localization.LanguageUiIndex;
        Anims.MultiplayerCaptureTheFlagOptionsArrowLeft.CurrentAnimation = 15;
        Anims.MultiplayerCaptureTheFlagOptionsArrowRight.CurrentAnimation = 16;
        Anims.MultiplayerCaptureTheFlagOptionsColon.CurrentAnimation = 17;
        Anims.MultiplayerCaptureTheFlagOptionsFlagsDigit.CurrentAnimation = 18;
        
        foreach (AnimatedObject digit in Anims.MultiplayerCaptureTheFlagOptionsTimeDigits)
            digit.CurrentAnimation = 18;

        CurrentStepAction = Step_TransitionToMultiplayerFlagOptions;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
        SetBackgroundPalette(0);
        ResetStem();
    }

    private void Step_TransitionToMultiplayerFlagOptions()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, 8);

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_MultiplayerFlagOptions;
            Anims.MultiplayerCaptureTheFlagOptions.BgPriority = 0;
            Anims.MultiplayerCaptureTheFlagOptionsArrowRight.BgPriority = 0;
            GameTime.Resume();
        }

        SelectedOption = 0;

        int y = ArrowYPositions[SelectedOption / 2];
        Anims.MultiplayerCaptureTheFlagOptionsArrowLeft.ScreenPos = Anims.MultiplayerCaptureTheFlagOptionsArrowLeft.ScreenPos with { Y = y };
        Anims.MultiplayerCaptureTheFlagOptionsArrowRight.ScreenPos = Anims.MultiplayerCaptureTheFlagOptionsArrowRight.ScreenPos with { Y = y };

        CaptureTheFlagMode = 0;
        CaptureTheFlagSoloMode = MultiplayerManager.PlayersCount > 2 ? 1 : 0;

        Anims.MultiplayerCaptureTheFlagOptionsFlagsDigit.CurrentAnimation = 18 + CaptureTheFlagTargetFlagsCount;
        Anims.MultiplayerCaptureTheFlagOptionsTimeDigits[0].CurrentAnimation = 18 + CaptureTheFlagTargetTime / 60;
        Anims.MultiplayerCaptureTheFlagOptionsTimeDigits[1].CurrentAnimation = 18 + (CaptureTheFlagTargetTime % 60) / 10;
        Anims.MultiplayerCaptureTheFlagOptionsTimeDigits[2].CurrentAnimation = 18 + CaptureTheFlagTargetTime - (CaptureTheFlagTargetTime / 10) * 10;

        AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptionsArrowLeft);
        AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptionsArrowRight);
        AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptionsColon);
        AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptionsFlagsDigit);
        AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagModeName);
        AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptions);

        foreach (AnimatedObject digit in Anims.MultiplayerCaptureTheFlagOptionsTimeDigits)
            AnimationPlayer.Play(digit);
    }

    private void Step_MultiplayerFlagOptions()
    {
        MubState state = MultiplayerManager.Step();

        if (state == MubState.Connected)
        {
            if (MultiplayerManager.HasReadJoyPads())
            {
                GameTime.Resume();

                if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.Up))
                {
                    SelectOption(SelectedOption == 0 ? 4 : SelectedOption - 2, true);

                    int y = ArrowYPositions[SelectedOption / 2];
                    Anims.MultiplayerCaptureTheFlagOptionsArrowLeft.ScreenPos = Anims.MultiplayerCaptureTheFlagOptionsArrowLeft.ScreenPos with { Y = y };
                    Anims.MultiplayerCaptureTheFlagOptionsArrowRight.ScreenPos = Anims.MultiplayerCaptureTheFlagOptionsArrowRight.ScreenPos with { Y = y };

                    // Huh?
                    Anims.OptionsSelection.CurrentAnimation = SelectedOption + Localization.LanguageUiIndex * 3;
                }
                else if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.Down))
                {
                    SelectOption(SelectedOption == 4 ? 0 : SelectedOption + 2, true);

                    int y = ArrowYPositions[SelectedOption / 2];
                    Anims.MultiplayerCaptureTheFlagOptionsArrowLeft.ScreenPos = Anims.MultiplayerCaptureTheFlagOptionsArrowLeft.ScreenPos with { Y = y };
                    Anims.MultiplayerCaptureTheFlagOptionsArrowRight.ScreenPos = Anims.MultiplayerCaptureTheFlagOptionsArrowRight.ScreenPos with { Y = y };

                    // Huh?
                    Anims.OptionsSelection.CurrentAnimation = SelectedOption + Localization.LanguageUiIndex * 3;
                }
                else if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.Left))
                {
                    int option = SelectedOption / 2;

                    if (option == 0)
                    {
                        if (CaptureTheFlagMode == CaptureTheFlagMode.Solo && MultiplayerManager.PlayersCount == 4)
                        {
                            CaptureTheFlagMode = CaptureTheFlagMode.Teams;
                            CaptureTheFlagSoloMode = 0;
                            Anims.MultiplayerCaptureTheFlagModeName.CurrentAnimation = 10 + Localization.LanguageUiIndex;
                        }
                        else
                        {
                            CaptureTheFlagMode = CaptureTheFlagMode.Solo;
                            CaptureTheFlagSoloMode = MultiplayerManager.PlayersCount > 2 ? 1 : 0;
                            Anims.MultiplayerCaptureTheFlagModeName.CurrentAnimation = 5 + Localization.LanguageUiIndex;
                        }
                    }
                    else if (option == 1)
                    {
                        if (CaptureTheFlagTargetFlagsCount != 0)
                            CaptureTheFlagTargetFlagsCount--;

                        if (CaptureTheFlagTargetTime == 0 && CaptureTheFlagTargetFlagsCount == 0)
                            CaptureTheFlagTargetFlagsCount = 1;

                        Anims.MultiplayerCaptureTheFlagOptionsFlagsDigit.CurrentAnimation = 18 + CaptureTheFlagTargetFlagsCount;
                    }
                    else if (option == 2)
                    {
                        if (CaptureTheFlagTargetTime >= 30)
                            CaptureTheFlagTargetTime -= 30;

                        if (CaptureTheFlagTargetTime == 0 && CaptureTheFlagTargetFlagsCount == 0)
                            CaptureTheFlagTargetTime = 30;

                        Anims.MultiplayerCaptureTheFlagOptionsTimeDigits[0].CurrentAnimation = 18 + CaptureTheFlagTargetTime / 60;
                        Anims.MultiplayerCaptureTheFlagOptionsTimeDigits[1].CurrentAnimation = 18 + (CaptureTheFlagTargetTime % 60) / 10;
                        Anims.MultiplayerCaptureTheFlagOptionsTimeDigits[2].CurrentAnimation = 18 + CaptureTheFlagTargetTime - (CaptureTheFlagTargetTime / 10) * 10;
                    }
                }
                else if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.Right))
                {
                    int option = SelectedOption / 2;

                    if (option == 0)
                    {
                        if (CaptureTheFlagMode == CaptureTheFlagMode.Solo && MultiplayerManager.PlayersCount == 4)
                        {
                            CaptureTheFlagMode = CaptureTheFlagMode.Teams;
                            CaptureTheFlagSoloMode = 0;
                            Anims.MultiplayerCaptureTheFlagModeName.CurrentAnimation = 10 + Localization.LanguageUiIndex;
                        }
                        else
                        {
                            CaptureTheFlagMode = CaptureTheFlagMode.Solo;
                            CaptureTheFlagSoloMode = MultiplayerManager.PlayersCount > 2 ? 1 : 0;
                            Anims.MultiplayerCaptureTheFlagModeName.CurrentAnimation = 5 + Localization.LanguageUiIndex;
                        }
                    }
                    else if (option == 1)
                    {
                        if (CaptureTheFlagTargetFlagsCount < 9)
                            CaptureTheFlagTargetFlagsCount++;

                        Anims.MultiplayerCaptureTheFlagOptionsFlagsDigit.CurrentAnimation = 18 + CaptureTheFlagTargetFlagsCount;
                    }
                    else if (option == 2)
                    {
                        if (CaptureTheFlagTargetTime < 540)
                            CaptureTheFlagTargetTime += 30;

                        Anims.MultiplayerCaptureTheFlagOptionsTimeDigits[0].CurrentAnimation = 18 + CaptureTheFlagTargetTime / 60;
                        Anims.MultiplayerCaptureTheFlagOptionsTimeDigits[1].CurrentAnimation = 18 + (CaptureTheFlagTargetTime % 60) / 10;
                        Anims.MultiplayerCaptureTheFlagOptionsTimeDigits[2].CurrentAnimation = 18 + CaptureTheFlagTargetTime - (CaptureTheFlagTargetTime / 10) * 10;
                    }
                }
                else if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.A) || MultiJoyPad.IsButtonJustPressed(0, GbaInput.Start))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                    TransitionOutCursorAndStem();
                    NextStepAction = Step_InitializeTransitionToMultiplayerMapSelection;
                    CurrentStepAction = Step_TransitionOutOfMultiplayerFlagOptions;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                }
                else if (MultiJoyPad.IsButtonJustPressed(0, GbaInput.B) || MultiJoyPad.IsButtonJustPressed(0, GbaInput.Select))
                {
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                    TransitionOutCursorAndStem();
                    NextStepAction = Step_InitializeTransitionToMultiplayerTypeSelection;
                    CurrentStepAction = Step_TransitionOutOfMultiplayerFlagOptions;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
                }

                MultiplayerManager.FrameProcessed();
            }
            else
            {
                GameTime.Pause();
            }

            AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptionsArrowLeft);
            AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptionsArrowRight);
            AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptionsColon);
            AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptionsFlagsDigit);
            AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagModeName);

            foreach (AnimatedObject digit in Anims.MultiplayerCaptureTheFlagOptionsTimeDigits)
                AnimationPlayer.Play(digit);
            
            AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptions);

            // NOTE: Hard-code for now
            if (Rom.Platform == Platform.NGage && false)
            {
                NextStepAction = Step_InitializeTransitionToMultiplayerConnectionSelection;
                CurrentStepAction = Step_TransitionOutOfMultiplayerTypeSelection; // NOTE: This seems to be wrong? Bug?
            }
        }
        else
        {
            GameTime.Resume();
            NextStepAction = Step_InitializeTransitionToMultiplayerLostConnection;
            CurrentStepAction = Step_TransitionOutOfMultiplayerFlagOptions;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
            TransitionOutCursorAndStem();
        }
    }

    private void Step_TransitionOutOfMultiplayerFlagOptions()
    {
        Anims.MultiplayerCaptureTheFlagOptions.BgPriority = 3;
        Anims.MultiplayerCaptureTheFlagOptionsArrowRight.BgPriority = 3;

        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, -4);
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
            Gfx.FadeControl = new FadeControl(FadeMode.None);
        }

        SelectedOption = PrevSelectedOption;

        AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptionsArrowLeft);
        AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptionsArrowRight);
        AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptionsColon);
        AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptionsFlagsDigit);
        AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagModeName);
        AnimationPlayer.Play(Anims.MultiplayerCaptureTheFlagOptions);

        foreach (AnimatedObject digit in Anims.MultiplayerCaptureTheFlagOptionsTimeDigits)
            AnimationPlayer.Play(digit);
    }

    #endregion

    #region Lost Connection Steps

    private void Step_InitializeTransitionToMultiplayerLostConnection()
    {
        if (Rom.Platform == Platform.GBA)
        {
            if (InitialPage == InitialMenuPage.MultiplayerLostConnection)
            {
                InitialPage = InitialMenuPage.Language;
                CurrentStepAction = Step_MultiplayerLostConnection;
                SetMenuText(1, true);
            }
            else
            {
                CurrentStepAction = Step_TransitionToMultiplayerPlayerSelection;
                SetMenuText(0, false);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);
            }
        }
        else if (Rom.Platform == Platform.NGage)
        {
            InitialPage = InitialMenuPage.Language;
            CurrentStepAction = Step_TransitionToMultiplayerLostConnection;

            int playerIdWhoLeftTheGame = -1; // NOTE: Hard-code for now
            if (playerIdWhoLeftTheGame == -1)
                NGageSetMenuText(1, true, null, 0); // Link Error! Press Left Soft Key
            else
                NGageSetMenuText(2, true, null, 100, playerIdWhoLeftTheGame + 1); // Player %i has left the game
        }
        else
        {
            throw new UnsupportedPlatformException();
        }

        SetBackgroundPalette(2);
    }

    // N-Gage only
    private void Step_TransitionToMultiplayerLostConnection()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, 8);

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            CurrentStepAction = Step_MultiplayerLostConnection;
        }

        DrawText(false);
    }

    private void Step_MultiplayerLostConnection()
    {
        if (Rom.Platform == Platform.GBA)
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.Start))
                CurrentStepAction = Step_TransitionOutOfMultiplayerLostConnection;
        }
        else if (Rom.Platform == Platform.NGage)
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.A) ||
                JoyPad.IsButtonJustPressed(GbaInput.B) ||
                JoyPad.IsButtonJustPressed(GbaInput.Start) ||
                JoyPad.IsButtonJustPressed(GbaInput.Select))
                CurrentStepAction = Step_TransitionOutOfMultiplayerLostConnection;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }

        DrawText(Rom.Platform == Platform.NGage);
    }

    private void Step_TransitionOutOfMultiplayerLostConnection()
    {
        TransitionValue += 4;

        if (TransitionValue <= Playfield.RenderContext.Resolution.Y)
        {
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, -4);
        }
        else if (TransitionValue >= Playfield.RenderContext.Resolution.Y + 60)
        {
            TransitionValue = 0;

            if (Rom.Platform == Platform.GBA)
            {
                NextStepAction = Step_InitializeTransitionToMultiplayerPlayerSelection;
                CurrentStepAction = Step_InitializeTransitionToMultiplayerPlayerSelection;
            }
            else if (Rom.Platform == Platform.NGage)
            {
                NextStepAction = Step_InitializeTransitionToMultiplayerConnectionSelection;
                CurrentStepAction = Step_InitializeTransitionToMultiplayerConnectionSelection;
            }
            else
            {
                throw new UnsupportedPlatformException();
            }
        }

        DrawText(false);
    }

    #endregion

    #region Single Pak Steps (GBA)

    private void Step_InitializeTransitionToMultiplayerSinglePak()
    {
        SetMenuText(3, false); // Please Wait...

        MultiplayerSinglePakConnectionTimer = 125;
        NextTextId = -1;
        field_0xe1 = 0;

        Anims.MultiplayerSinglePakPlayers.CurrentAnimation = 11;
        SinglePakPlayersOffsetY = 0x46;

        CurrentStepAction = Step_TransitionToMultiplayerSinglePak;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store02_Mix02);

        SetBackgroundPalette(2);

        PreviousTextId = 0;
    }

    private void Step_TransitionToMultiplayerSinglePak()
    {
        TransitionValue += 4;

        if (TransitionValue <= 80)
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, 8);

        if (TransitionValue >= 160)
        {
            TransitionValue = 0;
            // NOTE: Game gets the pointer and position to the SinglePak ROM here
            RSMultiplayer.UnInit();
            // NOTE: Game creates the SinglePakManager class for transferring the SinglePak ROM here
            CurrentStepAction = Step_MultiplayerSinglePak;
        }

        Anims.MultiplayerSinglePakPlayers.ScreenPos = Anims.MultiplayerSinglePakPlayers.ScreenPos with { Y = 40 - SinglePakPlayersOffsetY };

        DrawText(false);
        AnimationPlayer.Play(Anims.MultiplayerSinglePakPlayers);
    }

    private void Step_MultiplayerSinglePak()
    {
        // TODO: Implement

        if (NextTextId != -1)
        {
            SetMenuText(NextTextId, false);
            NextTextId = -1;
        }

        // TODO: Implement

        if (JoyPad.IsButtonJustPressed(GbaInput.B))
        {
            RSMultiplayer.Init();
            MultiplayerInititialGameTime = GameTime.ElapsedFrames;
            NextStepAction = Step_InitializeTransitionToMultiplayerModeSelection;
            CurrentStepAction = Step_TransitionOutOfMultiplayerSinglePak;
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Store01_Mix01);
        }

        Anims.MultiplayerSinglePakPlayers.ScreenPos = Anims.MultiplayerSinglePakPlayers.ScreenPos with { Y = 40 - SinglePakPlayersOffsetY };

        if (NextTextId == -1)
            DrawText(false);
        AnimationPlayer.Play(Anims.MultiplayerSinglePakPlayers);
    }

    private void Step_TransitionOutOfMultiplayerSinglePak()
    {
        TransitionValue += 4;

        if (TransitionValue <= 160)
        {
            Playfield.Camera.GetCluster(1).Position += new Vector2(0, -4);
        }
        else if (TransitionValue >= 220)
        {
            TransitionValue = 0;
            CurrentStepAction = NextStepAction;
        }

        if (SinglePakPlayersOffsetY <= 70)
            SinglePakPlayersOffsetY += 8;
        else
            SinglePakPlayersOffsetY = 70;

        DrawText(false);
        AnimationPlayer.Play(Anims.MultiplayerSinglePakPlayers);
    }

    #endregion
}