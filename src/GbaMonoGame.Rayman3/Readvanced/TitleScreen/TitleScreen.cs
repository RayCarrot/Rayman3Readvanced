using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework.Graphics;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace GbaMonoGame.Rayman3.Readvanced;

// TODO: Play music?
public class TitleScreen : Frame
{
    public TitleScreen(bool fadeIn)
    {
        FadeIn = fadeIn;
    }

    public bool FadeIn { get; }

    public AnimationPlayer AnimationPlayer { get; set; }

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
        // Load the rom asynchronously while fading out
        LoadRomTask = Task.Run(() =>
        {
            // Get the game paths
            GetGamePaths(platform, out string gameDirectory, out string[] gameFileNames);

            // Initialize the rom
            Rom.Init(gameDirectory, gameFileNames, Game.Rayman3, platform);
        });

        TransitionsFX.FadeOutInit(2);
    }

    private void StartGame()
    {
        // Save last played platform
        Engine.LocalConfig.General.LastPlayedPlatform = Games[SelectedGameIndex].Platform;

        // Load the language
        Localization.SetLanguage(Engine.LocalConfig.Display.Language);

        // TODO: Load saved volume

        int? lastSaveSlot = Games[SelectedGameIndex].Platform switch
        {
            Platform.GBA => Engine.LocalConfig.General.LastPlayedGbaSaveSlot,
            Platform.NGage => Engine.LocalConfig.General.LastPlayedNGageSaveSlot,
            _ => throw new UnsupportedPlatformException()
        };

        if (LoadLastSave && lastSaveSlot != null && SaveGameManager.SlotExists(lastSaveSlot.Value))
        {
            // The seed normally gets set in the intro, so do it now instead since we're skipping that
            Random.SetSeed(GameTime.ElapsedFrames);

            // Load the save slot
            GameInfo.Load(lastSaveSlot.Value);
            GameInfo.GotoLastSaveGame();
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
                Platform.GBA => Engine.LocalConfig.General.LastPlayedGbaSaveSlot,
                Platform.NGage => Engine.LocalConfig.General.LastPlayedNGageSaveSlot,
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
            ]);
        }
        // The rom does not exist
        else
        {
            game.SetOptions(
            [
                new TitleScreenOptionsList.Option("LOCATE ROM", () =>
                {
                    if (game.Platform == Platform.GBA)
                    {
                        string selectedFilePath = FileDialog.OpenFile("Select the game ROM", new FileDialog.FileFilter("gba", "GBA ROM files"));

                        if (selectedFilePath != null)
                        {
                            // TODO: Verify the file

                            // Copy the file
                            Directory.CreateDirectory(gameDirectory);
                            File.Copy(selectedFilePath, Path.Combine(gameDirectory, gameFileNames[0]));

                            // Update
                            UpdateGameOptions(game);
                        }
                    }
                    else if (game.Platform == Platform.NGage)
                    {
                        string selectedDirectoryPath = FileDialog.OpenFolder("Select the game folder");

                        if (selectedDirectoryPath != null)
                        {
                            // The user might have selected the game root directory, in which case we need to navigate down
                            if (Directory.Exists(Path.Combine(selectedDirectoryPath, "system")))
                                selectedDirectoryPath = Path.Combine(selectedDirectoryPath, "system", "apps", "rayman3");

                            // TODO: Verify the directory

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
                }),
            ]);
        }
    }

    public override void Init()
    {
        RenderContext renderContext = new FixedResolutionRenderContext(Resolution.Modern);

        AnimationPlayer = new AnimationPlayer(false, null);
        TransitionsFX.Init(true);

        if (FadeIn)
            TransitionsFX.FadeInInit(4);

        CloudsShader = Engine.FrameContentManager.Load<Effect>(Assets.TitleScreenCloudsShader);

        Texture2D gbaClouds = Engine.FrameContentManager.Load<Texture2D>(Assets.TitleScreenGBACloudsTexture);
        Texture2D nGageClouds = Engine.FrameContentManager.Load<Texture2D>(Assets.TitleScreenNGageCloudsTexture);
        Texture2D background = Engine.FrameContentManager.Load<Texture2D>(Assets.TitleScreenTexture);

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
                Scale = wrap, // Scale by the wrapping and correct in shader
            },
            RenderOptions = { RenderContext = renderContext, Shader = CloudsShader },
        });
        Gfx.AddScreen(new GfxScreen(1)
        {
            IsEnabled = true,
            Priority = 1,
            Offset = Vector2.Zero,
            Renderer = new TextureScreenRenderer(background),
            RenderOptions = { RenderContext = renderContext },
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

        SelectedGameIndex = Array.FindIndex(Games, x => x.Platform == Engine.LocalConfig.General.LastPlayedPlatform);
        Games[SelectedGameIndex].SelectedIndex = 0;
    }

    public override void Step()
    {
        TransitionsFX.StepAll();

        // Quit game
        if (SelectedGameIndex == -1)
        {
            QuitGameOptionsList.Step();
        }
        // Select game
        else if (!Cursor.IsMoving && LoadRomTask == null)
        {
            // Change selected game
            if (JoyPad.IsButtonJustPressed(GbaInput.Left) || JoyPad.IsButtonJustPressed(GbaInput.Right))
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
        // Error
        else if (LoadRomTask is { IsFaulted: true })
        {
            if (LoadRomTask.Exception?.InnerException != null)
                ExceptionDispatchInfo.Capture(LoadRomTask.Exception.InnerException).Throw();
            else
                throw new Exception("Unknown error when loading the ROM");
        }
        // Loaded
        else if (LoadRomTask is { IsCompletedSuccessfully: true })
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