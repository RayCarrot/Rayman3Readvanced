using System;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

// Original name: Murphy
[GenerateFsmFields]
public sealed partial class Murfy : MovableActor
{
    public Murfy(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        TargetActor = (Rayman)Scene.MainActor;
        ShouldSpawn = false;
        MainActorPosition = Vector2.Zero;
        HasPlayedCutscene = false;
        MoveTextBoxIn = false;
        IsForBonusInWorld1 = true;

        if (Rom.Platform == Platform.NGage)
            NGage_Unused = true;

        State.SetTo(_Fsm_PreInit);
    }

    public BaseActor TargetActor { get; set; }
    public TextBoxDialog TextBox { get; set; }
    public Vector2 TargetPosition { get; set; }
    public Vector2 MainActorPosition { get; set; }
    public Vector2 InitialPosition { get; set; }
    public Vector2 SavedSpeed { get; set; }
    public byte Timer { get; set; }
    public bool MoveTextBoxIn { get; set; }
    public bool HasPlayedCutscene { get; set; }
    public bool IsTargetActorFacingRight { get; set; }
    public bool ShouldSpawn { get; set; }
    public bool NGage_Unused { get; set; } // Unused
    
    public bool IsForBonusInWorld1 { get; set; }
    public bool SavedBlockPause { get; set; }

    private void SetText()
    {
        TextBox.SetCutsceneCharacter(TextBoxCutsceneCharacter.Murfy);

        switch (GameInfo.MapId)
        {
            case MapId.WoodLight_M1:
                TextBox.SetText(0);
                break;

            case MapId.WoodLight_M2:
                Vector2 mainActorPos = Scene.MainActor.Position;
                if (mainActorPos.X < 800)
                    TextBox.SetText(3);
                else if (mainActorPos.X < 2700)
                    TextBox.SetText(2);
                else
                    TextBox.SetText(1);
                break;

            case MapId.FairyGlade_M2:
                TextBox.SetText(5);
                break;

            case MapId.BossMachine:
                TextBox.SetText(13);
                break;

            case MapId.MenhirHills_M1:
                TextBox.SetText(16);
                break;

            case MapId.SanctuaryOfStoneAndFire_M1:
                TextBox.SetText(15);
                break;

            case MapId.ChallengeLy1:
                TextBox.SetText(11);
                break;

            case MapId.ChallengeLy2:
                TextBox.SetText(12);
                break;

            case MapId.World1:
                if (IsForBonusInWorld1)
                    TextBox.SetText(7);
                else
                    TextBox.SetText(14);
                break;

            case MapId.World2:
                TextBox.SetText(8);
                break;

            case MapId.World3:
                TextBox.SetText(9);
                break;

            case MapId.World4:
                TextBox.SetText(10);
                break;

            default:
                throw new Exception("Murfy was not set to be used in this map");
        }
    }

    private void SetTargetPosition()
    {
        if (TargetActor.IsFacingRight)
            TargetPosition = new Vector2(TargetActor.Position.X + 70, TargetActor.Position.Y - 15);
        else
            TargetPosition = new Vector2(TargetActor.Position.X - 70, TargetActor.Position.Y - 15);
    }

    private bool ManageFirstCutscene()
    {
        if (!HasPlayedCutscene && GameInfo.MapId == MapId.WoodLight_M1)
        {
            if (GameInfo.LastGreenLumAlive == 0)
                GameInfo.GreenLumTouchedByRayman(0, new Vector2(130, 264));

            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsAttackedByFist(RaymanBody fist)
    {
        return (fist.Speed.X > 0 && Position.X > fist.Position.X) ||
               (fist.Speed.X < 0 && Position.X < fist.Position.X);
    }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            case Message.Murfy_Spawn:
                ShouldSpawn = true;
                if (MainActorPosition == Vector2.Zero)
                    MainActorPosition = TargetActor.Position;
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        if (State == _Fsm_Init)
        {
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__MurfHeli_Mix01);
        }
        else
        {
            base.Draw(animationPlayer, forceDraw);

            if (AnimatedObject.IsFramed)
            {
                if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__MurfHeli_Mix01))
                    SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__MurfHeli_Mix01);
            }
            else
            {
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Stop__MurfHeli_Mix01);
            }
        }
    }
}