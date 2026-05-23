using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.AnimEngine;

public static class AnimationChannelHelpers
{
    // For channels where we override the texture, meaning we don't need the original tile attributes
    public static RawAnimationChannel CreateCustomSpriteTextureChannel(
        short xPosition,
        short yPosition,
        ushort tileIndex,
        bool flipX = false,
        bool flipY = false)
    {
        return RawAnimationChannel.CreateSpriteChannel(
            game: Rom.Game,
            xPosition: xPosition,
            yPosition: yPosition,
            spriteShape: 0,
            spriteSize: 0,
            objectMode: OBJ_ATTR_ObjectMode.REG,
            tileIndex: tileIndex,
            palIndex: 0,
            reusesTiles: false,
            flipX: flipX,
            flipY: flipY);
    }

    public static RawAnimationChannel CreateHiddenSpriteChannel()
    {
        return RawAnimationChannel.CreateSpriteChannel(
            game: Rom.Game,
            xPosition: 0,
            yPosition: 0,
            spriteShape: 0,
            spriteSize: 0,
            objectMode: OBJ_ATTR_ObjectMode.HIDE,
            tileIndex: 0,
            palIndex: 0,
            reusesTiles: false,
            flipX: false,
            flipY: false);
    }

    public static RawAnimationChannel CreateCustomAffineSpriteTextureChannel(
        short xPosition,
        short yPosition,
        ushort tileIndex,
        ushort affineMatrixIndex)
    {
        return RawAnimationChannel.CreateAffineSpriteChannel(
            game: Rom.Game,
            xPosition: xPosition,
            yPosition: yPosition,
            spriteShape: 0,
            spriteSize: 0,
            objectMode: OBJ_ATTR_ObjectMode.AFF,
            tileIndex: tileIndex,
            palIndex: 0,
            reusesTiles: false,
            affineMatrixIndex: affineMatrixIndex);
    }

}