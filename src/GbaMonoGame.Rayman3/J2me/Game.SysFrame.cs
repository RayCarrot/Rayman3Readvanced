namespace GbaMonoGame.Rayman3.J2me;

public partial class Game
{
    public SYS_FRAME_STATE curState { get; set; }
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
        curState = SYS_FRAME_STATE.LOADING;
        currentKey = GAME_KEY.NONE;
        pressedKey = GAME_KEY.NONE;
        releasedKey = GAME_KEY.NONE;
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
            case SYS_FRAME_STATE.LOADING:
                GameFrame_LoadSound();
                RM.LoadImage(RESOURCE_ID_IMG_GAMELOFT_LOGO);
                RM.LoadData<TextBankResource>(RESOURCE_ID_DATA_TEXTBANK_GAME);
                LoadSound(0);
                InitSounds();
                setSoundVolume(SoundVolume);
                curState = SYS_FRAME_STATE.SPLASH_SCREEN;
                Graphics.ForceOriginalResolution();
                break;

            // Show Gameloft logo
            case SYS_FRAME_STATE.SPLASH_SCREEN:
                g_graBackBuffer.ClearScreen(0);
                g_graBackBuffer.drawImage(RM.GetImage(RESOURCE_ID_IMG_GAMELOFT_LOGO), Resolution.X / 2, Resolution.Y / 2, ANCHOR.HCENTER | ANCHOR.VCENTER);
                lCurMill = System.currentTimeMillis();
                if (pressedKey != GAME_KEY.NONE || lCurMill - lStartMill > 5000)
                    curState = SYS_FRAME_STATE.TITLE_SCREEN;
                break;

            case SYS_FRAME_STATE.TITLE_SCREEN:
                if (m_byMainLoadingState == 0)
                {
                    Menu_LoadMain();
                    if (!bSoundBegin)
                    {
                        g_graBackBuffer.ClearScreen(0);
                        Menu_DrawString(RM.GetString(STRING_ID_ENABLE_SOUND), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_ENABLE_SOUND))) / 2, 128, 0);
                        Menu_DrawString(RM.GetString(STRING_ID_EMPTY), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_EMPTY))) / 2, 144, 0);
                        Menu_DrawString(RM.GetString(STRING_ID_NO), 0, 299, 0);
                        Menu_DrawString(RM.GetString(STRING_ID_YES), Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_YES)), 299, 0);
                        if ((pressedKey & (GAME_KEY.SOFTKEY_1 | GAME_KEY.SOFTKEY_2)) != 0)
                        {
                            bEnableSound = (pressedKey & GAME_KEY.SOFTKEY_1) == 0;
                            if (bEnableSound && SoundVolume == 0)
                            {
                                SoundVolume = VOL_MEDIUM;
                            }
                            else if (!bEnableSound)
                            {
                                SoundVolume = VOL_OFF;
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
                    RM.LoadData<SceneMapResource>(RESOURCE_ID_DATA_SCENE_MAP);
                    RM.Synchronize();
                    GameFrame_loadLevel(LEVEL_MENU);
                    PlaySound(SOUND_INDEX.music_splash, true, LOOP_INFINITE);
                    m_byMainLoadingState++;
                }
                else
                {
                    g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
                    g_graBackBuffer.drawImage(RM.GetImage(RESOURCE_ID_IMG_SPLASH_SCREEN), Resolution.X / 2, Resolution.Y / 2, ANCHOR.HCENTER | ANCHOR.VCENTER);
                    Actor.drawModule(Actor.aniData[(sbyte)OBJECT_TYPE.FONT], 23, Resolution.X - Actor.aniData[(sbyte)OBJECT_TYPE.FONT].modules[23].Width, Resolution.Y - Actor.aniData[(sbyte)OBJECT_TYPE.FONT].modules[23].Height - Menu_GetVArrowPos(), 0, g_graBackBuffer);
                    if (pressedKey != GAME_KEY.NONE)
                    {
                        m_gameFrame_curState = GAME_FRAME_STATE.DEFAULT;
                        curState = SYS_FRAME_STATE.GAME;
                        StopSound();
                        m_byMainLoadingState = (sbyte)(m_byMainLoadingState + 1);
                        m_keys = 0;
                    }
                }
                RM.LoadData<SlopeDisplacementsResource>(RESOURCE_ID_DATA_SLOPE_DISPLACEMENTS);
                RM.Synchronize();
                break;

            case SYS_FRAME_STATE.GAME:
                GAME_FRAME_STATE ret = GameFrame_doLoop();
                if (ret == GAME_FRAME_STATE.GAME_OVER)
                {
                    // TODO: Why sleep here?
                    //try
                    //{
                    //    Thread.sleep(300L);
                    //}
                    //catch (Exception e) { }
                    curState = SYS_FRAME_STATE.GAME_OVER;
                    PlaySound(SOUND_INDEX.music_gameover, true);
                }
                else if (ret == GAME_FRAME_STATE.EXITED_LEVEL)
                {
                    if (m_iPrevLevel != LEVEL_FINAL)
                    {
                        lStartMill = System.currentTimeMillis();
                        curState = SYS_FRAME_STATE.LEVEL_COMPLETE;
                    }
                    else
                    {
                        curState = SYS_FRAME_STATE.GAME_COMPLETE;
                        pRayman.anim.newAction = 38;
                        m_bBackgroundUsed = false;
                        pRayman.x = 0x7800;
                        pRayman.y = 0xAA00;
                        pRayman.stateFlag &= ~ACTOR_STATE.DEAD;
                    }
                    if (m_iPrevLevel > LEVEL_WORLD_MAP && pRayman.anim.curAction != 37)
                        PlaySound(SOUND_INDEX.music_leveldone, true, LOOP_NONE);
                }
                else if (ret == GAME_FRAME_STATE.CONFIRM_EXIT)
                {
                    bConfirmExit = false;
                    g_graBackBuffer.ClearScreen(0);
                    Menu_DrawString(RM.GetString(STRING_ID_EXIT_QUESTION), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_EXIT_QUESTION))) / 2, 128, 0);
                    Menu_DrawString(RM.GetString(STRING_ID_NO), 0, 299, 0);
                    Menu_DrawString(RM.GetString(STRING_ID_YES), Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_YES)), 299, 0);
                    if ((pressedKey & (GAME_KEY.SOFTKEY_1 | GAME_KEY.SOFTKEY_2)) != 0 && bConfirm)
                    {
                        if ((pressedKey & GAME_KEY.SOFTKEY_1) != 0)
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
                }
                else if (!bConfirmExit)
                {
                    g_graBackBuffer.ClearScreen(0);
                    if (!bConfirmToMainMenu)
                    {
                        Menu_DrawString(RM.GetString(STRING_ID_RESTART_QUESTION), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_RESTART_QUESTION))) / 2, 128, 0);
                    }
                    else
                    {
                        Menu_DrawString(RM.GetString(STRING_ID_TO_MAIN_MENU_QUESTION), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_TO_MAIN_MENU_QUESTION))) / 2, 128, 0);
                    }
                    Menu_DrawString(RM.GetString(STRING_ID_NO), 0, 299, 0);
                    Menu_DrawString(RM.GetString(STRING_ID_YES), Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_YES)), 299, 0);
                    if ((pressedKey & (GAME_KEY.SOFTKEY_1 | GAME_KEY.SOFTKEY_2)) != 0)
                    {
                        if ((pressedKey & GAME_KEY.SOFTKEY_1) != 0)
                            m_gameFrame_curState = m_gameFrame_prevState;
                        else if (bConfirmToMainMenu)
                            GameFrame_PostMessage(MESSAGE_ID.EXIT_TO_MENU, 0);

                        bConfirmExit = true;
                        bConfirmToMainMenu = false;
                    }
                }
                break;

            case SYS_FRAME_STATE.GAME_OVER:
                GameFrame_doLoop();
                if (pressedKey != GAME_KEY.NONE)
                {
                    m_gameStateStep = 0;
                    m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
                    curState = SYS_FRAME_STATE.GAME;
                    StopSound();
                    GameFrame_InitNewGame();
                }
                else
                {
                    Menu_DrawString(RM.GetString(STRING_ID_GAME_OVER), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_GAME_OVER))) / 2, 110, 0);
                }
                break;

            case SYS_FRAME_STATE.GAME_COMPLETE:
                g_graBackBuffer.ClearScreen(0);
                Actor.drawModule(Actor.aniData[(sbyte)OBJECT_TYPE.FONT], 23, Resolution.X - Actor.aniData[(sbyte)OBJECT_TYPE.FONT].modules[23].Width, Resolution.Y - Actor.aniData[(sbyte)OBJECT_TYPE.FONT].modules[23].Height, 0, g_graBackBuffer);
                
                if (pressedKey != GAME_KEY.NONE)
                {
                    m_bBackgroundUsed = true;
                    m_iAboutTicker = 0;
                    Menu_LoadCredits();
                    curState = SYS_FRAME_STATE.CREDITS;
                }
                else
                {
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
                    Menu_DrawString(RM.GetString(STRING_ID_VICTORY), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_VICTORY))) / 2, 170, 0);
                }
                break;

            case SYS_FRAME_STATE.CREDITS:
                g_graBackBuffer.ClearScreen(0);
                DrawCreditsPage();
                if (pressedKey != GAME_KEY.NONE)
                {
                    StopSound();
                    m_bBackgroundUsed = true;
                    m_gameStateStep = 0;
                    m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
                    curState = SYS_FRAME_STATE.GAME;
                    GameFrame_InitNewGame();
                }
                break;

            case SYS_FRAME_STATE.LEVEL_COMPLETE:
                if (m_iPrevLevel > LEVEL_WORLD_MAP && pRayman.anim.curAction != 37)
                {
                    lCurMill = System.currentTimeMillis();
                    if (pressedKey != GAME_KEY.NONE || lCurMill - lStartMill > 3000)
                    {
                        m_gameFrame_paused = true;
                        GameCore();
                        StopSound();
                        m_gameStateStep = 0;
                        m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
                        curState = SYS_FRAME_STATE.GAME;
                    }
                    else
                    {
                        Menu_DrawString(RM.GetString(STRING_ID_LEVEL_DONE), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_LEVEL_DONE))) / 2, 110, 0);
                    }
                }
                else
                {
                    m_gameStateStep = 0;
                    m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
                    curState = SYS_FRAME_STATE.GAME;
                }
                break;
        }

        return 0;
    }
}