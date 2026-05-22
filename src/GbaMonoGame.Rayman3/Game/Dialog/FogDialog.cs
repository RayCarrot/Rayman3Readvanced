using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Engine2d;

namespace GbaMonoGame.Rayman3;

public class FogDialog : Dialog
{
    public FogDialog(Scene2D scene) : base(scene)
    {
        ShouldDraw = true;
    }

    public AObjectFog Fog { get; set; }
    public float ScrollX { get; set; }
    public int ScrollSpeed { get; set; }

    public bool ShouldDraw { get; set; }

    public override void Load()
    {
        AnimatedObjectResource resource = Rom.Loader.ReadResource<AnimatedObjectResource>(Rayman3DefinedResource.FogAnimations);

        Fog = new AObjectFog(resource, resource.IsDynamic)
        {
            BgPriority = 0,
            ObjPriority = 63,
            Alpha = AlphaCoefficient.FromGbaValue(6),
            RenderContext = Scene.RenderContext,
            BlendMode = BlendMode.AlphaBlend,
        };

        ScrollX = 0;
        ScrollSpeed = 1;
    }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        if (!ShouldDraw) 
            return;

        // If we use visual improvements then we render in the modern mode where we
        // render each sprite one by one, making sure they don't overlap. If not then
        // we use the original rendering using the single-frame animation where they
        // overlap in some places.
        Fog.ModernMode = Engine.Settings.Active.Tweaks.VisualImprovements;

        Vector2 camPos = Scene.Playfield.Camera.Position;
        int height = Scene.Playfield.PhysicalLayer.PixelHeight;

        if (height - 32 < camPos.Y + Scene.Resolution.Y)
        {
            float yPos = height - camPos.Y - 32;
            if (Fog.ModernMode)
                Fog.ScreenPos = new Vector2(-(camPos.X + ScrollX) % AObjectFog.ModernWidth, yPos);
            else
                Fog.ScreenPos = new Vector2(AObjectFog.GbaWidth - (camPos.X + ScrollX) % AObjectFog.GbaWidth, yPos);
            
            animationPlayer.Play(Fog);
        }

        ScrollX += ScrollSpeed / 8f; // NOTE: Game scrolls every 8 frames
        
        if (Fog.ModernMode)
        {
            ScrollX %= AObjectFog.ModernWidth;
        }
        else
        {
            if (ScrollX > AObjectFog.GbaWidth)
                ScrollX = 0;
        }
    }
}