using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Murfy
{
    private bool FsmStep_AdvanceText()
    {
        if (JoyPad.IsButtonJustPressed(Rayman3Input.TextBoxNext))
            TextBox.MoveToNextText();

        if (Engine.ActiveConfig.Tweaks.CanSkipTextBoxes && JoyPad.IsButtonJustPressed(Rayman3Input.TextBoxSkip))
        {
            if (Frame.Current is FrameSideScroller frameSideScroller)
                frameSideScroller.CanPause = false;
            else if (Frame.Current is FrameWorldSideScroller frameWorldSideScroller)
                frameWorldSideScroller.BlockPause = true;
            else
                Debug.Assert(false, "Invalid frame for Murfy actor");

            TextBox.Skip();
        }

        if (TextBox.IsFinished)
        {
            MoveTextBoxIn = false;
            HasPlayedCutscene = true;
            State.MoveTo(_Fsm_Leave);
            return false;
        }

        return true;
    }

    public bool Fsm_PreInit(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                InitialPosition = Position;
                TextBox = Scene.GetRequiredDialog<TextBoxDialog>();
                State.MoveTo(_Fsm_Init);
                return false;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Init(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ShouldSpawn = false;
                Position = InitialPosition;
                break;

            case FsmAction.Step:
                MechModel.Speed = Vector2.Zero;

                if (ShouldSpawn)
                {
                    State.MoveTo(_Fsm_WaitToSpawn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                SetText();
                break;
        }

        return true;
    }

    public bool Fsm_WaitToSpawn(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                MechModel.Speed = Vector2.Zero;
                if (ShouldSpawn)
                {
                    MoveTextBoxIn = false;
                    State.MoveTo(_Fsm_MoveIn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MoveIn(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (TargetActor == Scene.MainActor)
                {
                    ManageFirstCutscene();
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_BeginCutscene);
                    if (TargetActor.Position.X < 120)
                    {
                        ((ActionActor)TargetActor).ChangeAction();
                        TargetActor.AnimatedObject.FlipX = false;
                        ((ActionActor)TargetActor).ChangeAction();
                    }
                }

                if (TargetActor.Position.X < Position.X)
                {
                    ActionId = Action.Fly_Left;
                    TargetPosition = new Vector2(TargetActor.Position.X - 45, TargetActor.Position.Y);
                }
                else
                {
                    ActionId = Action.Fly_Right;
                    TargetPosition = new Vector2(TargetActor.Position.X + 45, TargetActor.Position.Y);
                }

                SavedSpeed = Vector2.Zero;

                TextBox.MoveInOurOut(MoveTextBoxIn);

                if (GameInfo.MapId is not (MapId.World1 or MapId.World2 or MapId.World3 or MapId.World4))
                    ((FrameSideScroller)Frame.Current).UserInfo.MoveOutBars();

                // Don't allow pausing since it uses the same button as skipping
                if (Frame.Current is FrameSideScroller frameSideScroller)
                {
                    SavedBlockPause = !frameSideScroller.CanPause;
                    if (Engine.ActiveConfig.Tweaks.CanSkipTextBoxes)
                        frameSideScroller.CanPause = false;
                }
                else if (Frame.Current is FrameWorldSideScroller frameWorldSideScroller)
                {
                    SavedBlockPause = frameWorldSideScroller.BlockPause;
                    if (Engine.ActiveConfig.Tweaks.CanSkipTextBoxes)
                        frameWorldSideScroller.BlockPause = true;
                }
                else
                {
                    Debug.Assert(false, "Invalid frame for Murfy actor");
                }
                break;

            case FsmAction.Step:
                SetTargetPosition();

                // Set horizontal speed
                if (Position.X > TargetPosition.X + 20)
                {
                    if (ActionId != Action.Fly_Left)
                        ActionId = Action.Fly_Left;

                    if (SavedSpeed.X > 2.5)
                        SavedSpeed -= new Vector2(0.5f, 0);
                    else
                        SavedSpeed = SavedSpeed with { X = -2.5f };
                }
                else if (Position.X < TargetPosition.X - 20)
                {
                    if (ActionId != Action.Fly_Right)
                        ActionId = Action.Fly_Right;

                    if (SavedSpeed.X < 2.5)
                        SavedSpeed += new Vector2(0.5f, 0);
                    else
                        SavedSpeed = SavedSpeed with { X = 2.5f };
                }
                else
                {
                    if (SavedSpeed.X > 0.5)
                    {
                        SavedSpeed -= new Vector2(0.5f, 0);
                    }
                    else if (SavedSpeed.X < -0.5)
                    {
                        SavedSpeed += new Vector2(0.5f, 0);
                    }
                    else
                    {
                        SavedSpeed = SavedSpeed with { X = 0 };

                        if (ActionId is Action.Fly_Right or Action.Fly_Left)
                        {
                            if (Position.X > TargetActor.Position.X && ActionId != Action.Fly_Left)
                                ActionId = Action.Fly_Left;
                            else if (Position.X < TargetActor.Position.X && ActionId != Action.Fly_Right)
                                ActionId = Action.Fly_Right;
                        }
                    }
                }

                // Set vertical speed
                if (Position.Y < TargetPosition.Y - 5)
                {
                    if (SavedSpeed.Y < 2)
                        SavedSpeed += new Vector2(0, 15 / 256f);
                    else
                        SavedSpeed = SavedSpeed with { Y = 2 };
                }
                else if (Position.Y > TargetPosition.Y + 5)
                {
                    if (SavedSpeed.Y <= -2)
                        SavedSpeed = SavedSpeed with { Y = -2 };
                    else
                        SavedSpeed -= new Vector2(0, 15 / 256f);
                }
                else
                {
                    SavedSpeed = SavedSpeed with { Y = 0 };
                }

                // Set speed
                MechModel.Speed = SavedSpeed;

                // If stopped moving
                if (SavedSpeed == Vector2.Zero && ActionId is not (Action.BeginIdle_Right or Action.BeginIdle_Left))
                {
                    State.MoveTo(_Fsm_Talk);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Talk(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = TargetActor.IsFacingLeft ? Action.BeginIdle_Right : Action.BeginIdle_Left;
                IsTargetActorFacingRight = TargetActor.IsFacingRight;
                TextBox.MoveInOurOut(true);
                Timer = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_AdvanceText())
                    return false;

                Timer++;

                SetTargetPosition();
                bool isOutsideTargetArea = Position.Y < TargetPosition.Y - 30 ||
                                           TargetPosition.Y + 70 < Position.Y ||
                                           Position.X < TargetPosition.X - 30 ||
                                           TargetPosition.X + 30 < Position.X;

                if (IsActionFinished && Timer > 120)
                {
                    ActionId = Random.GetNumber(9) switch
                    {
                        0 => IsFacingRight ? Action.Talk1_Right : Action.Talk1_Left,
                        1 => IsFacingRight ? Action.Talk2_Right : Action.Talk2_Left,
                        2 => IsFacingRight ? Action.Talk3_Right : Action.Talk3_Left,
                        3 => IsFacingRight ? Action.Talk4_Right : Action.Talk4_Left,
                        4 => IsFacingRight ? Action.Talk5_Right : Action.Talk5_Left,
                        _ => IsFacingRight ? Action.Talk1_Right : Action.Talk1_Left
                    };

                    Timer = 0;
                }
                else if (IsActionFinished && ActionId is not (Action.Idle_Right or Action.Idle_Left))
                {
                    ActionId = IsFacingRight ? Action.Idle_Right : Action.Idle_Left;
                }

                bool isBeingAttacked = false;
                if (TargetActor == Scene.MainActor)
                {
                    Rayman rayman = (Rayman)TargetActor;
                    for (int i = 0; i < 2; i++)
                    {
                        RaymanBody activeFist = rayman.ActiveBodyParts[i];

                        if (activeFist == null)
                            continue;

                        if (IsAttackedByFist(activeFist))
                            isBeingAttacked = true;
                    }
                }

                // Unused since Rayman can't move while in a cutscene
                if (isOutsideTargetArea)
                {
                    MoveTextBoxIn = true;
                    State.MoveTo(_Fsm_MoveIn);
                    return false;
                }

                // Unused since Rayman can't attack while in a cutscene
                if (isBeingAttacked)
                {
                    State.MoveTo(_Fsm_AvoidAttack);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    // Unused
    public bool Fsm_AvoidAttack(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                TargetPosition = TargetPosition with { Y = Position.Y - 40 };
                Timer = 0;
                ActionId = IsFacingRight ? Action.Fly_Right : Action.Fly_Left;
                break;

            case FsmAction.Step:
                if (!FsmStep_AdvanceText())
                    return false;

                bool isSafe = true;
                Rayman rayman = (Rayman)TargetActor;
                for (int i = 0; i < 2; i++)
                {
                    RaymanBody activeFist = rayman.ActiveBodyParts[i];

                    if (activeFist == null)
                        continue;

                    if (IsAttackedByFist(activeFist))
                        isSafe = false;
                    else
                        Timer = 0;
                }

                Timer++;
                SavedSpeed = SavedSpeed with { Y = TargetPosition.Y < Position.Y ? -4 : 0 };
                MechModel.Speed = new Vector2(0, SavedSpeed.Y);

                if (Timer > 10 && isSafe && Position.Y < TargetPosition.Y + 5)
                {
                    MoveTextBoxIn = true;
                    State.MoveTo(_Fsm_MoveIn);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Leave(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = IsFacingRight ? Action.BeginLeave_Right : Action.BeginLeave_Left;
                SavedSpeed = SavedSpeed with { Y = -1 };
                TextBox.MoveInOurOut(false);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MurfyVO3A_Mix01);
                break;

            case FsmAction.Step:
                if (IsActionFinished && ActionId is Action.BeginLeave_Right or Action.BeginLeave_Left)
                    ActionId = IsFacingRight ? Action.Fly_Right : Action.Fly_Left;

                if (SavedSpeed.Y > -2)
                    SavedSpeed -= new Vector2(0, 0.05859375f);
                SavedSpeed = SavedSpeed with { X = IsFacingRight ? 1 : -1 };
                MechModel.Speed = SavedSpeed;

                if (ScreenPosition.Y < -10)
                {
                    State.MoveTo(_Fsm_Init);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                Position = InitialPosition;
                if (GameInfo.MapId is MapId.ChallengeLy1 or MapId.ChallengeLy2)
                {
                    Debug.Assert(TargetActor == Scene.MainActor, "Murfy incoherent behavior");
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_FinishLevel);
                }
                else if (TargetActor == Scene.MainActor)
                {
                    Scene.MainActor.ProcessMessage(this, Message.Rayman_Resume);
                }

                // Restore being able to pause
                if (Engine.ActiveConfig.Tweaks.CanSkipTextBoxes)
                {
                    if (Frame.Current is FrameSideScroller frameSideScroller)
                        frameSideScroller.CanPause = !SavedBlockPause;
                    else if (Frame.Current is FrameWorldSideScroller frameWorldSideScroller)
                        frameWorldSideScroller.BlockPause = SavedBlockPause;
                    else
                        Debug.Assert(false, "Invalid frame for Murfy actor");
                }
                break;
        }

        return true;
    }
}