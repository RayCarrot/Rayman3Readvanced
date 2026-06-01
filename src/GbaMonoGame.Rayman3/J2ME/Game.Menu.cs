using System;
using System.IO;

namespace GbaMonoGame.Rayman3.J2ME;

public partial class Game
{
    public const int VOL_HIGH = 100;
    public const int VOL_MEDIUM = 70;
    public const int VOL_LOW = 40;
    public const int VOL_OFF = 0;

    public const int MAX_MENU_ITEMS = 8;
    public const int MENU_ITEM_HEIGHT = 21;

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

    public void Menu_DrawString(string pStr, int x, int y, int nColor)
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

    public void Menu_DrawItem(string pStr, int y, int id)
    {
        if (id == m_gameMenu_idCurSel)
        {
            int w = Menu_GetStringWidth(pStr);
            int x = (Resolution.X - w) / 2;
            Menu_DrawString(pStr, x, y - 5 + 2, 0);
            // TODO: Original game bug or decompiler bug?
            bool bBackgroundState = m_bBackgroundUsed;
            bBackgroundState = false;
            Actor.drawModule(m_gameMenu_pData, 13, x - Actor.aniData[26].modules[13][2] - 4 - Menu_GetRArrowPos(), y, 0, g_graBackBuffer);
            Actor.drawModule(m_gameMenu_pData, 14, x + w + 4 + Menu_GetLArrowPos(), y, 0, g_graBackBuffer);
            m_bBackgroundUsed = bBackgroundState;
        }
        else
        {
            int w = Menu_GetStringWidth(pStr);
            int x = (Resolution.X - w) / 2;
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
                    Menu_DrawString(RM.GetString(STRING_ID_LOADING), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_LOADING))) >> 1, 263, 0);
                }
                else if (m_gameStateStep == 1)
                {
                    Menu_LoadCredits();
                }
                else
                {
                    strTitle = RM.GetString(STRING_ID_ABOUT);
                    g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
                    g_graBackBuffer.setColor(0x5209E3);
                    g_graBackBuffer.fillRect(0, 0, Resolution.X, Resolution.Y);
                    Menu_DrawString(strTitle, (Resolution.X - Menu_GetStringWidth(strTitle)) >> 1, 6, iColor);
                    Menu_DrawPageText();
                }
                m_gameStateStep++;
                return;

            case MENU_PAGE.HELP:
                if (m_iGlobalTicker is >= 0 and < 3)
                {
                    Menu_DrawString(RM.GetString(STRING_ID_LOADING), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_LOADING))) >> 1, 263, 0);
                }
                else if (m_iGlobalTicker == 3)
                {
                    Menu_LoadHelp();
                }
                else if (m_iGlobalTicker > 3)
                {
                    iColor = 0xFFFFFF;
                    strTitle = RM.GetString(STRING_ID_HELP_TITLE);
                    g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
                    g_graBackBuffer.setColor(0x5209E3);
                    g_graBackBuffer.fillRect(0, 0, Resolution.X, Resolution.Y);
                    Menu_DrawString(strTitle, (Resolution.X - Menu_GetStringWidth(strTitle)) >> 1, 6, iColor);
                    Menu_DrawPageText();
                }
                return;

            case MENU_PAGE.MAIN:
                Menu_DrawItem(m_gameMenu_items_pStr[m_gameMenu_idCurSel], 268, m_gameMenu_idCurSel);
                return;
        }

        int iStartMenuY = (Resolution.Y - m_gameMenu_nItem * MENU_ITEM_HEIGHT) >> 1;
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
                    Menu_InsertOption(RM.GetString(STRING_ID_CONTINUE), 1);
                Menu_InsertOption(RM.GetString(STRING_ID_NEW_GAME), 0);
                
                switch (SoundVolume)
                {
                    case VOL_HIGH:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_HIGH), 2);
                        break;
                    
                    case VOL_MEDIUM:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_MEDIUM), 2);
                        break;
                    
                    case VOL_LOW:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_LOW), 2);
                        break;
                    
                    case VOL_OFF:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_OFF), 2);
                        break;
                }

                Menu_InsertOption(RM.GetString(STRING_ID_ABOUT), 3);
                Menu_InsertOption(RM.GetString(STRING_ID_HELP), 4);
                Menu_InsertOption(RM.GetString(STRING_ID_EXIT), 5);
                break;

            case MENU_PAGE.PAUSE:
                Menu_InsertOption(RM.GetString(STRING_ID_RESUME), 0);
                Menu_InsertOption(RM.GetString(STRING_ID_RESTART), 1);

                switch (SoundVolume)
                {
                    case VOL_HIGH:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_HIGH), 2);
                        break;

                    case VOL_MEDIUM:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_MEDIUM), 2);
                        break;

                    case VOL_LOW:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_LOW), 2);
                        break;

                    case VOL_OFF:
                        Menu_InsertOption(RM.GetString(STRING_ID_MUSIC_OFF), 2);
                        break;
                }

                Menu_InsertOption(RM.GetString(STRING_ID_MAIN_MENU), 3);
                Menu_InsertOption(RM.GetString(STRING_ID_EXIT), 4);
                break;
        }
    }

    public void Menu_LoadMain()
    {
        RM.Free(0x60000411);
        RM.Load(0x60000412);
        RM.Load(0x60000405);
        RM.Load(0x60000107);
        RM.Synchronize();
        Actor.AniLoad(7, 5);
    }

    public void Menu_Free(bool bFreeMainMenu)
    {
        if (m_gameMenu_pAnims != null)
        {
            for (int i = 0; i < m_gameMenu_pAnims.Length; i++)
                m_gameMenu_pAnims[i] = null;
            m_gameMenu_pAnims = null;
        }

        RM.Free(0x6000040B);
        RM.Free(0x60000409);
        RM.Free(0x60000403);
        RM.Free(0x60000116);
        RM.Free(0x60000115);
        RM.Free(0x60000111);
        RM.Free(0x60000101);
        RM.Free(0x6000010C);
        RM.Free(0x60000104);
        RM.Free(0x6000012F);
        RM.Free(0x60000130);

        if (bFreeMainMenu)
        {
            RM.Free(0x60000412);
            RM.Free(0x60000405);
            RM.Free(0x60000107);
        }
        
        RM.Synchronize();
    }

    public void Menu_LoadCredits()
    {
        Menu_Free(false);
        RM.Load(0x6000012F);
        RM.Synchronize();
    }

    public void Menu_LoadHelp()
    {
        Menu_Free(false);
        RM.Load(0x60000130);
        RM.Load(0x6000040B);
        RM.Load(0x60000403);
        RM.Load(0x60000116);
        RM.Load(0x60000115);
        RM.Load(0x60000111);
        RM.Load(0x60000108);
        RM.Load(0x60000101);
        RM.Load(0x60000104);
        RM.Load(0x60000113);
        RM.Synchronize();
        Actor.AniLoad(17, 11);
        Actor.AniLoad(22, 11);
        Actor.AniLoad(21, 11);
        Actor.AniLoad(8, 11);
        Actor.AniLoad(1, 11);
        Actor.AniLoad(19, 11);
        Actor.AniLoad(4, 3);
        Actor.AniLoad(1, 11);
        m_gameMenu_pAnims = new Anim[7];
        for (int i = 0; i < m_gameMenu_pAnims.Length; i++)
            m_gameMenu_pAnims[i] = new Anim();
        m_gameMenu_pAnims[0].build(OBJECT_TYPE.LUM, 0);
        m_gameMenu_pAnims[1].build(OBJECT_TYPE.ENERGY, 0);
        m_gameMenu_pAnims[2].build(OBJECT_TYPE.CHECKPOINT, 0);
        m_gameMenu_pAnims[3].build(OBJECT_TYPE.LIFE, 0);
        m_gameMenu_pAnims[6].build(OBJECT_TYPE.BLUE_LUM, 0);
        m_gameMenu_pAnims[4].build(OBJECT_TYPE.CAGE, 0);
        m_gameMenu_pAnims[5].build(OBJECT_TYPE.SWING_LUM, 0);
        m_gameMenu_pAnims[6].build(OBJECT_TYPE.BLUE_LUM, 0); // TODO: Why load again?
        m_byHelpLength = 0;
        int iID = 0x300000;
        while (iID != -1)
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

        // Left
        if ((m_keys & (KEY.NUM_1 | KEY.NUM_2 | KEY.NUM_3 | KEY.NUM_4)) != 0)
        {
            PlaySound(SOUND_INDEX.menu_move, true);
            if (m_gameMenu_idCurPage == MENU_PAGE.HELP)
                m_gameMenu_idCurSel--;
            else if (--m_gameMenu_idCurSel < 0)
                m_gameMenu_idCurSel = m_gameMenu_nItem - 1;
        }
        // Right
        else if ((m_keys & (KEY.NUM_0 | KEY.NUM_6 | KEY.NUM_7 | KEY.NUM_8 | KEY.NUM_9)) != 0)
        {
            PlaySound(SOUND_INDEX.menu_move, true);
            if (m_gameMenu_idCurPage == MENU_PAGE.HELP)
                m_gameMenu_idCurSel++;
            else if (++m_gameMenu_idCurSel >= m_gameMenu_nItem)
                m_gameMenu_idCurSel = 0;
        }
        // Select
        else if ((m_keys & (KEY.SOFTKEY1 | KEY.SOFTKEY2 | KEY.SOFTKEY3)) != 0)
        {
            switch (m_gameMenu_idCurPage)
            {
                case MENU_PAGE.MAIN:
                    switch (m_gameMenu_items_id[m_gameMenu_idCurSel])
                    {
                        case 0:
                            StopSound();
                            m_gameStateStep = 0;
                            m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                            m_gameFrame_curState = GAME_FRAME_STATE.NEW_GAME;
                            GameFrame_InitNewGame();
                            break;

                        case 1:
                            m_gameStateStep = 0;
                            m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                            m_gameFrame_curState = GAME_FRAME_STATE.LOAD_GAME;
                            break;
                    
                        case 2:
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

                        case 3:
                            m_iAboutTicker = 0;
                            m_gameStateStep = 0;
                            Menu_SetCurrentPage(MENU_PAGE.ABOUT);
                            break;

                        case 4:
                            m_iGlobalTicker = 0;
                            Menu_SetCurrentPage(MENU_PAGE.HELP);
                            break;
                    
                        case 5:
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
                        case 0:
                            m_bBackgroundUsed = true;
                            m_gameFrame_paused = false;
                            if (pRayman != null && pRayman.V[3] == 32)
                            {
                                releasedKey = GAME_KEY.Middle;
                                GameCore();
                            }
                            if (m_gameFrame_curLevel == 0)
                            {
                                PlaySound(SOUND_INDEX.music_map, true, 255);
                                bStillPlay = true;
                            }
                            break;

                        case 1:
                            m_gameStateStep = 0;
                            m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                            m_gameFrame_curState = GAME_FRAME_STATE.CONFIRM_RESTART;
                            bConfirmExit = false;
                            break;
                    
                        case 2:
                            SoundVolume = SoundVolume switch
                            {
                                VOL_HIGH => VOL_MEDIUM,
                                VOL_MEDIUM => VOL_LOW,
                                VOL_LOW => VOL_OFF,
                                VOL_OFF => VOL_HIGH,
                                _ => SoundVolume
                            };
                            Menu_UpdatePage();
                            // TODO: Play sound
                            // if (m_gameFrame_curLevel == 0 && m_SoundPlayer == null) 
                            //     PlaySound(35, true, 255);
                            if (SoundVolume == VOL_OFF)
                                StopSound();
                            setSoundVolume(SoundVolume);
                            GameFrame_SaveSound();
                            break;

                        case 3:
                            m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                            bConfirmExit = false;
                            bConfirmToMainMenu = true;
                            break;
                    
                        case 4:
                            StopSound();
                            GameFrame_PostMessage(MESSAGE_ID.EXIT, 0);
                            m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                            bConfirm = false;
                            break;
                    }

                    switch (m_gameMenu_items_id[m_gameMenu_idCurSel])
                    {
                        case 0:
                            m_keys = KEY.NONE;
                            break;

                        case 1:
                        case 3:
                            m_keys = KEY.NONE;
                            m_bClearBackMenu = true;
                            break;
                    }
                    break;
                
                case MENU_PAGE.HELP:
                case MENU_PAGE.ABOUT:
                    Menu_SetCurrentPage(MENU_PAGE.MAIN);
                    break;
            }

            if (!bStillPlay)
                PlaySound(SOUND_INDEX.menu_select, true);
        }
    }

    public void Menu_Draw()
    {
        g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
        if (m_gameMenu_idCurPage != MENU_PAGE.PAUSE)
        {
            g_graBackBuffer.drawImage(RM.GetImage(18), Resolution.X / 2, Resolution.Y / 2, ANCHOR.HCENTER | ANCHOR.VCENTER);
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

    // TODO: Clean up this method
    public void Menu_DrawPageText()
    {
        string[] strArray = new string[13];
        int iBoxY = 34;
        if (iBoxY - 2 < 21)
            iBoxY = 23;
        DrawParchment(10, iBoxY - 2, 220, 272, false);
        g_graBackBuffer.setClip(10, iBoxY, 220, 268);
        g_graBackBuffer.setColor(3342489);

        if (m_gameMenu_idCurPage == MENU_PAGE.ABOUT)
        {
            const int total_credits_lines = 91;
            m_iAboutTicker += 2;
            int iStart = m_iAboutTicker / 21 % total_credits_lines;
            int iID = 3080192;
            for (int iLoop = 0; iLoop < iStart; iLoop++)
            {
                iID = RM.NextStringID(iID);
                if (iID == -1)
                    break;
            }
            if (iID == -1)
                return;
            for (int i = 0; i < 13; i++)
            {
                if (iID == -1)
                    break;
                strArray[i] = RM.GetString(iID);
                int supportmail_iID = 0x2F0046;
                if (iID == supportmail_iID)
                    strArray[i] = "support@gameloft.com";
                int version_iID = 0x2F009B;
                if (iID == version_iID)
                    strArray[i] = "Version " + ReadVersionFromManifest();
                iID = RM.NextStringID(iID);
            }
            for (int iIndex = 0; iIndex < 13; iIndex++)
            {
                iStart = -(m_iAboutTicker % 21);
                if (strArray[iIndex] != null)
                {
                    g_graBackBuffer.drawString(strArray[iIndex], (Resolution.X - m_fontGeneral.stringWidth(strArray[iIndex])) >> 1, iBoxY + iStart + iIndex * 21, ANCHOR.LEFT | ANCHOR.TOP);
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
            int iID = 0x300000;
            for (int iLoop = 0; iLoop < iStart; iLoop++)
            {
                iID = RM.NextStringID(iID);
                if (iID == -1)
                    break;
            }
            if (iID == -1)
                return;
            for (int i = 0; i < 12; i++)
            {
                if (iID == -1)
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
                        int iAdjust = 0;
                        iAdjust = -6;
                        if (iID == 0x3000B7)
                            iAdjust = -12;
                        g_graBackBuffer.drawString(strArray[iIndex], (Resolution.X - m_fontGeneral.stringWidth(strArray[iIndex])) >> 1, iBoxY + iIndex * 21 + iAdjust, ANCHOR.LEFT | ANCHOR.TOP);
                    }
                    strArray[iIndex] = null;
                }
            }
            if (m_gameMenu_idCurSel != 0)
                Actor.drawModule(m_gameMenu_pData, 14, 0 + Menu_GetLArrowPos(), 8, 0, g_graBackBuffer);
            if (!bLast)
                Actor.drawModule(m_gameMenu_pData, 13, 240 - (Actor.aniData[26]).modules[13][2] - Menu_GetRArrowPos(), 8, 0, g_graBackBuffer);
        }
    }

    // TODO: Clean up this method
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
        int iID = 0x2F0000;
        for (int iLoop = 0; iLoop < iStart; iLoop++)
        {
            iID = RM.NextStringID(iID);
            if (iID == -1)
                break;
        }
        if (iID == -1)
            return;
        for (int i = 0; i < 13; i++)
        {
            if (iID == -1)
                break;
            strArray[i] = RM.GetString(iID);
            int supportmail_iID = 0x2F0046;
            if (iID == supportmail_iID)
                strArray[i] = "support@gameloft.com";
            int version_iID = 0x2F009B;
            if (iID == version_iID)
                strArray[i] = "Version " + ReadVersionFromManifest();
            iID = RM.NextStringID(iID);
        }
        for (int iIndex = 0; iIndex < 13; iIndex++)
        {
            iStart = -(m_iAboutTicker % 21);
            if (strArray[iIndex] != null)
            {
                g_graBackBuffer.drawString(strArray[iIndex], (Resolution.X - m_fontGeneral.stringWidth(strArray[iIndex])) >> 1, iBoxY + iStart + iIndex * 21, ANCHOR.LEFT | ANCHOR.TOP);
                strArray[iIndex] = null;
            }
        }
        bool bBackgroundState = m_bBackgroundUsed;
        m_bBackgroundUsed = false;
        Actor.drawModule(Actor.aniData[26], 23, 240 - (Actor.aniData[26]).modules[23][2], 320 - (Actor.aniData[26]).modules[23][3], 0, g_graBackBuffer);
        m_bBackgroundUsed = bBackgroundState;
    }

    public string ReadVersionFromManifest()
    {
        // TODO: Implement
        return String.Empty;
        // return GameMidlet.Instance_Midlet.getAppProperty("MIDlet-Version");
    }

    // Unused
    public string ReadLine(Stream s)
    {
        string line = String.Empty;
        try
        {
            int next;
            while ((next = s.ReadByte()) != 10 && next != -1)
                line += (char)next;
            if (line == String.Empty && next == -1)
                return null;
            if (line[0] == '\r')
                line = line[1..];
        }
        catch (Exception e)
        {
            // e.printStackTrace();
        }
        return line;
    }

    public void DrawParchment(int iX, int iY, int iW, int iH, bool bInGame)
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
}