using System;
using BinarySerializer.Nintendo.GBA;
using BinarySerializer.Ubisoft.GbaEngine;
using BinarySerializer.Ubisoft.GbaEngine.Rayman3;
using GbaMonoGame.AnimEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.Readvanced;

public class GameModeMenuPage : MenuPage
{
    public GameModeMenuPage(ModernMenuAll menu) : base(menu) { }

    private const float GameLogoBaseX = 290;
    private const float GameLogoBaseY = 16;

    public override bool UsesCursor => true;
    public override int BackgroundPalette => 3;
    public override int LineHeight => 16;

    public AnimatedObject GameLogo { get; set; }

    public uint GameLogoPrevMovedTime { get; set; }
    public int GameLogoYOffset { get; set; }
    public int GameLogoYSpeed { get; set; }
    public int GameLogoSinValue { get; set; }
    public int GameLogoMovementXOffset { get; set; }
    public int GameLogoMovementWidth { get; set; }
    public int GameLogoMovementXCountdown { get; set; }

    // Custom values for smooth movement
    public float GameLogoStartX { get; set; }
    public float GameLogoEndX { get; set; }

    private void MoveGameLogo()
    {
        // Move Y
        if (GameLogoYOffset < 56)
        {
            // Move down
            GameLogo.ScreenPos = GameLogo.ScreenPos with { Y = GameLogoYOffset * 2 - 54 };
            GameLogoYOffset += 4;
        }
        else if (GameLogoYSpeed != 12)
        {
            // Bounce up and down
            GameLogoSinValue = (GameLogoSinValue + 16) % 256;

            float y = 56 + MathHelpers.Sin256(GameLogoSinValue) * GameLogoYSpeed;
            GameLogo.ScreenPos = GameLogo.ScreenPos with { Y = y };

            if (GameLogoYSpeed == 20 && GameLogoSinValue == 96)
                SoundEventsManager.ProcessEvent(Rayman3SoundEvent.Play__Pannel_BigFoot1_Mix02);

            if (GameLogoSinValue == 0)
                GameLogoYSpeed -= 4;
        }
        else if (GameLogo.ScreenPos.Y > GameLogoBaseY)
        {
            // Move up
            GameLogo.ScreenPos -= new Vector2(0, 1);
        }

        // Move X (back and forth from a width of 10 to 0)
        uint elapsedTime = GameTime.ElapsedFrames - GameLogoPrevMovedTime;
        uint targetTime = GameLogoMovementWidth switch
        {
            10 => 4,
            9 => 6,
            8 => 8,
            7 => 10,
            6 => 12,
            5 => 14,
            4 => 16,
            3 => 18,
            2 => 20,
            1 => 22,
            _ => 0
        };

        // Custom code for smooth movement
        if (targetTime != 0)
            GameLogo.ScreenPos = GameLogo.ScreenPos with { X = MathHelper.Lerp(GameLogoStartX, GameLogoEndX, elapsedTime / (float)targetTime) };

        if (targetTime != 0 && elapsedTime > targetTime)
        {
            int x;

            if (GameLogoMovementXOffset < GameLogoMovementWidth * 2)
            {
                x = GameLogoMovementXOffset - GameLogoMovementWidth;
            }
            else if (GameLogoMovementXOffset < GameLogoMovementWidth * 4)
            {
                x = GameLogoMovementWidth * 3 - GameLogoMovementXOffset;
            }
            else
            {
                GameLogoMovementXOffset = 0;
                if (GameLogoMovementXCountdown == 2)
                {
                    GameLogoMovementWidth--;
                    GameLogoMovementXCountdown = 0;
                }
                else
                {
                    GameLogoMovementXCountdown++;
                }

                x = -GameLogoMovementWidth;
            }

            GameLogoMovementXOffset++;
            GameLogoPrevMovedTime = GameTime.ElapsedFrames;

            // Custom code for smooth movement
            GameLogoStartX = GameLogo.ScreenPos.X;
            GameLogoEndX = GameLogoBaseX + x;
        }
    }

    private void ReplaceLogo()
    {
        const int animId = 0;
        const int baseCacheId = 1000;

        Animation anim = Rom.CopyResource(GameLogo.Resource.Animations[animId]);
        anim.AffineMatrices = GameLogo.Resource.Animations[animId].AffineMatrices;
        anim.PaletteCycleAnimation = GameLogo.Resource.Animations[animId].PaletteCycleAnimation;

        anim.ChannelsPerFrame = [1];
        anim.Channels =
        [
            new AnimationChannel
            {
                ChannelType = AnimationChannelType.Sprite,
                XPosition = -63,
                YPosition = -75,
                ObjectMode = OBJ_ATTR_ObjectMode.REG,
                TileIndex = baseCacheId,
                FlipX = false,
                FlipY = false,
            }
        ];

        GameLogo.ReplaceAnimation(animId, anim);

        // Load the logo texture
        Engine.TextureCache.SetObject(
            GameLogo.Resource.Offset,
            baseCacheId,
            Engine.FrameContentManager.Load<Texture2D>(Assets.MenuLogoTexture));
    }

    protected override void Init()
    {
        // Add menu options
        AddOption(new TextMenuOption("SINGLE PLAYER"));
        AddOption(new TextMenuOption("MULTIPLAYER"));
        AddOption(new TextMenuOption("BONUS"));
        AddOption(new TextMenuOption("OPTIONS"));
        AddOption(new TextMenuOption("CREDITS"));
        AddOption(new TextMenuOption("QUIT GAME"));

        // Create animations
        AnimatedObjectResource gameLogoAnimations = Rom.LoadResource<AnimatedObjectResource>(Rayman3DefinedResource.MenuGameLogoAnimations);

        GameLogo = new AnimatedObject(gameLogoAnimations, gameLogoAnimations.IsDynamic)
        {
            IsFramed = true,
            BgPriority = 1,
            ObjPriority = 0,
            ScreenPos = new Vector2(GameLogoBaseX, GameLogoBaseY),
            CurrentAnimation = 0,
            RenderContext = RenderContext,
        };

        if (Engine.Config.UseReadvancedLogo)
            ReplaceLogo();

        // Reset values
        GameLogoPrevMovedTime = 0;
        GameLogoMovementXOffset = 10;
        GameLogoMovementWidth = 10;
        GameLogoMovementXCountdown = 0;
        GameLogoYSpeed = 20;
        GameLogoSinValue = 0;
        GameLogoYOffset = 0;
        GameLogoStartX = GameLogo.ScreenPos.X;
        GameLogoEndX = GameLogo.ScreenPos.X;
    }

    protected override void Step_Active()
    {
        if (JoyPad.IsButtonJustPressed(GbaInput.Up))
        {
            SetSelectedOption(SelectedOption - 1);
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.Down))
        {
            SetSelectedOption(SelectedOption + 1);
        }
        else if (JoyPad.IsButtonJustPressed(GbaInput.A))
        {
            switch (SelectedOption)
            {
                case 0:
                    Menu.ChangePage(new SinglePlayerMenuPage(Menu), NewPageMode.Next);
                    break;

                case 1:
                    // TODO: Implement multiplayer menus
                    Menu.ChangePage(new GameModeMenuPage(Menu), NewPageMode.Next);
                    break;

                case 2:
                    Menu.ChangePage(new BonusMenuPage(Menu), NewPageMode.Next);
                    break;

                case 3:
                    Menu.ChangePage(new OptionsMenuPage(Menu), NewPageMode.Next);
                    break;

                case 4:
                    CursorClick(() =>
                    {
                        FadeOut(4, () =>
                        {
                            FrameManager.SetNextFrame(new Credits(false));
                        });
                    });
                    break;

                // TODO: Implement quit game menu
                case 5:
                    Menu.ChangePage(new GameModeMenuPage(Menu), NewPageMode.Next);
                    break;

                default:
                    throw new Exception("Invalid selection");
            }
        }
    }

    protected override void Step_TransitionOut()
    {
        // Move out the logo
        GameLogo.ScreenPos = GameLogo.ScreenPos with { Y = GameLogoBaseY - TransitionValue / 2f };
    }

    protected override void Draw(AnimationPlayer animationPlayer)
    {
        DrawOptions(animationPlayer);
        MoveGameLogo();
        animationPlayer.Play(GameLogo);
    }
}