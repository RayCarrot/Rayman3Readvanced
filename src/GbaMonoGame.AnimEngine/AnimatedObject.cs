using System;
using System.Collections.Generic;
using System.Diagnostics;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.AnimEngine;

// The game has different types of AnimatedObject. They however all act the same, just with some different properties
// depending on the class type. Doing that here would be a mess, so better we handle it using properties in this class.

/// <summary>
/// An object which can execute a sprite animation
/// </summary>
public class AnimatedObject : AObject
{
    #region Constructor

    public AnimatedObject(AnimatedObjectResource resource, bool isDynamic)
    {
        if (isDynamic)
            Debug.Assert(resource.IsDynamic, "The animated object data is not dynamic");

        IsSoundEnabled = true;
        IsDynamic = isDynamic;
        Resource = resource;
        ActivateAllChannels();

        // Load palettes
        Palettes = new SpritePalettes(resource.Palettes);
    }

    #endregion

    #region Private Fields

    private int _currentAnimation;
    private int _currentFrame;

    #endregion

    #region Public Properties

    // Resources
    public AnimatedObjectResource Resource { get; }
    public SpritePalettes Palettes { get; set; }

    // Flags
    public bool IsFramed { get; set; }
    public bool IsDelayMode { get; set; }
    public bool EndOfAnimation { get; set; }
    public bool IsPaused { get; set; }
    public bool IsAffine { get; set; }
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }
    public bool IsDynamic { get; set; } // Not used here

    // Render flags
    public bool IsDoubleAffine { get; set; } // Not used here

    // Class type
    public bool IsSoundEnabled { get; set; }
    
    // Animation state
    public int CurrentAnimation
    {
        get => _currentAnimation;
        set
        {
            _currentAnimation = value;
            Rewind();
        }
    }
    public int CurrentFrame
    {
        get => _currentFrame;
        set
        {
            Animation anim = GetAnimation();

            if (value != _currentFrame)
            {
                if (value == 0)
                {
                    ChannelIndex = 0;
                }
                else if (value > _currentFrame)
                {
                    int framesDiff = value - _currentFrame;

                    for (int i = 0; i < framesDiff; i++)
                        ChannelIndex += anim.ChannelsPerFrame[_currentFrame + i];
                }
                else
                {
                    int framesDiff = _currentFrame - value;

                    for (int i = 0; i < framesDiff; i++)
                        ChannelIndex -= anim.ChannelsPerFrame[_currentFrame - i - 1];
                }

                _currentFrame = value;
            }

            Timer = anim.Speed;
            IsDelayMode = false;
            EndOfAnimation = false;
        }
    }
    public int ChannelIndex { get; set; }
    public int Timer { get; set; }
    public int BasePaletteIndex { get; set; }

    // Extended options
    public BoxTable BoxTable { get; set; }
    public uint ActiveChannels { get; set; }
    public AffineMatrix? AffineMatrix { get; set; }

    // Custom
    public bool IsBackSprite { get; set; }
    public int PaletteCycleIndex { get; set; }
    public bool OverrideGfxColor { get; set; } // Needed for the curtains in the worldmap which are not effected by the palette fading
    public AlphaCoefficient Alpha { get; set; } = AlphaCoefficient.Max;
    public Dictionary<int, Texture2D> ReplacedSpriteTextures { get; set; }
    public Dictionary<int, Animation> ReplacedAnimations { get; set; }
    public Dictionary<int, AffineMatrix> AffineMatrixCache { get; set; }

    // TODO: Check if this needs to be applied to more animations
    // Custom. This is used to fix sprite wrapping. For bigger animations, like bosses, the sprites sometimes
    // wrap around to the other side due to the position values being stored as signed bytes.
    public float WrapMinX { get; set; } = Single.NaN;
    public float WrapMinY { get; set; } = Single.NaN;
    public float WrapMaxX { get; set; } = Single.NaN;
    public float WrapMaxY { get; set; } = Single.NaN;

    #endregion

    #region Private Methods

    private static int GetAffineMatrixCacheId(int animId, int affineMatrixIndex)
    {
        return animId * 10_000 + affineMatrixIndex;
    }

    #endregion

    #region Public Methods

    public IEnumerable<AnimationChannel> EnumerateCurrentChannels()
    {
        Animation anim = GetAnimation();

        for (int i = 0; i < anim.ChannelsPerFrame[CurrentFrame]; i++)
            yield return anim.Channels[i + ChannelIndex];
    }

    public void ReplaceSpriteTexture(int tileIndex, Texture2D texture)
    {
        ReplacedSpriteTextures ??= new Dictionary<int, Texture2D>();
        ReplacedSpriteTextures[tileIndex] = texture;
    }

    public Texture2D GetSpriteTexture(AnimationChannel spriteChannel)
    {
        if (ReplacedSpriteTextures != null && ReplacedSpriteTextures.TryGetValue(spriteChannel.TileIndex, out Texture2D texture))
            return texture;
        else
            return Engine.TextureCache.GetOrCreateObject(
                pointer: Resource.Offset,
                id: spriteChannel.TileIndex,
                data: new SpriteDefine(
                    resource: Resource,
                    spriteShape: spriteChannel.SpriteShape,
                    spriteSize: spriteChannel.SpriteSize,
                    tileIndex: spriteChannel.TileIndex),
                createObjFunc: static data => new IndexedSpriteTexture2D(data.Resource, data.SpriteShape, data.SpriteSize, data.TileIndex));
    }

    public Animation GetAnimation()
    {
        return GetAnimation(CurrentAnimation);
    }

    public Animation GetAnimation(int id)
    {
        if (ReplacedAnimations != null && ReplacedAnimations.TryGetValue(id, out Animation anim))
            return anim;
        else
            return Resource.Animations[id];
    }

    public Animation CopyAnimation(int id)
    {
        Animation originalAnim = Resource.Animations[id];
        Animation animCopy = Rom.CopyResource(originalAnim);
        animCopy.AffineMatrices = originalAnim.AffineMatrices;
        animCopy.PaletteCycleAnimation = originalAnim.PaletteCycleAnimation;
        return animCopy;
    }

    public void ReplaceAnimation(int id, Animation animation)
    {
        ReplacedAnimations ??= new Dictionary<int, Animation>();
        ReplacedAnimations[id] = animation;
    }

    public bool HasReplacedAnimation(int id)
    {
        return ReplacedAnimations != null && ReplacedAnimations.ContainsKey(id);
    }

    public Animation GetReplacedAnimation(int id)
    {
        return ReplacedAnimations[id];
    }

    public AffineMatrix GetAffineMatrix(int index)
    {
        AffineMatrixCache ??= new Dictionary<int, AffineMatrix>();

        int cacheId = GetAffineMatrixCacheId(CurrentAnimation, index);

        if (AffineMatrixCache.TryGetValue(cacheId, out AffineMatrix matrix))
            return matrix;

        Animation anim = GetAnimation();
        AffineMatrixResource matrixRessource = anim.AffineMatrices.Matrices[index];

        matrix = new AffineMatrix(matrixRessource.Pa, matrixRessource.Pb, matrixRessource.Pc, matrixRessource.Pd);
        AffineMatrixCache[cacheId] = matrix;
        return matrix;
    }

    public bool IsChannelVisible(int channel) => (ActiveChannels & (1 << channel)) != 0;
    public void ActivateAllChannels() => ActiveChannels = UInt32.MaxValue;
    public void ActivateChannel(int channel) => ActiveChannels = (uint)((int)ActiveChannels | (1 << channel));
    public void DeactivateAllChannels() => ActiveChannels = 0;
    public void DeactivateChannel(int channel) => ActiveChannels = (uint)((int)ActiveChannels & ~(1 << channel));

    public void SetFlagUseRotationScaling(bool useRotationScaling)
    {
        if (useRotationScaling)
        {
            IsAffine = true;
        }
        else
        {
            IsAffine = false;
            IsDoubleAffine = false;
        }
    }

    public void Rewind()
    {
        _currentFrame = 0;
        ChannelIndex = 0;
        Timer = GetAnimation().Speed;
        IsDelayMode = false;
        EndOfAnimation = false;
    }

    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;

    public void ComputeNextFrame()
    {
        EndOfAnimation = false;

        if (BoxTable != null)
            PlayChannelBox();

        Animation anim = GetAnimation();

        EndOfAnimation = false;

        if (IsPaused)
        {
            if (Timer != 0)
                IsDelayMode = true;

            if (CurrentFrame >= anim.FramesCount)
                EndOfAnimation = true;

            return;
        }

        if (Timer == 0)
        {
            ChannelIndex += anim.ChannelsPerFrame[CurrentFrame];
            _currentFrame++;
            Timer = anim.Speed;
            IsDelayMode = false;

            if (CurrentFrame < anim.FramesCount)
                return;

            if (anim.DoNotRepeat)
            {
                _currentFrame--;
                ChannelIndex -= anim.ChannelsPerFrame[CurrentFrame];
                IsDelayMode = true;
            }
            else
            {
                _currentFrame = 0;
                ChannelIndex = 0;
                IsDelayMode = false;
            }

            EndOfAnimation = true;
        }
        else
        {
            Timer--;
            IsDelayMode = true;
        }
    }

    public void PlayChannelBox()
    {
        Debug.Assert(BoxTable != null, "There's no box table");

        if (IsDelayMode || BoxTable == null)
            return;

        BoxTable.AttackBox = new Box();
        BoxTable.VulnerabilityBox = new Box();

        foreach (AnimationChannel channel in EnumerateCurrentChannels())
        {
            switch (channel.ChannelType)
            {
                case AnimationChannelType.AttackBox:
                    BoxTable.AttackBox = new Box(channel.Box);
                    break;

                case AnimationChannelType.VulnerabilityBox:
                    BoxTable.VulnerabilityBox = new Box(channel.Box);
                    break;
            }
        }
    }

    public void PlayChannelSound(AnimationPlayer animationPlayer)
    {
        if (IsDelayMode)
            return;

        foreach (AnimationChannel channel in EnumerateCurrentChannels())
        {
            if (channel.ChannelType == AnimationChannelType.Sound)
                animationPlayer.SoundEventRequest(channel.SoundId);
        }
    }

    public void FrameChannelSprite() => FrameChannelSprite(ScreenPos, RenderBox);
    public void FrameChannelSprite(Vector2 position, Box renderBox)
    {
        // NOTE: The game doesn't update this if in delay mode, however that causes issues and should probably be considered
        //       a bug. Cause the object's position might still change even though the animation frame is the same! It's most
        //       noticeable in boss fights where there's a frame or two where a sprite is missing when they move onto the screen.
        if (!IsDelayMode || Engine.ActiveConfig.Tweaks.FixBugs)
        {
            ActivateAllChannels();

            int channelIndex = 0;
            foreach (AnimationChannel channel in EnumerateCurrentChannels())
            {
                if (channel.ChannelType == AnimationChannelType.Sprite)
                {
                    // NOTE: The game doesn't do this, but we have to since affine sprites are rendered differently and thus
                    //       not constrained to the same dimensions, and it's not worth calculating the resulting size.
                    if (IsDoubleAffine || channel.ObjectMode is OBJ_ATTR_ObjectMode.AFF or OBJ_ATTR_ObjectMode.AFF_DBL)
                    {
                        channelIndex++;
                        continue;
                    }

                    Constants.Size shape = Constants.GetSpriteShape(channel.SpriteShape, channel.SpriteSize);

                    float width = shape.Width;
                    float height = shape.Height;

                    float xPos;
                    if (!FlipX)
                        xPos = position.X + channel.XPosition;
                    else
                        xPos = position.X - channel.XPosition - width;

                    if (IsDoubleAffine || channel.ObjectMode is OBJ_ATTR_ObjectMode.AFF or OBJ_ATTR_ObjectMode.AFF_DBL)
                    {
                        xPos -= width / 2;
                        width *= 2;
                    }

                    float yPos;
                    if (!FlipY)
                        yPos = position.Y + channel.YPosition;
                    else
                        yPos = position.Y - channel.YPosition - height;

                    if (IsDoubleAffine || channel.ObjectMode is OBJ_ATTR_ObjectMode.AFF or OBJ_ATTR_ObjectMode.AFF_DBL)
                    {
                        yPos -= height / 2;
                        height *= 2;
                    }

                    Box spriteBox = new(
                        position: new Vector2(xPos, yPos), 
                        size: new Vector2(width, height));

                    if (!spriteBox.Intersects(renderBox))
                        DeactivateChannel(channelIndex);
                }

                channelIndex++;
            }
        }
    }

    public override void Execute(Action<short> soundEventCallback)
    {
        Animation anim = GetAnimation();

        if (!IsDelayMode && BoxTable != null)
        {
            BoxTable.AttackBox = new Box();
            BoxTable.VulnerabilityBox = new Box();
        }

        EndOfAnimation = false;

        // Ideally we'd do this (the game only does it for 8-bit animations for some reason), but several
        // animations forget to set themselves as framed and it's a pain to fix them all. The IsFramed
        // property doesn't matter unless it's manually checked for anyway.
        // Debug.Assert(IsFramed, "This function should not be called when the object is not framed");

        // --- At this point the engine loads dynamic data which we don't need to ---

        // NOTE: This will only cycle the palette for this animated object instanced. This is different from the
        //       original game where all instances of the same animation share the same palette in VRAM. Because
        //       of this the original game "speeds up" the animations when more are on screen at once (because
        //       each instance shifts the palette one step). This is noticeable for the blue lum bar which has
        //       the fill area made out of multiple small animations. We could recreate this here, but it would
        //       cause issues for the lava, which also uses this, if you zoom out to show multiple on screen.
        if (anim.Idx_PaletteCycleAnimation != 0 && !IsDelayMode)
        {
            PaletteCycleAnimation palAnim = anim.PaletteCycleAnimation;
            int length = palAnim.ColorEndIndex - palAnim.ColorStartIndex + 1;

            PaletteCycleIndex++;

            if (PaletteCycleIndex >= length)
                PaletteCycleIndex = 0;
        }

        // Enumerate every channel
        int channelIndex = 0;
        foreach (AnimationChannel channel in EnumerateCurrentChannels())
        {
            // Play the channel based on the type
            switch (channel.ChannelType)
            {
                case AnimationChannelType.Sprite:

                    if (channel.ObjectMode == OBJ_ATTR_ObjectMode.HIDE || !IsChannelVisible(channelIndex))
                        break;

                    // On GBA the size of a sprite is determined based on
                    // the shape and size values. We use these to get the
                    // actual width and height of the sprite.
                    Constants.Size shape = Constants.GetSpriteShape(channel.SpriteShape, channel.SpriteSize);

                    // Get x position
                    float xPos = channel.XPosition;
                    if (WrapMinX != Single.NaN && xPos < WrapMinX)
                        xPos += 256;
                    else if (WrapMaxX != Single.NaN && xPos > WrapMaxX)
                        xPos -= 256;

                    Vector2 pos = GetAnchoredPosition();

                    if (!FlipX)
                        xPos += pos.X;
                    else
                        xPos = pos.X - xPos - shape.Width;

                    // Get y position
                    float yPos = channel.YPosition;
                    if (WrapMinY != Single.NaN && yPos < WrapMinY)
                        yPos += 256;
                    else if (WrapMaxY != Single.NaN && yPos > WrapMaxY)
                        yPos -= 256;

                    if (!FlipY)
                        yPos += pos.Y;
                    else
                        yPos = pos.Y - yPos - shape.Height;

                    AffineMatrix? affineMatrix = null;

                    // Get the matrix if it's affine
                    if (channel.ObjectMode == OBJ_ATTR_ObjectMode.REG && AffineMatrix != null && IsAffine)
                        affineMatrix = AffineMatrix.Value;
                    else if (channel.ObjectMode is OBJ_ATTR_ObjectMode.AFF or OBJ_ATTR_ObjectMode.AFF_DBL)
                        affineMatrix = GetAffineMatrix(channel.AffineMatrixIndex);

                    // Get the sprite texture
                    Texture2D texture = GetSpriteTexture(channel);

                    int paletteIndex = BasePaletteIndex + channel.PalIndex;

                    if (paletteIndex > 0)
                        Debug.Assert(!Resource.Is8Bit, "Can't use a palette index when 8-bit");

                    PaletteTexture paletteTexture;
                    if (texture is not IndexedSpriteTexture2D)
                    {
                        paletteTexture = null;
                    }
                    // If the palette cycle index is 0 or not the animated palette then it's the default, unmodified, palette
                    else if (PaletteCycleIndex == 0 || anim.PaletteCycleAnimation?.PaletteIndex != paletteIndex)
                    {
                        paletteTexture = new PaletteTexture(
                            Texture: Engine.TextureCache.GetOrCreateObject(
                                pointer: Palettes.CachePointer,
                                id: 0,
                                data: Palettes,
                                createObjFunc: static p => new PaletteTexture2D(p.Palettes)),
                            PaletteIndex: paletteIndex);
                    }
                    else
                    {
                        paletteTexture = new PaletteTexture(
                            Texture: Engine.TextureCache.GetOrCreateObject(
                                pointer: anim.PaletteCycleAnimation.Offset,
                                id: PaletteCycleIndex,
                                data: new PaletteAnimationDefine(Palettes, anim.PaletteCycleAnimation, PaletteCycleIndex),
                                createObjFunc: static data =>
                                {
                                    PaletteCycleAnimation palAnim = data.PaletteCycleAnimation;

                                    Color[] originalPal = data.SpritePalettes.Palettes[palAnim.PaletteIndex].Colors;
                                    Color[] newPal = new Color[originalPal.Length];
                                    Array.Copy(originalPal, newPal, originalPal.Length);

                                    int length = palAnim.ColorEndIndex - palAnim.ColorStartIndex + 1;

                                    for (int i = 0; i < length; i++)
                                    {
                                        int srcIndex = palAnim.ColorStartIndex + i;
                                        int dstIndex = palAnim.ColorStartIndex + (i + data.PaletteCycleIndex) % length;

                                        newPal[dstIndex] = originalPal[srcIndex];
                                    }

                                    return new PaletteTexture2D(newPal);
                                }),
                            PaletteIndex: 0);
                    }

                    // Add the sprite to vram. In the original engine this part
                    // is more complicated. If the object is dynamic then it loads
                    // in the data to vram first, then for all sprites it adds an
                    // entry to the list of object attributes in OAM memory.
                    Sprite sprite = new()
                    {
                        Texture = texture,
                        Position = new Vector2(xPos, yPos),
                        FlipX = !IsAffine ? channel.FlipX ^ FlipX : FlipX, // Sprite flip flags are ignored if affine (see Mode7 tree trunks)
                        FlipY = !IsAffine ? channel.FlipY ^ FlipY : FlipY,
                        Priority = BgPriority,
                        Center = true,
                        AffineMatrix = affineMatrix,
                        OverrideGfxColor = OverrideGfxColor,
                        Alpha = Alpha,
                        RenderOptions = RenderOptions with { PaletteTexture = paletteTexture },
                    };

                    if (IsBackSprite)
                        Gfx.AddBackSprite(sprite);
                    else
                        Gfx.AddSprite(sprite);
                    break;

                case AnimationChannelType.Sound:
                    if (!IsDelayMode && IsSoundEnabled)
                        soundEventCallback(channel.SoundId);
                    break;

                case AnimationChannelType.DisplacementVector:
                    if (!IsDelayMode)
                    {
                        // Appears mostly unused in Rayman 3. Only used for ship in final boss and Ly, but seems to always be 0 anyway.
                        Logger.NotImplemented("Not implemented displacement vectors");
                    }
                    break;

                case AnimationChannelType.AttackBox:
                    // Same as the IsFramed check where the game only does this for 8-bit animations. Ideally we'd do it for all,
                    // but Rayman's game over animations have boxes defined (probably a leftover from his in-game animations).
                    //Debug.Assert(BoxTable != null, "There's no box table");

                    if (!IsDelayMode && BoxTable != null)
                        BoxTable.AttackBox = new Box(channel.Box);
                    break;

                case AnimationChannelType.VulnerabilityBox:
                    // Same as above comment for attack box
                    //Debug.Assert(BoxTable != null, "There's no box table");

                    if (!IsDelayMode && BoxTable != null)
                        BoxTable.VulnerabilityBox = new Box(channel.Box);
                    break;
            }

            channelIndex++;
        }

        ComputeNextFrame();
    }

    #endregion

    #region Data Types

    private readonly struct SpriteDefine(AnimatedObjectResource resource, int spriteShape, int spriteSize, int tileIndex)
    {
        public AnimatedObjectResource Resource { get; } = resource;
        public int SpriteShape { get; } = spriteShape;
        public int SpriteSize { get; } = spriteSize;
        public int TileIndex { get; } = tileIndex;
    }

    private readonly struct PaletteAnimationDefine(SpritePalettes spritePalettes, PaletteCycleAnimation paletteCycleAnimation, int paletteCycleIndex)
    {
        public SpritePalettes SpritePalettes { get; } = spritePalettes;
        public PaletteCycleAnimation PaletteCycleAnimation { get; } = paletteCycleAnimation;
        public int PaletteCycleIndex { get; } = paletteCycleIndex;
    }

    #endregion
}