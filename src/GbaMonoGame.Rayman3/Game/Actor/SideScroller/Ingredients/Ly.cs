using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;

namespace GbaMonoGame.Rayman3;

[GenerateFsmFields]
public sealed partial class Ly : MovableActor
{
    public Ly(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        State.SetTo(_Fsm_Init);
    }

    public TextBoxDialog TextBox { get; set; }
    public ushort Timer { get; set; }
    public JoyPadReplayData ReplayData { get; set; }

    private void SetText()
    {
        TextBox.SetText(GameInfo.MapId switch
        {
            MapId.WoodLight_M2 => 0,
            MapId.BossMachine => 1,
            MapId.EchoingCaves_M2 => 2,
            MapId.SanctuaryOfStoneAndFire_M3 => 3,
            MapId.BossRockAndLava => 4,
            MapId.BossScaleMan => 5,
            _ => throw new Exception("Ly is not set to be used in the current map")
        });
    }

    private void StartCutScene()
    {
        Engine.JoyPad.SetReplayData(ReplayData.Inputs);
        
        Rayman rayman = (Rayman)Scene.MainActor;
        rayman.SetPower(GameInfo.MapId switch
        {
            MapId.WoodLight_M2 => Power.DoubleFist,
            MapId.BossMachine => Power.Grab,
            MapId.EchoingCaves_M2 => Power.WallJump,
            MapId.SanctuaryOfStoneAndFire_M3 => Power.SuperHelico,
            MapId.BossRockAndLava => Power.BodyShot,
            MapId.BossScaleMan => Power.SuperFist,
            _ => throw new Exception("Ly was not set to be used in this level"),
        });
    }

    // Custom override to preload the replay data
    public override void Init(ActorResource actorResource)
    {
        base.Init(actorResource);
        ReplayData = Rom.Loader.ReadNewPowerReplayData(GameInfo.MapId switch
        {
            MapId.WoodLight_M2 => 1,
            MapId.BossMachine => 2,
            MapId.EchoingCaves_M2 => 3,
            MapId.SanctuaryOfStoneAndFire_M3 => 4,
            MapId.BossRockAndLava => 5,
            MapId.BossScaleMan => 6,
            _ => throw new Exception("Ly was not set to be used in this level"),
        });
    }
}