using System;
using GbaMonoGame.Engine2d;
using GbaMonoGame.SourceGenerators;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

[GenerateFsmFields]
public sealed partial class TimeDecreaseProjectile : BaseActor
{
    public TimeDecreaseProjectile(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
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
            AnimatedObject.ReplaceSpriteTexture(0, Engine.Assets.FrameContentManager.Load<Texture2D>(value switch
            {
                3 => Assets.TimeAttack.TimeDecrease3,
                5 => Assets.TimeAttack.TimeDecrease5,
                _ => throw new InvalidOperationException("Unsupported time decrease value")
            }));
        }
    }
}