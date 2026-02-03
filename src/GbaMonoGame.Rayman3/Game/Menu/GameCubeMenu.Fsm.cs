using System;
using BinarySerializer;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.TgxEngine;

namespace GbaMonoGame.Rayman3;

public partial class GameCubeMenu
{
    private bool FsmStep_CheckConnection()
    {
        if (UseJoyBus && JoyBus.CheckForLostConnection())
        {
            State.MoveTo(_Fsm_ConnectionLost);
            return false;
        }

        return true;
    }

    public bool Fsm_PreInit(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ShowPleaseConnectText();
                TransitionsFX.Init(true);
                Timer = 0;
                break;

            case FsmAction.Step:
                TransitionInScreenEffect.Value = Timer;
                Timer += 8;

                if (Timer >= 240)
                {
                    State.MoveTo(_Fsm_WaitForConnection);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Gfx.ClearScreenEffect();
                break;
        }

        return true;
    }

    public bool Fsm_WaitForConnection(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Timer = 0;

                // If not set to use the JoyBus then we read from the file system
                if (!UseJoyBus)
                {
                    // Add the map infos file to the context
                    if (!Rom.Context.FileExists(MapInfosFileName))
                        Rom.Context.AddFile(new LinearFile(Rom.Context, MapInfosFileName));

                    // Check if the map infos file exists and read it if so
                    if (((LinearFile)Rom.Context.GetRequiredFile(MapInfosFileName)).SourceFileExists)
                    {
                        using (Rom.Context)
                        {
                            Engine.BeginLoad();
                            MapInfos = FileFactory.Read<GameCubeMapInfos>(Rom.Context, MapInfosFileName);
                        }
                    }
                    else
                    {
                        ShowCustomText("Please select a Rayman 3 GameCube ISO file to extract the bonus maps.");
                    }
                }
                break;

            case FsmAction.Step:
                if (!UseJoyBus)
                {
                    // Exit
                    if (JoyPad.IsButtonJustPressed(Rayman3Input.GameCubeMenuBack))
                    {
                        IsActive = false;
                        State.MoveTo(_Fsm_Exit);
                        return false;
                    }

                    // Had read map infos
                    if (MapInfos != null)
                    {
                        State.MoveTo(_Fsm_SelectMap);
                        return false;
                    }

                    // Select file
                    if (JoyPad.IsButtonJustPressed(Rayman3Input.GameCubeMenuConfirm))
                    {
                        string isoFilePath = FileDialog.OpenFile("Select the Rayman 3 GameCube ISO", new FileDialog.FileFilter("iso", "GameCube Disc"));
                        if (isoFilePath != null)
                        {
                            try
                            {
                                bool success = ExtractGameCubeFiles(isoFilePath);

                                if (success)
                                {
                                    State.MoveTo(_Fsm_WaitForConnection);
                                    return false;
                                }
                                else
                                {
                                    ShowCustomText("Invalid ISO file.");
                                }
                            }
                            catch (Exception ex)
                            {
                                ShowCustomText($"Error: {ex.Message}");
                            }
                        }
                    }
                }
                else
                {
                    JoyBus.CheckForLostConnection();
                    if (Timer < 10)
                    {
                        Timer++;

                        if (Timer == 9)
                        {
                            ShowPleaseConnectText();
                            WaitingForConnection = true;
                        }
                    }

                    // Exit
                    if (JoyPad.IsButtonJustPressed(Rayman3Input.GameCubeMenuBack))
                    {
                        IsActive = false;
                        State.MoveTo(_Fsm_Exit);
                        return false;
                    }

                    // Connected
                    if (JoyBus.IsConnected)
                    {
                        WaitingForConnection = false;
                        State.MoveTo(_Fsm_Connected);
                        return false;
                    }
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_ConnectionLost(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Anims.StatusText.Text = "";
                // Why does the game do this??
                MapInfoFileSize = (int)GameTime.ElapsedFrames;
                break;

            case FsmAction.Step:
                if (JoyPad.IsButtonJustPressed(Rayman3Input.GameCubeMenuBack))
                {
                    IsActive = false;
                    State.MoveTo(_Fsm_Exit);
                    return false;
                }

                ShowPleaseConnectText();
                State.MoveTo(_Fsm_WaitForConnection);
                return false;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Exit(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                TransitionOutScreenEffect = new GameCubeMenuTransitionOutScreenEffect()
                {
                    RenderContext = Rom.OriginalGameRenderContext,
                };
                Gfx.SetScreenEffect(TransitionOutScreenEffect);
                Timer = 0;
                break;

            case FsmAction.Step:
                TransitionOutScreenEffect.Value = Timer;
                Timer++;

                if (Timer >= 80)
                {
                    GameInfo.WorldId = 0;
                    GameInfo.LoadLevel(MapId.WorldMap);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Connected(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ResetReusableTexts();
                Anims.StatusText.ScreenPos = new Vector2(105, 88);
                Anims.StatusText.Text = "";
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckConnection())
                    return false;

                // Read data
                byte gbaUnlockFlags = 0;
                bool isValid = true;
                if (JoyBus.HasReceivedData)
                {
                    int data = JoyBus.ReceivedData;
                    MapInfoFileSize = BitHelpers.ExtractBits(data, 16, 0) * 4;
                    gbaUnlockFlags = (byte)BitHelpers.ExtractBits(data, 8, 16);
                    byte check = (byte)BitHelpers.ExtractBits(data, 8, 24);
                    isValid = gbaUnlockFlags == (byte)~check;
                }

                // Exit
                if (JoyPad.IsButtonJustPressed(Rayman3Input.GameCubeMenuBack))
                {
                    IsActive = false;
                    State.MoveTo(_Fsm_Exit);
                    return false;
                }

                // Error
                if (JoyBus.HasReceivedData && JoyBus.IsConnected && !isValid)
                {
                    State.MoveTo(_Fsm_ConnectionLost);
                    return false;
                }

                // Received data - download the map info
                if (JoyBus.HasReceivedData && JoyBus.IsConnected)
                {
                    GbaUnlockFlags = gbaUnlockFlags;
                    State.MoveTo(_Fsm_DownloadMapInfo);
                    return false;
                }

                // Disconnected
                if (!JoyBus.IsConnected)
                {
                    ShowPleaseConnectText();
                    State.MoveTo(_Fsm_WaitForConnection);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (IsActive)
                {
                    JoyBus.NewTransfer(MapInfoFileSize);

                    int value = 0;
                    value = BitHelpers.SetBits(value, MapInfoFileSize / 4, 16, 0);
                    value = BitHelpers.SetBits(value, GcnUnlockFlags, 8, 16);
                    value = BitHelpers.SetBits(value, ~GcnUnlockFlags, 8, 24);
                    JoyBus.SendValue(value);
                }
                break;
        }

        return true;
    }

    public bool Fsm_DownloadMapInfo(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckConnection())
                    return false;

                // Exit
                if (JoyPad.IsButtonJustPressed(Rayman3Input.GameCubeMenuBack))
                {
                    IsActive = false;
                    State.MoveTo(_Fsm_Exit);
                    return false;
                }

                // Disconnected
                if (!JoyBus.IsConnected)
                {
                    ShowPleaseConnectText();
                    State.MoveTo(_Fsm_WaitForConnection);
                    return false;
                }

                // Error
                if (JoyBus.ErrorState == 0xFF)
                {
                    State.MoveTo(_Fsm_ConnectionLost);
                    return false;
                }

                // Finished downloading map info
                if (JoyBus.RemainingSize == 0)
                {
                    State.MoveTo(_Fsm_DownloadMapInfoAck);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (IsActive)
                {
                    JoyBus.SendValue(JoyBus.ReceivedData);
                    JoyBus.HasReceivedData = false;
                    JoyBus.Checksum = 0;
                }
                break;
        }

        return true;
    }

    public bool Fsm_DownloadMapInfoAck(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckConnection())
                    return false;

                // Exit
                if (JoyPad.IsButtonJustPressed(Rayman3Input.GameCubeMenuBack))
                {
                    IsActive = false;
                    State.MoveTo(_Fsm_Exit);
                    return false;
                }

                // Select map - Received download acknowledgement
                if (JoyBus.HasReceivedData && JoyBus.ReceivedData == 0x22222222)
                {
                    State.MoveTo(_Fsm_SelectMap);
                    return false;
                }

                // Error - received invalid data
                if (JoyBus.HasReceivedData)
                {
                    State.MoveTo(_Fsm_ConnectionLost);
                    return false;
                }

                // Disconnected
                if (!JoyBus.IsConnected)
                {
                    ShowPleaseConnectText();
                    State.MoveTo(_Fsm_WaitForConnection);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_SelectMap(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SelectedMap = 0;
                MapScroll = 0;

                // Unlock Ly's Punch Challenge 3 by completing all GCN bonus levels
                bool unlockedLyChallenge;
                if (UseJoyBus)
                    unlockedLyChallenge = (GbaUnlockFlags & 1) != 0;
                else
                    unlockedLyChallenge = GameInfo.PersistentInfo.CompletedGCNBonusLevels == 10;

                if (unlockedLyChallenge && !GameInfo.PersistentInfo.UnlockedLyChallengeGCN)
                {
                    GameInfo.PersistentInfo.UnlockedLyChallengeGCN = true;

                    string[] text = Localization.GetText(TextBankId.Connectivity, 8);

                    Anims.StatusText.Text = text[0];
                    Anims.StatusText.ScreenPos = new Vector2(140 - Anims.StatusText.GetStringWidth() / 2f, 50);

                    Anims.ReusableTexts[0].Text = text[1];
                    Anims.ReusableTexts[0].ScreenPos = new Vector2(140 - Anims.ReusableTexts[0].GetStringWidth() / 2f, 70);

                    IsShowingLyChallengeUnlocked = true;
                }
                else
                {
                    Anims.StatusText.Text = "";
                    ResetReusableTexts();
                    MapSelectionUpdateText();
                }
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckConnection())
                    return false;

                bool hasSelectedMap = false;

                if (IsShowingLyChallengeUnlocked)
                {
                    if (JoyPad.IsButtonJustPressed(Rayman3Input.GameCubeMenuConfirm))
                    {
                        IsShowingLyChallengeUnlocked = false;
                        Anims.StatusText.Text = "";
                        ResetReusableTexts();
                        MapSelectionUpdateText();
                    }
                }
                else
                {
                    // Select map
                    if (JoyPad.IsButtonJustPressed(Rayman3Input.GameCubeMenuConfirm))
                    {
                        // Make sure map is unlocked
                        if (IsMapUnlocked(SelectedMap))
                        {
                            if (UseJoyBus)
                            {
                                JoyBus.NewTransfer(MapInfos.Maps[SelectedMap].FileSize);
                                JoyBus.SendValue(3);
                                hasSelectedMap = true;
                            }
                            else
                            {
                                Engine.BeginLoad();

                                using (Rom.Context)
                                {
                                    string filePath = $"map.{SelectedMap:000}";

                                    if (!Rom.Context.FileExists(filePath))
                                        Rom.Context.AddFile(new LinearFile(Rom.Context, filePath));

                                    Map = FileFactory.Read<GameCubeMap>(Rom.Context, filePath);
                                }

                                FrameManager.SetNextFrame(new FrameSideScrollerGCN(MapInfos.Maps[SelectedMap], Map, SelectedMap));
                            }

                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Valid01_Mix01);
                        }
                        else
                        {
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Back01_Mix01);
                        }
                    }

                    // Move up
                    if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuUp) && SelectedMap != 0)
                    {
                        SelectedMap--;
                        if (MapScroll > SelectedMap)
                            MapScroll = SelectedMap;

                        MapSelectionUpdateText();
                        JoyBus.SendValue(2);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }

                    // Move down
                    if (JoyPad.IsButtonJustPressed(Rayman3Input.MenuDown) && SelectedMap < MapInfos.MapsCount - 1)
                    {
                        SelectedMap++;
                        if (MapScroll + 2 < SelectedMap)
                            MapScroll = SelectedMap - 2;

                        MapSelectionUpdateText();
                        JoyBus.SendValue(1);
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MenuMove);
                    }
                }

                // Exit
                if (JoyPad.IsButtonJustPressed(Rayman3Input.GameCubeMenuBack))
                {
                    IsActive = false;
                    State.MoveTo(_Fsm_Exit);
                    return false;
                }

                if (hasSelectedMap)
                {
                    State.MoveTo(_Fsm_DownloadMap);
                    return false;
                }

                // Disconnected
                if (UseJoyBus && !JoyBus.IsConnected)
                {
                    ShowPleaseConnectText();
                    State.MoveTo(_Fsm_WaitForConnection);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (IsActive)
                    IsShowingLyChallengeUnlocked = false;
                break;
        }

        return true;
    }

    public bool Fsm_DownloadMap(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Anims.StatusText.Text = "   %";

                Anims.ReusableTexts[0].Color = TextColor.GameCubeMenu;
                Anims.ReusableTexts[1].Color = TextColor.GameCubeMenu;

                string[] text = Localization.GetText(TextBankId.Connectivity, 7);
                Anims.ReusableTexts[0].Text = text[0];
                Anims.ReusableTexts[0].ScreenPos = new Vector2(140 - Anims.ReusableTexts[0].GetStringWidth() / 2f, 40);

                Anims.ReusableTexts[1].Text = MapInfos.Maps[SelectedMap].Name;
                Anims.ReusableTexts[1].ScreenPos = new Vector2(140 - Anims.ReusableTexts[1].GetStringWidth() / 2f, 60);

                Anims.StatusText.ScreenPos = new Vector2(122, 80);
                break;

            case FsmAction.Step:
                if (!FsmStep_CheckConnection())
                    return false;

                // Calculate download percentage
                int percentage = (JoyBus.Size - JoyBus.RemainingSize) * 100 / JoyBus.Size;
                string percentageString = $"{percentage}%";
                percentageString = percentageString.PadLeft(4);

                Anims.StatusText.Text = percentageString;

                // Stop download
                if (JoyPad.IsButtonJustPressed(Rayman3Input.GameCubeMenuBack))
                {
                    JoyBus.Disconnect();
                    JoyBus.Connect();
                    State.MoveTo(_Fsm_WaitForConnection);
                    return false;
                }

                // Error
                if (JoyBus.ErrorState == 0xFF)
                {
                    State.MoveTo(_Fsm_ConnectionLost);
                    return false;
                }

                // Finished downloading map
                if (JoyBus.RemainingSize == 0)
                {
                    State.MoveTo(_Fsm_DownloadMapAck);
                    return false;
                }

                // Disconnected
                if (!JoyBus.IsConnected)
                {
                    ShowPleaseConnectText();
                    State.MoveTo(_Fsm_WaitForConnection);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (IsActive)
                {
                    JoyBus.SendValue(JoyBus.ReceivedData);
                    JoyBus.HasReceivedData = false;
                    JoyBus.Checksum = 0;
                }
                break;
        }

        return true;
    }

    public bool Fsm_DownloadMapAck(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Anims.StatusText.Text = "100%";
                break;

            case FsmAction.Step:
                if (JoyBus.HasReceivedData)
                {
                    FrameManager.SetNextFrame(new FrameSideScrollerGCN(MapInfos.Maps[SelectedMap], Map, SelectedMap));

                    if (JoyBus.ReceivedData == 0x22222222)
                    {
                        State.MoveTo(_Fsm_WaitForConnection);
                        return false;
                    }
                    else
                    {
                        State.MoveTo(_Fsm_ConnectionLost);
                        return false;
                    }
                }

                // Disconnected
                if (!JoyBus.IsConnected)
                {
                    ShowPleaseConnectText();
                    State.MoveTo(_Fsm_WaitForConnection);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}