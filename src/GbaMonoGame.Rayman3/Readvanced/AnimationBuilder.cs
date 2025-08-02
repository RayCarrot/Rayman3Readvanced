using System.Collections.Generic;
using System.Linq;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.Readvanced;

/// <summary>
/// Helper class for building custom animations
/// </summary>
public class AnimationBuilder
{
    private readonly List<List<AnimationChannel>> _channels = [];
    private List<AffineMatrixResource> _affineMatrices;

    public byte Speed { get; set; }
    public bool DoNotRepeat { get; set; }

    public void NewFrame()
    {
        _channels.Add([]);
    }

    public void AddChannel(AnimationChannel channel)
    {
        _channels[^1].Add(channel);
    }

    public void AddSprite(short xPosition, short yPosition, ushort tileIndex, bool flipX = false, bool flipY = false)
    {
        AddChannel(new AnimationChannel
        {
            ChannelType = AnimationChannelType.Sprite,
            XPosition = xPosition,
            YPosition = yPosition,
            ObjectMode = OBJ_ATTR_ObjectMode.REG,
            TileIndex = tileIndex,
            FlipX = false,
            FlipY = false,
        });
    }

    public void AddAffineSprite(short xPosition, short yPosition, ushort tileIndex, float rotation256, float scaleX, float scaleY, bool flipX = false, bool flipY = false)
    {
        _affineMatrices ??= [];

        AddChannel(new AnimationChannel
        {
            ChannelType = AnimationChannelType.Sprite,
            XPosition = xPosition,
            YPosition = yPosition,
            ObjectMode = OBJ_ATTR_ObjectMode.AFF,
            TileIndex = tileIndex,
            FlipX = false,
            FlipY = false,
            AffineMatrixIndex = (ushort)_affineMatrices.Count,
        });

        _affineMatrices.Add(new AffineMatrixResource
        {
            Pa = scaleX * MathHelpers.Cos256(rotation256),
            Pb = scaleX * MathHelpers.Sin256(rotation256),
            Pc = scaleY * -MathHelpers.Sin256(rotation256),
            Pd = scaleY * MathHelpers.Cos256(rotation256)
        });
    }

    public void AddVulnerabilityBox(ChannelBox box)
    {
        AddChannel(new AnimationChannel
        {
            ChannelType = AnimationChannelType.VulnerabilityBox,
            Box = box,
        });
    }

    public Animation Build()
    {
        return new Animation
        {
            Speed = Speed,
            DoNotRepeat = DoNotRepeat,
            FramesCount = (byte)_channels.Count,
            ChannelsPerFrame = _channels.Select(x => (byte)x.Count).ToArray(),
            Channels = _channels.SelectMany(x => x).ToArray(),
            AffineMatrices = _affineMatrices == null ? null : new AffineMatrices
            {
                Matrices = _affineMatrices.ToArray()
            },
            PaletteCycleAnimation = null
        };
    }
}