using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

public class MenuHorizontalArrows
{
    public MenuHorizontalArrows(RenderContext renderContext, int bgPriority, float scale, VerticalAlignment verticalRenderContextAlignment = VerticalAlignment.Center)
    {
        Scale = scale;
        AnimatedObjectResource multiplayerTypeFrameAnimations = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.MenuMultiplayerTypeFrameAnimations);

        // A bit hacky, but create a new render context for the arrows in order to scale them. We could do it through the
        // affine matrix, but that will misalign the animation sprites.
        RenderContext arrowRenderContext = new FixedResolutionRenderContext(renderContext.Resolution * (1 / scale), verticalAlignment: verticalRenderContextAlignment);
        ArrowLeft = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = bgPriority,
            ObjPriority = 0,
            CurrentAnimation = 1,
            RenderContext = arrowRenderContext,
        };
        ArrowRight = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = bgPriority,
            ObjPriority = 0,
            CurrentAnimation = 0,
            RenderContext = arrowRenderContext,
        };
    }

    private const float Padding = 5;

    public float Scale { get; }

    public AnimatedObject ArrowLeft { get; }
    public AnimatedObject ArrowRight { get; }

    public Vector2 Position { get; set; }
    public float Width { get; set; }

    public void Pause()
    {
        ArrowLeft.Pause();
        ArrowRight.Pause();
    }

    public void Resume()
    {
        ArrowLeft.Resume();
        ArrowRight.Resume();
    }

    public void Start()
    {
        // Start arrow animations on frame 4 since it looks nicer
        ArrowLeft.CurrentFrame = 4;
        ArrowRight.CurrentFrame = 4;
    }

    public void Draw(AnimationPlayer animationPlayer)
    {
        ArrowLeft.ScreenPos = (Position + new Vector2(-Padding, 0)) * (1 / Scale);
        ArrowRight.ScreenPos = (Position + new Vector2(Width + Padding, 1)) * (1 / Scale);

        animationPlayer.Play(ArrowLeft);
        animationPlayer.Play(ArrowRight);
    }
}