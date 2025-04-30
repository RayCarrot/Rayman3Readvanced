using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public partial class Machine
{
    public bool Fsm_Init(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                // Set up cog wheel object parts from parent
                if (BossHealth == 4)
                {
                    Rotation = 0;
                    AnimatedObject.AffineMatrix = new AffineMatrix(Rotation, 1, 1);

                    Machine part1 = Scene.GetGameObject<Machine>(Resource.Links[0]!.Value);
                    part1.BossHealth = 2;
                    part1.Parent = this;
                    part1.Rotation = 0;
                    part1.AnimatedObject.AffineMatrix = new AffineMatrix(part1.Rotation, 1, 1);

                    Machine part2 = Scene.GetGameObject<Machine>(Resource.Links[1]!.Value);
                    part2.BossHealth = 3;
                    part2.Parent = this;
                    part2.Rotation = 0;
                    part2.AnimatedObject.AffineMatrix = new AffineMatrix(part2.Rotation, 1, 1);
                    part2.AnimatedObject.IsDoubleAffine = true;

                    Machine part3 = Scene.GetGameObject<Machine>(Resource.Links[2]!.Value);
                    part3.BossHealth = 1;
                    part3.Parent = this;
                    part3.Rotation = 0;
                    part3.AnimatedObject.AffineMatrix = new AffineMatrix(part3.Rotation, 1, 1);
                }

                // Show text box if player has died
                if (GameInfo.LastGreenLumAlive != 0 && GameInfo.CanShowMurfyHelp)
                    InitTextBox();

                // Cog wheel part
                if (BossHealth is 1 or 2 or 3)
                {
                    State.MoveTo(Fsm_CogWheelSpinning);
                    return false;
                }
                // Parent if died at least once
                else if (BossHealth == 4 && GameInfo.LastGreenLumAlive != 0)
                {
                    State.MoveTo(Fsm_CannonFire);
                    return false;
                }
                // Parent first time
                else if (BossHealth == 4 && GameInfo.LastGreenLumAlive == 0)
                {
                    State.MoveTo(Fsm_CannonIntro);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_CogWheelSpinning(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                if (BossHealth == 2)
                    ActionId = Action.CogWheel2;
                else if (BossHealth == 3)
                    ActionId = Action.CogWheel3;

                AnimatedObject.ObjPriority = 30;
                break;

            case FsmAction.Step:
                if (Parent.BossHealth != 0)
                {
                    float scale;
                    if (BossHealth == 3)
                    {
                        Rotation++;
                        scale = MathHelpers.FromFixedPoint(0x9999);
                    }
                    else if (BossHealth == 2)
                    {
                        Rotation -= 2;
                        scale = MathHelpers.FromFixedPoint(0x10000);
                    }
                    else
                    {
                        Rotation += 2;
                        scale = MathHelpers.FromFixedPoint(0x15000);
                    }
                 
                    AnimatedObject.AffineMatrix = new AffineMatrix(Rotation, scale, scale);
                }

                if (CogDestroyed)
                {
                    State.MoveTo(Fsm_PartDestroyed);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_PartDestroyed(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                // Do nothing
                break;

            case FsmAction.Step:
                // Fall down
                Position += new Vector2(0, 4);

                float scale;
                if (BossHealth == 3)
                    scale = MathHelpers.FromFixedPoint(0x9999);
                else
                    scale = MathHelpers.FromFixedPoint(0x10000);

                // The original game has a bug where it doesn't set the scale for the small cog
                if (Engine.Config.FixBugs && BossHealth is not (2 or 3))
                    scale = MathHelpers.FromFixedPoint(0x15000);

                AnimatedObject.AffineMatrix = new AffineMatrix(Rotation, scale, scale);

                if (ScreenPosition.Y > Scene.Resolution.Y)
                {
                    State.MoveTo(Fsm_Init);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                ProcessMessage(this, Message.Destroy);
                break;
        }

        return true;
    }

    public bool Fsm_CannonIntro(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                Scene.MainActor.ProcessMessage(this, Message.Rayman_Stop);
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                if (Timer == 60)
                {
                    Scene.Camera.ProcessMessage(this, Message.Cam_MoveToTarget, Position + new Vector2(-60, 20));
                }
                else if (Timer > 190 && ActionId == Action.CannonIdle2 && IsActionFinished)
                {
                    Scene.Camera.ProcessMessage(this, Message.Cam_MoveToLinkedObject, false);
                }
                else if (ActionId == Action.CannonFire)
                {
                    if (IsActionFinished)
                    {
                        ActionId = Action.CannonIdle2;
                    }
                    else if (AnimatedObject.CurrentFrame == 3)
                    {
                        SpawnHelicopterBomb();
                        SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MachAtk2_Mix02);
                        AnimatedObject.CurrentFrame = 4;
                    }
                }
                else if (ActionId == Action.CannonBeginFire)
                {
                    if (IsActionFinished)
                        ActionId = Action.CannonFire;
                }
                else if (Timer == 130)
                {
                    ActionId = Action.CannonBeginFire;
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MachAtk1_Mix01);
                }

                if (Timer > 190 && ActionId == Action.CannonIdle2 && IsActionFinished)
                {
                    State.MoveTo(Fsm_CannonFire);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Create a checkpoint to avoid showing the intro cutscene each time
                if (GameInfo.LastGreenLumAlive == 0)
                    GameInfo.GreenLumTouchedByRayman(0, new Vector2(191, 136));
                break;
        }

        return true;
    }

    public bool Fsm_CannonFire(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                ActionId = Action.CannonIdle1;
                BossHealth = 3;
                Timer = 0;
                break;

            case FsmAction.Step:
                Timer++;

                if (TextBox != null)
                {
                    if (GameInfo.CanShowMurfyHelp)
                        ManageTextBox();
                    else if (Timer > 30)
                        UnInitTextBox();
                }
                else
                {
                    if (BossHealth == 0)
                    {
                        if (ActionId == Action.CannonFire)
                            ActionId = Action.CannonIdle1;
                    }
                    else if (ActionId == Action.CannonFire)
                    {
                        if (IsActionFinished)
                        {
                            ActionId = Action.CannonIdle2;
                        }
                        else if (AnimatedObject.CurrentFrame == 2)
                        {
                            SpawnHelicopterBomb();
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MachAtk2_Mix02);
                            AnimatedObject.CurrentFrame = 4;
                        }
                    }
                    else if (ActionId == Action.CannonBeginFire)
                    {
                        if (IsActionFinished)
                            ActionId = Action.CannonFire;
                    }
                    // Fire after certain time
                    else if ((Timer > 430 && BossHealth == 3) || 
                             (Timer > 380 && BossHealth == 2) || 
                             (Timer > 330 && BossHealth == 1))
                    {
                        Rayman rayman = (Rayman)Scene.MainActor;
                        if (rayman.State != rayman.Fsm_Cutscene)
                        {
                            ActionId = Action.CannonBeginFire;
                            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MachAtk1_Mix01);
                            Timer = 0;
                        }
                    }
                }

                if (BossHealth == 0)
                {
                    State.MoveTo(Fsm_CannonDestroyed);
                    return false;
                }
                break;

            case FsmAction.UnInit:
                // Do nothing
                break;
        }

        return true;
    }

    public bool Fsm_CannonDestroyed(FsmAction action)
    {
        switch (action)
        {
            case FsmAction.Init:
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__MachAtk1_Mix01);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__MachAtk2_Mix02);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__MachMotr_Mix01, this);
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__ScalDead_Mix02);
                break;

            case FsmAction.Step:
                // End level when Rayman is on the ground
                if (Scene.MainActor.Speed.Y == 0)
                {
                    Rayman rayman = (Rayman)Scene.MainActor;
                    if (rayman.State != rayman.Fsm_Dying && rayman.State != rayman.Fsm_EndMap)
                        rayman.ProcessMessage(this, Message.Rayman_FinishLevel);
                }

                // Fall down
                Position += new Vector2(0, 2);

                // Explode
                if (GameTime.ElapsedFrames % 8 == 0)
                {
                    Explosion explosion = Scene.CreateProjectile<Explosion>(ActorType.Explosion);

                    if (explosion != null)
                    {
                        if (BossHealth % 4 == 0 && BossHealth < 30)
                            explosion.Position = explosion.Position with { X = Position.X + 16 + _explosionOffsets[BossHealth % 8].X };
                        
                        explosion.Position = explosion.Position with { Y = Position.Y + 32 - _explosionOffsets[BossHealth % 8].Y };
                    }

                    BossHealth++;
                }

                if (BossHealth > 30)
                {
                    BossHealth = 0;
                    State.MoveTo(Fsm_PartDestroyed);
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