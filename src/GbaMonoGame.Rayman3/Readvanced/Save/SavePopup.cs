using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class SavePopup
{
    private const int Padding = 3;
    private const float MoveSpeed = 1.5f;
    private const int MoveInDelayTime = 30;
    private const int WaitTime = 2 * 60;

    public AnimationPlayer AnimationPlayer { get; set; }

    public SpriteTextureObject SaveIcon { get; set; }

    public Vector2 SaveIconScreenPos { get; set; }

    public BarDrawStep DrawStep { get; set; }
    public float MinX { get; set; }
    public float MaxX { get; set; }
    public float OffsetX { get; set; }
    public float WaitTimer { get; set; }

    public void Show()
    {
        switch (DrawStep)
        {
            case BarDrawStep.Hide:
                // Move in
                DrawStep = BarDrawStep.MoveIn;
                OffsetX = MinX;
                WaitTimer = 0;
                break;

            case BarDrawStep.MoveIn:
                // Do nothing, already moving in
                break;

            case BarDrawStep.MoveOut:
                // Move back in
                DrawStep = BarDrawStep.MoveIn;
                WaitTimer = MoveInDelayTime;
                break;

            case BarDrawStep.Wait:
                // Reset wait timer
                WaitTimer = 0;
                break;
        }
    }

    public void Init()
    {
        AnimationPlayer = new AnimationPlayer(false, null);
        RenderContext renderContext = Engine.ViewPort.GameRenderContext;

        // Load textures
        Texture2D saveIconTexture = Engine.Assets.FixContentManager.Load<Texture2D>(Assets.Overlay.SaveIcon);

        // Create objects
        SaveIcon = new SpriteTextureObject()
        {
            HorizontalAnchor = HorizontalAnchorMode.Right,
            VerticalAnchor = VerticalAnchorMode.Bottom,
            RenderContext = renderContext,
            Texture = saveIconTexture,
            SpriteType = SpriteType.Overlay,
        };
        SaveIconScreenPos = new Vector2(-(SaveIcon.Texture.Width + Padding), -(SaveIcon.Texture.Height + Padding));

        // Initialize transition values
        DrawStep = BarDrawStep.Hide;
        MinX = SaveIconScreenPos.X;
        MaxX = 0;
        OffsetX = MinX;
    }

    public void Step()
    {
        // Manage transition and multiply by GbaDeltaTime since this will also be used in the J2ME version which has a lower framerate
        switch (DrawStep)
        {
            case BarDrawStep.MoveIn:
                if (WaitTimer < MoveInDelayTime)
                {
                    WaitTimer += 1 * Engine.App.GbaDeltaTime;
                }
                else
                {
                    OffsetX += MoveSpeed * Engine.App.GbaDeltaTime;
                    if (OffsetX >= MaxX)
                    {
                        OffsetX = MaxX;
                        DrawStep = BarDrawStep.Wait;
                        WaitTimer = 0;
                    }
                }
                break;

            case BarDrawStep.MoveOut:
                OffsetX -= MoveSpeed * Engine.App.GbaDeltaTime;
                if (OffsetX <= MinX)
                {
                    OffsetX = MinX;
                    DrawStep = BarDrawStep.Hide;
                }
                break;

            case BarDrawStep.Wait:
                if (WaitTimer >= WaitTime)
                    DrawStep = BarDrawStep.MoveOut;
                else
                    WaitTimer += 1 * Engine.App.GbaDeltaTime;
                break;
        }

        // Update position
        SaveIcon.ScreenPos = SaveIconScreenPos - new Vector2(OffsetX, 0);
    }

    public void Draw()
    {
        if (DrawStep == BarDrawStep.Hide)
            return;

        AnimationPlayer.Play(SaveIcon);
        AnimationPlayer.Execute();
    }
}