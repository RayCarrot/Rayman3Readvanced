using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class ChainedSparkles
{
    private bool FsmStep_UpdateAlpha()
    {
        if (RSMultiplayer.IsActive)
        {
            int invisibleActorId = ((FrameMultiSideScroller)Frame.Current).InvisibleActorId;
            if (invisibleActorId != -1)
            {
                BaseActor actor = Scene.GetGameObject<BaseActor>(invisibleActorId);
                AnimatedObject.BlendMode = actor != null && actor == TargetActor ? BlendMode.AlphaBlend : BlendMode.None;
            }
        }

        return true;
    }

    public bool Fsm_InitSwirl(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                AnimatedObject.SetCurrentPriority(false);
                SwirlValue = 0;
                break;

            case FsmAction.Step:
                bool finished = false;

                Position = OriginalTargetActor.Position + new Vector2(
                    x: 16 * MathHelpers.Sin256(SwirlValue),
                    y: 16 * MathHelpers.Cos256(SwirlValue) - 8);

                if (AreSparklesFacingLeft)
                {
                    SwirlValue += 4;
                    if (SwirlValue > 192)
                        finished = true;
                }
                else
                {
                    SwirlValue -= 4;
                    if (SwirlValue < 64)
                        finished = true;
                }

                if (finished)
                {
                    State.MoveTo(Fsm_MoveToTarget);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Wait(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                AnimatedObject.SetCurrentPriority(false);
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                if (Timer == 60)
                {
                    State.MoveTo(Fsm_MoveToTarget);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_MoveToTarget(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ShouldUpdateTarget = false;

                if (RSMultiplayer.IsActive)
                {
                    TargetActor = Scene.GetGameObject<BaseActor>(((FrameMultiSideScroller)Frame.Current).UserInfo.GetTagId());

                    // Why is this here? The actor can never be null.
                    if (Rom.Platform == Platform.NGage && TargetActor == null)
                        ProcessMessage(this, Message.Destroy);
                }
                break;

            case FsmAction.Step:
                Vector2 posDiff = TargetActor.Position - Position - new Vector2(0, 22);
                float value = MathHelpers.Atan2_256(posDiff);

                if (posDiff.X == 0)
                    Position = Position with { X = TargetActor.Position.X };
                else
                    Position += new Vector2(MathHelpers.Cos256(value) * 3, 0);

                if (posDiff.Y == 0)
                    Position = Position with { X = TargetActor.Position.X - 22 };
                else
                    Position += new Vector2(0, MathHelpers.Sin256(value) * 3);

                if (Math.Abs(posDiff.X) + Math.Abs(posDiff.Y) < 2)
                {
                    State.MoveTo(Fsm_SwirlAround);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_SwirlAround(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SwirlValue = 0;
                Timer = 0;
                break;

            case FsmAction.Step:
                if (!FsmStep_UpdateAlpha())
                    return false;

                if (AreSparklesFacingLeft)
                {
                    // Create the illusion of the sparkles flying around Rayman in 3D by changing the priority
                    AnimatedObject.SetCurrentPriority(SwirlValue < 128);

                    Position = TargetActor.Position + new Vector2(
                        x: MathHelpers.Sin256(SwirlValue) * 26,
                        y: MathHelpers.Cos256(SwirlValue) * 6 - 22);

                    SwirlValue -= 3;
                }
                else
                {
                    // Create the illusion of the sparkles flying around Rayman in 3D by changing the priority
                    AnimatedObject.SetCurrentPriority(SwirlValue is >= 64 and < 192);

                    Position = TargetActor.Position + new Vector2(
                        x: MathHelpers.Sin256(SwirlValue) * 6,
                        y: MathHelpers.Cos256(SwirlValue) * 26 - 22);
                    
                    SwirlValue += 3;
                }

                Timer++;

                if (Timer >= TimerTarget)
                {
                    ProcessMessage(this, Message.Destroy);
                    State.MoveTo(Fsm_InitSwirl);
                    return false;
                }

                if (ShouldUpdateTarget)
                {
                    State.MoveTo(Fsm_MoveToTarget);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                if (RSMultiplayer.IsActive)
                    AnimatedObject.BlendMode = BlendMode.None;
                break;
        }

        return true;
    }

    public bool Fsm_NewPower(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SwirlValue = 64;
                Timer = 0;
                break;

            case FsmAction.Step:
                if (Timer < 94)
                {
                    // Create the illusion of the sparkles flying around Rayman in 3D by changing the priority
                    AnimatedObject.SetCurrentPriority(SwirlValue < 128);

                    float x;
                    if (AreSparklesFacingLeft)
                        x = TargetActor.Position.X - MathHelpers.Sin256(SwirlValue) * 26;
                    else
                        x = TargetActor.Position.X + MathHelpers.Sin256(SwirlValue) * 26;

                    Position = new Vector2(x, TargetActor.Position.Y - Timer / 2f);

                    // This wraps at 256 because it's a byte
                    SwirlValue += 6;
                }

                Timer++;

                if (Timer == 40 && SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__OnoWin_Mix02__or__OnoWinRM_Mix02))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__OnoWin_Mix02__or__OnoWinRM_Mix02);

                if (Timer > 125)
                {
                    ProcessMessage(this, Message.Destroy);
                    State.MoveTo(Fsm_NewPower);
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