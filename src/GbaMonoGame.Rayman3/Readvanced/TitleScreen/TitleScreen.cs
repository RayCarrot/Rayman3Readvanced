using System.Threading.Tasks;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO: Unload textures
// TODO: Check if roms exist, if not then prompt user to select the rom file
// TODO: Default to select last played version
public class TitleScreen : Frame
{
    public AnimationPlayer AnimationPlayer { get; set; }
    public TransitionsFX TransitionsFX { get; set; }

    public Task LoadRomTask { get; set; }

    public Effect CloudsShader { get; set; }
    public Cursor Cursor { get; set; }
    public TitleScreenGame[] Games { get; set; }
    public int SelectedGameIndex { get; set; }

    public override void Init()
    {
        Engine.GameViewPort.SetFixedResolution(new Vector2(384, 216));

        AnimationPlayer = new AnimationPlayer(false, null);
        TransitionsFX = new TransitionsFX(true);

        CloudsShader = Engine.ContentManager.Load<Effect>("TitleScreenCloudsShader");

        Texture2D gbaClouds = Engine.ContentManager.Load<Texture2D>("TitleScreenGBAClouds");
        Texture2D nGageClouds = Engine.ContentManager.Load<Texture2D>("TitleScreenNGageClouds");
        Texture2D background = Engine.ContentManager.Load<Texture2D>("TitleScreen");

        CloudsShader.Parameters["SecondaryTexture"].SetValue(nGageClouds);

        // Wrap horizontally twice
        Vector2 wrap = new(2, 1);

        // Center so that the blending happens in the middle
        Vector2 cloudsPos = new((gbaClouds.Width * wrap.X - Engine.GameViewPort.GameResolution.X) / 2f, 0);
        
        Gfx.AddScreen(new GfxScreen(0)
        {
            IsEnabled = true,
            Priority = 2,
            Offset = cloudsPos,
            Renderer = new TextureScreenRenderer(gbaClouds)
            {
                Shader = CloudsShader,
                Scale = wrap, // Scale by the wrapping and correct in shader
            },
        });
        Gfx.AddScreen(new GfxScreen(1)
        {
            IsEnabled = true,
            Priority = 1,
            Offset = Vector2.Zero,
            Renderer = new TextureScreenRenderer(background),
        });

        Cursor = new Cursor();

        Games =
        [
            new TitleScreenGame(Platform.GBA, Cursor, new Vector2(65, 172)),
            new TitleScreenGame(Platform.NGage, Cursor, new Vector2(255, 172))
        ];

        SelectedGameIndex = 0;
        Games[0].SelectedIndex = 0;
    }

    public override void Step()
    {
        TransitionsFX.StepAll();

        if (!Cursor.IsMoving && LoadRomTask == null)
        {
            if (JoyPad.IsButtonJustPressed(GbaInput.Left) || JoyPad.IsButtonJustPressed(GbaInput.Right))
            {
                int prevSelectedGameIndex = SelectedGameIndex;
                SelectedGameIndex = SelectedGameIndex == 1 ? 0 : 1;

                Games[SelectedGameIndex].SelectedIndex = 0;
                Games[prevSelectedGameIndex].SelectedIndex = -1;
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.A))
            {
                // TODO: Check selected menu index for what to do when you press A
                
                // TODO: Handle exceptions - right now they'll be ignored since we're not awaiting the task
                // Load the rom asynchronously while fading out
                LoadRomTask = Task.Run(() =>
                {
                    Platform platform = Games[SelectedGameIndex].Platform;

                    string gameDirectory;
                    string gameFileName;
                    string saveFileName;

                    if (platform == Platform.GBA)
                    {
                        gameDirectory = FileManager.GetDataDirectory("Gba");
                        gameFileName = "Rayman 3";
                        saveFileName = "Rayman 3.sav";
                    }
                    else if (platform == Platform.NGage)
                    {
                        gameDirectory = FileManager.GetDataDirectory("NGage");
                        gameFileName = "rayman3";
                        saveFileName = "save.dat";
                    }
                    else
                    {
                        throw new UnsupportedPlatformException();
                    }

                    // Initialize the rom
                    Rom.Init(gameDirectory, gameFileName, saveFileName, Game.Rayman3, platform);
                });

                TransitionsFX.FadeOutInit(2 / 16f);
            }

            foreach (TitleScreenGame game in Games)
                game.Step();
        }
        else if (LoadRomTask is { IsCompleted: true } && TransitionsFX.IsFadeOutFinished)
        {
            // TODO: Load from save - normalize how that works between GBA and N-Gage
            // Set the language
            Localization.SetLanguage(0);

            // Set the initial frame
            FrameManager.SetNextFrame(Games[SelectedGameIndex].Platform switch
            {
                Platform.GBA => new Intro(),
                Platform.NGage => new NGageSplashScreensAct(),
                _ => throw new UnsupportedPlatformException()
            });

            // Reset game time
            GameTime.Reset();
        }

        Cursor.Step();

        foreach (TitleScreenGame game in Games)
            game.Draw(AnimationPlayer);

        Cursor.Draw(AnimationPlayer);

        AnimationPlayer.Execute();

        // Update the time in the clouds shader for the scrolling
        CloudsShader.Parameters["Time"].SetValue(GameTime.ElapsedFrames);
    }
}