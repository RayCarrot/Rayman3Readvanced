using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3.J2me;

public partial class Game
{
    public Game()
    {
        // Custom
        RM = new ResourceManager();

        Graphics = new Graphics();

        bSoundBegin = false;
        bEnableSound = false;
        bConfirm = true;
        bConfirmExit = true;
        bConfirmToMainMenu = false;
        // setFullScreenMode(true); // Irrelevant here
        RM.Initialize();
        SysFrame_PhysicalInitI();
        m_fontGeneral = Font.getFont(32, 1, 0);
        m_iPrevLevel = LEVEL_INVALID;
        m_bBackgroundUsed = false;
        InitSound();
        // NOTE: Here the game sets up the display and starts a new thread for the game loop, but we don't need to re-implement that here
        // Display.getDisplay(GameMidlet.Instance_Midlet).setCurrent((Displayable)this);
        // (new Thread(this)).start();
    }

    // Custom
    public ResourceManager RM { get; }
    public Graphics Graphics { get; set; }
    public Vector2 Resolution => Graphics.Resolution; // Use for rendering
    public Point IntegerResolution => Graphics.Resolution.ToFloorPoint(); // Use for game logic

    public Graphics g_graBackBuffer { get; set; }
    public bool painting { get; set; } // Unused in Readvanced since we're not multi-threading
    public int m_iGlobalTicker { get; set; }
    public GAME_STATE m_chGameState { get; set; } = GAME_STATE.ACTIVE;
    public Font m_fontGeneral { get; set; }
    public bool showDebug { get; set; }
    public long _nUpdateTimer { get; set; } // Unused in Readvanced

    public void drawImageEx(float dstx, float dsty, float w, float h, int iImageIndex, float sx, float sy, int flag)
    {
        if (m_bBackgroundUsed)
        {
            dstx -= m_iBackgroundX;
            dsty -= m_iBackgroundY;
        }

        g_graBackBuffer.setClip(dstx, dsty, w, h);

        // Normal
        if ((flag & 0x4000) == 0)
            g_graBackBuffer.drawImage(RM.GetImage(iImageIndex), dstx - sx, dsty - sy, ANCHOR.TOP | ANCHOR.LEFT);
        // Flip X
        else
            g_graBackBuffer.drawRegion(RM.GetImage(iImageIndex), sx, sy, w, h, TRANS.MIRROR, dstx, dsty, ANCHOR.TOP | ANCHOR.LEFT);
    }

    // Unused in Readvanced
    public void hideNotify()
    {
        // Suspend game
        GameMidlet.bSuspended = true;
        StopSound();
        currentKey = GAME_KEY.NONE;
        pressedKey = GAME_KEY.NONE;
        releasedKey = GAME_KEY.NONE;
        m_keys = KEY.NONE;
    }

    // Unused in Readvanced
    public void showNotify()
    {
        // Resume game
        GameMidlet.bSuspended = false;
        currentKey = GAME_KEY.NONE;
        pressedKey = GAME_KEY.NONE;
        releasedKey = GAME_KEY.NONE;
        m_keys = KEY.NONE;
        
        // Pause if in a level
        if (m_gameFrame_curLevel >= LEVEL_WORLD_MAP && curState == SYS_FRAME_STATE.GAME)
        {
            m_gameFrame_paused = true;
            Menu_SetCurrentPage(MENU_PAGE.PAUSE);
            pressedKey = GAME_KEY.NONE;
        }
    }

    public void repaint()
    {
        paint(Graphics);

        // Keep running the game loop until we're done loading since the game loads in several steps
        if (m_gameFrame_curState is GAME_FRAME_STATE.EXITING_LEVEL or GAME_FRAME_STATE.EXITED_LEVEL
                or GAME_FRAME_STATE.LOADING && curState == SYS_FRAME_STATE.GAME)
        {
            while (m_gameFrame_curState is GAME_FRAME_STATE.EXITING_LEVEL or GAME_FRAME_STATE.EXITED_LEVEL
                   or GAME_FRAME_STATE.LOADING && curState == SYS_FRAME_STATE.GAME)
            {
                Graphics.ClearSprites();
                paint(Graphics);
            }

            Graphics.ClearSprites();
            paint(Graphics);
        }

        Graphics.DrawGfx();
    }

    public void paint(Graphics graFrontBuffer)
    {
        // NOTE: This check will never be true in Readvanced since we're not multi-threading
        if (painting || GameMidlet.bSuspended)
            return;
        
        painting = true;
        m_iGlobalTicker++;
        
        if (m_iKeyCheckCounter > 0)
            Input_Tick();
        
        g_graBackBuffer = graFrontBuffer;
        
        if (SysFrame_doLoop() != 0)
            m_chGameState = GAME_STATE.EXITING;

        if ((pressedKey & GAME_KEY.STAR) != 0)
            showDebug = !showDebug;

        if (showDebug)
        {
            // Empty
        }

        painting = false;
    }

    public void start()
    {
        GameMidlet.bSuspended = false;
        repaint();
    }

    // Unused in Readvanced
    public void run()
    {
        while (true)
        {
            if (m_chGameState == GAME_STATE.EXITING)
            {
                GameFrame_SaveSound();
                break;
            }

            if (!painting)
            {
                repaint();
                // serviceRepaints();
            }

            while (System.currentTimeMillis() - _nUpdateTimer < 45L)
            {
                // Thread.yield();
            }

            _nUpdateTimer = System.currentTimeMillis();
        }

        // GameMidlet.Instance_Midlet.destroyApp(true);
    }
}