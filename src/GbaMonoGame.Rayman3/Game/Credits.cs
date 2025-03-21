using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.Rayman3.Readvanced;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = BinarySerializer.Ubisoft.GbaEngine.Texture;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace GbaMonoGame.Rayman3;

public class Credits : Frame
{
    public Credits(bool calledFromOptionsMenu)
    {
        ObjAlpha = 0;
        field16_0x2c = 0;
        TextMode = 0;
        field18_0x34 = 0;
        field19_0x38 = 0;
        LocString = null;
        Unused = 0;
        CurrentStringIndex = 0;
        NextStringIndex = 0;
        HeadersCount = 0;
        NamesCount = 0;
        IsTextDirty = true;
        CalledFromOptionsMenu = calledFromOptionsMenu;
        IsExiting = false;
    }

    // The original game lags in the credits and runs at 15fps, so we need to simulate that with a 4-frame lag
    private const int LagFrames = 4;

    public TransitionsFX TransitionsFX { get; set; }
    public AnimationPlayer AnimationPlayer { get; set; }

    public AnimatedObject BackgroundStructure { get; set; }
    public AnimActor Wheel { get; set; }

    public uint Timer { get; set; }
    public float ObjAlpha { get; set; }
    public int field16_0x2c { get; set; } // TODO: Name
    public int TextMode { get; set; }
    public uint field18_0x34 { get; set; } // TODO: Name
    public uint field19_0x38 { get; set; } // TODO: Name
    public string[] LocString { get; set; }
    public uint Unused { get; set; }
    public int CurrentStringIndex { get; set; }
    public int NextStringIndex { get; set; }
    public int HeadersCount { get; set; }
    public int NamesCount { get; set; }
    public bool IsTextDirty { get; set; }
    public bool CalledFromOptionsMenu { get; set; }
    public bool IsExiting { get; set; }

    private void InitText()
    {
        // TODO: Implement
    }

    private void InitWheel()
    {
        // Load resources
        AnimActorResource animActorResource = Rom.LoadResource<AnimActorResource>(GameResource.CreditsWheelAnimActor);
        TextureTable textureTable = Rom.LoadResource<TextureTable>(GameResource.CreditsWheelTextureTable);
        PaletteTable paletteTable = Rom.LoadResource<PaletteTable>(GameResource.CreditsWheelPaletteTable);

        // Create textures
        Texture2D[] textures = new Texture2D[textureTable.TexturesCount];
        for (int i = 0; i < textures.Length; i++)
        {
            Texture tex = textureTable.Textures[i].Value;
            textures[i] = Engine.TextureCache.GetOrCreateObject(
                pointer: textureTable.Offset,
                id: i,
                data: (Texture: tex, Palette: paletteTable.Palettes[0].Value),
                createObjFunc: static data =>
                {
                    Palette palette = Engine.PaletteCache.GetOrCreateObject(
                        pointer: data.Palette.Offset,
                        id: 0,
                        data: data.Palette,
                        createObjFunc: static paletteData => new Palette(paletteData));

                    return new BitmapTexture2D(data.Texture.Width, data.Texture.Height, data.Texture.ImgData, palette);
                });
        }

        // The game hard-codes it so that only triangles 8-23 are used, and applies textures to 2 at a time.
        // Triangles 0-7 and 24-31 define the sides of the wheel mesh and are unused.
        const int trianglesPerTexture = 2;
        const int triangleBaseIndex = 8;
        const int trianglesCount = 16;
        const int verticesPerTriangle = 3;

        // Create mesh, with one fragment per texture
        GeometryObject geometryObject = animActorResource.GeometryTable.GeometryObjects[0];
        MeshFragment[] meshFragments = new MeshFragment[trianglesCount / trianglesPerTexture];
        for (int globalTriIndex = 0; globalTriIndex < trianglesCount; globalTriIndex += trianglesPerTexture)
        {
            VertexPositionColorTexture[] vertexData = new VertexPositionColorTexture[trianglesPerTexture * verticesPerTriangle];
            Texture2D tex = textures[globalTriIndex / trianglesPerTexture];

            for (int fragTriIndex = 0; fragTriIndex < trianglesPerTexture; fragTriIndex++)
            {
                Triangle triangle = geometryObject.Triangles[triangleBaseIndex + globalTriIndex + fragTriIndex];

                for (int vertexIndex = 0; vertexIndex < verticesPerTriangle; vertexIndex++)
                {
                    BinarySerializer.Ubisoft.GbaEngine.Vector3 vertex = geometryObject.Vertices[triangle.Vertices[vertexIndex]];
                    UV uv = geometryObject.TriangleUVs[triangle.UVsOffset / 6].UVs[vertexIndex];

                    Vector3 pos = new(vertex.X, vertex.Y, vertex.Z);
                    Vector2 textureCoordinate = new(uv.U / (float)(tex.Width - 1), uv.V / (float)(tex.Height - 1));

                    vertexData[fragTriIndex * verticesPerTriangle + vertexIndex] = new VertexPositionColorTexture(
                        position: pos,
                        color: Color.White,
                        textureCoordinate: textureCoordinate);
                }
            }

            meshFragments[globalTriIndex / trianglesPerTexture] = new MeshFragment(PrimitiveType.TriangleList, vertexData, trianglesPerTexture, tex);
        }

        // Create a screen renderer for the mesh
        MeshScreenRenderer meshScreenRenderer = new()
        {
            // NOTE: The scale and position values aren't what the original game uses, but produce basically
            //       the same result as the original game. The rotation value however is the same.
            Scale = new Vector3(32),
            Rotation = new Vector3(0, MathHelpers.Angle256ToRadians(190), 0),
            Position = new Vector3(Rom.OriginalResolution.X / 2 + 16, Rom.OriginalResolution.Y - 26, 0),
            MeshFragments = meshFragments,
        };

        // Add screen to render mesh to
        Gfx.AddScreen(new GfxScreen(2)
        {
            Priority = 1,
            Wrap = false,
            IsEnabled = true,
            Renderer = meshScreenRenderer,
            RenderOptions = { RenderContext = Rom.OriginalGameRenderContext }
        });

        // Create the actor for the wheel
        Wheel = new AnimActor(animActorResource, meshScreenRenderer);

        // Set the animation to 1 and default the speed to half
        Wheel.SetAnimation(1);
        Wheel.AnimSpeed = 0.5f;
    }

    private void StepWheel()
    {
        if (JoyPad.IsButtonPressed(GbaInput.Up))
        {
            if (Wheel.AnimSpeed < 5)
                Wheel.AnimSpeed += MathHelpers.FromFixedPoint(0x800);
        }
        else if (JoyPad.IsButtonPressed(GbaInput.Down))
        {
            if (Wheel.AnimSpeed > -5)
                Wheel.AnimSpeed -= MathHelpers.FromFixedPoint(0x800);
        }

        Wheel.Animate(1 / GbaGame.Framerate / LagFrames);
    }

    public override void Init()
    {
        TransitionsFX = new TransitionsFX(true);
        TransitionsFX.FadeInInit(2 / 16f);

        AnimationPlayer = new AnimationPlayer(false, null);

        AnimatedObjectResource resource = Rom.LoadResource<AnimatedObjectResource>(GameResource.CreditsAnimations);

        BackgroundStructure = new AnimatedObject(resource, false)
        {
            IsFramed = true,
            BgPriority = 0,
            CurrentAnimation = 0,
            ScreenPos = Rom.Platform switch
            {
                Platform.GBA => new Vector2(32, 94),
                Platform.NGage => new Vector2(88, 104),
                _ => throw new UnsupportedPlatformException()
            },
            RenderContext = Rom.OriginalGameRenderContext,
        };

        AnimationPlayer.PlayFront(BackgroundStructure);
        AnimationPlayer.Execute();

        InitText();
        InitWheel();

        if (!SoundEventsManager.IsSongPlaying(Rayman3SoundEvent.Play__happyslide))
            SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__happyslide);

        Timer = 0;
    }

    public override void UnInit()
    {
        Gfx.ClearColor = Color.Black;
        Gfx.FadeControl = new FadeControl(FadeMode.BrightnessDecrease);
        Gfx.Fade = 1;
        
        SoundEventsManager.StopAllSongs();
    }

    public override void Step()
    {
        if (!IsExiting)
        {
            // TODO: N-Gage checks other buttons too
            if (JoyPad.IsButtonJustPressed(GbaInput.B) ||
                JoyPad.IsButtonJustPressed(GbaInput.A) ||
                JoyPad.IsButtonJustPressed(GbaInput.Start))
            {
                TransitionsFX.FadeOutInit(2 / 16f);
                IsExiting = true;
            }
        }
        else if (TransitionsFX.IsFadeOutFinished)
        {
            SoundEventsManager.StopAllSongs();

            if (CalledFromOptionsMenu)
                FrameManager.SetNextFrame(new MenuAll(InitialMenuPage.Options));
            else
                FrameManager.SetNextFrame(new ModernMenuAll(InitialMenuPage.GameMode));
        }

        // TODO: Step text

        StepWheel();

        // NOTE: The GBA version calls this from a vsync callback instead of here - probably due to the lag
        TransitionsFX.StepAll();

        // TODO: Draw text

        Timer++;

        // NOTE: The game doesn't do this - it only calls Play in the Init and then keeps it loaded in the OAM while manually drawing the text
        AnimationPlayer.PlayFront(BackgroundStructure);
        AnimationPlayer.Execute();
    }
}