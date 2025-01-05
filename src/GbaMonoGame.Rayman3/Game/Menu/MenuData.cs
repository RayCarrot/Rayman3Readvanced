using BinarySerializer.Ubisoft.GbaEngine;
using GbaMonoGame.AnimEngine;

namespace GbaMonoGame.Rayman3;

// TODO: Add N-Gage support
public class MenuData
{
    public MenuData(int multiplayerMultiPakPlayersOffsetY, int multiplayerSinglePakPlayersOffsetY)
    {
        if (Engine.Settings.Platform == Platform.GBA)
        {
            AnimatedObjectResource propsAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuPropAnimations);
            AnimatedObjectResource startEraseAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuStartEraseAnimations);
            AnimatedObjectResource gameLogoAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuGameLogoAnimations);
            AnimatedObjectResource gameModeAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuGameModeAnimations);
            AnimatedObjectResource languageListAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuLanguageListAnimations);
            AnimatedObjectResource optionsAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuOptionsAnimations);
            AnimatedObjectResource slotEmptyAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuSlotEmptyAnimations);
            AnimatedObjectResource multiplayerModeAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerModeAnimations);
            AnimatedObjectResource multiplayerPlayersAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerPlayersAnimations);
            AnimatedObjectResource multiplayerTypeAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerTypeAnimations);
            AnimatedObjectResource multiplayerTypeFrameAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerTypeFrameAnimations);
            AnimatedObjectResource multiplayerTypeIconAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerTypeIconAnimations);
            AnimatedObjectResource multiplayerMapAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuMultiplayerMapAnimations);
            AnimatedObjectResource steamAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuSteamAnimations);

            Wheel1 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                ScreenPos = new Vector2(7, 110),
                CurrentAnimation = 2,
                AffineMatrix = AffineMatrix.Identity
            };

            Wheel2 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                ScreenPos = new Vector2(136, 110),
                CurrentAnimation = 3,
                AffineMatrix = AffineMatrix.Identity
            };

            Wheel3 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 33,
                ScreenPos = new Vector2(172, 110),
                CurrentAnimation = 4,
                AffineMatrix = AffineMatrix.Identity
            };

            Wheel4 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                ScreenPos = new Vector2(66, 144),
                CurrentAnimation = 3,
                AffineMatrix = AffineMatrix.Identity
            };

            Cursor = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(33, 67),
                CurrentAnimation = 0,
            };

            Stem = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(47, 160),
                CurrentAnimation = 14,
            };

            Steam = new AnimatedObject(steamAnimations, steamAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(27, 20),
                CurrentAnimation = 0
            };

            LanguageList = new AnimatedObject(languageListAnimations, languageListAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = new Vector2(120, 28),
                CurrentAnimation = 0,
            };

            GameModeList = new AnimatedObject(gameModeAnimations, gameModeAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(73, 52),
                CurrentAnimation = 0,
            };

            GameLogo = new AnimatedObject(gameLogoAnimations, gameLogoAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(174, 16),
                CurrentAnimation = 0,
            };

            SoundsBase = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(210, 65),
                CurrentAnimation = 7,
            };

            MusicVolume = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(210, 65),
                CurrentAnimation = 5,
            };

            SfxVolume = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(210, 89),
                CurrentAnimation = 5,
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
                };
                SlotCageIcons[i] = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(165, 51 + i * 18),
                    CurrentAnimation = 11,
                };
                SlotIcons[i] = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(84, 54 + i * 18),
                    CurrentAnimation = 8 + i,
                };
                SlotLumTexts[i] = new SpriteTextObject()
                {
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(129, 55 + i * 18),
                    Text = "1000",
                    FontSize = FontSize.Font16,
                    Color = TextColor.Menu,
                };
                SlotCageTexts[i] = new SpriteTextObject()
                {
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(190, 55 + i * 18),
                    Text = "50",
                    FontSize = FontSize.Font16,
                    Color = TextColor.Menu,
                };
                SlotEmptyTexts[i] = new AnimatedObject(slotEmptyAnimations, slotEmptyAnimations.IsDynamic)
                {
                    IsFramed = true,
                    BgPriority = 3,
                    ObjPriority = 0,
                    ScreenPos = new Vector2(126, 54 + i * 18),
                    CurrentAnimation = 0,
                };
            }

            OptionsSelection = new AnimatedObject(optionsAnimations, optionsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(73, 52),
                CurrentAnimation = 0,
            };

            MultiplayerModeSelection = new AnimatedObject(multiplayerModeAnimations, multiplayerModeAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(73, 52),
                CurrentAnimation = 0,
            };

            StartEraseSelection = new AnimatedObject(startEraseAnimations, startEraseAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(80, 30),
                CurrentAnimation = 1,
            };

            StartEraseCursor = new AnimatedObject(startEraseAnimations, startEraseAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(106, 12),
                CurrentAnimation = 40
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
                };
            }

            MultiplayerPlayerSelection = new AnimatedObject(multiplayerPlayersAnimations, multiplayerPlayersAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                ScreenPos = new Vector2(145, 40 - multiplayerMultiPakPlayersOffsetY),
                CurrentAnimation = 0
            };

            MultiplayerPlayerNumberIcons = new AnimatedObject(multiplayerPlayersAnimations, multiplayerPlayersAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(102, 22 - multiplayerMultiPakPlayersOffsetY),
                CurrentAnimation = 4
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
                    CurrentAnimation = 8
                };
            }

            MultiplayerPlayerSelectionHighlight = new AnimatedObject(multiplayerPlayersAnimations, multiplayerPlayersAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(104, 26 - multiplayerMultiPakPlayersOffsetY),
                CurrentAnimation = 10
            };

            MultiplayerTypeName = new AnimatedObject(multiplayerTypeAnimations, multiplayerTypeAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(142, 94),
                CurrentAnimation = 0
            };

            MultiplayerTypeFrame = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(140, 35),
                CurrentAnimation = 2
            };

            MultiplayerTypeLeftArrow = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(100, 50),
                CurrentAnimation = 1
            };

            MultiplayerTypeRightArrow = new AnimatedObject(multiplayerTypeFrameAnimations, multiplayerTypeFrameAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(184, 50),
                CurrentAnimation = 1
            };

            MultiplayerTypeIcon = new AnimatedObject(multiplayerTypeIconAnimations, multiplayerTypeIconAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(115, 24),
                CurrentAnimation = 0
            };

            MultiplayerMapSelection = new AnimatedObject(multiplayerMapAnimations, multiplayerMapAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(120, 82),
                CurrentAnimation = 0
            };

            MultiplayerMapName1 = new SpriteTextObject()
            {
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(80, 56),
                FontSize = FontSize.Font16,
                Color = TextColor.Menu,
            };

            MultiplayerMapName2 = new SpriteTextObject()
            {
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(70, 96),
                FontSize = FontSize.Font16,
                Color = TextColor.Menu,
            };

            MultiplayerSinglePakPlayers = new AnimatedObject(multiplayerPlayersAnimations, multiplayerPlayersAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(168, 40 - multiplayerSinglePakPlayersOffsetY),
                CurrentAnimation = 11
            };
        }
        else if (Engine.Settings.Platform == Platform.NGage)
        {
            AnimatedObjectResource propsAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuPropAnimations);
            AnimatedObjectResource symbolAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.NGageButtonSymbols);
            AnimatedObjectResource languageListAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuLanguageListAnimations);
            AnimatedObjectResource gameModeAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuGameModeAnimations);
            AnimatedObjectResource pauseSelectionAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.PauseSelectionAnimations);
            AnimatedObjectResource gameLogoAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuGameLogoAnimations);
            AnimatedObjectResource optionsAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.MenuOptionsAnimations);
            AnimatedObjectResource helpArrowAnimations = Storage.LoadResource<AnimatedObjectResource>(GameResource.NGageMenuHelpArrowAnimations);

            Wheel2 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                ScreenPos = new Vector2(124, 145),
                CurrentAnimation = 3,
                AffineMatrix = AffineMatrix.Identity
            };

            Wheel4 = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 32,
                ScreenPos = new Vector2(59, 174),
                CurrentAnimation = 3,
                AffineMatrix = AffineMatrix.Identity
            };

            Cursor = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(18, 77),
                CurrentAnimation = 0,
            };

            Stem = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(32, 170),
                CurrentAnimation = 14,
            };

            SelectSymbol = new AnimatedObject(symbolAnimations, false)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 0,
                CurrentAnimation = Localization.LanguageUiIndex,
                ScreenPos = new Vector2(-1, 190),
            };

            BackSymbol = new AnimatedObject(symbolAnimations, false)
            {
                IsFramed = true,
                CurrentAnimation = 5 + Localization.LanguageUiIndex,
                ScreenPos = new Vector2(-1, 190),
                HorizontalAnchor = HorizontalAnchorMode.Right,
            };

            LanguageList = new AnimatedObject(languageListAnimations, languageListAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(108, 100),
                CurrentAnimation = 13,
            };

            // TODO: field_0x568
            // TODO: field_0x590
            // TODO: field_0x5b8
            // TODO: field_0x5e0
            // TODO: field_0x608
            // TODO: field_0x630
            // TODO: field_0x658
            // TODO: multiplayerPlayerSelection
            // TODO: multiplayerPlayerNumberIcons
            // TODO: multiplayerPlayerSelectionIcons
            // TODO: multiplayerPlayerSelectionHighlight

            GameModeList = new AnimatedObject(gameModeAnimations, gameModeAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(58, 62),
                CurrentAnimation = 0,
            };

            QuitSelection = new AnimatedObject(pauseSelectionAnimations, pauseSelectionAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(58, 69),
                CurrentAnimation = 15,
            };

            QuitHeader = new AnimatedObject(pauseSelectionAnimations, pauseSelectionAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 0,
                ObjPriority = 0,
                ScreenPos = new Vector2(50, 20),
                CurrentAnimation = 34,
            };

            GameLogo = new AnimatedObject(gameLogoAnimations, gameLogoAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(110, 16),
                CurrentAnimation = 0,
            };

            SoundsBase = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(152, 75),
                CurrentAnimation = 7,
            };

            MusicVolume = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(152, 75),
                CurrentAnimation = 28,
            };

            SfxVolume = new AnimatedObject(propsAnimations, propsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(152, 99),
                CurrentAnimation = 28,
            };

            // TODO: field_0x2e0
            // TODO: field_0x3ac
            // TODO: field_0x268
            // TODO: field_0x290
            // TODO: field_0x2b8
            // TODO: slotLumTexts
            // TODO: slotCageTexts
            // TODO: slotEmptyTexts

            OptionsSelection = new AnimatedObject(optionsAnimations, optionsAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 3,
                ObjPriority = 0,
                ScreenPos = new Vector2(58, 62),
                CurrentAnimation = 0,
            };

            // TODO: multiplayerConnectionSelection
            // TODO: startEraseSelection
            // TODO: startEraseCursor

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
                };
            }

            // TODO: field_0x8c8
            // TODO: field_0x8f0

            HelpArrowLeft = new AnimatedObject(helpArrowAnimations, helpArrowAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(68, 80),
                CurrentAnimation = 1,
            };

            HelpArrowRight = new AnimatedObject(helpArrowAnimations, helpArrowAnimations.IsDynamic)
            {
                IsFramed = true,
                BgPriority = 1,
                ObjPriority = 0,
                ScreenPos = new Vector2(152, 80),
                CurrentAnimation = 0,
            };

            // TODO: field_0x970
            // TODO: multiplayerMapSelection
            // TODO: multiplayerMapName1
            // TODO: multiplayerMapName2
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
    public AnimatedObject MultiplayerTypeName { get; set; }
    public AnimatedObject MultiplayerTypeFrame { get; set; }
    public AnimatedObject MultiplayerTypeLeftArrow { get; set; }
    public AnimatedObject MultiplayerTypeRightArrow { get; set; }
    public AnimatedObject MultiplayerTypeIcon { get; set; }
    public AnimatedObject MultiplayerMapSelection { get; set; }
    public SpriteTextObject MultiplayerMapName1 { get; set; }
    public SpriteTextObject MultiplayerMapName2 { get; set; }
    public AnimatedObject MultiplayerSinglePakPlayers { get; }

    // N-Gage exclusive
    public AnimatedObject SelectSymbol { get; set; }
    public AnimatedObject BackSymbol { get; set; }
    public AnimatedObject QuitSelection { get; set; }
    public AnimatedObject QuitHeader { get; set; }
    public AnimatedObject HelpArrowLeft { get; set; }
    public AnimatedObject HelpArrowRight { get; set; }
}