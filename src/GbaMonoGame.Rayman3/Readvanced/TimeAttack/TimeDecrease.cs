using System;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

[GenerateFsmFields]
public sealed partial class TimeDecrease : BaseActor
{
    public TimeDecrease(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        State.SetTo(_Fsm_Default);
    }

    public uint Timer { get; set; }
    public int Value { get; set; }

    public void SetValue(int value)
    {
        if (value != Value)
        {
            Value = value;
            AnimatedObject.ReplaceSpriteTexture(0, Engine.FrameContentManager.Load<Texture2D>(value switch
            {
                3 => Assets.TimeDecrease3,
                5 => Assets.TimeDecrease5,
                _ => throw new InvalidOperationException("Unsupported time decrease value")
            }));
        }
    }
}