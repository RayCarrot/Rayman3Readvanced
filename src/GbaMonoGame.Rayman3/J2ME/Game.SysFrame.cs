namespace GbaMonoGame.Rayman3.J2ME;

public partial class Game
{
    public int curState { get; set; }
    public long lStartMill { get; set; }
    public sbyte m_byMainLoadingState { get; set; }
    public bool bSoundBegin { get; set; }
    public bool bEnableSound { get; set; }
    public bool bConfirm { get; set; }
    public bool bConfirmExit { get; set; }
    public bool bConfirmToMainMenu { get; set; }

    public bool SysFrame_PhysicalInitI()
    {
        lStartMill = System.currentTimeMillis();
        curState = 0;
        currentKey = GAME_KEY.None;
        pressedKey = GAME_KEY.None;
        releasedKey = GAME_KEY.None;
        GameFrame_PhysicalInitI();
        return true;
    }

    public int SysFrame_doLoop()
    {
        long lCurMill;
        g_graBackBuffer.setFont(m_fontGeneral);
        switch (curState)
        {
            // Load
            case 0:
                GameFrame_LoadSound();
                RM.Load(0x60000411);
                RM.Load(0x6000012D);
                LoadSound(0);
                InitSounds();
                setSoundVolume(SoundVolume);
                curState = 1;
                break;

            // Show Gameloft logo
            case 1:
                g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
                g_graBackBuffer.setColor(0);
                g_graBackBuffer.fillRect(0, 0, Resolution.X, Resolution.Y);
                g_graBackBuffer.drawImage(RM.GetImage(17), Resolution.X / 2, Resolution.Y / 2, ANCHOR.HCENTER | ANCHOR.VCENTER);
                lCurMill = System.currentTimeMillis();
                if (pressedKey != GAME_KEY.None || lCurMill - lStartMill > 5000)
                    curState = 2;
                break;

            case 2:
                if (m_byMainLoadingState == 0)
                {
                    Menu_LoadMain();
                    if (!bSoundBegin)
                    {
                        g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
                        g_graBackBuffer.setColor(0);
                        g_graBackBuffer.fillRect(0, 0, Resolution.X, Resolution.Y);
                        Menu_DrawString(RM.GetString(2949300), (Resolution.X - Menu_GetStringWidth(RM.GetString(2949300))) >> 1, 128, 0);
                        Menu_DrawString(RM.GetString(2949314), (Resolution.X - Menu_GetStringWidth(RM.GetString(2949314))) >> 1, 144, 0);
                        Menu_DrawString(RM.GetString(2949319), 0, 299, 0);
                        Menu_DrawString(RM.GetString(2949315), Resolution.X - Menu_GetStringWidth(RM.GetString(2949315)), 299, 0);
                        if ((pressedKey & (GAME_KEY.Softkey1 | GAME_KEY.Softkey2)) != 0)
                        {
                            bEnableSound = (pressedKey & GAME_KEY.Softkey1) == 0;
                            if (bEnableSound && SoundVolume == 0)
                            {
                                SoundVolume = VOL_MEDIUM;
                            }
                            else if (!bEnableSound)
                            {
                                SoundVolume = 0;
                                bEnableSound = true;
                            }
                            bSoundBegin = true;
                        }
                        break;
                    }
                    m_byMainLoadingState = (sbyte)(m_byMainLoadingState + 1);
                }
                else if (m_byMainLoadingState == 1)
                {
                    RM.Load(0x60000913);
                    RM.Synchronize();
                    GameFrame_loadLevel(-1);
                    PlaySound(SOUND_INDEX.music_splash, true, 255);
                    m_byMainLoadingState = (sbyte)(m_byMainLoadingState + 1);
                }
                else
                {
                    g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
                    g_graBackBuffer.drawImage(RM.GetImage(18), Resolution.X / 2, Resolution.Y / 2, ANCHOR.HCENTER | ANCHOR.VCENTER);
                    Actor.drawModule(Actor.aniData[26], 23, Resolution.X - Actor.aniData[26].modules[23].Width, Resolution.Y - Actor.aniData[26].modules[23].Height - Menu_GetVArrowPos(), 0, g_graBackBuffer);
                    if (pressedKey != GAME_KEY.None)
                    {
                        m_gameFrame_curState = GAME_FRAME_STATE.DEFAULT;
                        curState = 3;
                        StopSound();
                        m_byMainLoadingState = (sbyte)(m_byMainLoadingState + 1);
                        m_keys = 0;
                    }
                }
                RM.Load(0x60000914);
                RM.Synchronize();
                break;

            case 3:
                GAME_FRAME_STATE ret = GameFrame_doLoop();
                if (ret == GAME_FRAME_STATE.GAME_OVER)
                {
                    // TODO: Why sleep here?
                    //try
                    //{
                    //    Thread.sleep(300L);
                    //}
                    //catch (Exception e) { }
                    curState = 4;
                    PlaySound(SOUND_INDEX.music_gameover, true);
                    break;
                }
                if (ret == GAME_FRAME_STATE.EXITED_LEVEL)
                {
                    if (m_iPrevLevel != 8)
                    {
                        lStartMill = System.currentTimeMillis();
                        curState = 5;
                    }
                    else
                    {
                        curState = 6;
                        pRayman.anim.newAction = 38;
                        m_bBackgroundUsed = false;
                        pRayman.x = 0x7800;
                        pRayman.y = 0xAA00;
                        pRayman.stateFlag &= ~ACTOR_STATE.DEAD;
                    }
                    if (m_iPrevLevel > 0 && pRayman.anim.curAction != 37)
                        PlaySound(SOUND_INDEX.music_leveldone, true, 1);
                    break;
                }
                if (ret == GAME_FRAME_STATE.CONFIRM_EXIT)
                {
                    bConfirmExit = false;
                    g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
                    g_graBackBuffer.setColor(0);
                    g_graBackBuffer.fillRect(0, 0, Resolution.X, Resolution.Y);
                    Menu_DrawString(RM.GetString(0x2D00CA), (Resolution.X - Menu_GetStringWidth(RM.GetString(0x2D00CA))) >> 1, 128, 0);
                    Menu_DrawString(RM.GetString(0x2D00C7), 0, 299, 0);
                    Menu_DrawString(RM.GetString(0x2D00C3), Resolution.X - Menu_GetStringWidth(RM.GetString(0x2D00C3)), 299, 0);
                    if ((pressedKey & (GAME_KEY.Softkey1 | GAME_KEY.Softkey2)) != 0 && bConfirm)
                    {
                        if ((pressedKey & GAME_KEY.Softkey1) != 0)
                        {
                            m_gameFrame_curState = m_gameFrame_prevState;
                        }
                        else
                        {
                            return 1;
                        }
                        bConfirmExit = true;
                    }
                    bConfirm = true;
                    break;
                }
                if (!bConfirmExit)
                {
                    g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
                    g_graBackBuffer.setColor(0);
                    g_graBackBuffer.fillRect(0, 0, Resolution.X, Resolution.Y);
                    if (!bConfirmToMainMenu)
                    {
                        Menu_DrawString(RM.GetString(0x2D00D0), (Resolution.X - Menu_GetStringWidth(RM.GetString(0x2D00D0))) >> 1, 128, 0);
                    }
                    else
                    {
                        Menu_DrawString(RM.GetString(0x2D00D9), (Resolution.X - Menu_GetStringWidth(RM.GetString(0x2D00D9))) >> 1, 128, 0);
                    }
                    Menu_DrawString(RM.GetString(0x2D00C7), 0, 299, 0);
                    Menu_DrawString(RM.GetString(0x2D00C3), Resolution.X - Menu_GetStringWidth(RM.GetString(0x2D00C3)), 299, 0);
                    if ((pressedKey & (GAME_KEY.Softkey1 | GAME_KEY.Softkey2)) != 0)
                    {
                        if ((pressedKey & GAME_KEY.Softkey1) != 0)
                            m_gameFrame_curState = m_gameFrame_prevState;
                        else if (bConfirmToMainMenu)
                            GameFrame_PostMessage(MESSAGE_ID.EXIT_TO_MENU, 0);

                        bConfirmExit = true;
                        bConfirmToMainMenu = false;
                    }
                }
                break;

            case 4:
                GameFrame_doLoop();
                if (pressedKey != GAME_KEY.None)
                {
                    m_gameStateStep = 0;
                    m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
                    curState = 3;
                    StopSound();
                    GameFrame_InitNewGame();
                    break;
                }
                Menu_DrawString(RM.GetString(0x2D009E), (Resolution.X - Menu_GetStringWidth(RM.GetString(0x2D009E))) >> 1, 110, 0);
                break;

            case 6:
                g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
                g_graBackBuffer.setColor(0);
                g_graBackBuffer.fillRect(0, 0, Resolution.X, Resolution.Y);
                Actor.drawModule(Actor.aniData[26], 23, Resolution.X - Actor.aniData[26].modules[23].Width, Resolution.Y - Actor.aniData[26].modules[23].Height, 0, g_graBackBuffer);
                
                if (pressedKey != GAME_KEY.None)
                {
                    m_bBackgroundUsed = true;
                    m_iAboutTicker = 0;
                    Menu_LoadCredits();
                    curState = 7;
                    break;
                }

                if (pRayman.Ani_CheckEnd())
                {
                    if ((pRayman.stateFlag & ACTOR_STATE.FLIP_X) == 0)
                    {
                        pRayman.stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        pRayman.stateFlag |= ACTOR_STATE.FLIP_X;
                    }
                    else
                    {
                        pRayman.stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        pRayman.stateFlag &= ~ACTOR_STATE.FLIP_X;
                    }
                }

                pRayman.step();
                pRayman.draw();
                Menu_DrawString(RM.GetString(2949288), (Resolution.X - Menu_GetStringWidth(RM.GetString(2949288))) >> 1, 170, 0);
                break;

            case 7:
                g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
                g_graBackBuffer.setColor(0);
                g_graBackBuffer.fillRect(0, 0, Resolution.X, Resolution.Y);
                DrawCreditsPage();
                if (pressedKey != GAME_KEY.None)
                {
                    StopSound();
                    m_bBackgroundUsed = true;
                    m_gameStateStep = 0;
                    m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
                    curState = 3;
                    GameFrame_InitNewGame();
                }
                break;

            case 5:
                if (m_iPrevLevel > 0 && pRayman.anim.curAction != 37)
                {
                    lCurMill = System.currentTimeMillis();
                    if (pressedKey != GAME_KEY.None || lCurMill - lStartMill > 3000)
                    {
                        m_gameFrame_paused = true;
                        GameCore();
                        StopSound();
                        m_gameStateStep = 0;
                        m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
                        curState = 3;
                        break;
                    }
                    Menu_DrawString(RM.GetString(0x2D0093), (Resolution.X - Menu_GetStringWidth(RM.GetString(0x2D0093))) >> 1, 110, 0);
                    break;
                }
                m_gameStateStep = 0;
                m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
                curState = 3;
                break;
        }

        return 0;
    }
}