using System;
using System.Linq;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

// Popolopoï
public sealed partial class TimeFreezeItem : MovableActor
{
    public TimeFreezeItem(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        // Load the sprites
        if ((Action)actorResource.FirstActionId == Action.Init_Blue)
        {
            AnimatedObject.ReplaceSpriteTexture(0, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Blue0));
            AnimatedObject.ReplaceSpriteTexture(1, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Blue1));
            AnimatedObject.ReplaceSpriteTexture(2, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Blue2));
            AnimatedObject.ReplaceSpriteTexture(3, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Blue3));
            AnimatedObject.ReplaceSpriteTexture(4, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Blue4));
        }
        else if ((Action)actorResource.FirstActionId == Action.Init_Orange)
        {
            AnimatedObject.ReplaceSpriteTexture(0, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Orange0));
            AnimatedObject.ReplaceSpriteTexture(1, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Orange1));
            AnimatedObject.ReplaceSpriteTexture(2, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Orange2));
            AnimatedObject.ReplaceSpriteTexture(3, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Orange3));
            AnimatedObject.ReplaceSpriteTexture(4, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Orange4));
        }
        else
        {
            throw new Exception("Invalid initial action");
        }

        // Load the sparkles from another level (a bit hacky, but there's sadly no better solution)
        Scene2DResource sceneResource = Rom.LoadResource<Scene2DResource>((int)MapId.Power1);
        Sparkles = new AObjectChain(sceneResource.AlwaysActors.First(x => (ActorType)x.Type == ActorType.ChainedSparkles).Model.AnimatedObject, false);
        Sparkles.Init(6, Position, 0, false);
        Sparkles.CurrentAnimation = 0;
        Sparkles.BgPriority = scene.ActorDrawPriority;
        Sparkles.ObjPriority = 32;
        Sparkles.RenderContext = scene.RenderContext;

        // Save the initial position
        InitialPosition = Position;

        // Set the idle state
        State.SetTo(Fsm_Idle);
    }

    private const int SineWaveLength = 2;
    private const int SineWaveSpeed = 4;

    private const float DeathSpeedY = -4;
    private const float DeathSpeedXMaxDistance = 3f;
    private const int DeathDuration = 30;
    private const int DeathFadeOutStart = 22;
    private const int SparklesFadeOutDuration = 12;

    public AObjectChain Sparkles { get; }
    public Vector2 InitialPosition { get; set; }
    public byte SinValue { get; set; }
    public uint Timer { get; set; }
    public float HitSpeedX { get; set; }

    protected override bool ProcessMessageImpl(object sender, Message message, object param)
    {
        if (base.ProcessMessageImpl(sender, message, param))
            return false;

        // Handle messages
        switch (message)
        {
            // Save the hit speed
            case Message.Actor_Hit:
                RaymanBody raymanBody = (RaymanBody)param;
                HitSpeedX = raymanBody.Speed.X;
                return false;

            default:
                return false;
        }
    }

    public override void Draw(AnimationPlayer animationPlayer, bool forceDraw)
    {
        // Draw the item, unless dead
        if (State != Fsm_Dead)
            base.Draw(animationPlayer, forceDraw);

        // Draw sparkles when dying (force to always draw since the actor's viewbox doesn't correspond to the sparkles!)
        if (State == Fsm_Dying || State == Fsm_Dead)
            Sparkles.Draw(this, animationPlayer, forceDraw: true);
    }
}