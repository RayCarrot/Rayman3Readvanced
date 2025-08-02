using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3.Readvanced;

public partial class TimeFreezeItem
{
    public bool Fsm_Idle(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Reset values
                Position = InitialPosition;
                SinValue = 0;
                
                // Set the action
                ActionId = Action.Idle;
                break;

            case FsmAction.Step:
                // Move up and down in a sine wave
                float offsetY = MathHelpers.Sin256(SinValue) * SineWaveLength;
                Position = InitialPosition + new Vector2(0, offsetY);
                SinValue += SineWaveSpeed;

                // Check for hit
                if (HitPoints == 0)
                {
                    // Play sound
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__LumRed_Mix03);
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__LumRed_Mix03);
                    
                    // Random pitch (only very slight variation)
                    SoundEventsManager.SetSoundPitch(Rayman3SoundEvent.Play__LumRed_Mix03, Random.GetNumber(192));
                    
                    State.MoveTo(Fsm_Dying);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_Dying(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Change action to the dying one, playing a faster animation
                ActionId = Action.Dying;

                // Move up, with a slight angle to the side dependent on the hit speed
                MechModel.Speed = new Vector2(HitSpeedX / 8 * DeathSpeedXMaxDistance, DeathSpeedY);

                // Enable transparency
                AnimatedObject.RenderOptions.BlendMode = BlendMode.AlphaBlend;
                Sparkles.RenderOptions.BlendMode = BlendMode.AlphaBlend;

                // Enable rotation and scaling
                AnimatedObject.SetFlagUseRotationScaling(true);

                // Reset values
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                // Fade out
                if (Timer > DeathFadeOutStart)
                    AnimatedObject.Alpha -= 1 / (float)(DeathDuration - DeathFadeOutStart);

                // Scale down horizontally
                AnimatedObject.AffineMatrix = new AffineMatrix(0, new Vector2(1 - Timer / (float)DeathDuration, 1));

                if (Timer == DeathDuration)
                {
                    State.MoveTo(Fsm_Dead);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                AnimatedObject.SetFlagUseRotationScaling(false);
                AnimatedObject.AffineMatrix = null;
                break;
        }

        return true;
    }

    public bool Fsm_Dead(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Reset values
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                Sparkles.Alpha -= 1 / (float)SparklesFadeOutDuration;

                if (Timer == SparklesFadeOutDuration)
                {
                    State.MoveTo(Fsm_Idle);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }
}