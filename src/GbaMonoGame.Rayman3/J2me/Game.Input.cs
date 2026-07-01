using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame.Rayman3.J2me;

public partial class Game
{
    public long lStartMillForKeyDelay { get; set; }
    public GAME_KEY currentKey { get; set; }
    public GAME_KEY pressedKey { get; set; }
    public GAME_KEY releasedKey { get; set; }
    public int m_iKeyCheckCounter { get; set; }
    public GbaInput m_keys { get; set; }

    public void Input_Tick()
    {
        // Game's priority:
        // - SOFTKEY2
        // - SOFTKEY1
        // - POUND
        // - STAR
        // - NUM_5
        // - NUM_9 -> NUM_0

        GAME_KEY nKeyCode;
        if ((m_keys & GbaInput.Start) != 0)
        {
            nKeyCode = GAME_KEY.CONFIRM_YES | GAME_KEY.PAUSE;
        }
        else if ((m_keys & GbaInput.Select) != 0)
        {
            nKeyCode = GAME_KEY.DEBUG;
        }
        else if ((m_keys & GbaInput.R) != 0)
        {
            nKeyCode = GAME_KEY.CONFIRM_YES;
        }
        else if ((m_keys & GbaInput.L) != 0)
        {
            nKeyCode = GAME_KEY.CONFIRM_NO;
        }
        else if ((m_keys & GbaInput.B) != 0)
        {
            nKeyCode = GAME_KEY.ACTION | GAME_KEY.CONFIRM_NO;
        }
        else if ((m_keys & GbaInput.Down) != 0)
        {
            if ((m_keys & GbaInput.Left) != 0)
                nKeyCode = GAME_KEY.DOWN_LEFT;
            else if ((m_keys & GbaInput.Right) != 0)
                nKeyCode = GAME_KEY.DOWN_RIGHT;
            else
                nKeyCode = GAME_KEY.DOWN;
        }
        else if ((m_keys & GbaInput.Right) != 0)
        {
            if ((m_keys & (GbaInput.Up | GbaInput.A)) != 0)
                nKeyCode = GAME_KEY.UP_RIGHT;
            else if ((m_keys & GbaInput.Down) != 0)
                nKeyCode = GAME_KEY.DOWN_RIGHT;
            else
                nKeyCode = GAME_KEY.RIGHT;
        }
        else if ((m_keys & GbaInput.Left) != 0)
        {
            if ((m_keys & (GbaInput.Up | GbaInput.A)) != 0)
                nKeyCode = GAME_KEY.UP_LEFT;
            else if ((m_keys & GbaInput.Down) != 0)
                nKeyCode = GAME_KEY.DOWN_LEFT;
            else
                nKeyCode = GAME_KEY.LEFT;
        }
        else if ((m_keys & (GbaInput.Up | GbaInput.A)) != 0)
        {
            if ((m_keys & GbaInput.Left) != 0)
                nKeyCode = GAME_KEY.UP_LEFT;
            else if ((m_keys & GbaInput.Right) != 0)
                nKeyCode = GAME_KEY.UP_RIGHT;
            else
                nKeyCode = GAME_KEY.UP;

            if ((m_keys & GbaInput.A) != 0)
                nKeyCode |= GAME_KEY.CONFIRM_YES;
        }
        else
        {
            nKeyCode = GAME_KEY.NONE;
        }

        pressedKey = (GAME_KEY)(((short)currentKey ^ 0xFFFFFFFF) & (int)nKeyCode);
        releasedKey = (GAME_KEY)((short)currentKey & ((int)nKeyCode ^ 0xFFFFFFFF));
        currentKey = nKeyCode;
        m_iKeyCheckCounter--;
        
        // Pause
        if (m_gameFrame_curLevel >= LEVEL_WORLD_MAP && 
            (pressedKey & GAME_KEY.PAUSE) != 0 && // NOTE: Original game checks CONFIRM_NO and CONFIRM_YES
            !m_gameFrame_paused && 
            curState == SYS_FRAME_STATE.GAME)
        {
            m_gameFrame_paused = true;
            Menu_SetCurrentPage(MENU_PAGE.PAUSE);
            pressedKey = GAME_KEY.NONE;
            StopSound();
        }

        // Custom cheat menu
        if (Engine.Settings.Active.Tweaks.AllowCheatMenu &&
            m_gameFrame_curLevel >= LEVEL_WORLD_MAP && 
            (pressedKey & GAME_KEY.DEBUG) != 0 && 
            !m_gameFrame_paused && 
            curState == SYS_FRAME_STATE.GAME)
        {
            m_gameFrame_paused = true;
            Menu_SetCurrentPage(MENU_PAGE.CHEAT);
            pressedKey = GAME_KEY.NONE;
            StopSound();
        }
    }

    public void keyPressed(GbaInput keyCode)
    {
        if (m_gameFrame_prevState == GAME_FRAME_STATE.LOADING && 
            m_gameFrame_curState == GAME_FRAME_STATE.DEFAULT && 
            System.currentTimeMillis() - lStartMillForKeyDelay < 200) 
            return;

        m_keys |= keyCode;

        if (m_gameFrame_paused || m_gameFrame_curLevel < LEVEL_WORLD_MAP)
            Menu_DoAI();
        m_iKeyCheckCounter = 2;
        m_bUpdateStatus = false;
    }

    public void keyReleased(GbaInput keyCode)
    {
        m_keys &= ~keyCode;
        m_iKeyCheckCounter = 2;
    }
}