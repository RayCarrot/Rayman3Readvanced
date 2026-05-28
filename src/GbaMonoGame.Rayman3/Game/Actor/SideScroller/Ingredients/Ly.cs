using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;

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
        TextBox.SetText(Rayman3.GameInfo.MapId switch
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
        rayman.SetPower(Rayman3.GameInfo.MapId switch
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
        // NOTE: The game checks the map id of the previous level, such as Woods of Light map 2, since the game changes it
        //       to that after loading the scene. But since we're doing this when initializing the actor we have to use the
        //       power map id instead.
        ReplayData = Rom.Loader.ReadNewPowerReplayData(Rayman3.GameInfo.MapId switch
        {
            MapId.Power1 => 1,
            MapId.Power2 => 2,
            MapId.Power3 => 3,
            MapId.Power5 => 4, // The order swap is intentional
            MapId.Power4 => 5,
            MapId.Power6 => 6,
            _ => throw new Exception("Ly was not set to be used in this level"),
        });
    }
}