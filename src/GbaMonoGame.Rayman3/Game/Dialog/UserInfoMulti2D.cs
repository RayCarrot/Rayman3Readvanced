using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class UserInfoMulti2D : Dialog
{
    #region Constructor

    public UserInfoMulti2D(Scene2D scene) : base(scene)
    {
        switch (MultiplayerInfo.GameType)
        {
            case MultiplayerGameType.RayTag:
                Times = new int[RSMultiplayer.MaxPlayersCount];
                EnergyShots = new int[RSMultiplayer.MaxPlayersCount];
                for (int i = 0; i < RSMultiplayer.MaxPlayersCount; i++)
                {
                    Times[i] = 60;
                    EnergyShots[i] = 0;
                }
                break;
            
            case MultiplayerGameType.CatAndMouse:
                Times = new int[RSMultiplayer.MaxPlayersCount];
                EnergyShots = new int[RSMultiplayer.MaxPlayersCount];
                for (int i = 0; i < RSMultiplayer.MaxPlayersCount; i++)
                {
                    Times[i] = 0;
                    EnergyShots[i] = 0;
                }
                break;
            
            case MultiplayerGameType.CaptureTheFlag when Engine.Settings.Platform == Platform.NGage:
                EnergyShots = new int[RSMultiplayer.MaxPlayersCount];
                for (int i = 0; i < RSMultiplayer.MaxPlayersCount; i++)
                    EnergyShots[i] = 0;

                CaptureTheFlagTime = ((FrameMultiCaptureTheFlag)Frame.Current).Time;
                SuddenDeathDisplayCountdown = 100;
                FlagBar = new FlagBar(Scene);
                break;

            case MultiplayerGameType.Missile:
            default:
                throw new InvalidOperationException("Invalid game type");
        }

        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
        {
            TagId = -1;
            TagIdHudIndex = -1;
        }
        else
        {
            TagId = (int)(MultiplayerInfo.InitialGameTime % RSMultiplayer.PlayersCount);

            if (TagId == RSMultiplayer.MachineId)
                TagIdHudIndex = 0;
            else if (TagId <= RSMultiplayer.MachineId)
                TagIdHudIndex = TagId + 1;
            else
                TagIdHudIndex = TagId;
        }

        LastTimeChangeTime = 0;
        Timer = 0;
        GloboxCountdown = 0;
        EnergyShotsBlinkCountdown = 0;
        Unused1 = -1;
        IsGameOver = false;
        IsPlayerDead = false;

        if (Engine.Settings.Platform == Platform.NGage)
            UnknownCaptureTheFlagValue2 = false;
        
        Unused2 = -1;
        StartCountdownValue = 4;
        IsPaused = false;
        PlayerRanks = [0, 1, 2, 3];
        AlivePlayersCount = RSMultiplayer.PlayersCount;

        ScoreBar = new ScoreBar(Scene);

        State.MoveTo(Fsm_Play);
    }

    #endregion

    #region Properties

    public ScoreBar ScoreBar { get; set; }
    public FlagBar FlagBar { get; set; }

    public AnimatedObject[] TimerFrames { get; set; }
    public AnimatedObject[][] TimerDigits { get; set; }
    public AnimatedObject[] TimerColons { get; set; }
    public AnimatedObject[] PlayerIcons { get; set; }
    public AnimatedObject EnergyShotsCounterFrame { get; set; }
    public AnimatedObject[] EnergyShotsCounterDigits { get; set; }
    public AnimatedObject StartCountdown { get; set; }
    public AnimatedObject EnergyShotsIcon { get; set; }
    public AnimatedObject PlayerArrow { get; set; }
    public AnimatedObject GameOverSign { get; set; }
    public AnimatedObject SuddenDeathSign { get; set; } // N-Gage exclusive
    public AnimatedObject Globox { get; set; }
    public AnimatedObject ReverseControlsEffectIcon { get; set; }
    public AnimatedObject InvisibilityEffectIcon { get; set; }
    public AnimatedObject GloboxEffectIcon { get; set; }

    public int AlivePlayersCount { get; set; }
    public int TagId { get; set; }
    public int TagIdHudIndex { get; set; }

    public uint Timer { get; set; }
    public uint LastTimeChangeTime { get; set; }

    public int[] Times { get; set; }
    public int[] EnergyShots { get; set; }

    public byte StartCountdownValue { get; set; }
    public bool IsPaused { get; set; }
    public bool IsPlayerDead { get; set; }
    public bool IsGameOver { get; set; }

    public ushort EnergyShotsBlinkCountdown { get; set; }

    public ushort GloboxCountdown { get; set; }
    public int GloboxMachineId { get; set; }

    public int[] PlayerRanks { get; set; }

    public int Unused1 { get; set; }
    public int Unused2 { get; set; }
    public int Unused3 { get; set; }

    // // N-Gage exclusive
    public ushort CaptureTheFlagTime { get; set; }
    public bool UnknownCaptureTheFlagValue2 { get; set; }
    public byte SuddenDeathDisplayCountdown { get; set; }

    #endregion

    #region Private Methods

    private int HudIndexToMachineId(int hudIndex)
    {
        if (hudIndex == 0)
            return MultiplayerManager.MachineId;
        else if (hudIndex <= MultiplayerManager.MachineId)
            return hudIndex - 1;
        else
            return hudIndex;
    }

    private int MachineIdToHudIndex(int machineId)
    {
        if (machineId == MultiplayerManager.MachineId)
            return 0;
        else if (machineId <= MultiplayerManager.MachineId)
            return machineId + 1;
        else
            return machineId;
    }

    #endregion

    #region Methods

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        return false;
    }

    public int GetTagId()
    {
        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            return -1;
        else
            return TagId;
    }

    public void SetTagId(int machineId)
    {
        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            return;

        if (machineId == TagId || IsGameOver)
            return;

        EnergyShots[TagId] = 0;
        EnergyShots[machineId] = 0;

        if (MultiplayerInfo.GameType == MultiplayerGameType.RayTag)
        {
            if (machineId == MultiplayerManager.MachineId)
                PrintEnergyShots(machineId);
        }
        else if (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse)
        {
            if (TagId == MultiplayerManager.MachineId)
                PrintEnergyShots(TagId);
        }

        TagId = machineId;
        LastTimeChangeTime = Timer;

        if (TagId == RSMultiplayer.MachineId)
            TagIdHudIndex = 0;
        else if (TagId <= RSMultiplayer.MachineId)
            TagIdHudIndex = TagId + 1;
        else
            TagIdHudIndex = TagId;

        ChainedSparkles.SetUnknownValue();

        SetArrow();

        for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
        {
            if (id != TagId)
                Scene.GetGameObject(id).ProcessMessage(this, Message.Main_MultiplayerTagMoved);
        }
    }

    public int GetNewTagId()
    {
        int newTagId = -1;
        int highestTime = 0;

        for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
        {
            if (Times[id] > highestTime)
            {
                newTagId = id;
                highestTime = Times[id];
            }
        }

        return newTagId;
    }

    public int GetWinnerId()
    {
        if (!IsGameOver)
            throw new Exception("Can only get the winner id if the game is over");

        return TagId;
    }

    public int GetTime(int machineId)
    {
        return Times[machineId];
    }

    // Unused
    public void AddTime(int machineId, int time)
    {
        Times[machineId] += time;
        LastTimeChangeTime = Timer;

        switch (MultiplayerInfo.GameType)
        {
            case MultiplayerGameType.RayTag:
                if (Times[machineId] > 60)
                    Times[machineId] = 60;
                break;
            
            case MultiplayerGameType.CatAndMouse:
                if (Times[machineId] >= 60)
                {
                    Times[machineId] = 60;
                    Win(machineId);
                }
                break;

            case MultiplayerGameType.CaptureTheFlag when Engine.Settings.Platform == Platform.NGage:
                if (CaptureTheFlagTime > 360)
                    CaptureTheFlagTime = 360;
                break;

            case MultiplayerGameType.Missile:
            default:
                throw new InvalidOperationException("Invalid game type");
        }

        PrintTime();
    }

    public void RemoveTime(int machineId, int time)
    {
        if (time < Times[machineId])
        {
            Times[machineId] -= time;
        }
        else
        {
            Times[machineId] = 0;

            if (MultiplayerInfo.GameType == MultiplayerGameType.RayTag)
                GameOver(machineId);
        }

        LastTimeChangeTime = Timer;
        PrintTime();
    }

    public void AddEnergyShots(int machineId, int energyShots)
    {
        EnergyShots[machineId] += energyShots;

        if (EnergyShots[machineId] > 99)
            EnergyShots[machineId] = 99;

        if (machineId == MultiplayerManager.MachineId)
            PrintEnergyShots(machineId);
    }

    public void DecrementEnergyShots(int machineId, int energyShots)
    {
        if (energyShots < EnergyShots[machineId])
            EnergyShots[machineId] -= energyShots;
        else
            EnergyShots[machineId] = 0;

        if (machineId == MultiplayerManager.MachineId)
            PrintEnergyShots(machineId);
    }

    // Tag
    public void GameOver(int machineId)
    {
        int timedOutPlayers = 0;
        int lastAlivePlayer = -1;
        for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
        {
            if (Times[id] == 0)
                timedOutPlayers++;
            else
                lastAlivePlayer = id;
        }

        // Game over - only one player left alive
        if (lastAlivePlayer != -1 && timedOutPlayers == MultiplayerManager.PlayersCount - 1)
        {
            if (lastAlivePlayer != MultiplayerManager.MachineId && machineId == MultiplayerManager.MachineId)
                IsPlayerDead = true;

            TagIdHudIndex = MachineIdToHudIndex(lastAlivePlayer);

            IsGameOver = true;
            TagId = lastAlivePlayer;

            removePlayer(machineId);
            removePlayer(lastAlivePlayer);

            ChainedSparkles.SetUnknownValue();

            for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
                Scene.GetGameObject(id).ProcessMessage(this, Message.Main_MultiplayerGameOver);
        }
        else
        {
            if (machineId == MultiplayerManager.MachineId)
            {
                IsPlayerDead = true;
                GloboxCountdown = 0;
            }

            removePlayer(machineId);

            if (TagId == machineId)
                SetTagId(GetNewTagId());

            Scene.GetGameObject(machineId).ProcessMessage(this, Message.Exploded);
        }

        int hudIndex = MachineIdToHudIndex(machineId);

        PlayerIcons[hudIndex].CurrentAnimation = machineId + 4;
        foreach (AnimatedObject digit in TimerDigits[hudIndex])
            digit.CurrentAnimation = 15;

        if (IsGameOver)
            State.MoveTo(Fsm_GameOver);

        // Helper method
        void removePlayer(int machineIdToRemove)
        {
            AlivePlayersCount--;
            PlayerRanks[AlivePlayersCount] = machineIdToRemove;
        }
    }

    // Cat and Mouse
    public void Win(int machineId)
    {
        IsGameOver = true;
        TagId = machineId;

        ChainedSparkles.SetUnknownValue();

        TagIdHudIndex = MachineIdToHudIndex(machineId);

        // Update the player icons
        for (int hudIndex = 0; hudIndex < MultiplayerManager.PlayersCount; hudIndex++)
        {
            if (hudIndex != TagIdHudIndex)
            {
                PlayerIcons[hudIndex].CurrentAnimation += 4;

                foreach (AnimatedObject digit in TimerDigits[hudIndex])
                    digit.CurrentAnimation = 15;
            }
        }

        // Sort the player animations based on the times
        int[] times = new int[Times.Length];
        Array.Copy(Times, times, Times.Length);
        for (int i = 0; i < MultiplayerManager.PlayersCount; i++) 
        {
            for (int j = 0; j < MultiplayerManager.PlayersCount - 1; j++) 
            {
                if (times[j] < times[j + 1])
                {
                    (times[j], times[j + 1]) = (times[j + 1], times[j]);
                    (PlayerRanks[j], PlayerRanks[j + 1]) = (PlayerRanks[j + 1], PlayerRanks[j]);
                }
            }
        }

        for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
            Scene.GetGameObject(id).ProcessMessage(this, Message.Main_MultiplayerGameOver);

        State.MoveTo(Fsm_GameOver);
    }

    public void GameOverCaptureTheFlag1(int machineId)
    {
        throw new NotImplementedException();
    }

    public void GameOverCaptureTheFlag2()
    {
        throw new NotImplementedException();
    }

    public void InitGlobox(int machineId)
    {
        GloboxCountdown = 360;
        GloboxMachineId = machineId;
    }

    public void PrintTime()
    {
        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
        {
            int v1 = CaptureTheFlagTime / 60;
            int v2 = CaptureTheFlagTime + v1 * -60;
            const int hudIndex = 0;

            TimerDigits[hudIndex][0].CurrentAnimation = v1;

            if (v2 < 10)
            {
                TimerDigits[hudIndex][1].CurrentAnimation = 0;
                TimerDigits[hudIndex][2].CurrentAnimation = v2;
            }
            else
            {
                TimerDigits[hudIndex][1].CurrentAnimation = v2 / 10;
                TimerDigits[hudIndex][2].CurrentAnimation = v2 % 10;
            }
        }
        else
        {
            for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
            {
                if ((MultiplayerInfo.GameType != MultiplayerGameType.RayTag || Times[id] != 0) &&
                    (MultiplayerInfo.GameType != MultiplayerGameType.CatAndMouse || !IsGameOver || Times[id] == 60))
                {
                    int v1 = Times[id] / 60;
                    int v2 = Times[id] + v1 * -60;
                    int hudIndex = MachineIdToHudIndex(id);

                    TimerDigits[hudIndex][0].CurrentAnimation = v1;

                    if (v2 < 10)
                    {
                        TimerDigits[hudIndex][1].CurrentAnimation = 0;
                        TimerDigits[hudIndex][2].CurrentAnimation = v2;
                    }
                    else
                    {
                        TimerDigits[hudIndex][1].CurrentAnimation = v2 / 10;
                        TimerDigits[hudIndex][2].CurrentAnimation = v2 % 10;
                    }
                }
            }
        }
    }

    public void PrintEnergyShots(int machineId)
    {
        EnergyShotsCounterDigits[0].CurrentAnimation = EnergyShots[machineId] / 10;
        EnergyShotsCounterDigits[1].CurrentAnimation = EnergyShots[machineId] % 10;
    }

    // Unused
    public void PrintInfo(string info, int time)
    {
        Unused2 = 0;
        Unused3 = time;

        if (time == -1)
            Unused2 = -1;

        // NOTE: The rest is not implemented in the final version of the game
    }

    public void MoveBouncingSprites()
    {
        float signTargetY = Engine.Settings.Platform switch
        {
            Platform.GBA => 36,
            Platform.NGage => 72,
            _ => throw new UnsupportedPlatformException()
        };

        if (IsPlayerDead)
        {
            if (GameOverSign.ScreenPos.Y > signTargetY || (GameOverSign.ScreenPos.Y > -8 && IsGameOver))
                GameOverSign.ScreenPos -= new Vector2(0, 2);
        }

        if (Engine.Settings.Platform == Platform.NGage && UnknownCaptureTheFlagValue2)
        {
            if (SuddenDeathSign.ScreenPos.Y > signTargetY || (SuddenDeathSign.ScreenPos.Y > -8 && SuddenDeathDisplayCountdown == 0))
                SuddenDeathSign.ScreenPos -= new Vector2(0, 2);
            else if (SuddenDeathSign.ScreenPos.Y > -8 && SuddenDeathDisplayCountdown != 0)
                SuddenDeathDisplayCountdown--;

        }

        if (MultiplayerInfo.GameType == MultiplayerGameType.RayTag)
        {
            if (TagId == MultiplayerManager.MachineId)
            {
                // Move down
                if (EnergyShotsCounterFrame.ScreenPos.Y < 38)
                {
                    EnergyShotsCounterFrame.ScreenPos += new Vector2(0, 2);
                    foreach (AnimatedObject digit in EnergyShotsCounterDigits)
                        digit.ScreenPos += new Vector2(0, 2);
                    EnergyShotsIcon.ScreenPos += new Vector2(0, 2);
                }
            }
            else if (Times[MultiplayerManager.MachineId] != 0)
            {
                // Move up
                if (EnergyShotsCounterFrame.ScreenPos.Y > 0)
                {
                    EnergyShotsCounterFrame.ScreenPos -= new Vector2(0, 2);
                    foreach (AnimatedObject digit in EnergyShotsCounterDigits)
                        digit.ScreenPos -= new Vector2(0, 2);
                    EnergyShotsIcon.ScreenPos -= new Vector2(0, 2);
                }
            }
        }
        else if (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse)
        {
            if (TagId != MultiplayerManager.MachineId)
            {
                // Move down
                if (Times[TagId] != 60 && EnergyShotsCounterFrame.ScreenPos.Y < 38)
                {
                    EnergyShotsCounterFrame.ScreenPos += new Vector2(0, 2);
                    foreach (AnimatedObject digit in EnergyShotsCounterDigits)
                        digit.ScreenPos += new Vector2(0, 2);
                    EnergyShotsIcon.ScreenPos += new Vector2(0, 2);
                }
            }
            else
            {
                // Move up
                if (EnergyShotsCounterFrame.ScreenPos.Y > 0)
                {
                    EnergyShotsCounterFrame.ScreenPos -= new Vector2(0, 2);
                    foreach (AnimatedObject digit in EnergyShotsCounterDigits)
                        digit.ScreenPos -= new Vector2(0, 2);
                    EnergyShotsIcon.ScreenPos -= new Vector2(0, 2);
                }
            }
        }
    }

    public void DrawItemEffect(AnimationPlayer animationPlayer)
    {
        Rayman rayman = (Rayman)Scene.MainActor;

        if (rayman != null)
        {
            Vector2 mainActorPos = rayman.ScreenPosition;
            float offsetY = rayman.State == rayman.Fsm_Crouch ? 30 : 50;

            ReverseControlsEffectIcon.ScreenPos = mainActorPos + new Vector2(-5, -offsetY);
            InvisibilityEffectIcon.ScreenPos = mainActorPos + new Vector2(0, -offsetY - 5);
            GloboxEffectIcon.ScreenPos = mainActorPos + new Vector2(5, -offsetY + 5);

            if (rayman.ReverseControlsTimer != 0)
                animationPlayer.Play(ReverseControlsEffectIcon);

            if (rayman.InvisibilityTimer is >= 56 and <= 424)
                animationPlayer.Play(InvisibilityEffectIcon);

            if (GloboxCountdown != 0 && GloboxMachineId != MultiplayerManager.MachineId)
                animationPlayer.Play(GloboxEffectIcon);
        }
    }

    public void SetArrow()
    {
        PlayerArrow.CurrentAnimation = 1;
        PlayerArrow.BasePaletteIndex = TagId * 2 + 1;

        if (Engine.Settings.Platform == Platform.GBA)
        {
            if (TagIdHudIndex is 0 or 3)
            {
                PlayerArrow.ScreenPos = PlayerArrow.ScreenPos with { X = 94 };
                PlayerArrow.FlipX = true;
            }
            else
            {
                PlayerArrow.ScreenPos = PlayerArrow.ScreenPos with { X = -94 };
                PlayerArrow.HorizontalAnchor = HorizontalAnchorMode.Right;
                PlayerArrow.FlipX = false;
            }

            if (TagIdHudIndex is 0 or 1)
            {
                PlayerArrow.ScreenPos = PlayerArrow.ScreenPos with { Y = 20 };
            }
            else
            {
                PlayerArrow.ScreenPos = PlayerArrow.ScreenPos with { Y = -4 };
                PlayerArrow.VerticalAnchor = VerticalAnchorMode.Bottom;
            }
        }
        else if (Engine.Settings.Platform == Platform.NGage)
        {
            if (TagIdHudIndex is 0 or 2)
            {
                PlayerArrow.ScreenPos = PlayerArrow.ScreenPos with { X = 67 };
                PlayerArrow.FlipX = true;
            }
            else
            {
                PlayerArrow.ScreenPos = PlayerArrow.ScreenPos with { X = -67 };
                PlayerArrow.HorizontalAnchor = HorizontalAnchorMode.Right;
                PlayerArrow.FlipX = false;
            }

            if (TagIdHudIndex is 0 or 1)
            {
                PlayerArrow.ScreenPos = PlayerArrow.ScreenPos with { Y = 14 };
            }
            else
            {
                PlayerArrow.ScreenPos = PlayerArrow.ScreenPos with { Y = -14 };
                PlayerArrow.VerticalAnchor = VerticalAnchorMode.Bottom;
            }
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    public override void Load()
    {
        AnimatedObjectResource timersResource = Storage.LoadResource<AnimatedObjectResource>(GameResource.MultiplayerTimerAnimations);
        AnimatedObjectResource iconsResource = Storage.LoadResource<AnimatedObjectResource>(GameResource.MultiplayerIconAnimations);
        AnimatedObjectResource playerIconsResource = Storage.LoadResource<AnimatedObjectResource>(GameResource.MultiplayerPlayerIconAnimations);
        AnimatedObjectResource countdownResource = Storage.LoadResource<AnimatedObjectResource>(GameResource.MultiplayerCountdownAnimations);
        AnimatedObjectResource gameOverSignResource = Storage.LoadResource<AnimatedObjectResource>(GameResource.MultiplayerGameOverSignAnimations);
        AnimatedObjectResource itemsResource = Storage.LoadResource<AnimatedObjectResource>(GameResource.MultiplayerItemAnimations);

        TimerFrames = new AnimatedObject[RSMultiplayer.MaxPlayersCount];
        for (int i = 0; i < TimerFrames.Length; i++)
        {
            TimerFrames[i] = new AnimatedObject(timersResource, timersResource.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 4,
                RenderContext = Scene.HudRenderContext,
            };
        }

        TimerFrames[0].CurrentAnimation = 10;
        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            TimerFrames[0].ScreenPos = new Vector2(66, 18);
        else
            TimerFrames[0].ScreenPos = new Vector2(13, 18);
        
        TimerFrames[1].CurrentAnimation = 11;
        TimerFrames[1].ScreenPos = new Vector2(-13, 18);
        TimerFrames[1].HorizontalAnchor = HorizontalAnchorMode.Right;

        // 2 and 3 have reversed positions between GBA and N-Gage
        if (Engine.Settings.Platform == Platform.GBA)
        {
            TimerFrames[2].CurrentAnimation = 11;
            TimerFrames[2].ScreenPos = new Vector2(-13, -8);
            TimerFrames[2].HorizontalAnchor = HorizontalAnchorMode.Right;
            TimerFrames[2].VerticalAnchor = VerticalAnchorMode.Bottom;

            TimerFrames[3].CurrentAnimation = 10;
            TimerFrames[3].ScreenPos = new Vector2(13, -8);
            TimerFrames[3].VerticalAnchor = VerticalAnchorMode.Bottom;
        }
        else if (Engine.Settings.Platform == Platform.NGage)
        {
            TimerFrames[2].CurrentAnimation = 10;
            TimerFrames[2].ScreenPos = new Vector2(13, -8);
            TimerFrames[2].VerticalAnchor = VerticalAnchorMode.Bottom;

            TimerFrames[3].CurrentAnimation = 11;
            TimerFrames[3].ScreenPos = new Vector2(-13, -8);
            TimerFrames[3].HorizontalAnchor = HorizontalAnchorMode.Right;
            TimerFrames[3].VerticalAnchor = VerticalAnchorMode.Bottom;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }

        TimerDigits = new AnimatedObject[RSMultiplayer.MaxPlayersCount][];
        for (int i = 0; i < TimerDigits.Length; i++)
        {
            TimerDigits[i] = new AnimatedObject[3];
            for (int j = 0; j < TimerDigits[i].Length; j++)
            {
                TimerDigits[i][j] = new AnimatedObject(timersResource, timersResource.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 0,
                    ObjPriority = 3,
                    RenderContext = Scene.HudRenderContext,
                };

                if (i < 2)
                {
                    TimerDigits[i][j].ScreenPos = TimerDigits[i][j].ScreenPos with { Y = 18 };
                }
                else
                {
                    TimerDigits[i][j].ScreenPos = TimerDigits[i][j].ScreenPos with { Y = -8 };
                    TimerDigits[i][j].VerticalAnchor = VerticalAnchorMode.Bottom;
                }
            }
        }

        void setTimerDigitPositions(int i, float x, HorizontalAnchorMode anchor)
        {
            TimerDigits[i][0].ScreenPos = TimerDigits[i][0].ScreenPos with { X = x };
            TimerDigits[i][1].ScreenPos = TimerDigits[i][1].ScreenPos with { X = x + 17 };
            TimerDigits[i][2].ScreenPos = TimerDigits[i][2].ScreenPos with { X = x + 27 };
            
            TimerDigits[i][0].HorizontalAnchor = anchor;
            TimerDigits[i][1].HorizontalAnchor = anchor;
            TimerDigits[i][2].HorizontalAnchor = anchor;
        }

        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            setTimerDigitPositions(0, 78, HorizontalAnchorMode.Left);
        else
            setTimerDigitPositions(0, 25, HorizontalAnchorMode.Left);

        setTimerDigitPositions(1, -58, HorizontalAnchorMode.Right);

        // 2 and 3 have reversed positions between GBA and N-Gage
        if (Engine.Settings.Platform == Platform.GBA)
        {
            setTimerDigitPositions(2, -58, HorizontalAnchorMode.Right);
            setTimerDigitPositions(3, 25, HorizontalAnchorMode.Left);
        }
        else if (Engine.Settings.Platform == Platform.NGage)
        {
            setTimerDigitPositions(2, 25, HorizontalAnchorMode.Left);
            setTimerDigitPositions(3, -58, HorizontalAnchorMode.Right);
        }
        else
        {
            throw new UnsupportedPlatformException();
        }

        TimerColons = new AnimatedObject[RSMultiplayer.MaxPlayersCount];
        for (int i = 0; i < TimerColons.Length; i++)
        {
            TimerColons[i] = new AnimatedObject(timersResource, timersResource.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 3,
                CurrentAnimation = 16,
                RenderContext = Scene.HudRenderContext,
            };

            if (i < 2)
            {
                TimerColons[i].ScreenPos = TimerColons[i].ScreenPos with { Y = 18 };
            }
            else
            {
                TimerColons[i].ScreenPos = TimerColons[i].ScreenPos with { Y = -8 };
                TimerColons[i].VerticalAnchor = VerticalAnchorMode.Bottom;
            }
        }

        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            TimerColons[0].ScreenPos = TimerColons[0].ScreenPos with { X = 86 };
        else
            TimerColons[0].ScreenPos = TimerColons[0].ScreenPos with { X = 33 };
        
        TimerColons[1].ScreenPos = TimerColons[1].ScreenPos with { X = -50 };
        TimerColons[1].HorizontalAnchor = HorizontalAnchorMode.Right;

        // 2 and 3 have reversed positions between GBA and N-Gage
        if (Engine.Settings.Platform == Platform.GBA)
        {
            TimerColons[2].ScreenPos = TimerColons[2].ScreenPos with { X = -50 };
            TimerColons[2].HorizontalAnchor = HorizontalAnchorMode.Right;

            TimerColons[3].ScreenPos = TimerColons[3].ScreenPos with { X = 33 };
        }
        else if (Engine.Settings.Platform == Platform.NGage)
        {
            TimerColons[2].ScreenPos = TimerColons[2].ScreenPos with { X = 33 };
            
            TimerColons[3].ScreenPos = TimerColons[3].ScreenPos with { X = -50 };
            TimerColons[3].HorizontalAnchor = HorizontalAnchorMode.Right;
        }
        else
        {
            throw new UnsupportedPlatformException();
        }

        if (Engine.Settings.Platform != Platform.NGage || MultiplayerInfo.GameType != MultiplayerGameType.CaptureTheFlag)
        {
            EnergyShotsCounterFrame = new AnimatedObject(timersResource, timersResource.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 6,
                CurrentAnimation = 12,
                RenderContext = Scene.HudRenderContext,
            };

            if (MultiplayerInfo.GameType == MultiplayerGameType.RayTag)
                EnergyShotsCounterFrame.ScreenPos = new Vector2(26, TagId == MultiplayerManager.MachineId ? 38 : 0);
            else if (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse)
                EnergyShotsCounterFrame.ScreenPos = new Vector2(26, TagId == MultiplayerManager.MachineId ? 0 : 38);
        }

        EnergyShotsCounterDigits = new AnimatedObject[2];
        for (int i = 0; i < EnergyShotsCounterDigits.Length; i++)
        {
            EnergyShotsCounterDigits[i] = new AnimatedObject(timersResource, timersResource.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 5,
                RenderContext = Scene.HudRenderContext,
            };

            if (MultiplayerInfo.GameType == MultiplayerGameType.RayTag)
                EnergyShotsCounterDigits[i].ScreenPos = new Vector2(0, TagId == MultiplayerManager.MachineId ? 37 : -1);
            else if (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse)
                EnergyShotsCounterDigits[i].ScreenPos = new Vector2(0, TagId == MultiplayerManager.MachineId ? -1 : 37);
        }

        EnergyShotsCounterDigits[0].ScreenPos = EnergyShotsCounterDigits[0].ScreenPos with { X = 33 };
        EnergyShotsCounterDigits[1].ScreenPos = EnergyShotsCounterDigits[1].ScreenPos with { X = 42 };

        // Hacky code, but the game uses Rayman's palettes for the icons
        iconsResource.Palettes = Scene.MainActor.AnimatedObject.Resource.Palettes;

        EnergyShotsIcon = new AnimatedObject(iconsResource, iconsResource.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 6,
            CurrentAnimation = 0,
            RenderContext = Scene.HudRenderContext,
        };

        if (MultiplayerInfo.GameType == MultiplayerGameType.RayTag)
            EnergyShotsIcon.ScreenPos = new Vector2(10, TagId == MultiplayerManager.MachineId ? 38 : 0);
        else if (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse)
            EnergyShotsIcon.ScreenPos = new Vector2(10, TagId == MultiplayerManager.MachineId ? 0 : 38);

        EnergyShotsIcon.BasePaletteIndex = MultiplayerManager.MachineId * 2 + 1;

        if (Engine.Settings.Platform != Platform.NGage || MultiplayerInfo.GameType != MultiplayerGameType.CaptureTheFlag)
        {
            PlayerArrow = new AnimatedObject(iconsResource, iconsResource.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 2,
                RenderContext = Scene.HudRenderContext,
            };

            SetArrow();
        }

        PlayerIcons = new AnimatedObject[RSMultiplayer.MaxPlayersCount];
        for (int i = 0; i < PlayerIcons.Length; i++)
        {
            PlayerIcons[i] = new AnimatedObject(playerIconsResource, playerIconsResource.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 2,
                RenderContext = Scene.HudRenderContext,
            };
        }

        if (Engine.Settings.Platform == Platform.NGage && 
            MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag && 
            MultiplayerInfo.CaptureTheFlagMode == CaptureTheFlagMode.Teams)
        {
            if (MultiplayerManager.MachineId is 0 or 1)
            {
                PlayerIcons[0].CurrentAnimation = 0;
                PlayerIcons[1].CurrentAnimation = 2;
                PlayerIcons[2].CurrentAnimation = 1;
                PlayerIcons[3].CurrentAnimation = 3;
            }
            else
            {
                PlayerIcons[0].CurrentAnimation = 2;
                PlayerIcons[1].CurrentAnimation = 0;
                PlayerIcons[2].CurrentAnimation = 3;
                PlayerIcons[3].CurrentAnimation = 1;
            }

            PlayerIcons[0].ScreenPos = new Vector2(0, 6);

            PlayerIcons[1].ScreenPos = new Vector2(-17, 6);
            PlayerIcons[1].HorizontalAnchor = HorizontalAnchorMode.Right;

            PlayerIcons[2].ScreenPos = new Vector2(0, 20);

            PlayerIcons[3].ScreenPos = new Vector2(-17, 20);
            PlayerIcons[3].HorizontalAnchor = HorizontalAnchorMode.Right;
        }
        else
        {
            for (int i = 0; i < PlayerIcons.Length; i++)
                PlayerIcons[i].CurrentAnimation = HudIndexToMachineId(i);

            PlayerIcons[0].ScreenPos = new Vector2(0, 6);

            PlayerIcons[1].ScreenPos = new Vector2(-17, 6);
            PlayerIcons[1].HorizontalAnchor = HorizontalAnchorMode.Right;

            // 2 and 3 have reversed positions between GBA and N-Gage
            if (Engine.Settings.Platform == Platform.GBA)
            {
                PlayerIcons[2].ScreenPos = new Vector2(-17, -20);
                PlayerIcons[2].HorizontalAnchor = HorizontalAnchorMode.Right;
                PlayerIcons[2].VerticalAnchor = VerticalAnchorMode.Bottom;

                PlayerIcons[3].ScreenPos = new Vector2(0, -20);
                PlayerIcons[3].VerticalAnchor = VerticalAnchorMode.Bottom;
            }
            else if (Engine.Settings.Platform == Platform.NGage)
            {
                PlayerIcons[2].ScreenPos = new Vector2(0, -20);
                PlayerIcons[2].VerticalAnchor = VerticalAnchorMode.Bottom;

                PlayerIcons[3].ScreenPos = new Vector2(-17, -20);
                PlayerIcons[3].HorizontalAnchor = HorizontalAnchorMode.Right;
                PlayerIcons[3].VerticalAnchor = VerticalAnchorMode.Bottom;
            }
            else
            {
                throw new UnsupportedPlatformException();
            }
        }

        // NOTE: In the original game this is set up so if Load is called again then it's reloaded rather than recreated
        StartCountdown = new AnimatedObject(countdownResource, countdownResource.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 2,
            ScreenPos = Engine.Settings.Platform switch
            {
                Platform.GBA => new Vector2(0, 10),
                Platform.NGage => new Vector2(0, 0),
                _ => throw new ArgumentOutOfRangeException()
            },
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Center,
            RenderContext = Scene.HudRenderContext,
        };

        if (StartCountdownValue != 0)
            StartCountdown.CurrentAnimation = StartCountdownValue - 1;

        GameOverSign = new AnimatedObject(gameOverSignResource, gameOverSignResource.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 0,
            ObjPriority = 2,
            CurrentAnimation = 0,
            ScreenPos = Engine.Settings.Platform switch
            {
                Platform.GBA => new Vector2(10, IsPlayerDead ? 36 : 180),
                Platform.NGage => new Vector2(0, IsPlayerDead ? 36 : 180),
                _ => throw new ArgumentOutOfRangeException()
            },
            HorizontalAnchor = HorizontalAnchorMode.Center,
            VerticalAnchor = VerticalAnchorMode.Scale,
            RenderContext = Scene.HudRenderContext,
        };

        if (Engine.Settings.Platform == Platform.NGage)
        {
            AnimatedObjectResource suddenDeathSignResource = Storage.LoadResource<AnimatedObjectResource>(GameResource.NGageMultiplayerSuddenDeathSignAnimations);

            SuddenDeathSign = new AnimatedObject(suddenDeathSignResource, suddenDeathSignResource.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 2,
                CurrentAnimation = Localization.LanguageUiIndex,
                ScreenPos = new Vector2(0, UnknownCaptureTheFlagValue2 ? 36 : 180),
                HorizontalAnchor = HorizontalAnchorMode.Center,
                VerticalAnchor = VerticalAnchorMode.Scale,
                RenderContext = Scene.HudRenderContext,
            };
        }

        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
        {
            FlagBar.Load();
            FlagBar.Set();
        }
        else
        {
            Globox = new AnimatedObject(itemsResource, itemsResource.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 2,
                CurrentAnimation = 5,
                ScreenPos = new Vector2(100, -9),
                HorizontalAnchor = HorizontalAnchorMode.Scale,
                VerticalAnchor = VerticalAnchorMode.Bottom,

                // Use the original render context as to avoid scaling this (we want it to cover the screen!)
                RenderContext = Engine.OriginalGameRenderContext,
            };

            ReverseControlsEffectIcon = new AnimatedObject(itemsResource, itemsResource.IsDynamic)
            {
                IsFramed = true,
                CurrentAnimation = 8,
                BgPriority = 0,
                ObjPriority = 16,
                RenderContext = Scene.HudRenderContext,
            };
            InvisibilityEffectIcon = new AnimatedObject(itemsResource, itemsResource.IsDynamic)
            {
                IsFramed = true,
                CurrentAnimation = 9,
                BgPriority = 0,
                ObjPriority = 16,
                RenderContext = Scene.HudRenderContext,
            };
            GloboxEffectIcon = new AnimatedObject(itemsResource, itemsResource.IsDynamic)
            {
                IsFramed = true,
                CurrentAnimation = 12,
                BgPriority = 0,
                ObjPriority = 16,
                RenderContext = Scene.HudRenderContext,
            };
        }

        ScoreBar.Load();
        ScoreBar.SetToStayVisible();

        if (Engine.Settings.Platform != Platform.NGage || MultiplayerInfo.GameType != MultiplayerGameType.CaptureTheFlag)
            PrintEnergyShots(TagId);

        PrintTime();
    }

    public override void Init() { }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        MoveBouncingSprites();

        // Helper to make the code cleaner
        void play(AObject obj, bool front)
        {
            if (front)
                animationPlayer.PlayFront(obj);
            else
                animationPlayer.Play(obj);
        }

        if (Engine.Settings.Platform != Platform.NGage || MultiplayerInfo.GameType != MultiplayerGameType.CaptureTheFlag)
        {
            if (EnergyShotsCounterFrame.ScreenPos.Y != 0 && !IsPlayerDead && !IsGameOver)
            {
                play(EnergyShotsCounterFrame, Engine.Settings.Platform == Platform.GBA);
                play(EnergyShotsIcon, Engine.Settings.Platform == Platform.GBA);

                if (EnergyShotsBlinkCountdown == 0 || (EnergyShotsBlinkCountdown & 8) != 0)
                {
                    foreach (AnimatedObject digit in EnergyShotsCounterDigits)
                        play(digit, Engine.Settings.Platform == Platform.GBA);
                }
            }

            play(PlayerArrow, Engine.Settings.Platform == Platform.GBA);
        }

        int timersCount = Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag
            ? 1
            : MultiplayerManager.PlayersCount;
        for (int id = 0; id < timersCount; id++)
        {
            play(TimerFrames[id], Engine.Settings.Platform == Platform.GBA);

            if (id != TagIdHudIndex || (GameTime.ElapsedFrames & 0x20) != 0)
                play(TimerColons[id], Engine.Settings.Platform == Platform.GBA);

            foreach (AnimatedObject digit in TimerDigits[id])
                play(digit, Engine.Settings.Platform == Platform.GBA);
        }

        for (int id = 0; id < MultiplayerManager.PlayersCount; id++)
            play(PlayerIcons[id], Engine.Settings.Platform == Platform.GBA);

        if (GloboxCountdown != 0 && !IsPaused && GloboxMachineId != MultiplayerManager.MachineId)
            animationPlayer.PlayFront(Globox);

        if (StartCountdownValue != 0 && !IsPaused)
            animationPlayer.PlayFront(StartCountdown);

        if (GameOverSign.ScreenPos.Y != 180 && !IsPaused)
            animationPlayer.PlayFront(GameOverSign);

        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
        {
            if (SuddenDeathSign.ScreenPos.Y != 180 && !IsPaused)
                animationPlayer.PlayFront(SuddenDeathSign);
        }

        if (!IsPaused && (MultiplayerInfo.GameType == MultiplayerGameType.CatAndMouse || Times[MultiplayerManager.MachineId] != 0))
            DrawItemEffect(animationPlayer);

        if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            FlagBar.Draw(animationPlayer);

        if (IsGameOver)
        {
            if (ScoreBar.DrawStep == BarDrawStep.Hide)
                ScoreBar.DrawStep = BarDrawStep.MoveIn;

            if (Engine.Settings.Platform == Platform.NGage && MultiplayerInfo.GameType == MultiplayerGameType.CaptureTheFlag)
            {
                if (((FrameMultiCaptureTheFlag)Frame.Current).field_0x5b)
                {
                    if (MultiplayerInfo.CaptureTheFlagMode == CaptureTheFlagMode.Teams)
                        ScoreBar.DrawTeamsScore(animationPlayer, PlayerRanks);
                    else
                        ScoreBar.DrawScore(animationPlayer, PlayerRanks);
                }
            }
            else
            {
                ScoreBar.DrawScore(animationPlayer, PlayerRanks);
            }
        }
    }

    #endregion
}