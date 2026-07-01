using System;
using System.IO;
using BinarySerializer.Gameloft.J2me;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.J2me;

public partial class Game
{
    public const int VOL_HIGH = 100;
    public const int VOL_MEDIUM = 70;
    public const int VOL_LOW = 40;
    public const int VOL_OFF = 0;

    public const int MAX_MENU_ITEMS = 8;
    public const int MENU_ITEM_HEIGHT = 21;

    public const int MENU_MAIN_OPTION_ID_NEW_GAME = 0;
    public const int MENU_MAIN_OPTION_ID_CONTINUE = 1;
    public const int MENU_MAIN_OPTION_ID_MUSIC = 2;
    public const int MENU_MAIN_OPTION_ID_ABOUT = 3;
    public const int MENU_MAIN_OPTION_ID_HELP = 4;
    public const int MENU_MAIN_OPTION_ID_EXIT = 5;

    public const int MENU_PAUSE_OPTION_ID_RESUME = 0;
    public const int MENU_PAUSE_OPTION_ID_RESTART = 1;
    public const int MENU_PAUSE_OPTION_ID_MUSIC = 2;
    public const int MENU_PAUSE_OPTION_ID_MAIN_MENU = 3;
    public const int MENU_PAUSE_OPTION_ID_EXIT = 4;
    public const int MENU_PAUSE_OPTION_ID_RESOLUTION = 5; // Custom
    public const int MENU_PAUSE_OPTION_ID_FIX_BUGS = 6; // Custom

    // Custom
    public const int MENU_CHEAT_OPTION_ID_RESUME = 0;
    public const int MENU_CHEAT_OPTION_ID_COMPLETE_LEVEL = 1;
    public const int MENU_CHEAT_OPTION_ID_RESTORE_HEALTH = 2;
    public const int MENU_CHEAT_OPTION_ID_99_LIVES = 3;
    public const int MENU_CHEAT_OPTION_ID_SHOW_PHYSICAL_COLLISION = 4;
    public const int MENU_CHEAT_OPTION_ID_SHOW_ACTOR_BOXES = 5;

    public int SoundVolume { get; set; } = VOL_MEDIUM;
    public sbyte m_byMenuRArrowDirection { get; set; } = 1;
    public sbyte m_byMenuLArrowDirection { get; set; } = 1;
    public sbyte m_byMenuVArrowDirection { get; set; } = 1;
    public int m_iAboutTicker { get; set; }
    public string[] m_gameMenu_items_pStr { get; } = new string[MAX_MENU_ITEMS];
    public short[] m_gameMenu_items_id { get; } = new short[MAX_MENU_ITEMS];
    public AnimData m_gameMenu_pData { get; set; }
    public Anim[] m_gameMenu_pAnims { get; set; }
    public MENU_PAGE m_gameMenu_idCurPage { get; set; }
    public int m_gameMenu_idCurSel { get; set; }
    public int m_gameMenu_nItem { get; set; }
    public sbyte m_byHelpLength { get; set; }
    public bool m_bClearBackMenu { get; set; }

    public int Menu_GetStringWidth(string pStr)
    {
        return m_fontGeneral.stringWidth(pStr);
    }

    public void Menu_DrawString(string pStr, float x, float y, int nColor)
    {
        g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);

        // Draw shadow
        g_graBackBuffer.setColor(0x731810);
        g_graBackBuffer.drawString(pStr, x + 1, y, ANCHOR.TOP | ANCHOR.LEFT);
        g_graBackBuffer.drawString(pStr, x, y + 1, ANCHOR.TOP | ANCHOR.LEFT);
        g_graBackBuffer.drawString(pStr, x - 1, y, ANCHOR.TOP | ANCHOR.LEFT);
        g_graBackBuffer.drawString(pStr, x, y - 1, ANCHOR.TOP | ANCHOR.LEFT);
        
        // Draw text
        g_graBackBuffer.setColor(nColor != 0 ? nColor : 0xF8E109);
        g_graBackBuffer.drawString(pStr, x, y, ANCHOR.TOP | ANCHOR.LEFT);
    }

    public sbyte Menu_GetRArrowPos()
    {
        sbyte b;
        if (m_byMenuRArrowDirection > 0)
        {
            b = (sbyte)(m_iGlobalTicker & 0x3);
            if (b == 3)
                m_byMenuRArrowDirection *= -1;
        }
        else
        {
            b = (sbyte)(3 - (m_iGlobalTicker & 0x3));
            if (b == 0)
                m_byMenuRArrowDirection *= -1;
        }
        return b;
    }

    public sbyte Menu_GetLArrowPos()
    {
        sbyte b;
        if (m_byMenuLArrowDirection > 0)
        {
            b = (sbyte)(m_iGlobalTicker & 0x3);
            if (b == 3)
                m_byMenuLArrowDirection *= -1;
        }
        else
        {
            b = (sbyte)(3 - (m_iGlobalTicker & 0x3));
            if (b == 0)
                m_byMenuLArrowDirection *= -1;
        }
        return b;
    }

    public sbyte Menu_GetVArrowPos()
    {
        sbyte b;
        if (m_byMenuVArrowDirection > 0)
        {
            b = (sbyte)(m_iGlobalTicker & 0x3);
            if (b == 3)
                m_byMenuVArrowDirection *= -1;
        }
        else
        {
            b = (sbyte)(3 - (m_iGlobalTicker & 0x3));
            if (b == 0)
                m_byMenuVArrowDirection *= -1;
        }
        return b;
    }

    public void Menu_DrawItem(string pStr, float y, float id)
    {
        if (id == m_gameMenu_idCurSel)
        {
            int w = Menu_GetStringWidth(pStr);
            float x = (Resolution.X - w) / 2;
            Menu_DrawString(pStr, x, y - 5 + 2, 0);
            
            // ReSharper disable once RedundantAssignment
            bool bBackgroundState = m_bBackgroundUsed;
            // Bug in the original game - setting the wrong variable, forcing it to be false after this rendered frame. This however
            // makes the first frame render the arrows with background scrolling, making the positions wrong. This only appears in
            // some versions however, but we optionally fix it here.
            bBackgroundState = false;
            if (Engine.Settings.Local.J2me.FixBugs)
                m_bBackgroundUsed = false;

            Actor.drawModule(m_gameMenu_pData, 13, x - Actor.aniData[(sbyte)ACTOR_TYPE.FONT].modules[13].Width - 4 - Menu_GetRArrowPos(), y, 0, g_graBackBuffer);
            Actor.drawModule(m_gameMenu_pData, 14, x + w + 4 + Menu_GetLArrowPos(), y, 0, g_graBackBuffer);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            m_bBackgroundUsed = bBackgroundState;
        }
        else
        {
            int w = Menu_GetStringWidth(pStr);
            float x = (Resolution.X - w) / 2;
            Menu_DrawString(pStr, x, y - 5 + 2, 0);
        }
    }

    public void Menu_DrawPage()
    {
        string strTitle;
        int iColor = 0;
        switch (m_gameMenu_idCurPage)
        {
            case MENU_PAGE.ABOUT:
                if (m_gameStateStep == 0)
                {
                    Menu_DrawString(RM.GetString(STRING_ID_LOADING), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_LOADING))) / 2, 263, 0);
                }
                else if (m_gameStateStep == 1)
                {
                    Menu_LoadCredits();
                }
                else
                {
                    strTitle = RM.GetString(STRING_ID_ABOUT);
                    g_graBackBuffer.ClearScreen(0x5209E3);
                    Menu_DrawString(strTitle, (Resolution.X - Menu_GetStringWidth(strTitle)) / 2, 6, iColor);
                    Menu_DrawPageText();
                }
                m_gameStateStep++;
                return;

            case MENU_PAGE.HELP:
                if (m_iGlobalTicker is >= 0 and < 3)
                {
                    Menu_DrawString(RM.GetString(STRING_ID_LOADING), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_LOADING))) / 2, 263, 0);
                }
                else if (m_iGlobalTicker == 3)
                {
                    Menu_LoadHelp();
                }
                else if (m_iGlobalTicker > 3)
                {
                    iColor = 0xFFFFFF;
                    strTitle = RM.GetString(STRING_ID_HELP_TITLE);
                    g_graBackBuffer.ClearScreen(0x5209E3);
                    Menu_DrawString(strTitle, (Resolution.X - Menu_GetStringWidth(strTitle)) / 2, 6, iColor);
                    Menu_DrawPageText();
                }
                return;

            case MENU_PAGE.MAIN:
                Menu_DrawItem(m_gameMenu_items_pStr[m_gameMenu_idCurSel], 268, m_gameMenu_idCurSel);
                return;
        }

        float iStartMenuY = (Resolution.Y - m_gameMenu_nItem * MENU_ITEM_HEIGHT) / 2;
        if (iStartMenuY < 0)
            iStartMenuY = 0;
        for (int i = 0; i < m_gameMenu_nItem; i++)
            Menu_DrawItem(m_gameMenu_items_pStr[i], iStartMenuY + i * MENU_ITEM_HEIGHT, i);
    }

    public void Menu_InsertOption(string pStr, int iID)
    {
        m_gameMenu_items_id[m_gameMenu_nItem] = (short)iID;
        m_gameMenu_items_pStr[m_gameMenu_nItem] = pStr;
        m_gameMenu_nItem++;
    }

    public void Menu_UpdatePage()
    {
        m_gameMenu_nItem = 0;
        switch (m_gameMenu_idCurPage)
        {
            case MENU_PAGE.MAIN:
                if (GameFrame_IsSavedFileExists())
                    Menu_InsertOption(RM.GetString(STRING_ID_CONTINUE), MENU_MAIN_OPTION_ID_CONTINUE);
                Menu_InsertOption(RM.GetString(STRING_ID_NEW_GAME), MENU_MAIN_OPTION_ID_NEW_GAME);
                
                switch (SoundVolume)
                {
                    case VOL_HIGH:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_HIGH), MENU_MAIN_OPTION_ID_MUSIC);
                        break;
                    
                    case VOL_MEDIUM:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_MEDIUM), MENU_MAIN_OPTION_ID_MUSIC);
                        break;
                    
                    case VOL_LOW:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_LOW), MENU_MAIN_OPTION_ID_MUSIC);
                        break;
                    
                    case VOL_OFF:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_OFF), MENU_MAIN_OPTION_ID_MUSIC);
                        break;
                }

                Menu_InsertOption(RM.GetString(STRING_ID_ABOUT), MENU_MAIN_OPTION_ID_ABOUT);
                Menu_InsertOption(RM.GetString(STRING_ID_HELP), MENU_MAIN_OPTION_ID_HELP);
                Menu_InsertOption(RM.GetString(STRING_ID_EXIT), MENU_MAIN_OPTION_ID_EXIT);
                break;

            case MENU_PAGE.PAUSE:
                Menu_InsertOption(RM.GetString(STRING_ID_RESUME), MENU_PAUSE_OPTION_ID_RESUME);
                Menu_InsertOption(RM.GetString(STRING_ID_RESTART), MENU_PAUSE_OPTION_ID_RESTART);

                // Custom for changing the resolution
                if (Engine.ViewPort.InternalGameResolution == J2meRom.OriginalResolution)
                    Menu_InsertOption($"Resolution : Original ({Resolution.X}x{Resolution.Y})", MENU_PAUSE_OPTION_ID_RESOLUTION);
                else if (Engine.ViewPort.InternalGameResolution == GbaMonoGame.Resolution.J2meModern)
                    Menu_InsertOption($"Resolution : Widescreen ({Resolution.X}x{Resolution.Y})", MENU_PAUSE_OPTION_ID_RESOLUTION);
                else
                    Menu_InsertOption($"Resolution : {Resolution.X}x{Resolution.Y}", MENU_PAUSE_OPTION_ID_RESOLUTION);

                // Custom for bug fixes option
                if (Engine.Settings.Local.J2me.FixBugs)
                    Menu_InsertOption("Fix bugs : On", MENU_PAUSE_OPTION_ID_FIX_BUGS);
                else
                    Menu_InsertOption("Fix bugs : Off", MENU_PAUSE_OPTION_ID_FIX_BUGS);
                
                switch (SoundVolume)
                {
                    case VOL_HIGH:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_HIGH), MENU_PAUSE_OPTION_ID_MUSIC);
                        break;

                    case VOL_MEDIUM:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_MEDIUM), MENU_PAUSE_OPTION_ID_MUSIC);
                        break;

                    case VOL_LOW:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_LOW), MENU_PAUSE_OPTION_ID_MUSIC);
                        break;

                    case VOL_OFF:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_OFF), MENU_PAUSE_OPTION_ID_MUSIC);
                        break;
                }

                Menu_InsertOption(RM.GetString(STRING_ID_MAIN_MENU), MENU_PAUSE_OPTION_ID_MAIN_MENU);
                Menu_InsertOption(RM.GetString(STRING_ID_EXIT), MENU_PAUSE_OPTION_ID_EXIT);
                break;

            // Custom
            case MENU_PAGE.CHEAT:
                Menu_InsertOption(RM.GetString(STRING_ID_RESUME), MENU_CHEAT_OPTION_ID_RESUME);
                if (m_gameFrame_curLevel > LEVEL_WORLD_MAP)
                    Menu_InsertOption("Complete level", MENU_CHEAT_OPTION_ID_COMPLETE_LEVEL);
                Menu_InsertOption("Restore health", MENU_CHEAT_OPTION_ID_RESTORE_HEALTH);
                Menu_InsertOption("99 lives", MENU_CHEAT_OPTION_ID_99_LIVES);
                if (ShowPhysicalCollision)
                    Menu_InsertOption("Show physical collision : On", MENU_CHEAT_OPTION_ID_SHOW_PHYSICAL_COLLISION);
                else
                    Menu_InsertOption("Show physical collision : Off", MENU_CHEAT_OPTION_ID_SHOW_PHYSICAL_COLLISION);
                if (ShowActorBoxes)
                    Menu_InsertOption("Show actor boxes : On", MENU_CHEAT_OPTION_ID_SHOW_ACTOR_BOXES);
                else
                    Menu_InsertOption("Show actor boxes : Off", MENU_CHEAT_OPTION_ID_SHOW_ACTOR_BOXES);
                break;
        }
    }

    public void Menu_LoadMain()
    {
        RM.Free(RESOURCE_ID_IMG_GAMELOFT_LOGO);
        RM.LoadImage(RESOURCE_ID_IMG_SPLASH_SCREEN);
        RM.LoadImage(RESOURCE_ID_IMG_FONT);
        RM.LoadData<AnimationDataResource>(RESOURCE_ID_DATA_ANIM_FONT);
        RM.Synchronize();
        Actor.AniLoad(RM.ResourceID_To_Index(RESOURCE_ID_DATA_ANIM_FONT), RM.ResourceID_To_Index(RESOURCE_ID_IMG_FONT));
    }

    public void Menu_Free(bool bFreeMainMenu)
    {
        if (m_gameMenu_pAnims != null)
        {
            for (int i = 0; i < m_gameMenu_pAnims.Length; i++)
                m_gameMenu_pAnims[i] = null;
            m_gameMenu_pAnims = null;
        }

        RM.Free(RESOURCE_ID_IMG_LUMS);
        RM.Free(RESOURCE_ID_IMG_LEVEL_POST); // Never loaded though...?
        RM.Free(RESOURCE_ID_IMG_CAGE);
        RM.Free(RESOURCE_ID_DATA_ANIM_YELLOW_LUM);
        RM.Free(RESOURCE_ID_DATA_ANIM_WHITE_LUM);
        RM.Free(RESOURCE_ID_DATA_ANIM_RED_LUM);
        RM.Free(RESOURCE_ID_DATA_ANIM_BLUE_LUM);
        RM.Free(RESOURCE_ID_DATA_ANIM_LEVEL_POST); // Never loaded though...?
        RM.Free(RESOURCE_ID_DATA_ANIM_CAGE);
        RM.Free(RESOURCE_ID_DATA_TEXTBANK_CREDITS);
        RM.Free(RESOURCE_ID_DATA_TEXTBANK_HELP);

        if (bFreeMainMenu)
        {
            RM.Free(RESOURCE_ID_IMG_SPLASH_SCREEN);
            RM.Free(RESOURCE_ID_IMG_FONT);
            RM.Free(RESOURCE_ID_DATA_ANIM_FONT);
        }
        
        RM.Synchronize();
    }

    public void Menu_LoadCredits()
    {
        Menu_Free(false);
        RM.LoadData<BinarySerializer.Gameloft.J2me.TextBankResource>(RESOURCE_ID_DATA_TEXTBANK_CREDITS);
        RM.Synchronize();
    }

    public void Menu_LoadHelp()
    {
        Menu_Free(false);
        RM.LoadData<BinarySerializer.Gameloft.J2me.TextBankResource>(RESOURCE_ID_DATA_TEXTBANK_HELP);
        RM.LoadImage(RESOURCE_ID_IMG_LUMS);
        RM.LoadImage(RESOURCE_ID_IMG_CAGE);
        RM.LoadData<AnimationDataResource>(RESOURCE_ID_DATA_ANIM_YELLOW_LUM);
        RM.LoadData<AnimationDataResource>(RESOURCE_ID_DATA_ANIM_WHITE_LUM);
        RM.LoadData<AnimationDataResource>(RESOURCE_ID_DATA_ANIM_RED_LUM);
        RM.LoadData<AnimationDataResource>(RESOURCE_ID_DATA_ANIM_GREEN_LUM);
        RM.LoadData<AnimationDataResource>(RESOURCE_ID_DATA_ANIM_BLUE_LUM);
        RM.LoadData<AnimationDataResource>(RESOURCE_ID_DATA_ANIM_CAGE);
        RM.LoadData<AnimationDataResource>(RESOURCE_ID_DATA_ANIM_SWING);
        RM.Synchronize();

        // NOTE: Blue lum animations are loaded twice for some reason?
        Actor.AniLoad(RM.ResourceID_To_Index(RESOURCE_ID_DATA_ANIM_RED_LUM),    RM.ResourceID_To_Index(RESOURCE_ID_IMG_LUMS));
        Actor.AniLoad(RM.ResourceID_To_Index(RESOURCE_ID_DATA_ANIM_YELLOW_LUM), RM.ResourceID_To_Index(RESOURCE_ID_IMG_LUMS));
        Actor.AniLoad(RM.ResourceID_To_Index(RESOURCE_ID_DATA_ANIM_WHITE_LUM),  RM.ResourceID_To_Index(RESOURCE_ID_IMG_LUMS));
        Actor.AniLoad(RM.ResourceID_To_Index(RESOURCE_ID_DATA_ANIM_GREEN_LUM),  RM.ResourceID_To_Index(RESOURCE_ID_IMG_LUMS));
        Actor.AniLoad(RM.ResourceID_To_Index(RESOURCE_ID_DATA_ANIM_BLUE_LUM),   RM.ResourceID_To_Index(RESOURCE_ID_IMG_LUMS));
        Actor.AniLoad(RM.ResourceID_To_Index(RESOURCE_ID_DATA_ANIM_SWING),      RM.ResourceID_To_Index(RESOURCE_ID_IMG_LUMS));
        Actor.AniLoad(RM.ResourceID_To_Index(RESOURCE_ID_DATA_ANIM_CAGE),       RM.ResourceID_To_Index(RESOURCE_ID_IMG_CAGE));
        Actor.AniLoad(RM.ResourceID_To_Index(RESOURCE_ID_DATA_ANIM_BLUE_LUM),   RM.ResourceID_To_Index(RESOURCE_ID_IMG_LUMS));

        m_gameMenu_pAnims = new Anim[7];
        for (int i = 0; i < m_gameMenu_pAnims.Length; i++)
            m_gameMenu_pAnims[i] = new Anim();
        m_gameMenu_pAnims[0].build(ACTOR_TYPE.YELLOW_LUM, 0);
        m_gameMenu_pAnims[1].build(ACTOR_TYPE.RED_LUM, 0);
        m_gameMenu_pAnims[2].build(ACTOR_TYPE.GREEN_LUM, 0);
        m_gameMenu_pAnims[3].build(ACTOR_TYPE.WHITE_LUM, 0);
        m_gameMenu_pAnims[6].build(ACTOR_TYPE.BLUE_LUM, 0);
        m_gameMenu_pAnims[4].build(ACTOR_TYPE.CAGE, 0);
        m_gameMenu_pAnims[5].build(ACTOR_TYPE.SWING, 0);
        m_gameMenu_pAnims[6].build(ACTOR_TYPE.BLUE_LUM, 0);

        m_byHelpLength = 0;
        StringId iID = new(0, TEXTBANK_INDEX_HELP);
        while (!iID.IsNull)
        {
            m_byHelpLength = (sbyte)(m_byHelpLength + 1);
            iID = RM.NextStringID(iID);
        }
    }

    public void Menu_SetCurrentPage(MENU_PAGE id)
    {
        m_gameMenu_idCurPage = id;
        m_gameMenu_idCurSel = 0;
        Menu_UpdatePage();
    }

    public void Menu_DoAI()
    {
        bool bStillPlay = false;
        if (!bConfirmExit)
            return;
        if (m_byMainLoadingState < 3)
            return;

        if (Menu_PressedUp())
        {
            PlaySound(SOUND_INDEX.menu_move, true);
            if (m_gameMenu_idCurPage == MENU_PAGE.HELP)
                m_gameMenu_idCurSel--;
            else if (--m_gameMenu_idCurSel < 0)
                m_gameMenu_idCurSel = m_gameMenu_nItem - 1;
        }
        else if (Menu_PressedDown())
        {
            PlaySound(SOUND_INDEX.menu_move, true);
            if (m_gameMenu_idCurPage == MENU_PAGE.HELP)
                m_gameMenu_idCurSel++;
            else if (++m_gameMenu_idCurSel >= m_gameMenu_nItem)
                m_gameMenu_idCurSel = 0;
        }
        else if (Menu_PressedConfirm())
        {
            switch (m_gameMenu_idCurPage)
            {
                case MENU_PAGE.MAIN:
                    switch (m_gameMenu_items_id[m_gameMenu_idCurSel])
                    {
                        case MENU_MAIN_OPTION_ID_NEW_GAME:
                            StopSound();
                            m_gameStateStep = 0;
                            m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                            m_gameFrame_curState = GAME_FRAME_STATE.NEW_GAME;
                            GameFrame_InitNewGame();
                            break;

                        case MENU_MAIN_OPTION_ID_CONTINUE:
                            m_gameStateStep = 0;
                            m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                            m_gameFrame_curState = GAME_FRAME_STATE.LOAD_GAME;
                            break;
                    
                        case MENU_MAIN_OPTION_ID_MUSIC:
                            SoundVolume = SoundVolume switch
                            {
                                VOL_HIGH => VOL_MEDIUM,
                                VOL_MEDIUM => VOL_LOW,
                                VOL_LOW => VOL_OFF,
                                VOL_OFF => VOL_HIGH,
                                _ => SoundVolume
                            };
                            Menu_UpdatePage();
                            if (SoundVolume == VOL_OFF)
                                StopSound();
                            else
                                setSoundVolume(SoundVolume);
                            GameFrame_SaveSound();
                            break;

                        case MENU_MAIN_OPTION_ID_ABOUT:
                            m_iAboutTicker = 0;
                            m_gameStateStep = 0;
                            Menu_SetCurrentPage(MENU_PAGE.ABOUT);
                            break;

                        case MENU_MAIN_OPTION_ID_HELP:
                            m_iGlobalTicker = 0;
                            Menu_SetCurrentPage(MENU_PAGE.HELP);
                            break;
                    
                        case MENU_MAIN_OPTION_ID_EXIT:
                            StopSound();
                            GameFrame_PostMessage(MESSAGE_ID.EXIT, 0);
                            m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                            bConfirm = false;
                            break;
                    }
                    break;

                case MENU_PAGE.PAUSE:
                    StopSound();
                    switch (m_gameMenu_items_id[m_gameMenu_idCurSel])
                    {
                        case MENU_PAUSE_OPTION_ID_RESUME:
                            m_bBackgroundUsed = true;
                            m_gameFrame_paused = false;
                            if (pRayman != null && (GAME_KEY)pRayman.V[3] == GAME_KEY.ACTION)
                            {
                                releasedKey = GAME_KEY.ACTION;
                                GameCore();
                            }
                            if (m_gameFrame_curLevel == LEVEL_WORLD_MAP)
                            {
                                PlaySound(SOUND_INDEX.music_map, true, LOOP_INFINITE);
                                bStillPlay = true;
                            }
                            break;

                        case MENU_PAUSE_OPTION_ID_RESTART:
                            m_gameStateStep = 0;
                            m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                            m_gameFrame_curState = GAME_FRAME_STATE.CONFIRM_RESTART;
                            bConfirmExit = false;
                            break;
                    
                        case MENU_PAUSE_OPTION_ID_MUSIC:
                            SoundVolume = SoundVolume switch
                            {
                                VOL_HIGH => VOL_MEDIUM,
                                VOL_MEDIUM => VOL_LOW,
                                VOL_LOW => VOL_OFF,
                                VOL_OFF => VOL_HIGH,
                                _ => SoundVolume
                            };
                            Menu_UpdatePage();
                            if (m_gameFrame_curLevel == 0 && m_SoundPlayer == null) // m_SoundPlayer is never null though...
                                PlaySound(SOUND_INDEX.music_map, true, LOOP_INFINITE);
                            if (SoundVolume == VOL_OFF)
                                StopSound();
                            setSoundVolume(SoundVolume);
                            GameFrame_SaveSound();
                            break;

                        case MENU_PAUSE_OPTION_ID_MAIN_MENU:
                            m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                            bConfirmExit = false;
                            bConfirmToMainMenu = true;
                            break;
                    
                        case MENU_PAUSE_OPTION_ID_EXIT:
                            StopSound();
                            GameFrame_PostMessage(MESSAGE_ID.EXIT, 0);
                            m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                            bConfirm = false;
                            break;

                        // Custom for changing the resolution
                        case MENU_PAUSE_OPTION_ID_RESOLUTION:
                            Vector2 res = Resolution == GbaMonoGame.Resolution.J2meModern ? J2meRom.OriginalResolution : GbaMonoGame.Resolution.J2meModern;
                            Engine.Settings.Local.J2me.InternalGameResolution = res;
                            Engine.ViewPort.SetInternalGameResolution(res);
                            Menu_UpdatePage();
                            Camera_Tick();
                            break;

                        // Custom for bug fixes option
                        case MENU_PAUSE_OPTION_ID_FIX_BUGS:
                            Engine.Settings.Local.J2me.FixBugs = !Engine.Settings.Local.J2me.FixBugs;
                            Menu_UpdatePage();
                            break;
                    }

                    switch (m_gameMenu_items_id[m_gameMenu_idCurSel])
                    {
                        case MENU_PAUSE_OPTION_ID_RESUME:
                            m_keys = GbaInput.None;
                            break;

                        case MENU_PAUSE_OPTION_ID_RESTART:
                        case MENU_PAUSE_OPTION_ID_MAIN_MENU:
                            m_keys = GbaInput.None;
                            m_bClearBackMenu = true;
                            break;
                    }
                    break;
                
                case MENU_PAGE.HELP:
                case MENU_PAGE.ABOUT:
                    Menu_SetCurrentPage(MENU_PAGE.MAIN);
                    break;

                // Custom
                case MENU_PAGE.CHEAT:
                    StopSound();
                    switch (m_gameMenu_items_id[m_gameMenu_idCurSel])
                    {
                        case MENU_CHEAT_OPTION_ID_RESUME:
                            m_bBackgroundUsed = true;
                            m_gameFrame_paused = false;
                            if (pRayman != null && (GAME_KEY)pRayman.V[3] == GAME_KEY.ACTION)
                            {
                                releasedKey = GAME_KEY.ACTION;
                                GameCore();
                            }
                            if (m_gameFrame_curLevel == LEVEL_WORLD_MAP)
                            {
                                PlaySound(SOUND_INDEX.music_map, true, LOOP_INFINITE);
                                bStillPlay = true;
                            }

                            m_keys = GbaInput.None;
                            break;

                        case MENU_CHEAT_OPTION_ID_COMPLETE_LEVEL:
                            s_iLeftToDie = 0;
                            goto case MENU_CHEAT_OPTION_ID_RESUME;

                        case MENU_CHEAT_OPTION_ID_RESTORE_HEALTH:
                            m_gameFrame_nEnergy = 5;
                            Status_Show(0);
                            goto case MENU_CHEAT_OPTION_ID_RESUME;

                        case MENU_CHEAT_OPTION_ID_99_LIVES:
                            GameMidlet.Instance_Game.m_gameFrame_nLife = 99;
                            Status_Show(0);
                            goto case MENU_CHEAT_OPTION_ID_RESUME;

                        case MENU_CHEAT_OPTION_ID_SHOW_PHYSICAL_COLLISION:
                            ShowPhysicalCollision = !ShowPhysicalCollision;
                            Menu_UpdatePage();
                            goto case MENU_CHEAT_OPTION_ID_RESUME;

                        case MENU_CHEAT_OPTION_ID_SHOW_ACTOR_BOXES:
                            ShowActorBoxes = !ShowActorBoxes;
                            Menu_UpdatePage();
                            goto case MENU_CHEAT_OPTION_ID_RESUME;
                    }
                    break;
            }

            if (!bStillPlay)
                PlaySound(SOUND_INDEX.menu_select, true);
        }
    }

    public void Menu_Draw()
    {
        g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
        if (m_gameMenu_idCurPage is not (MENU_PAGE.PAUSE or MENU_PAGE.CHEAT))
        {
            g_graBackBuffer.drawImage(RM.GetImage(RESOURCE_ID_IMG_SPLASH_SCREEN), Resolution.X / 2, Resolution.Y / 2, ANCHOR.HCENTER | ANCHOR.VCENTER);
            if (m_gameMenu_pAnims != null)
            {
                for (int i = 0; i < m_gameMenu_pAnims.Length; i++)
                {
                    if (m_gameMenu_pAnims[i] != null)
                        m_gameMenu_pAnims[i].step(false);
                }
            }
        }
        Menu_DrawPage();
    }

    public void Menu_DrawPageText()
    {
        string[] strArray = new string[13];
        int iBoxY = 34;
        if (iBoxY - 2 < 21)
            iBoxY = 23;
        DrawParchment(10, iBoxY - 2, 220, 272, false);
        g_graBackBuffer.setClip(10, iBoxY, 220, 268);
        g_graBackBuffer.setColor(0x330099);

        if (m_gameMenu_idCurPage == MENU_PAGE.ABOUT)
        {
            const int total_credits_lines = 91;
            m_iAboutTicker += 2;
            int iStart = m_iAboutTicker / 21 % total_credits_lines;
            StringId iID = new(0, TEXTBANK_INDEX_CREDITS);
            for (int iLoop = 0; iLoop < iStart; iLoop++)
            {
                iID = RM.NextStringID(iID);
                if (iID.IsNull)
                    break;
            }

            if (iID.IsNull)
                return;

            for (int i = 0; i < 13; i++)
            {
                if (iID.IsNull)
                    break;
                strArray[i] = RM.GetString(iID);
                StringId supportmail_iID = STRING_ID_SUPPORT_MAIL;
                if (iID == supportmail_iID)
                    strArray[i] = "support@gameloft.com";
                StringId version_iID = STRING_ID_VERSION_NUMBER;
                if (iID == version_iID)
                    strArray[i] = "Version " + ReadVersionFromManifest();
                iID = RM.NextStringID(iID);
            }

            for (int iIndex = 0; iIndex < 13; iIndex++)
            {
                iStart = -(m_iAboutTicker % 21);
                if (strArray[iIndex] != null)
                {
                    g_graBackBuffer.drawString(strArray[iIndex], (Resolution.X - m_fontGeneral.stringWidth(strArray[iIndex])) / 2, iBoxY + iStart + iIndex * 21, ANCHOR.LEFT | ANCHOR.TOP);
                    strArray[iIndex] = null;
                }
            }
        }
        else if (m_gameMenu_idCurPage == MENU_PAGE.HELP)
        {
            iBoxY += 12;
            bool bLast = false;
            if (m_gameMenu_idCurSel < 0)
            {
                m_gameMenu_idCurSel = 0;
            }
            else if (m_gameMenu_idCurSel >= (m_byHelpLength - 1) / 12)
            {
                m_gameMenu_idCurSel = (m_byHelpLength - 1) / 12;
                bLast = true;
            }
            int iStart = m_gameMenu_idCurSel * 12;
            StringId iID = new(0, TEXTBANK_INDEX_HELP);
            for (int iLoop = 0; iLoop < iStart; iLoop++)
            {
                iID = RM.NextStringID(iID);
                if (iID.IsNull)
                    break;
            }

            if (iID.IsNull)
                return;

            for (int i = 0; i < 12; i++)
            {
                if (iID.IsNull)
                {
                    bLast = true;
                    break;
                }
                strArray[i] = RM.GetString(iID);
                iID = RM.NextStringID(iID);
            }

            for (int iIndex = 0; iIndex < 12; iIndex++)
            {
                if (strArray[iIndex] != null)
                {
                    if (strArray[iIndex].Length == 2 && strArray[iIndex][0] == '_')
                    {
                        int iIcon = 0;
                        int iAdjust = -4;
                        switch (strArray[iIndex][1])
                        {
                            case '0':
                                iIcon = 0;
                                break;
                            case '1':
                                iIcon = 1;
                                break;
                            case '2':
                                iIcon = 2;
                                break;
                            case '3':
                                iIcon = 3;
                                break;
                            case '4':
                                iIcon = 4;
                                break;
                            case '5':
                                iAdjust = 1;
                                iIcon = 5;
                                break;
                            case '6':
                                iIcon = 6;
                                break;
                        }
                        m_gameMenu_pAnims[iIcon].draw(120, iBoxY + (iIndex + 1) * 21 + iAdjust, ACTOR_STATE.NONE);
                        g_graBackBuffer.setClip(0, iBoxY, 240, 252);
                    }
                    else
                    {
                        int iAdjust = -6;
                        if (iID == STRING_ID_5_LAUNCH_THE_FIST) // NOTE: This condition is always false
                            iAdjust = -12;
                        g_graBackBuffer.drawString(strArray[iIndex], (Resolution.X - m_fontGeneral.stringWidth(strArray[iIndex])) / 2, iBoxY + iIndex * 21 + iAdjust, ANCHOR.LEFT | ANCHOR.TOP);
                    }
                    strArray[iIndex] = null;
                }
            }

            if (m_gameMenu_idCurSel != 0)
                Actor.drawModule(m_gameMenu_pData, 14, 0 + Menu_GetLArrowPos(), 8, 0, g_graBackBuffer);
            if (!bLast)
                Actor.drawModule(m_gameMenu_pData, 13, Resolution.X - Actor.aniData[(sbyte)ACTOR_TYPE.FONT].modules[13].Width - Menu_GetRArrowPos(), 8, 0, g_graBackBuffer);

            // Optionally fix bug with arrows de-syncing when only rendering one by updating positions even when not drawing
            if (Engine.Settings.Local.J2me.FixBugs)
            {
                if (m_gameMenu_idCurSel == 0)
                    Menu_GetLArrowPos();
                if (bLast)
                    Menu_GetRArrowPos();
            }
        }
    }

    public void DrawCreditsPage()
    {
        string[] strArray = new string[13];
        int iBoxY = 34;
        if (iBoxY - 2 < 21)
            iBoxY = 23;
        DrawParchment(10, iBoxY - 2, 220, 272, false);
        g_graBackBuffer.setClip(10, iBoxY, 220, 268);
        g_graBackBuffer.setColor(0x330099);
        int total_credits_lines = 91;
        m_iAboutTicker += 2;
        int iStart = m_iAboutTicker / 21 % total_credits_lines;
        StringId iID = new(0, TEXTBANK_INDEX_CREDITS);

        for (int iLoop = 0; iLoop < iStart; iLoop++)
        {
            iID = RM.NextStringID(iID);
            if (iID.IsNull)
                break;
        }
        
        if (iID.IsNull)
            return;
        
        for (int i = 0; i < 13; i++)
        {
            if (iID.IsNull)
                break;
            strArray[i] = RM.GetString(iID);
            StringId supportmail_iID = STRING_ID_SUPPORT_MAIL;
            if (iID == supportmail_iID)
                strArray[i] = "support@gameloft.com";
            StringId version_iID = STRING_ID_VERSION_NUMBER;
            if (iID == version_iID)
                strArray[i] = "Version " + ReadVersionFromManifest();
            iID = RM.NextStringID(iID);
        }

        for (int iIndex = 0; iIndex < 13; iIndex++)
        {
            iStart = -(m_iAboutTicker % 21);
            if (strArray[iIndex] != null)
            {
                g_graBackBuffer.drawString(strArray[iIndex], (Resolution.X - m_fontGeneral.stringWidth(strArray[iIndex])) / 2, iBoxY + iStart + iIndex * 21, ANCHOR.LEFT | ANCHOR.TOP);
                strArray[iIndex] = null;
            }
        }

        bool bBackgroundState = m_bBackgroundUsed;
        m_bBackgroundUsed = false;
        Actor.drawModule(Actor.aniData[(sbyte)ACTOR_TYPE.FONT], 23, Resolution.X - Actor.aniData[(sbyte)ACTOR_TYPE.FONT].modules[23].Width, 320 - (Actor.aniData[(sbyte)ACTOR_TYPE.FONT]).modules[23].Height, 0, g_graBackBuffer);
        m_bBackgroundUsed = bBackgroundState;
    }

    public string ReadVersionFromManifest()
    {
        return J2meRom.Manifest.GetValue("MIDlet-Version");
    }

    // Unused
    public string ReadLine(Stream s)
    {
        string line = String.Empty;
        int next;
        while ((next = s.ReadByte()) != 10 && next != -1)
            line += (char)next;
        if (line == String.Empty && next == -1)
            return null;
        if (line[0] == '\r')
            line = line[1..];
        return line;
    }

    public void DrawParchment(float iX, float iY, float iW, float iH, bool bInGame)
    {
        // Border
        if (bInGame)
            g_graBackBuffer.setColor(0x5209E3);
        else
            g_graBackBuffer.setColor(0x0);
        g_graBackBuffer.fillRoundRect(iX, iY, iW, iH, 8, 8);

        // Fill
        g_graBackBuffer.setColor(0xF6F3F0);
        g_graBackBuffer.fillRoundRect(iX + 1, iY + 1, iW - 2, iH - 2, 8, 8);
    }

    // Custom
    public bool Menu_PressedUp()
    {
        // NOTE: Original game checks NUM_1, NUM_2, NUM_3 and NUM_4
        return (m_keys & (GbaInput.Up | GbaInput.Left)) != 0;
    }

    public bool Menu_PressedDown()
    {
        // NOTE: Original game checks NUM_0, NUM_6, NUM_7, NUM_8 and NUM_9
        return (m_keys & (GbaInput.Down | GbaInput.Right)) != 0;
    }

    public bool Menu_PressedConfirm()
    {
        // Custom hack to allow un-pausing with the pause key
        if (m_gameMenu_idCurPage is MENU_PAGE.PAUSE or MENU_PAGE.CHEAT &&
            (m_keys & (GbaInput.Start | GbaInput.Select)) != 0)
        {
            m_gameMenu_idCurSel = 0;
            return true;
        }

        // NOTE: Original game checks SOFTKEY1, SOFTKEY2 and SOFTKEY3
        return (m_keys & GbaInput.A) != 0;
    }
}