using BinarySerializer.Gameloft.J2me;
using GbaMonoGame.TgxEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GbaMonoGame.Rayman3.J2me;

public partial class Game
{
    public const int TILE_SIZE = 16;

    // Custom
    public GfxScreen BackgroundScreen { get; set; }
    public GfxScreen CollisionScreen { get; set; }

    // A lot of these are unused in Readvanced since we don't draw the background in the fast mode
    public short m_sBackgroundHeight { get; set; }
    public short m_sBackgroundWidth { get; set; }
    public int m_iBackgroundX { get; set; }
    public int m_iBackgroundY { get; set; }
    public int m_iBackgroundTileCountX { get; set; }
    public int m_iBackgroundLeftUpX { get; set; }
    public int m_iBackgroundLeftUpY { get; set; }
    public bool m_bBackgroundFastMode { get; set; }
    public int m_iBackgroundPrevMapX0 { get; set; }
    public int m_iBackgroundPrevMapY0 { get; set; }
    public int m_iBackgroundPrevMapX1 { get; set; }
    public int m_iBackgroundPrevMapY1 { get; set; }
    public Texture2D m_imgBackground  { get; set; } 
    public Graphics m_graBackground { get; set; }
    public int m_iImageResID { get; set; }
    public int m_iBackgroundDataIndex { get; set; }
    public bool m_bBackgroundUsed { get; set; }

    public void CreateTiledBackground(int iImageResID, int iBackgroundDataIndex)
    {
        m_iBackgroundLeftUpX = 0;
        m_iBackgroundLeftUpY = 0;
        m_iBackgroundDataIndex = iBackgroundDataIndex;
        m_iImageResID = iImageResID;
        m_sBackgroundWidth = (short)RM.DirectVQ_GetWidth(m_iBackgroundDataIndex);
        m_sBackgroundHeight = (short)RM.DirectVQ_GetHeight(m_iBackgroundDataIndex);
        m_iBackgroundTileCountX = RM.GetImage(m_iImageResID).Width / 16;
        m_bBackgroundFastMode = false;
        m_bBackgroundUsed = true;
        Fast_Init();

        // Custom - create a single texture for the entire map
        Texture2D tileSet = RM.GetImage(m_iImageResID);
        Color[] tileSetPixels = new Color[tileSet.Width * tileSet.Height];
        tileSet.GetData(tileSetPixels);
        
        Texture2D bgTexture = new(Engine.Assets.GraphicsDevice, m_sBackgroundWidth * TILE_SIZE, m_sBackgroundHeight * TILE_SIZE); // TODO: Dispose
        Color[] pixels = new Color[bgTexture.Width * bgTexture.Height];
        for (int y = 0; y < bgTexture.Height; y += TILE_SIZE)
        {
            for (int x = 0; x < bgTexture.Width; x += TILE_SIZE)
            {
                int id = RM.DirectTwinVQ_Read(m_iBackgroundDataIndex, x / TILE_SIZE, y / TILE_SIZE, 0) & 0xFF;
                int tileSetX = (id % m_iBackgroundTileCountX) * TILE_SIZE;
                int tileSetY = (id / m_iBackgroundTileCountX) * TILE_SIZE;

                for (int yy = 0; yy < TILE_SIZE; yy++)
                {
                    for (int xx = 0; xx < TILE_SIZE; xx++)
                    {
                        pixels[(y + yy) * bgTexture.Width + (x + xx)] = tileSetPixels[(tileSetY + yy) * tileSet.Width + (tileSetX + xx)];
                    }
                }
            }
        }
        bgTexture.SetData(pixels);

        BackgroundScreen = new GfxScreen(0)
        {
            Priority = 1,
            RenderOptions = Graphics.RenderOptions,
            IsEnabled = true,
            Renderer = new TextureScreenRenderer(bgTexture)
        };
        Gfx.AddScreen(BackgroundScreen);

        // TODO: Debug only
        byte[] collisionMap = new byte[m_sBackgroundWidth * m_sBackgroundHeight];
        for (int y = 0; y < m_sBackgroundHeight; y++)
        {
            for (int x = 0; x < m_sBackgroundWidth; x++)
            {
                PHB_TYPE physicalType = PF_getPHBI(x, y);
                collisionMap[y * m_sBackgroundWidth + x] = (byte)physicalType;
            }
        }

        CollisionScreen = new GfxScreen(-1)
        {
            Priority = 0,
            RenderOptions = Graphics.RenderOptions,
            IsEnabled = true,
            Renderer = new CollisionMapScreenRenderer(
                collisionTileSet: Engine.Assets.FrameContentManager.Load<Texture2D>(Assets.J2me.CollisionTileSet),
                tileSize: TILE_SIZE,
                width: m_sBackgroundWidth, 
                height: m_sBackgroundHeight, 
                collisionMap: collisionMap)
        };
        Gfx.AddScreen(CollisionScreen);
    }

    public PHB_TYPE PF_getPHBI(int x, int y)
    {
        if (y < -6 || x < 0 || x >= m_sBackgroundWidth)
            return PHB_TYPE.SOLID;
        if (y < 0)
            return PHB_TYPE.NONE;
        if (y >= m_sBackgroundHeight)
            return PHB_TYPE.SOLID;
        return (PHB_TYPE)RM.DirectTwinVQ_Read(m_iBackgroundDataIndex, x, y, 1);
    }

    public int PF_getSlopeDisp(PHB_TYPE phb, int off)
    {
        if (phb == PHB_TYPE.SOLID_HALF)
        {
            return RM.GetData<SlopeDisplacementsResource>(RESOURCE_ID_DATA_SLOPE_DISPLACEMENTS).Displacements[6][off] << 4;
        }
        else
        {
            int whichIndex = ((sbyte)phb - 4) % 6;
            return RM.GetData<SlopeDisplacementsResource>(RESOURCE_ID_DATA_SLOPE_DISPLACEMENTS).Displacements[whichIndex][off] << 4;
        }
    }

    public void Fast_Init()
    {
        m_iBackgroundPrevMapX0 = -1;
        m_iBackgroundPrevMapY0 = -1;
    }

    public void setFastMode(bool mode, float w, float h)
    {
        if (mode)
        {
            m_bBackgroundFastMode = true;
            if (m_imgBackground == null)
            {
                // Unused in Readvanced since we don't draw the background in the fast mode
                // int bw = (w & 0xF) != 0 ? (int)(w & 0xFFFFFFF0) + TILE_SIZE : w + TILE_SIZE;
                // int bh = (h & 0xF) != 0 ? (int)(h & 0xFFFFFFF0) + TILE_SIZE : h + TILE_SIZE;
                // m_imgBackground = Image.createImage(bw, bh);
                // m_graBackground = m_imgBackground.getGraphics();
            }
        }
        else
        {
            m_bBackgroundFastMode = false;
            m_graBackground = null;
            m_imgBackground = null;
        }
    }

    public void mappedDraw(int alignedX0, int alignedY0, int alignedX1, int alignedY1, int bufW, int bufH)
    {
        int _y = alignedY0 % bufH;
        for (int y = alignedY0; y <= alignedY1; y += TILE_SIZE)
        {
            int _x = alignedX0 % bufW;
            for (int x = alignedX0; x <= alignedX1; x += TILE_SIZE)
            {
                int id = RM.DirectTwinVQ_Read(m_iBackgroundDataIndex, x >> 4, y >> 4, 0);
                m_graBackground.setClip(_x, _y, TILE_SIZE, TILE_SIZE);
                m_graBackground.drawImage(RM.GetImage(m_iImageResID), _x - ((id % m_iBackgroundTileCountX) << 4), _y - ((id / m_iBackgroundTileCountX) << 4), ANCHOR.LEFT | ANCHOR.TOP);
                _x += TILE_SIZE;

                if (_x >= bufW)
                    _x = 0;
            }
            _y += TILE_SIZE;

            if (_y >= bufH)
                _y = 0;
        }
    }

    public void copyFromBackImage(Graphics g, float modX, float modY, float screenX, float screenY)
    {
        g.drawImage(m_imgBackground, screenX - modX, screenY - modY, ANCHOR.LEFT | ANCHOR.TOP);
    }

    public void fastDraw(Graphics g, float cw, float ch)
    {
        // Readvanced code
        if (true)
        {
            int cx = m_iBackgroundX;
            int cy = m_iBackgroundY;
            BackgroundScreen.Offset = new Vector2(cx, cy);
            CollisionScreen.Offset = new Vector2(cx, cy);
            CollisionScreen.IsEnabled = !m_gameFrame_paused;
        }
        // Original game code
        else if (false)
        {
            int cx = m_iBackgroundX;
            int cy = m_iBackgroundY;
            int bufW = m_imgBackground.Width;
            int bufH = m_imgBackground.Height;
            int alignedX0 = (int)(cx & 0xFFFFFFF0);
            int alignedY0 = (int)(cy & 0xFFFFFFF0);
            int alignedX1 = (int)((cx + bufW - TILE_SIZE) & 0xFFFFFFF0);
            int alignedY1 = (int)((cy + bufH - TILE_SIZE) & 0xFFFFFFF0);
            if (m_iBackgroundPrevMapX0 < 0)
            {
                mappedDraw(alignedX0, alignedY0, alignedX1, alignedY1, bufW, bufH);
                m_iBackgroundPrevMapX0 = alignedX0;
                m_iBackgroundPrevMapY0 = alignedY0;
                m_iBackgroundPrevMapX1 = alignedX1;
                m_iBackgroundPrevMapY1 = alignedY1;
            }
            else
            {
                if (m_iBackgroundPrevMapX0 != alignedX0)
                {
                    int i, j;
                    if (m_iBackgroundPrevMapX0 < alignedX0)
                    {
                        i = m_iBackgroundPrevMapX1 + TILE_SIZE;
                        j = alignedX1;
                    }
                    else
                    {
                        i = alignedX0;
                        j = m_iBackgroundPrevMapX0 - TILE_SIZE;
                    }
                    mappedDraw(i, alignedY0, j, alignedY1, bufW, bufH);
                    m_iBackgroundPrevMapX0 = alignedX0;
                    m_iBackgroundPrevMapX1 = alignedX1;
                }
                if (m_iBackgroundPrevMapY0 != alignedY0)
                {
                    int i, j;
                    if (m_iBackgroundPrevMapY0 < alignedY0)
                    {
                        i = m_iBackgroundPrevMapY1 + TILE_SIZE;
                        j = alignedY1;
                    }
                    else
                    {
                        i = alignedY0;
                        j = m_iBackgroundPrevMapY0 - TILE_SIZE;
                    }
                    mappedDraw(alignedX0, i, alignedX1, j, bufW, bufH);
                    m_iBackgroundPrevMapY0 = alignedY0;
                    m_iBackgroundPrevMapY1 = alignedY1;
                }
            }
            int modX0 = cx % bufW;
            int modY0 = cy % bufH;
            float modX1 = (cx + cw) % bufW;
            float modY1 = (cy + ch) % bufH;
            copyFromBackImage(g, modX0, modY0, 0, 0);
            if (modX1 > modX0)
            {
                if (modY1 <= modY0)
                    copyFromBackImage(g, modX0, 0, 0, ch - modY1);
            }
            else
            {
                copyFromBackImage(g, 0, modY0, cw - modX1, 0);
                if (modY1 <= modY0)
                {
                    copyFromBackImage(g, modX0, 0, 0, ch - modY1);
                    copyFromBackImage(g, 0, 0, cw - modX1, ch - modY1);
                }
            }
        }
    }
}