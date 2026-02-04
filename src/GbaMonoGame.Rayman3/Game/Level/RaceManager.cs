using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using GbaMonoGame.Rayman3.Readvanced;

namespace GbaMonoGame.Rayman3;

public class RaceManager
{
    public RaceManager(Scene2D scene, UserInfoSingleMode7 userInfo, ushort[] lapTimes)
    {
        Scene = scene;
        UserInfo = userInfo;
        Timer = 0;
        LapsCount = lapTimes.Length;
        LapTimes = lapTimes;
        CurrentLap = 1;
        RemainingTime = -1;
        CurrentTempLap = 0;
        IsRacing = false;
        DrivingTheRightWay = true;
    }

    public Scene2D Scene { get; }
    public UserInfoSingleMode7 UserInfo { get; }
    public int LapsCount { get; }
    public ushort[] LapTimes { get; }
    public uint Timer { get; set; }
    public int RemainingTime { get; set; }
    public int CurrentLap { get; set; }
    public int CurrentTempLap { get; set; }
    public bool IsRacing { get; set; }
    public bool DrivingTheRightWay { get; set; }

    public void Step()
    {
        if (TimeAttackInfo.IsActive)
        {
            if (Timer == 0 && TimeAttackInfo.Mode == TimeAttackMode.Play)
            {
                Timer = 1;
                IsRacing = true;
                Scene.MainActor.ProcessMessage(this, Message.MissileMode7_StartRace);
            }
        }
        else
        {
            Timer++;

            switch (Timer)
            {
                case 1:
                    RemainingTime = LapTimes[0] * 60;
                    break;

                case 60:
                    UserInfo.SetCountdownValue(3);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P1_);
                    break;

                case 120:
                    UserInfo.SetCountdownValue(2);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P2_);
                    break;

                case 180:
                    UserInfo.SetCountdownValue(1);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__CountDwn_Mix07_P3_);
                    break;

                case 240:
                    UserInfo.SetCountdownValue(0);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoGO_Mix02);
                    IsRacing = true;
                    Scene.MainActor.ProcessMessage(this, Message.MissileMode7_StartRace);
                    break;

                case 300:
                    UserInfo.IsCountdownActive = false;
                    break;
            }

            if (IsRacing)
            {
                if (RemainingTime <= 0)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Actor_Explode);
                }
                else
                {
                    RemainingTime--;

                    // NOTE: This cheat is normally only in the game prototypes
                    if (Engine.ActiveConfig.Tweaks.AllowPrototypeCheats &&
                        JoyPad.IsButtonPressed(GbaInput.R) && JoyPad.IsButtonPressed(GbaInput.L) && JoyPad.IsButtonJustPressed(GbaInput.Select))
                    {
                        RemainingTime = 356400; // 99:00:00

                        if (Engine.LocalConfig.Tweaks.PlayCheatTriggerSound)
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Switch1_Mix03);
                    }
                }
            }
        }
    }
}