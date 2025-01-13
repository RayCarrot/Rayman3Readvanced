using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;
using Microsoft.Xna.Framework;
using Action = System.Action;

namespace GbaMonoGame.Rayman3;

public class World : FrameWorldSideScroller
{
    public World(MapId mapId) : base(mapId) { }

    // NOTE: The game uses 16, but it only updates every 2 frames. We instead update every frame.
    private const int PaletteFadeMaxValue = 16 * 2;

    public Action CurrentExStepAction { get; set; }
    public Action NextExStepAction { get; set; }

    public uint MurfyTimer { get; set; }
    public TextBoxDialog TextBox { get; set; }
    public byte MurfyLevelCurtainTargetId { get; set; }
    public byte MurfyId { get; set; }

    public int PaletteFadeValue { get; set; }
    public bool FinishedTransitioningOut { get; set; }

    public void InitEntering()
    {
        FinishedTransitioningOut = false;
        Gfx.Color = Color.Black;
        UserInfo.Hide = true;
        CurrentExStepAction = StepEx_MoveInCurtains;
        PaletteFadeValue = 0;
        Gfx.FadeControl = FadeControl.None;
        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Curtain_YoyoMove_Mix02);
    }

    public void InitExiting()
    {
        CurrentExStepAction = StepEx_FadeOut;
        PaletteFadeValue = PaletteFadeMaxValue;
        FinishedTransitioningOut = false;
        UserInfo.Hide = true;
        BlockPause = true;
    }

    public override void Init()
    {
        base.Init();

        UserInfo = new UserInfoWorld(Scene, GameInfo.Level.HasBlueLum);
        Scene.AddDialog(UserInfo, false, false);

        BlockPause = true;

        TextBox = new TextBoxDialog(Scene);
        Scene.AddDialog(TextBox, false, false);

        CurrentExStepAction = StepEx_MoveInCurtains;
        NextExStepAction = null;

        Vector2 camLockOffset = Rom.Platform switch
        {
            Platform.GBA => new Vector2(110, 120),
            Platform.NGage => new Vector2(75, 120),
            _ => throw new UnsupportedPlatformException()
        };

        if (GameInfo.MapId == MapId.World1 && 
            !GameInfo.PersistentInfo.PlayedMurfyWorldHelp)
        {
            MurfyLevelCurtainTargetId = 16;
            MurfyId = 41;

            // Lock the camera on N-Gage, otherwise Murfy is off-screen
            if (Rom.Platform == Platform.NGage)
                Scene.Camera.ProcessMessage(this, Message.Cam_Lock, Scene.GetGameObject(MurfyLevelCurtainTargetId).Position - camLockOffset);

            Scene.MainActor.ProcessMessage(this, Message.Main_EnterCutscene);
            NextExStepAction = StepEx_SpawnMurfy;
            
            GameInfo.PersistentInfo.PlayedMurfyWorldHelp = true;
            GameInfo.Save(GameInfo.CurrentSlot);

            Murfy murfy = Scene.GetGameObject<Murfy>(MurfyId);
            murfy.Position = murfy.Position with { Y = Scene.Playfield.Camera.Position.Y };
            murfy.IsForBonusInWorld1 = false;
        }
        else if (GameInfo.MapId == MapId.World1 && 
                 GameInfo.World1LumsCompleted() &&
                 !GameInfo.PersistentInfo.UnlockedBonus1)
        {
            MurfyLevelCurtainTargetId = 20;
            MurfyId = 41;

            Scene.Camera.ProcessMessage(this, Message.Cam_Lock, Scene.GetGameObject(MurfyLevelCurtainTargetId).Position - camLockOffset);
            Scene.MainActor.ProcessMessage(this, Message.Main_EnterCutscene);
            NextExStepAction = StepEx_SpawnMurfy;

            GameInfo.PersistentInfo.UnlockedBonus1 = true;
            GameInfo.Save(GameInfo.CurrentSlot);

            UserInfo.Hide = true;
        }
        else if (GameInfo.MapId == MapId.World2 && 
                 GameInfo.World2LumsCompleted() &&
                 !GameInfo.PersistentInfo.UnlockedBonus2)
        {
            MurfyLevelCurtainTargetId = Rom.Platform switch
            {
                Platform.GBA => 9,
                Platform.NGage => 10,
                _ => throw new UnsupportedPlatformException()
            };
            MurfyId = 22;

            Scene.Camera.ProcessMessage(this, Message.Cam_Lock, Scene.GetGameObject(MurfyLevelCurtainTargetId).Position - camLockOffset);
            Scene.MainActor.ProcessMessage(this, Message.Main_EnterCutscene);
            NextExStepAction = StepEx_SpawnMurfy;

            GameInfo.PersistentInfo.UnlockedBonus2 = true;
            GameInfo.Save(GameInfo.CurrentSlot);

            UserInfo.Hide = true;
        }
        else if (GameInfo.MapId == MapId.World3 && 
                 GameInfo.World3LumsCompleted() &&
                 !GameInfo.PersistentInfo.UnlockedBonus3)
        {
            MurfyLevelCurtainTargetId = Rom.Platform switch
            {
                Platform.GBA => 14,
                Platform.NGage => 18,
                _ => throw new UnsupportedPlatformException()
            };
            MurfyId = Rom.Platform switch
            {
                Platform.GBA => 22,
                Platform.NGage => 23,
                _ => throw new UnsupportedPlatformException()
            };

            Scene.Camera.ProcessMessage(this, Message.Cam_Lock, Scene.GetGameObject(MurfyLevelCurtainTargetId).Position - camLockOffset);
            Scene.MainActor.ProcessMessage(this, Message.Main_EnterCutscene);
            NextExStepAction = StepEx_SpawnMurfy;

            GameInfo.PersistentInfo.UnlockedBonus3 = true;
            GameInfo.Save(GameInfo.CurrentSlot);

            UserInfo.Hide = true;
        }
        else if (GameInfo.MapId == MapId.World4 && 
                 GameInfo.World4LumsCompleted() &&
                 !GameInfo.PersistentInfo.UnlockedBonus4)
        {
            MurfyLevelCurtainTargetId = 11;
            MurfyId = Rom.Platform switch
            {
                Platform.GBA => 22,
                Platform.NGage => 21,
                _ => throw new UnsupportedPlatformException()
            };

            Scene.Camera.ProcessMessage(this, Message.Cam_Lock, Scene.GetGameObject(MurfyLevelCurtainTargetId).Position - camLockOffset);
            Scene.MainActor.ProcessMessage(this, Message.Main_EnterCutscene);
            NextExStepAction = StepEx_SpawnMurfy;

            GameInfo.PersistentInfo.UnlockedBonus4 = true;
            GameInfo.Save(GameInfo.CurrentSlot);

            UserInfo.Hide = true;
        }

        InitEntering();

        Scene.Playfield.Step();
        Scene.AnimationPlayer.Execute();
    }

    public override void Step()
    {
        base.Step();

        if (CurrentStepAction == Step_Normal)
            CurrentExStepAction?.Invoke();
    }

    #region Steps

    public void StepEx_MoveInCurtains()
    {
        if (UserInfo.HasFinishedMovingInCurtains())
            CurrentExStepAction = StepEx_FadeIn;
    }

    public void StepEx_FadeIn()
    {
        // In the original code this function is actually two step functions
        // that it cycles between every second frame. The first one modifies
        // the background palette and second one the object palette.

        float colorValue = PaletteFadeValue / (float)PaletteFadeMaxValue;
        Gfx.Color = new Color(colorValue, colorValue, colorValue, 1);

        PaletteFadeValue++;

        if (PaletteFadeValue > PaletteFadeMaxValue)
        {
            if (NextExStepAction == null)
                UserInfo.Hide = false;

            BlockPause = false;
            CurrentExStepAction = NextExStepAction;
        }
    }

    public void StepEx_FadeOut()
    {
        // In the original code this function is actually two step functions
        // that it cycles between every second frame. The first one modifies
        // the background palette and second one the object palette.

        float colorValue = PaletteFadeValue / (float)PaletteFadeMaxValue;
        Gfx.Color = new Color(colorValue, colorValue, colorValue, 1);

        if (PaletteFadeValue == 0)
        {
            CurrentExStepAction = StepEx_MoveOutCurtains;
            UserInfo.MoveOutCurtains();
        }
        else
        {
            PaletteFadeValue--;
        }
    }

    public void StepEx_MoveOutCurtains()
    {
        if (UserInfo.HasFinishedMovingOutCurtains())
            FinishedTransitioningOut = true;
    }

    public void StepEx_SpawnMurfy()
    {
        Murfy murfy = Scene.GetGameObject<Murfy>(MurfyId);
        murfy.TargetActor = Scene.GetGameObject<BaseActor>(MurfyLevelCurtainTargetId);
        murfy.ProcessMessage(this, Message.Murfy_Spawn);

        if (MurfyLevelCurtainTargetId == 16)
            CurrentExStepAction = StepEx_MurfyIntroCutscene;
        else
            CurrentExStepAction = StepEx_MurfyBonusLevelCutscene;

        MurfyTimer = 0;
    }

    public void StepEx_MurfyIntroCutscene()
    {
        if (MurfyTimer == 0)
        {
            if (TextBox.IsFinished)
                MurfyTimer = 1;
        }
        else
        {
            if (MurfyTimer > 90)
            {
                Scene.MainActor.ProcessMessage(this, Message.Main_ExitStopOrCutscene);
                
                if (Rom.Platform == Platform.NGage)
                    Scene.Camera.ProcessMessage(this, Message.Cam_Unlock);
                
                UserInfo.Hide = false;
                CurrentExStepAction = null;
                BlockPause = false;
            }

            MurfyTimer++;
        }
    }

    public void StepEx_MurfyBonusLevelCutscene()
    {
        if (MurfyTimer == 0)
        {
            if (TextBox.IsFinished)
            {
                // Unlock curtain
                LevelCurtain levelCurtain = Scene.GetGameObject<LevelCurtain>(MurfyLevelCurtainTargetId);
                levelCurtain.AnimatedObject.BasePaletteIndex = 0;
                levelCurtain.IsLocked = false;
                
                MurfyTimer = 1;
            }
        }
        else
        {
            if (MurfyTimer > 90)
            {
                CurrentExStepAction = StepEx_MurfyFadeOut;
                TransitionsFX.FadeOutInit(2 / 16f);
            }

            MurfyTimer++;
        }
    }

    public void StepEx_MurfyFadeOut()
    {
        if (TransitionsFX.IsFadeOutFinished)
        {
            Scene.Camera.ProcessMessage(this, Message.Cam_SetPosition, Scene.MainActor.Position - new Vector2(120, 120));
            Scene.Camera.ProcessMessage(this, Message.Cam_Unlock);
            UserInfo.Hide = false;
            TransitionsFX.FadeInInit(2 / 16f);
            CurrentExStepAction = StepEx_MurfyFadeIn;
        }
    }

    public void StepEx_MurfyFadeIn()
    {
        if (TransitionsFX.IsFadeInFinished)
        {
            Scene.MainActor.ProcessMessage(this, Message.Main_ExitStopOrCutscene);
            CurrentExStepAction = null;
        }
    }

    #endregion
}