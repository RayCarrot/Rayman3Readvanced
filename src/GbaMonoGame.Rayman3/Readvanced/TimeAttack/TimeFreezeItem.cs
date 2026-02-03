using System;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;
using GbaMonoGame.FsmSourceGenerator;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

// Popolopoï
[GenerateFsmFields]
public sealed partial class TimeFreezeItem : MovableActor
{
    public TimeFreezeItem(int instanceId, Scene2D scene, ActorResource actorResource) : base(instanceId, scene, actorResource)
    {
        CreateGeneratedStates();

        // Load the sprites and set the time decrease value
        if ((Action)actorResource.FirstActionId == Action.Init_Decrease3)
        {
            AnimatedObject.ReplaceSpriteTexture(0, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Blue0));
            AnimatedObject.ReplaceSpriteTexture(1, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Blue1));
            AnimatedObject.ReplaceSpriteTexture(2, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Blue2));
            AnimatedObject.ReplaceSpriteTexture(3, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Blue3));
            AnimatedObject.ReplaceSpriteTexture(4, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Blue4));

            TimeDecreaseSecondsValue = 3;
        }
        else if ((Action)actorResource.FirstActionId == Action.Init_Decrease5)
        {
            AnimatedObject.ReplaceSpriteTexture(0, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Orange0));
            AnimatedObject.ReplaceSpriteTexture(1, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Orange1));
            AnimatedObject.ReplaceSpriteTexture(2, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Orange2));
            AnimatedObject.ReplaceSpriteTexture(3, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Orange3));
            AnimatedObject.ReplaceSpriteTexture(4, Engine.FrameContentManager.Load<Texture2D>(Assets.TimeFreezeItem_Orange4));

            TimeDecreaseSecondsValue = 5;
        }
        else
        {
            throw new Exception("Invalid initial action");
        }

        // Save the initial action and position
        InitialAction = (Action)actorResource.FirstActionId;
        InitialPosition = Position;

        // Set the idle state
        State.SetTo(_Fsm_Idle);
    }

    private const int SineWaveLength = 2;
    private const int SineWaveSpeed = 4;

    private const float DeathSpeedY = -4;
    private const float DeathSpeedXMaxDistance = 3f;
    private const int DeathDuration = 30;
    private const int DeathFadeOutStart = 22;
    private const int SparklesFadeOutDuration = 12;

    public int TimeDecreaseSecondsValue { get; }
    public Action InitialAction { get; }
    public Vector2 InitialPosition { get; }
    public byte SinValue { get; set; }
    public uint Timer { get; set; }
    public float HitSpeedX { get; set; }
    public TimeFreezeItemSparkles SparklesProjectile { get; set; }

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
        if (State != _Fsm_Dead)
            base.Draw(animationPlayer, forceDraw);
    }
}