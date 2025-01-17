using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework.Graphics;
using Keys = Microsoft.Xna.Framework.Input.Keys;

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
    public SpriteFontTextObject QuitGameHeader { get; set; }
    public TitleScreenOptionsList QuitGameOptionsList { get; set; }
    public int SelectedGameIndex { get; set; }

    private void GetGamePaths(Platform platform, out string gameDirectory, out string[] gameFileNames)
    {
        if (platform == Platform.GBA)
        {
            gameDirectory = FileManager.GetDataDirectory("Gba");
            gameFileNames =
            [
               "rayman3.gba"
            ];
        }
        else if (platform == Platform.NGage)
        {
            gameDirectory = FileManager.GetDataDirectory("NGage");
            gameFileNames =
            [
                "rayman3.app",
                "rayman3.dat",
            ];
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
            GetGamePaths(platform, out string gameDirectory, out string[] gameFileNames);

            // Initialize the rom
            Rom.Init(gameDirectory, gameFileNames, Game.Rayman3, platform);
        });

        TransitionsFX.FadeOutInit(1 / 16f);
    }

    private void StartGame()
    {
        // Load the language
        Localization.SetLanguage(Engine.Config.Language);

        // TODO: Load saved volume

        int? lastSaveSlot = Games[SelectedGameIndex].Platform switch
        {
            Platform.GBA => Engine.Config.LastPlayedGbaSaveSlot,
            Platform.NGage => Engine.Config.LastPlayedNGageSaveSlot,
            _ => throw new UnsupportedPlatformException()
        };

        if (LoadLastSave && lastSaveSlot != null && SaveGameManager.SlotExists(lastSaveSlot.Value))
        {
            // Load the save slot
            GameInfo.Load(lastSaveSlot.Value);
            GameInfo.LoadLastWorld();
            GameInfo.CurrentSlot = lastSaveSlot.Value;
        }
        else
        {
            // Set the initial frame
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

    private void UpdateGameOptions(TitleScreenGame game)
    {
        // Get the game paths
        GetGamePaths(game.Platform, out string gameDirectory, out string[] gameFileNames);

        // The rom exists!
        if (gameFileNames.All(x => File.Exists(Path.Combine(gameDirectory, x))))
        {
            int? lastSaveSlot = game.Platform switch
            {
                Platform.GBA => Engine.Config.LastPlayedGbaSaveSlot,
                Platform.NGage => Engine.Config.LastPlayedNGageSaveSlot,
                _ => throw new UnsupportedPlatformException()
            };

            bool canContinue = lastSaveSlot != null;

            game.SetOptions(
            [
                new TitleScreenOptionsList.Option("CONTINUE", canContinue, () =>
                {
                    LoadLastSave = true;
                    LoadRom(game.Platform);
                }),
                new TitleScreenOptionsList.Option("START", () =>
                {
                    LoadLastSave = false;
                    LoadRom(game.Platform);
                }),
                new TitleScreenOptionsList.Option(
                [
                    // TODO: Implement
                    new TitleScreenOptionsList.Option("MODERN", () =>
                    {

                    }),
                    new TitleScreenOptionsList.Option("ORIGINAL", () =>
                    {

                    })
                ])
            ]);
        }
        // The rom does not exist
        else
        {
            game.SetOptions(
            [
                new TitleScreenOptionsList.Option("LOCATE ROM", () =>
                {
                    // Force windowed mode for this
                    DisplayMode prevDisplayMode = Engine.GameWindow.DisplayMode;
                    if (Engine.GameWindow.DisplayMode != DisplayMode.Windowed)
                        Engine.GameWindow.DisplayMode = DisplayMode.Windowed;

                    if (game.Platform == Platform.GBA)
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
                            UpdateGameOptions(game);

                        }
                    }
                    else if (game.Platform == Platform.NGage)
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
                            UpdateGameOptions(game);
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
        RenderContext renderContext = new FixedResolutionRenderContext(new Vector2(384, 216));

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
        Vector2 cloudsPos = new((gbaClouds.Width * wrap.X - renderContext.Resolution.X) / 2f, 0);
        
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
            RenderContext = renderContext,
        });
        Gfx.AddScreen(new GfxScreen(1)
        {
            IsEnabled = true,
            Priority = 1,
            Offset = Vector2.Zero,
            Renderer = new TextureScreenRenderer(background),
            RenderContext = renderContext,
        });

        Cursor = new Cursor(renderContext);

        const float basePosX = 98;
        const float basePosY = 172;
        const float gamesDistance = 190;

        Games =
        [
            new TitleScreenGame(renderContext, Platform.GBA, Cursor, new Vector2(basePosX, basePosY)),
            new TitleScreenGame(renderContext, Platform.NGage, Cursor, new Vector2(basePosX + gamesDistance, basePosY))
        ];

        QuitGameHeader = new SpriteFontTextObject
        {
            BgPriority = 0,
            ObjPriority = 0,
            ScreenPos = new Vector2(basePosX + gamesDistance / 2f, basePosY - 6),
            Font = ReadvancedFonts.MenuYellow,
            Text = "ARE YOU SURE YOU WANT TO QUIT?",
            RenderContext = renderContext,
        };

        QuitGameHeader.ScreenPos -= new Vector2(QuitGameHeader.Font.GetWidth(QuitGameHeader.Text) / 2, 0);

        QuitGameOptionsList = new TitleScreenOptionsList(renderContext, Cursor, new Vector2(basePosX + gamesDistance / 2f, basePosY + 16));
        QuitGameOptionsList.SetOptions(
        [
            new TitleScreenOptionsList.Option("YES", () =>
            {
                Engine.GbaGame.Exit();
            }),
            new TitleScreenOptionsList.Option("NO", () =>
            {
                foreach (TitleScreenGame game in Games)
                    game.SelectedIndex = -1;

                SelectedGameIndex = 0;
                Games[SelectedGameIndex].SelectedIndex = 0;
            }),
        ]);

        foreach (TitleScreenGame game in Games)
            UpdateGameOptions(game);

        SelectedGameIndex = 0;
        Games[0].SelectedIndex = 0;
    }

    public override void Step()
    {
        TransitionsFX.StepAll();

        if (SelectedGameIndex == -1)
        {
            QuitGameOptionsList.Step();
        }
        else if (!Cursor.IsMoving && LoadRomTask == null)
        {
            // Change selected game
            bool canPressLeftRight = Games[SelectedGameIndex].Options[Games[SelectedGameIndex].SelectedIndex].SubOptions == null;
            if (canPressLeftRight && (JoyPad.IsButtonJustPressed(GbaInput.Left) || JoyPad.IsButtonJustPressed(GbaInput.Right)))
            {
                int prevSelectedGameIndex = SelectedGameIndex;
                SelectedGameIndex = SelectedGameIndex == 1 ? 0 : 1;

                Games[SelectedGameIndex].SelectedIndex = 0;
                Games[prevSelectedGameIndex].SelectedIndex = -1;
            }
            else if (JoyPad.IsButtonJustPressed(GbaInput.B) || InputManager.IsButtonJustPressed(Keys.Escape))
            {
                SelectedGameIndex = -1;
                QuitGameOptionsList.SelectedIndex = 0;
            }

            // Step games
            if (SelectedGameIndex != -1)
            {
                foreach (TitleScreenGame game in Games)
                    game.Step();
            }
        }
        else if (LoadRomTask is { IsCompleted: true } && TransitionsFX.IsFadeOutFinished)
        {
            StartGame();
        }

        Cursor.Step();

        if (SelectedGameIndex == -1)
        {
            AnimationPlayer.Play(QuitGameHeader);
            QuitGameOptionsList.Draw(AnimationPlayer);
        }
        else
        {
            foreach (TitleScreenGame game in Games)
                game.Draw(AnimationPlayer);
        }

        Cursor.Draw(AnimationPlayer);

        AnimationPlayer.Execute();

        // Update the time in the clouds shader for the scrolling
        CloudsShader.Parameters["Time"].SetValue(GameTime.ElapsedFrames);
    }
}