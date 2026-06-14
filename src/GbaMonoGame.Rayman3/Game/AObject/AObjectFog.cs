using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

// Custom object for rendering fog. The game doesn't do this - it uses a normal AnimatedObject. However, there
// are issues with how it's implemented. The game has sprites wrap around at 512 because of the x position
// in the OAM being a 9-bit signed value, but the wrapping isn't seamless and causes sprites to overlap. It's
// barely noticeable, but we can provide a modern render mode where this doesn't happen.
public class AObjectFog : AnimatedObject
{
    #region Constructor

    public AObjectFog(AnimatedObjectResource resource, bool isDynamic) : base(resource, isDynamic)
    {
        // The sprites in the first two channels are the only unique sprites. So we can use these and tile them across.
        SpriteChannels =
        [
            resource.Animations[0].Channels[0],
            resource.Animations[0].Channels[1]
        ];
    }

    #endregion

    #region Public Properties

    // The actual animation width is 540, but it gets wrapped as 512, so we treat it as 512 with an overflow part
    public const int GbaWidth = 512;
    
    public const int ModernWidth = ModernSpriteWidth * ModernSpritesCount;
    public const int ModernSpriteWidth = 32;
    public const int ModernSpritesCount = 2;

    public AnimationChannel[] SpriteChannels { get; }
    public bool ModernMode { get; set; }

    #endregion

    #region Private Methods

    private void DrawSprite(AnimationChannel channel, Vector2 screenPos)
    {
        // Get or create the sprite texture
        Texture2D texture = Engine.Assets.BinaryTextureCache.GetOrCreateObject(
            pointer: Resource.Offset,
            id: channel.TileIndex,
            data: new SpriteDefine(
                resource: Resource,
                spriteShape: channel.SpriteShape,
                spriteSize: channel.SpriteSize,
                tileIndex: channel.TileIndex),
            createObjFunc: static data => new IndexedSpriteTexture2D(data.Resource, data.SpriteShape, data.SpriteSize, data.TileIndex));

        int paletteIndex = channel.PalIndex;
            
        RenderOptions = RenderOptions with { PaletteTexture = new PaletteTexture(
            Texture: Engine.Assets.BinaryTextureCache.GetOrCreateObject(
                pointer: Resource.Palettes.Offset,
                id: 0,
                data: Resource.Palettes,
                createObjFunc: static p => new PaletteTexture2D(p.Palettes)),
            PaletteIndex: paletteIndex) };

        Sprite sprite = Gfx.GetNewSprite();
        sprite.Texture = texture;
        sprite.Position = new Vector2(screenPos.X, screenPos.Y);
        sprite.FlipX = false;
        sprite.FlipY = false;
        sprite.Priority = BgPriority;
        sprite.Center = true;
        sprite.AffineMatrix = null;
        sprite.Alpha = Alpha;
        sprite.RenderOptions = RenderOptions;

        Gfx.AddSprite(sprite, SpriteType);
    }

    #endregion

    #region Public Methods

    public override void Execute(Action<short> soundEventCallback)
    {
        if (ModernMode)
        {
            // In modern mode we manually draw the sprites so they wrap
            Vector2 pos = GetAnchoredPosition();

            float camWidth = RenderContext.Resolution.X;
            for (int i = 0; i < camWidth / ModernSpriteWidth + ModernSpritesCount; i++)
                DrawSprite(SpriteChannels[i % ModernSpritesCount], pos + new Vector2(ModernSpriteWidth * i, 0));
        }
        else
        {
            // In the original mode we use the original animation, however we still need to wrap
            // since the game might be rendering in a higher resolution!

            Vector2 screenPos = ScreenPos;

            // Get the camera bounds
            const float camMinX = 0;
            float maxResX = RenderOptions.RenderContext.Resolution.X;

            // Wrap the position
            float wrappedPos = MathHelpers.Mod(screenPos.X, GbaWidth);

            // Calculate the start and end positions
            float startX = camMinX - GbaWidth + (wrappedPos == 0 ? GbaWidth : wrappedPos);
            float fullWidth = maxResX - startX;
            float wrappedEnd = MathHelpers.Mod(fullWidth, GbaWidth);
            float endX = maxResX + GbaWidth - (wrappedEnd == 0 ? GbaWidth : wrappedEnd);

            // We need to increment the end one more step since the animation has a min value of -255 rather than 0
            endX += GbaWidth;

            // Draw wrapped
            int countX = (int)Math.Ceiling((endX - startX) / GbaWidth);
            for (int x = 0; x < countX; x++)
            {
                ScreenPos = screenPos with { X = startX + x * GbaWidth };
                base.Execute(soundEventCallback);
            }

            // Restore the original position
            ScreenPos = screenPos;
        }
    }

    #endregion

    #region Data Types

    private readonly struct SpriteDefine(
        AnimatedObjectResource resource,
        int spriteShape,
        int spriteSize,
        int tileIndex)
    {
        public AnimatedObjectResource Resource { get; } = resource;
        public int SpriteShape { get; } = spriteShape;
        public int SpriteSize { get; } = spriteSize;
        public int TileIndex { get; } = tileIndex;
    }

    #endregion
}