using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO: Default to select last played version
public class TitleScreen : Frame
{
    public AnimationPlayer AnimationPlayer { get; set; }
    public TransitionsFX TransitionsFX { get; set; }

    public Task LoadRomTask { get; set; }
    public bool LoadLastSave { get; set; }

    public Effect CloudsShader { get; set; }
    public Cursor Cursor { get; set; }
    public TitleScreenGame[] Games { get; set; }
    public int SelectedGameIndex { get; set; }

    private void GetGamePaths(Platform platform, out string gameDirectory, out string[] gameFileNames, out string saveFileName)
    {
        if (platform == Platform.GBA)
        {
            gameDirectory = FileManager.GetDataDirectory("Gba");
            gameFileNames =
            [
               "Rayman 3.gba"
            ];
            saveFileName = "Rayman 3.sav";
        }
        else if (platform == Platform.NGage)
        {
            gameDirectory = FileManager.GetDataDirectory("NGage");
            gameFileNames =
            [
                "rayman3.app",
                "rayman3.dat",
            ];
            saveFileName = "save.dat";
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    private void LoadRom(Platform platform)
    {
        // TODO: Handle exceptions - right now they'll be ignored since we're not awaiting the task
        // Load the rom asynchronously while fading out
        LoadRomTask = Task.Run(() =>
        {
            // Get the game paths
            GetGamePaths(platform, out string gameDirectory, out string[] gameFileNames, out string saveFileName);

            // Initialize the rom
            Rom.Init(gameDirectory, gameFileNames, saveFileName, Game.Rayman3, platform);
        });

        TransitionsFX.FadeOutInit(1 / 16f);
    }

    private void UpdateGameOptions(TitleScreenGame game)
    {
        // Get the game paths
        GetGamePaths(game.Platform, out string gameDirectory, out string[] gameFileNames, out string _);

        // The rom exists!
        if (gameFileNames.All(x => File.Exists(Path.Combine(gameDirectory, x))))
        {
            game.SetOptions(
            [
                new TitleScreenGame.Option("CONTINUE", x =>
                {
                    LoadLastSave = true;
                    LoadRom(x.Platform);
                }),
                new TitleScreenGame.Option("START", x =>
                {
                    LoadLastSave = false;
                    LoadRom(x.Platform);
                }),
                new TitleScreenGame.Option("OPTIONS", _ =>
                {

                })
            ]);
        }
        // The rom does not exist
        else
        {
            game.SetOptions(
            [
                new TitleScreenGame.Option("LOCATE ROM", x =>
                {
                    // Force windowed mode for this
                    DisplayMode prevDisplayMode = Engine.GameWindow.DisplayMode;
                    if (Engine.GameWindow.DisplayMode != DisplayMode.Windowed)
                        Engine.GameWindow.DisplayMode = DisplayMode.Windowed;

                    if (x.Platform == Platform.GBA)
                    {
                        OpenFileDialog fileDialog = new()
                        {
                            Title = "Select the game ROM",
                            Filter = "gba files (*.gba)|*.gba|All files (*.*)|*.*",
                        };

                        if (fileDialog.ShowDialog() == DialogResult.OK)
                        {
                            // Get the selected file
                            string selectedFilePath = fileDialog.FileName;

                            // TODO: Verify the file

                            // TODO: Try/catch
                            // Copy the file
                            Directory.CreateDirectory(gameDirectory);
                            File.Copy(selectedFilePath, Path.Combine(gameDirectory, gameFileNames[0]));

                            // Update
                            UpdateGameOptions(x);

                        }
                    }
                    else if (x.Platform == Platform.NGage)
                    {
                        FolderBrowserDialog folderDialog = new()
                        {
                            Description = "Select the game folder",
                        };

                        if (folderDialog.ShowDialog() == DialogResult.OK)
                        {
                            // Get the selected directory
                            string selectedDirectoryPath = folderDialog.SelectedPath;

                            // The user might have selected the game root directory, in which case we need to navigate down
                            if (Directory.Exists(Path.Combine(selectedDirectoryPath, "system")))
                                selectedDirectoryPath = Path.Combine(selectedDirectoryPath, "system", "apps", "rayman3");

                            // TODO: Verify the directory

                            // TODO: Try/catch
                            // Copy the files
                            Directory.CreateDirectory(gameDirectory);
                            foreach (string gameFileName in gameFileNames)
                                File.Copy(Path.Combine(selectedDirectoryPath, gameFileName), Path.Combine(gameDirectory, gameFileName));

                            // Update
                            UpdateGameOptions(x);
                        }
                    }
                    else
                    {
                        throw new UnsupportedPlatformException();
                    }

                    // Restore the display mode
                    if (prevDisplayMode != DisplayMode.Windowed)
                        Engine.GameWindow.DisplayMode = prevDisplayMode;
                }),
            ]);
        }
    }

    public override void Init()
    {
        Engine.GameViewPort.SetFixedResolution(new Vector2(384, 216));

        AnimationPlayer = new AnimationPlayer(false, null);
        TransitionsFX = new TransitionsFX(true);

        CloudsShader = Engine.FrameContentManager.Load<Effect>("TitleScreenCloudsShader");

        Texture2D gbaClouds = Engine.FrameContentManager.Load<Texture2D>("TitleScreenGBAClouds");
        Texture2D nGageClouds = Engine.FrameContentManager.Load<Texture2D>("TitleScreenNGageClouds");
        Texture2D background = Engine.FrameContentManager.Load<Texture2D>("TitleScreen");

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
            new TitleScreenGame(Platform.GBA, Cursor, new Vector2(98, 172)),
            new TitleScreenGame(Platform.NGage, Cursor, new Vector2(98 + 190, 172))
        ];

        foreach (TitleScreenGame game in Games)
            UpdateGameOptions(game);

        SelectedGameIndex = 0;
        Games[0].SelectedIndex = 0;
    }

    public override void Step()
    {
        TransitionsFX.StepAll();

        if (!Cursor.IsMoving && LoadRomTask == null)
        {
            // Change selected game
            if (JoyPad.IsButtonJustPressed(GbaInput.Left) || JoyPad.IsButtonJustPressed(GbaInput.Right))
            {
                int prevSelectedGameIndex = SelectedGameIndex;
                SelectedGameIndex = SelectedGameIndex == 1 ? 0 : 1;

                Games[SelectedGameIndex].SelectedIndex = 0;
                Games[prevSelectedGameIndex].SelectedIndex = -1;
            }

            // Step games
            foreach (TitleScreenGame game in Games)
                game.Step();
        }
        else if (LoadRomTask is { IsCompleted: true } && TransitionsFX.IsFadeOutFinished)
        {
            // TODO: Load from save - normalize how that works between GBA and N-Gage
            // Set the language
            Localization.SetLanguage(0);

            // Set the initial frame
            if (LoadLastSave)
            {
                // TODO: Implement
            }
            else
            {
                FrameManager.SetNextFrame(Games[SelectedGameIndex].Platform switch
                {
                    Platform.GBA => new Intro(),
                    Platform.NGage => new NGageSplashScreensAct(),
                    _ => throw new UnsupportedPlatformException()
                });
            }

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