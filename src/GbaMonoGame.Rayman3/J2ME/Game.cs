using Microsoft.Xna.Framework;

namespace GbaMonoGame.Rayman3.J2ME;

public partial class Game
{
    public Game()
    {
        // Custom
        RM = new ResourceManager();
        Graphics = new Graphics(new Point(240, 320));

        bSoundBegin = false;
        bEnableSound = false;
        bConfirm = true;
        bConfirmExit = true;
        bConfirmToMainMenu = false;
        // setFullScreenMode(true); // Irrelevant here
        RM.Initialize();
        SysFrame_PhysicalInitI();
        m_fontGeneral = Font.getFont(32, 1, 0);
        m_iPrevLevel = -2;
        m_bBackgroundUsed = false;
        InitSound();
        // NOTE: Here the game sets up the display and starts a new thread for the game loop, but we don't need to re-implement that here
        // Display.getDisplay(GameMidlet.Instance_Midlet).setCurrent((Displayable)this);
        // (new Thread(this)).start();
    }

    // Custom
    public ResourceManager RM { get; }
    public Graphics Graphics { get; set; }
    public Point Resolution => Graphics.Resolution;

    public Graphics g_graBackBuffer { get; set; }
    public bool painting { get; set; } // Irrelevant in Readvanced since we're not multi-threading
    public static int m_iGlobalTicker { get; set; }
    public sbyte m_chGameState { get; set; } // TODO: Enum/constants
    public Font m_fontGeneral { get; set; }
    public bool showDebug { get; set; }
    public static long _nUpdateTimer { get; set; }

    public void drawImageEx(int dstx, int dsty, int w, int h, int iImageIndex, int sx, int sy, int flag)
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
        currentKey = GAME_KEY.None;
        pressedKey = GAME_KEY.None;
        releasedKey = GAME_KEY.None;
        m_keys = KEY.NONE;
    }

    // Unused in Readvanced
    public void showNotify()
    {
        // Resume game
        GameMidlet.bSuspended = false;
        currentKey = GAME_KEY.None;
        pressedKey = GAME_KEY.None;
        releasedKey = GAME_KEY.None;
        m_keys = KEY.NONE;
        
        // Pause if in a level
        if (m_gameFrame_curLevel >= 0 && curState == 3)
        {
            m_gameFrame_paused = true;
            Menu_SetCurrentPage(MENU_PAGE.PAUSE);
            pressedKey = GAME_KEY.None;
        }
    }

    public void repaint()
    {
        paint(Graphics);
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
            m_chGameState = 1;

        if ((pressedKey & GAME_KEY.Star) != 0)
            showDebug = !showDebug;

        if (showDebug)
        {
            // Empty
        }

        painting = false;
    }

    public void start()
    {
        if (GameMidlet.bSuspended)
            GameMidlet.bSuspended = false;

        repaint();
    }

    // Unused in Readvanced
    public void run()
    {
        while (true)
        {
            if (m_chGameState == 1)
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