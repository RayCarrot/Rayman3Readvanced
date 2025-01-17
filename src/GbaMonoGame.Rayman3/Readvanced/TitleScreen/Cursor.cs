using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class Cursor
{
    public Cursor(RenderContext renderContext)
    {
        Texture2D tex = Engine.FrameContentManager.Load<Texture2D>("Cursor");

        CursorSprite = new SpriteObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = Vector2.Zero,
            Texture = tex,
            RenderContext = renderContext,
        };
    }

    public SpriteObject CursorSprite { get; set; }
    public Vector2 PreviousPosition { get; set; }
    public Vector2 TargetPosition { get; set; }
    public float TargetMovementAmount { get; set; }
    public bool IsMoving => TargetMovementAmount < 1;

    public void SetTargetPosition(Vector2 targetPosition)
    {
        targetPosition -= new Vector2(45, 14);

        if (CursorSprite.ScreenPos == Vector2.Zero)
        {
            TargetMovementAmount = 1;
            CursorSprite.ScreenPos = targetPosition;
        }
        else
        {
            TargetMovementAmount = 0;
            PreviousPosition = CursorSprite.ScreenPos;
            TargetPosition = targetPosition;
        }
    }

    public void Step()
    {
        if (TargetMovementAmount < 1)
        {
            CursorSprite.ScreenPos = Vector2.Lerp(PreviousPosition, TargetPosition, TargetMovementAmount);
            TargetMovementAmount += 0.1f;

            if (TargetMovementAmount >= 1)
                CursorSprite.ScreenPos = TargetPosition;
        }
    }

    public void Draw(AnimationPlayer animationPlayer)
    {
        animationPlayer.PlayFront(CursorSprite);
    }
}