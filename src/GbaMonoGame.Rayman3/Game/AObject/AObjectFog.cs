using System;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3;

// Custom object for rendering fog. The game doesn't do this - it uses a normal AnimatedObject. However, there
// are issues with how it's implemented. The game has sprites wrap around at 512 because of the x position
// in the OAM being a 9-bit signed value, but the wrapping isn't seamless and causes sprites to overlap. This
// is an issue here because of alpha being used, so the overlapping sprites would have their alpha blend together.
public class AObjectFog : AObject
{
    #region Constructor

    public AObjectFog(AnimatedObjectResource resource)
    {
        Resource = resource;

        // The sprites in the first two channels are the only unique sprites. So we can use these and tile them across.
        SpriteChannels =
        [
            resource.Animations[0].Channels[0],
            resource.Animations[0].Channels[1]
        ];
    }

    #endregion

    #region Public Properties

    public const int Width = SpriteWidth * SpritesCount;
    public const int SpriteWidth = 32;
    public const int SpritesCount = 2;

    public AnimatedObjectResource Resource { get; }
    public AnimationChannel[] SpriteChannels { get; }

    public AlphaCoefficient Alpha { get; set; } = AlphaCoefficient.Max;

    #endregion

    #region Private Methods

    private void DrawSprite(AnimationChannel channel, Vector2 screenPos)
    {
        // Get or create the sprite texture
        Texture2D texture = Engine.TextureCache.GetOrCreateObject(
            pointer: Resource.Offset,
            id: channel.TileIndex,
            data: new SpriteDefine(
                resource: Resource,
                spriteShape: channel.SpriteShape,
                spriteSize: channel.SpriteSize,
                tileIndex: channel.TileIndex),
            createObjFunc: static data => new IndexedSpriteTexture2D(data.Resource, data.SpriteShape, data.SpriteSize, data.TileIndex));

        int paletteIndex = channel.PalIndex;
            
        RenderOptions.PaletteTexture = new PaletteTexture(
            Texture: Engine.TextureCache.GetOrCreateObject(
                pointer: Resource.Palettes.Offset,
                id: 0,
                data: Resource.Palettes,
                createObjFunc: static p => new PaletteTexture2D(p.Palettes)),
            PaletteIndex: paletteIndex);

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

        Gfx.AddSprite(sprite);
    }

    #endregion

    #region Public Methods

    public override void Execute(Action<short> soundEventCallback)
    {
        Vector2 pos = GetAnchoredPosition();

        float camWidth = RenderContext.Resolution.X;
        for (int i = 0; i < camWidth / SpriteWidth + SpritesCount; i++)
            DrawSprite(SpriteChannels[i % SpritesCount], pos + new Vector2(SpriteWidth * i, 0));
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