using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class MenuTabBar
{
    public MenuTabBar(RenderContext renderContext, Vector2 position, int bgPriority, string[] tabNames)
    {
        AnimatedObjectResource startEraseAnimations = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.MenuStartEraseAnimations);
        Texture2D tabHeadersTexture = Engine.FixContentManager.Load<Texture2D>(Assets.OptionsMenuTabsTexture);

        Position = position;

        TabsCursor = new AnimatedObject(startEraseAnimations, startEraseAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = bgPriority,
            ObjPriority = 0,
            CurrentAnimation = 40,
            RenderContext = renderContext,
        };

        TabHeaders = new SpriteTextureObject
        {
            BgPriority = bgPriority,
            ObjPriority = 0,
            Texture = tabHeadersTexture,
            RenderContext = renderContext,
        };

        TabHeaderTexts = new SpriteFontTextObject[tabNames.Length];
        for (int i = 0; i < tabNames.Length; i++)
        {
            TabHeaderTexts[i] = new SpriteFontTextObject()
            {
                BgPriority = bgPriority,
                ObjPriority = 0,
                RenderContext = renderContext,
                AffineMatrix = new AffineMatrix(0, new Vector2(TabHeaderTextScale), false, false),
                Text = tabNames[i],
                Font = ReadvancedFonts.MenuYellow,
            };
        }
    }

    private const float TabHeaderWidth = 60;
    private const float TabHeaderTextOffsetX = 26;
    private const float TabHeaderTextOffsetY = 67;
    private const float TabHeaderTextScale = 1 / 2f;
    private const float TabsCursorOffsetX = 27;
    private const float TabsCursorOffsetY = 49;
    private const float TabsCursorMoveTime = 12;

    public AnimatedObject TabsCursor { get; }
    public SpriteTextureObject TabHeaders { get; }
    public SpriteFontTextObject[] TabHeaderTexts { get; }

    public Vector2 Position { get; set; }

    public float TabsCursorX { get; set; }
    public float? TabsCursorStartX { get; set; }
    public float? TabsCursorDestX { get; set; }

    private void SetCursorMovement(float startX, float endX)
    {
        TabsCursorStartX = startX;
        TabsCursorDestX = endX;
    }

    public void SetSelectedTab(int selectedIndex)
    {
        SetCursorMovement(TabsCursorX, selectedIndex * TabHeaderWidth);
    }

    public void Step()
    {
        if (TabsCursorStartX == null || TabsCursorDestX == null)
            return;

        float startX = TabsCursorStartX.Value;
        float destX = TabsCursorDestX.Value;

        // Move with a speed based on the distance
        float dist = destX - startX;
        float speed = dist / TabsCursorMoveTime;

        // Move
        if ((destX < startX && TabsCursorX > destX) ||
            (destX > startX && TabsCursorX < destX))
        {
            TabsCursorX += speed;
        }
        // Finished moving
        else
        {
            TabsCursorX = destX;
            TabsCursorStartX = null;
            TabsCursorDestX = null;
        }
    }

    public void Draw(AnimationPlayer animationPlayer)
    {
        TabHeaders.ScreenPos = Position;
        animationPlayer.Play(TabHeaders);

        for (int i = 0; i < TabHeaderTexts.Length; i++)
        {
            SpriteFontTextObject tabHeaderText = TabHeaderTexts[i];

            float width = ReadvancedFonts.MenuYellow.GetWidth(tabHeaderText.Text) * TabHeaderTextScale;
            tabHeaderText.ScreenPos = Position + new Vector2(TabHeaderTextOffsetX + i * TabHeaderWidth - width / 2, TabHeaderTextOffsetY);
            
            animationPlayer.Play(tabHeaderText);
        }

        TabsCursor.ScreenPos = Position + new Vector2(TabsCursorOffsetX + TabsCursorX, TabsCursorOffsetY);
        animationPlayer.Play(TabsCursor);
    }
}