namespace GbaMonoGame.Rayman3.J2ME;

public partial class Game
{
    public long lStartMillForKeyDelay { get; set; }
    public GAME_KEY currentKey { get; set; }
    public GAME_KEY pressedKey { get; set; }
    public GAME_KEY releasedKey { get; set; }
    public int m_iKeyCheckCounter { get; set; }
    public JAVA_KEY_CODE realKeyCode { get; set; }
    public KEY m_keys { get; set; }

    public void Input_Tick()
    {
        GAME_KEY nKeyCode;
        KEY keys = m_keys;
        KEY mask = KEY.MASK_START;
        while (mask != 0)
        {
            if ((mask & keys) != 0)
            {
                keys = mask;
                break;
            }
            mask = (KEY)((int)mask >> 1);
        }

        switch (keys)
        {
            case KEY.NUM_1:
                nKeyCode = GAME_KEY.UP_LEFT;
                break;

            case KEY.NUM_2:
                if ((m_keys & (KEY.NUM_1 | KEY.NUM_4)) != 0)
                    nKeyCode = GAME_KEY.UP_LEFT;
                else if ((m_keys & (KEY.NUM_3 | KEY.NUM_6)) != 0)
                    nKeyCode = GAME_KEY.UP_RIGHT;
                else
                    nKeyCode = GAME_KEY.UP;
                break;
            
            case KEY.NUM_3:
                nKeyCode = GAME_KEY.UP_RIGHT;
                break;

            case KEY.NUM_4:
                if ((m_keys & (KEY.NUM_1 | KEY.NUM_2)) != 0)
                    nKeyCode = GAME_KEY.UP_LEFT;
                else if ((m_keys & (KEY.NUM_7 | KEY.NUM_8)) != 0)
                    nKeyCode = GAME_KEY.DOWN_LEFT;
                else
                    nKeyCode = GAME_KEY.LEFT;
                break;
            
            case KEY.NUM_5:
                nKeyCode = GAME_KEY.MIDDLE;
                break;
            
            case KEY.NUM_6:
                if ((m_keys & (KEY.NUM_2 | KEY.NUM_3)) != 0)
                    nKeyCode = GAME_KEY.UP_RIGHT;
                else if ((m_keys & (KEY.NUM_8 | KEY.NUM_9)) != 0)
                    nKeyCode = GAME_KEY.DOWN_RIGHT;
                else
                    nKeyCode = GAME_KEY.RIGHT;
                break;
            
            case KEY.NUM_7:
                nKeyCode = GAME_KEY.DOWN_LEFT;
                break;
            
            case KEY.NUM_8:
                if ((m_keys & (KEY.NUM_4 | KEY.NUM_7)) != 0)
                    nKeyCode = GAME_KEY.DOWN_LEFT;
                else if ((m_keys & (KEY.NUM_6 | KEY.NUM_9)) != 0)
                    nKeyCode = GAME_KEY.DOWN_RIGHT;
                else
                    nKeyCode = GAME_KEY.DOWN;
                break;
            
            case KEY.NUM_9:
                nKeyCode = GAME_KEY.DOWN_RIGHT;
                break;
            
            case KEY.SOFTKEY1:
                nKeyCode = GAME_KEY.SOFTKEY_1;
                break;
            
            case KEY.SOFTKEY2:
                nKeyCode = GAME_KEY.SOFTKEY_2;
                break;
            
            case KEY.POUND:
                nKeyCode = GAME_KEY.POUND;
                break;
            
            case KEY.STAR:
                nKeyCode = GAME_KEY.STAR;
                break;
            
            case KEY.NUM_0:
                nKeyCode = GAME_KEY.ZERO;
                break;
            
            case KEY.END:
                nKeyCode = GAME_KEY.END;
                break;
            
            default:
                nKeyCode = GAME_KEY.NONE;
                break;
        }

        pressedKey = (GAME_KEY)(((short)currentKey ^ 0xFFFFFFFF) & (int)nKeyCode);
        releasedKey = (GAME_KEY)((short)currentKey & ((int)nKeyCode ^ 0xFFFFFFFF));
        currentKey = nKeyCode;
        m_iKeyCheckCounter--;
        
        // Pause
        if (m_gameFrame_curLevel >= LEVEL_WORLD_MAP && 
            (pressedKey & (GAME_KEY.SOFTKEY_1 | GAME_KEY.SOFTKEY_2)) != 0 && 
            !m_gameFrame_paused && 
            curState == 3)
        {
            m_gameFrame_paused = true;
            Menu_SetCurrentPage(MENU_PAGE.PAUSE);
            pressedKey = 0;
            StopSound();
        }
    }

    public void keyPressed(JAVA_KEY_CODE keyCode)
    {
        if (m_gameFrame_prevState == GAME_FRAME_STATE.LOADING && 
            m_gameFrame_curState == GAME_FRAME_STATE.DEFAULT && 
            System.currentTimeMillis() - lStartMillForKeyDelay < 200) 
            return;
        
        realKeyCode = keyCode;
        switch (keyCode)
        {
            case JAVA_KEY_CODE.END:
                m_keys |= KEY.END;
                break;

            case JAVA_KEY_CODE.SOFTKEY2:
                m_keys |= KEY.SOFTKEY2;
                break;

            case JAVA_KEY_CODE.SOFTKEY1:
                m_keys |= KEY.SOFTKEY1;
                break;

            case JAVA_KEY_CODE.SOFTKEY3:
                m_keys |= KEY.SOFTKEY3;
                break;

            case JAVA_KEY_CODE.RIGHT_ARROW:
                m_keys |= KEY.RIGHT_ARROW;
                break;

            case JAVA_KEY_CODE.LEFT_ARROW:
                m_keys |= KEY.LEFT_ARROW;
                break;

            case JAVA_KEY_CODE.DOWN_ARROW:
                m_keys |= KEY.DOWN_ARROW;
                break;

            case JAVA_KEY_CODE.UP_ARROW:
                m_keys |= KEY.UP_ARROW;
                break;

            case JAVA_KEY_CODE.POUND:
                m_keys |= KEY.POUND;
                break;

            case JAVA_KEY_CODE.STAR:
                m_keys |= KEY.STAR;
                break;

            case JAVA_KEY_CODE.NUM_0:
                m_keys |= KEY.NUM_0;
                break;

            case JAVA_KEY_CODE.NUM_1:
                m_keys |= KEY.NUM_1;
                break;

            case JAVA_KEY_CODE.NUM_2:
                m_keys |= KEY.NUM_2;
                break;

            case JAVA_KEY_CODE.NUM_3:
                m_keys |= KEY.NUM_3;
                break;

            case JAVA_KEY_CODE.NUM_4:
                m_keys |= KEY.NUM_4;
                break;

            case JAVA_KEY_CODE.NUM_5:
                m_keys |= KEY.NUM_5;
                break;

            case JAVA_KEY_CODE.NUM_6:
                m_keys |= KEY.NUM_6;
                break;

            case JAVA_KEY_CODE.NUM_7:
                m_keys |= KEY.NUM_7;
                break;

            case JAVA_KEY_CODE.NUM_8:
                m_keys |= KEY.NUM_8;
                break;

            case JAVA_KEY_CODE.NUM_9:
                m_keys |= KEY.NUM_9;
                break;
        }
        if (m_gameFrame_paused || m_gameFrame_curLevel < LEVEL_WORLD_MAP)
            Menu_DoAI();
        m_iKeyCheckCounter = 2;
        m_bUpdateStatus = false;
    }

    public void keyReleased(JAVA_KEY_CODE keyCode)
    {
        switch (keyCode)
        {
            case JAVA_KEY_CODE.END:
                m_keys &= ~KEY.END;
                break;

            case JAVA_KEY_CODE.SOFTKEY2:
                m_keys &= ~KEY.SOFTKEY2;
                break;

            case JAVA_KEY_CODE.SOFTKEY1:
                m_keys &= ~KEY.SOFTKEY1;
                break;

            case JAVA_KEY_CODE.SOFTKEY3:
                m_keys &= ~KEY.SOFTKEY3;
                break;

            case JAVA_KEY_CODE.RIGHT_ARROW:
                m_keys &= ~KEY.RIGHT_ARROW;
                break;

            case JAVA_KEY_CODE.LEFT_ARROW:
                m_keys &= ~KEY.LEFT_ARROW;
                break;

            case JAVA_KEY_CODE.DOWN_ARROW:
                m_keys &= ~KEY.DOWN_ARROW;
                break;

            case JAVA_KEY_CODE.UP_ARROW:
                m_keys &= ~KEY.UP_ARROW;
                break;

            case JAVA_KEY_CODE.POUND:
                m_keys &= ~KEY.POUND;
                break;

            case JAVA_KEY_CODE.STAR:
                m_keys &= ~KEY.STAR;
                break;

            case JAVA_KEY_CODE.NUM_0:
                m_keys &= ~KEY.NUM_0;
                break;

            case JAVA_KEY_CODE.NUM_1:
                m_keys &= ~KEY.NUM_1;
                break;

            case JAVA_KEY_CODE.NUM_2:
                m_keys &= ~KEY.NUM_2;
                break;

            case JAVA_KEY_CODE.NUM_3:
                m_keys &= ~KEY.NUM_3;
                break;

            case JAVA_KEY_CODE.NUM_4:
                m_keys &= ~KEY.NUM_4;
                break;

            case JAVA_KEY_CODE.NUM_5:
                m_keys &= ~KEY.NUM_5;
                break;

            case JAVA_KEY_CODE.NUM_6:
                m_keys &= ~KEY.NUM_6;
                break;

            case JAVA_KEY_CODE.NUM_7:
                m_keys &= ~KEY.NUM_7;
                break;

            case JAVA_KEY_CODE.NUM_8:
                m_keys &= ~KEY.NUM_8;
                break;

            case JAVA_KEY_CODE.NUM_9:
                m_keys &= ~KEY.NUM_9;
                break;
        }
        m_iKeyCheckCounter = 2;
    }
}