using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

public class MenuData
{
    public MenuData(RenderContext renderContext, int multiplayerMultiPakPlayersOffsetY, int multiplayerSinglePakPlayersOffsetY)
    {
        if (Rom.Platform == Platform.GBA)
        {
            AnimatedObjectResource propsAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuPropAnimations);
            AnimatedObjectResource startEraseAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuStartEraseAnimations);
            AnimatedObjectResource gameLogoAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuGameLogoAnimations);
            AnimatedObjectResource gameModeAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuGameModeAnimations);
            AnimatedObjectResource languageListAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuLanguageListAnimations);
            AnimatedObjectResource optionsAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuOptionsAnimations);
            AnimatedObjectResource slotEmptyAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuSlotEmptyAnimations);
            AnimatedObjectResource multiplayerModeAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerModeAnimations);
            AnimatedObjectResource multiplayerPlayersAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerPlayersAnimations);
            AnimatedObjectResource multiplayerTypeAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerTypeAnimations);
            AnimatedObjectResource multiplayerTypeFrameAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerTypeFrameAnimations);
            AnimatedObjectResource multiplayerTypeIconAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerTypeIconAnimations);
            AnimatedObjectResource multiplayerMapAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerMapAnimations);
            AnimatedObjectResource steamAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuSteamAnimations);

            Wheel1 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                ScreenPos = new Vector2(7, 110),
                CurrentAnimation = 2,
                AffineMatrix = AffineMatrix.Identity,
                RenderContext = renderContext,
            };

            Wheel2 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                ScreenPos = new Vector2(136, 110),
                CurrentAnimation = 3,
                AffineMatrix = AffineMatrix.Identity,
                RenderContext = renderContext,
            };

            Wheel3 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 33,
                ScreenPos = new Vector2(172, 110),
                CurrentAnimation = 4,
                AffineMatrix = AffineMatrix.Identity,
                RenderContext = renderContext,
            };

            Wheel4 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                ScreenPos = new Vector2(66, 144),
                CurrentAnimation = 3,
                AffineMatrix = AffineMatrix.Identity,
                RenderContext = renderContext,
            };

            Cursor = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(33, 67),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            Stem = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(47, 160),
                CurrentAnimation = 14,
                RenderContext = renderContext,
            };

            Steam = new AnimatedObject(steamAnimations, steamAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(27, 20),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            LanguageList = new AnimatedObject(languageListAnimations, languageListAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = new Vector2(120, 28),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            GameModeList = new AnimatedObject(gameModeAnimations, gameModeAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(73, 52),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            GameLogo = new AnimatedObject(gameLogoAnimations, gameLogoAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(174, 16),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            SoundsBase = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(210, 65),
                CurrentAnimation = 7,
                RenderContext = renderContext,
            };

            MusicVolume = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(210, 65),
                CurrentAnimation = 5,
                RenderContext = renderContext,
            };

            SfxVolume = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(210, 89),
                CurrentAnimation = 5,
                RenderContext = renderContext,
            };

            SlotLumIcons = new AnimatedObject[3];
            SlotCageIcons = new AnimatedObject[3];
            SlotIcons = new AnimatedObject[3];
            SlotLumTexts = new SpriteTextObject[3];
            SlotCageTexts = new SpriteTextObject[3];
            SlotEmptyTexts = new AnimatedObject[3];
            for (int i = 0; i < 3; i++)
            {
                SlotLumIcons[i] = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(107, 55 + i * 18),
                    CurrentAnimation = 13,
                    RenderContext = renderContext,
                };
                SlotCageIcons[i] = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(165, 51 + i * 18),
                    CurrentAnimation = 11,
                    RenderContext = renderContext,
                };
                SlotIcons[i] = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(84, 54 + i * 18),
                    CurrentAnimation = 8 + i,
                    RenderContext = renderContext,
                };
                SlotLumTexts[i] = new SpriteTextObject()
                {
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(129, 55 + i * 18),
                    Text = "1000",
                    FontSize = FontSize.Font16,
                    Color = TextColor.Menu,
                    RenderContext = renderContext,
                };
                SlotCageTexts[i] = new SpriteTextObject()
                {
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(190, 55 + i * 18),
                    Text = "50",
                    FontSize = FontSize.Font16,
                    Color = TextColor.Menu,
                    RenderContext = renderContext,
                };
                SlotEmptyTexts[i] = new AnimatedObject(slotEmptyAnimations, slotEmptyAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(126, 54 + i * 18),
                    CurrentAnimation = 0,
                    RenderContext = renderContext,
                };
            }

            OptionsSelection = new AnimatedObject(optionsAnimations, optionsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(73, 52),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            MultiplayerModeSelection = new AnimatedObject(multiplayerModeAnimations, multiplayerModeAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(73, 52),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            StartEraseSelection = new AnimatedObject(startEraseAnimations, startEraseAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(80, 30),
                CurrentAnimation = 1,
                RenderContext = renderContext,
            };

            StartEraseCursor = new AnimatedObject(startEraseAnimations, startEraseAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(106, 12),
                CurrentAnimation = 40,
                RenderContext = renderContext,
            };

            Texts = new SpriteTextObject[5];
            for (int i = 0; i < Texts.Length; i++)
            {
                Texts[i] = new SpriteTextObject()
                {
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(70, 32 + i * 16),
                    FontSize = FontSize.Font16,
                    Color = TextColor.Menu,
                    RenderContext = renderContext,
                };
            }

            MultiplayerPlayerSelection = new AnimatedObject(multiplayerPlayersAnimations, multiplayerPlayersAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                ScreenPos = new Vector2(145, 40 - multiplayerMultiPakPlayersOffsetY),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            MultiplayerPlayerNumberIcons = new AnimatedObject(multiplayerPlayersAnimations, multiplayerPlayersAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(102, 22 - multiplayerMultiPakPlayersOffsetY),
                CurrentAnimation = 4,
                RenderContext = renderContext,
            };

            MultiplayerPlayerSelectionIcons = new AnimatedObject[RSMultiplayer.MaxPlayersCount];
            for (int i = 0; i < MultiplayerPlayerSelectionIcons.Length; i++)
            {
                MultiplayerPlayerSelectionIcons[i] = new AnimatedObject(multiplayerPlayersAnimations, multiplayerPlayersAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 1,
                    ObjPriority = 16,
                    ScreenPos = new Vector2(104 + 24 * i, 49 - multiplayerMultiPakPlayersOffsetY),
                    CurrentAnimation = 8,
                    RenderContext = renderContext,
                };
            }

            MultiplayerPlayerSelectionHighlight = new AnimatedObject(multiplayerPlayersAnimations, multiplayerPlayersAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(104, 26 - multiplayerMultiPakPlayersOffsetY),
                CurrentAnimation = 10,
                RenderContext = renderContext,
            };

            MultiplayerTypeName = new AnimatedObject(multiplayerTypeAnimations, multiplayerTypeAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(142, 94),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            MultiplayerTypeFrame = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(140, 35),
                CurrentAnimation = 2,
                RenderContext = renderContext,
            };

            ArrowLeft = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(100, 50),
                CurrentAnimation = 1,
                RenderContext = renderContext,
            };

            ArrowRight = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(184, 50),
                CurrentAnimation = 1,
                RenderContext = renderContext,
            };

            MultiplayerTypeIcon = new AnimatedObject(multiplayerTypeIconAnimations, multiplayerTypeIconAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(115, 24),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            MultiplayerMapSelection = new AnimatedObject(multiplayerMapAnimations, multiplayerMapAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(120, 82),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            MultiplayerMapName1 = new SpriteTextObject()
            {
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(80, 56),
                FontSize = FontSize.Font16,
                Color = TextColor.Menu,
                RenderContext = renderContext,
            };

            MultiplayerMapName2 = new SpriteTextObject()
            {
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(70, 96),
                FontSize = FontSize.Font16,
                Color = TextColor.Menu,
                RenderContext = renderContext,
            };

            MultiplayerSinglePakPlayers = new AnimatedObject(multiplayerPlayersAnimations, multiplayerPlayersAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(168, 40 - multiplayerSinglePakPlayersOffsetY),
                CurrentAnimation = 11,
                RenderContext = renderContext,
            };
        }
        else if (Rom.Platform == Platform.NGage)
        {
            AnimatedObjectResource propsAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuPropAnimations);
            AnimatedObjectResource startEraseAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuStartEraseAnimations);
            AnimatedObjectResource symbolAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.NGageButtonSymbols);
            AnimatedObjectResource languageListAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuLanguageListAnimations);
            AnimatedObjectResource gameModeAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuGameModeAnimations);
            AnimatedObjectResource pauseSelectionAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.PauseSelectionAnimations);
            AnimatedObjectResource gameLogoAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuGameLogoAnimations);
            AnimatedObjectResource optionsAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuOptionsAnimations);
            AnimatedObjectResource connectionAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerConnectionAnimations);
            AnimatedObjectResource slotEmptyAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuSlotEmptyAnimations);
            AnimatedObjectResource multiplayerPlayersAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerPlayersAnimations);
            AnimatedObjectResource multiplayerTypeAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerTypeAnimations);
            AnimatedObjectResource multiplayerTypeFrameAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerTypeFrameAnimations);
            AnimatedObjectResource multiplayerTypeIconAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerTypeIconAnimations);
            AnimatedObjectResource multiplayerMapAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerMapAnimations);
            AnimatedObjectResource multiplayerCaptureTheFlagAnimations = Rom.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerCaptureTheFlagAnimations);

            Wheel2 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                ScreenPos = new Vector2(124, 145),
                CurrentAnimation = 3,
                AffineMatrix = AffineMatrix.Identity,
                RenderContext = renderContext,
            };

            Wheel4 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                ScreenPos = new Vector2(59, 174),
                CurrentAnimation = 3,
                AffineMatrix = AffineMatrix.Identity,
                RenderContext = renderContext,
            };

            Cursor = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(18, 77),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            Stem = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(32, 170),
                CurrentAnimation = 14,
                RenderContext = renderContext,
            };

            SelectSymbol = new AnimatedObject(symbolAnimations, false)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 0,
                CurrentAnimation = Localization.LanguageUiIndex,
                ScreenPos = new Vector2(-1, 190),
                RenderContext = renderContext,
            };

            BackSymbol = new AnimatedObject(symbolAnimations, false)
            {
                IsFramed = true,
                CurrentAnimation = 5 + Localization.LanguageUiIndex,
                ScreenPos = new Vector2(-1, 190),
                HorizontalAnchor = HorizontalAnchorMode.Right,
                RenderContext = renderContext,
            };

            LanguageList = new AnimatedObject(languageListAnimations, languageListAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(108, 100),
                CurrentAnimation = 13,
                RenderContext = renderContext,
            };

            MultiplayerCaptureTheFlagOptions = new AnimatedObject(multiplayerCaptureTheFlagAnimations, multiplayerCaptureTheFlagAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(84, 104),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            MultiplayerCaptureTheFlagModeName = new AnimatedObject(multiplayerCaptureTheFlagAnimations, multiplayerCaptureTheFlagAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(125, 76),
                CurrentAnimation = 5,
                RenderContext = renderContext,
            };

            MultiplayerCaptureTheFlagOptionsArrowLeft = new AnimatedObject(multiplayerCaptureTheFlagAnimations, multiplayerCaptureTheFlagAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(117, 0),
                CurrentAnimation = 15,
                RenderContext = renderContext,
            };

            MultiplayerCaptureTheFlagOptionsArrowRight = new AnimatedObject(multiplayerCaptureTheFlagAnimations, multiplayerCaptureTheFlagAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(166, 0),
                CurrentAnimation = 16,
                RenderContext = renderContext,
            };

            MultiplayerCaptureTheFlagOptionsColon = new AnimatedObject(multiplayerCaptureTheFlagAnimations, multiplayerCaptureTheFlagAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(134, 140),
                CurrentAnimation = 17,
                RenderContext = renderContext,
            };

            MultiplayerCaptureTheFlagOptionsFlagsDigit = new AnimatedObject(multiplayerCaptureTheFlagAnimations, multiplayerCaptureTheFlagAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(125, 109),
                CurrentAnimation = 18,
                RenderContext = renderContext,
            };

            MultiplayerCaptureTheFlagOptionsTimeDigits = new AnimatedObject[3];
            for (int i = 0; i < MultiplayerCaptureTheFlagOptionsTimeDigits.Length; i++)
            {
                MultiplayerCaptureTheFlagOptionsTimeDigits[i] = new AnimatedObject(multiplayerCaptureTheFlagAnimations, multiplayerCaptureTheFlagAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(i == 0 ? 125 : (i + 1) * 9 + 125, 140),
                    CurrentAnimation = 18,
                    RenderContext = renderContext,
                };
            }

            MultiplayerPlayerSelection = new AnimatedObject(multiplayerPlayersAnimations, multiplayerPlayersAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                ScreenPos = new Vector2(113, 60 - multiplayerMultiPakPlayersOffsetY),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            MultiplayerPlayerNumberIcons = new AnimatedObject(multiplayerPlayersAnimations, multiplayerPlayersAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(70, 42 - multiplayerMultiPakPlayersOffsetY),
                CurrentAnimation = 4,
                RenderContext = renderContext,
            };

            MultiplayerPlayerSelectionIcons = new AnimatedObject[4];
            for (int i = 0; i < MultiplayerPlayerSelectionIcons.Length; i++)
            {
                MultiplayerPlayerSelectionIcons[i] = new AnimatedObject(multiplayerPlayersAnimations, multiplayerPlayersAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 1,
                    ObjPriority = 16,
                    ScreenPos = new Vector2(72 + 24 * i, 69 - multiplayerMultiPakPlayersOffsetY),
                    CurrentAnimation = 8,
                    RenderContext = renderContext,
                };
            }

            MultiplayerPlayerSelectionHighlight = new AnimatedObject(multiplayerPlayersAnimations, multiplayerPlayersAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(72, 46 - multiplayerMultiPakPlayersOffsetY),
                CurrentAnimation = 10,
                RenderContext = renderContext,
            };

            GameModeList = new AnimatedObject(gameModeAnimations, gameModeAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(58, 62),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            QuitSelection = new AnimatedObject(pauseSelectionAnimations, pauseSelectionAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(58, 69),
                CurrentAnimation = 15,
                RenderContext = renderContext,
            };

            QuitHeader = new AnimatedObject(pauseSelectionAnimations, pauseSelectionAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = new Vector2(50, 20),
                CurrentAnimation = 34,
                RenderContext = renderContext,
            };

            GameLogo = new AnimatedObject(gameLogoAnimations, gameLogoAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(110, 16),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            SoundsBase = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(152, 75),
                CurrentAnimation = 7,
                RenderContext = renderContext,
            };

            MusicVolume = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(152, 75),
                CurrentAnimation = 28,
                RenderContext = renderContext,
            };

            SfxVolume = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(152, 99),
                CurrentAnimation = 28,
                RenderContext = renderContext,
            };

            SlotLumIcons = new AnimatedObject[3];
            SlotCageIcons = new AnimatedObject[3];
            SlotIcons = new AnimatedObject[3];
            SlotLumTexts = new SpriteTextObject[3];
            SlotCageTexts = new SpriteTextObject[3];
            SlotEmptyTexts = new AnimatedObject[3];
            for (int i = 0; i < 3; i++)
            {
                SlotLumIcons[i] = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(74, 65 + i * 18),
                    CurrentAnimation = 13,
                    RenderContext = renderContext,
                };
                SlotCageIcons[i] = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(124, 61 + i * 18),
                    CurrentAnimation = 11,
                    RenderContext = renderContext,
                };
                SlotIcons[i] = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(56, 64 + i * 18),
                    CurrentAnimation = 8 + i,
                    RenderContext = renderContext,
                };
                SlotLumTexts[i] = new SpriteTextObject()
                {
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(96, 65 + i * 18),
                    Text = "1000",
                    FontSize = FontSize.Font16,
                    Color = TextColor.Menu,
                    RenderContext = renderContext,
                };
                SlotCageTexts[i] = new SpriteTextObject()
                {
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(149, 65 + i * 18),
                    Text = "50",
                    FontSize = FontSize.Font16,
                    Color = TextColor.Menu,
                    RenderContext = renderContext,
                };
                SlotEmptyTexts[i] = new AnimatedObject(slotEmptyAnimations, slotEmptyAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(94, 64 + i * 18),
                    CurrentAnimation = 0,
                    RenderContext = renderContext,
                };
            }

            OptionsSelection = new AnimatedObject(optionsAnimations, optionsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(58, 62),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            MultiplayerConnectionSelection = new AnimatedObject(connectionAnimations, connectionAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(58, 62),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            StartEraseSelection = new AnimatedObject(startEraseAnimations, startEraseAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(52, 30),
                CurrentAnimation = 1,
                RenderContext = renderContext,
            };

            StartEraseCursor = new AnimatedObject(startEraseAnimations, startEraseAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(78, 12),
                CurrentAnimation = 40,
                RenderContext = renderContext,
            };

            Texts = new SpriteTextObject[8];
            for (int i = 0; i < Texts.Length; i++)
            {
                Texts[i] = new SpriteTextObject()
                {
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(70, 48 + i * 16),
                    FontSize = FontSize.Font16,
                    Color = TextColor.Menu,
                    RenderContext = renderContext,
                };
            }

            MultiplayerTypeName = new AnimatedObject(multiplayerTypeAnimations, multiplayerTypeAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(110, 124),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            MultiplayerTypeFrame = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(108, 65),
                CurrentAnimation = 2,
                RenderContext = renderContext,
            };

            ArrowLeft = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(68, 80),
                CurrentAnimation = 1,
                RenderContext = renderContext,
            };

            ArrowRight = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(152, 80),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            MultiplayerTypeIcon = new AnimatedObject(multiplayerTypeIconAnimations, multiplayerTypeIconAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(83, 54),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            MultiplayerMapSelection = new AnimatedObject(multiplayerMapAnimations, multiplayerMapAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(88, 102),
                CurrentAnimation = 0,
                RenderContext = renderContext,
            };

            MultiplayerMapName1 = new SpriteTextObject()
            {
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(48, 80),
                FontSize = FontSize.Font16,
                Color = TextColor.Menu,
                RenderContext = renderContext,
            };

            MultiplayerMapName2 = new SpriteTextObject()
            {
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(38, 120),
                FontSize = FontSize.Font16,
                Color = TextColor.Menu,
                RenderContext = renderContext,
            };
        }
        else
        {
            throw new UnsupportedPlatformException();
        }
    }

    public AnimatedObject Wheel1 { get; }
    public AnimatedObject Wheel2 { get; }
    public AnimatedObject Wheel3 { get; }
    public AnimatedObject Wheel4 { get; }
    public AnimatedObject Cursor { get; }
    public AnimatedObject Stem { get; }
    public AnimatedObject Steam { get; }
    public AnimatedObject LanguageList { get; }
    public AnimatedObject GameModeList { get; }
    public AnimatedObject GameLogo { get; }
    public AnimatedObject SoundsBase { get; }
    public AnimatedObject MusicVolume { get; }
    public AnimatedObject SfxVolume { get; }
    public AnimatedObject[] SlotLumIcons { get; }
    public AnimatedObject[] SlotCageIcons { get; }
    public AnimatedObject[] SlotIcons { get; }
    public SpriteTextObject[] SlotLumTexts { get; }
    public SpriteTextObject[] SlotCageTexts { get; }
    public AnimatedObject[] SlotEmptyTexts { get; }
    public AnimatedObject OptionsSelection { get; }
    public AnimatedObject MultiplayerModeSelection { get; }
    public AnimatedObject StartEraseSelection { get; }
    public AnimatedObject StartEraseCursor { get; }
    public SpriteTextObject[] Texts { get; }
    public AnimatedObject MultiplayerPlayerSelection { get; set; }
    public AnimatedObject MultiplayerPlayerNumberIcons { get; set; }
    public AnimatedObject[] MultiplayerPlayerSelectionIcons { get; set; }
    public AnimatedObject MultiplayerPlayerSelectionHighlight { get; set; }
    public AnimatedObject MultiplayerTypeName { get; }
    public AnimatedObject MultiplayerTypeFrame { get; }
    public AnimatedObject ArrowLeft { get; }
    public AnimatedObject ArrowRight { get; }
    public AnimatedObject MultiplayerTypeIcon { get; }
    public AnimatedObject MultiplayerMapSelection { get; }
    public SpriteTextObject MultiplayerMapName1 { get; }
    public SpriteTextObject MultiplayerMapName2 { get; }
    public AnimatedObject MultiplayerSinglePakPlayers { get; }

    // N-Gage exclusive
    public AnimatedObject SelectSymbol { get; }
    public AnimatedObject BackSymbol { get; }
    public AnimatedObject QuitSelection { get; }
    public AnimatedObject QuitHeader { get; }
    public AnimatedObject MultiplayerConnectionSelection { get; }
    public AnimatedObject MultiplayerCaptureTheFlagOptions {  get; }
    public AnimatedObject MultiplayerCaptureTheFlagModeName { get; }
    public AnimatedObject MultiplayerCaptureTheFlagOptionsArrowLeft { get; }
    public AnimatedObject MultiplayerCaptureTheFlagOptionsArrowRight { get; }
    public AnimatedObject MultiplayerCaptureTheFlagOptionsColon { get; }
    public AnimatedObject MultiplayerCaptureTheFlagOptionsFlagsDigit { get; }
    public AnimatedObject[] MultiplayerCaptureTheFlagOptionsTimeDigits { get; }
}