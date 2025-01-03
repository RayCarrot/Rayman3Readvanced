using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;

namespace GbaMonoGame.Rayman3;

public partial class UserInfoMulti2D
{
    public bool Fsm_Play(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                Timer++;

                // Update times
                if (Timer - LastTimeChangeTime >= 60)
                {
                    LastTimeChangeTime = Timer;

                    if (StartCountdownValue == 0)
                    {
                        if (!IsGameOver)
                        {
                            switch (MultiplayerInfo.GameType)
                            {
                                case MultiplayerGameType.RayTag:
                                    if (Times[TagId] != 0)
                                    {
                                        Times[TagId]--;

                                        if (Times[TagId] == 0)
                                            GameOver(TagId);
                                    }
                                    break;
                                
                                case MultiplayerGameType.CatAndMouse:
                                    if (Times[TagId] < 60)
                                    {
                                        Times[TagId]++;

                                        if (Times[TagId] == 60)
                                            Win(TagId);
                                    }
                                    break;
                                
                                case MultiplayerGameType.CaptureTheFlag when Engine.Settings.Platform == Platform.NGage:
                                    // TODO: Implement
                                    break;
                            }

                            PrintTime();
                        }
                    }
                    else if (StartCountdownValue < 2)
                    {
                        StartCountdownValue--;

                        if (Engine.Settings.Platform == Platform.NGage)
                            Scene.NGage_Flag_6 = false;
                    }
                    else
                    {
                        SoundEventsManager.ProcessEvent(StartCountdownValue switch
                        {
                            5 => Rayman3SoundEvent.Play__CountDwn_Mix07_P1_,
                            4 => Rayman3SoundEvent.Play__CountDwn_Mix07_P2_,
                            3 => Rayman3SoundEvent.Play__CountDwn_Mix07_P3_,
                            _ => Rayman3SoundEvent.Play__OnoGO_Mix02,
                        });

                        StartCountdownValue--;
                        StartCountdown.CurrentAnimation = StartCountdownValue - 1;
                    }
                }

                // Update Globox
                if (GloboxCountdown != 0)
                {
                    if (GloboxMachineId != MultiplayerManager.MachineId)
                    {
                        if (GloboxCountdown is 360 or 180)
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Globox_Mix04);

                        if (GloboxCountdown is >= 120 and <= 125 or >= 200 and <= 205)
                            Globox.CurrentAnimation = 6;
                        else
                            Globox.CurrentAnimation = 5;
                    }

                    GloboxCountdown--;
                }

                // Blink energy shots digits if trying to attack without shots
                if (MultiJoyPad.IsButtonJustPressed(MultiplayerManager.MachineId, GbaInput.B))
                {
                    if ((TagId == MultiplayerManager.MachineId && MultiplayerInfo.GameType == MultiplayerGameType.RayTag) ||
                        (TagId != MultiplayerManager.MachineId && MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse))
                    {
                        if (EnergyShots[MultiplayerManager.MachineId] == 0)
                            EnergyShotsBlinkCountdown = 60;
                    }
                }

                // Update energy shots digits blinking
                if (EnergyShotsBlinkCountdown != 0)
                {
                    if ((MultiplayerInfo.GameType == MultiplayerGameType.RayTag && EnergyShots[MultiplayerManager.MachineId] != 0) ||
                        (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse && EnergyShots[MultiplayerManager.MachineId] != 0))
                    {
                        EnergyShotsBlinkCountdown = 0;
                    }
                    else
                    {
                        EnergyShotsBlinkCountdown--;
                    }
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_GameOver(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                GloboxCountdown = 0;
                
                if (Engine.Settings.Platform != Platform.NGage || MultiplayerInfo.GameType != MultiplayerGameType.CaptureTheFlag)
                    SetArrow();
                break;

            case FsmAction.Step:
                // Do nothing
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }
}