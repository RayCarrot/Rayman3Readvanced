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

    protected override bool ProcessMessageImpl(object sender, Message message, object param) => false;

    public override void Load()
    {
        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.FogAnimations);

        Fog = new AObjectFog(resource)
        {
            BgPriority = 0,
            ObjPriority = 63,
            GbaAlpha = 6,
            RenderContext = Scene.RenderContext,
            RenderOptions = { BlendMode = BlendMode.AlphaBlend },
        };

        ScrollX = 0;
        ScrollSpeed = 1;
    }

    public override void Init() { }

    public override void Draw(AnimationPlayer animationPlayer)
    {
        if (!ShouldDraw) 
            return;
        
        Vector2 camPos = Scene.Playfield.Camera.Position;
        int height = Scene.Playfield.PhysicalLayer.PixelHeight;

        if (height - 32 < camPos.Y + Scene.Resolution.Y)
        {
            // What the game does:
            // Fog.ScreenPos = new Vector2(512 - (camPos.X + ScrollX) % 512, height - camPos.Y - 32);
            Fog.ScreenPos = new Vector2(-(camPos.X + ScrollX) % AObjectFog.Width, height - camPos.Y - 32);
            
            animationPlayer.Play(Fog);
        }

        ScrollX += ScrollSpeed / 8f; // NOTE: Game scrolls every 8 frames
        
        // What the game does:
        // if (ScrollX > 512)
        //     ScrollX = 0;
        ScrollX %= AObjectFog.Width;
    }
}