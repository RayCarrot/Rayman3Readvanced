using System;

namespace GbaMonoGame.Rayman3.J2ME;

public partial class Game
{
    public sbyte[] m_byStatusDisplay { get; } = new sbyte[3];
    public sbyte[] m_byStatusDisplayCounter { get; } = new sbyte[3];
    public bool m_bUpdateStatus { get; set; }
    public int[][] s_iTutorialArray { get; } = new int[][]
    {
        [0x3001DE, 3],
        [0x300016, 3],
        [0x300047, 5],
        [0x30019C, 3],
        [0x300127, 4],
        [0x30020B, 4]
    };

    public void Status_ShowLock()
    {
        Status_ShowAll();
        m_bUpdateStatus = true;
    }

    public void Status_ShowAll()
    {
        Status_Show(0);
        Status_Show(2);
        Status_Show(1);
    }

    public void Status_Show(int statusType)
    {
        switch (m_byStatusDisplay[statusType])
        {
            case 1:
                return;

            case 3:
                m_byStatusDisplay[statusType] = 1;
                goto case 2;
            
            case 2:
                m_byStatusDisplayCounter[statusType] = 0;
                break;
        }
        
        m_byStatusDisplayCounter[statusType] = 0;
        m_byStatusDisplay[statusType] = 1;
    }

    public int Status_DrawNumber(int iNumber, int iX, int iY, bool bRightAlign)
    {
        int iCurrentX = iX;
        if (bRightAlign)
        {
            do
            {
                int iToken = iNumber % 10;
                iCurrentX -= Actor.aniData[26].modules[0 + iToken].Width;
                Actor.drawModule(GameMidlet.Instance_Game.m_gameMenu_pData, 0 + iToken, iCurrentX, iY, 0, GameMidlet.Instance_Game.g_graBackBuffer);
                iNumber /= 10;
            } while (iNumber != 0);
        }
        else
        {
            bool bStarted = false;
            int iDivisor = 1000;
            while (true)
            {
                int i = iNumber / iDivisor;
                if (i != 0 || bStarted || iDivisor == 1)
                {
                    bStarted = true;
                    Actor.drawModule(GameMidlet.Instance_Game.m_gameMenu_pData, 0 + i, iCurrentX, iY, 0, GameMidlet.Instance_Game.g_graBackBuffer);
                    iCurrentX += Actor.aniData[26].modules[0 + i].Width;
                    iNumber -= i * iDivisor;
                }
                if (iDivisor == 1)
                    break;
                iDivisor /= 10;
            }
        }
        return iCurrentX;
    }

    public void Status_Draw()
    {
        bool bBackgroundState = m_bBackgroundUsed;
        for (sbyte statusType = 0; statusType < 3; statusType = (sbyte)(statusType + 1))
        {
            if (m_bUpdateStatus)
                if (m_byStatusDisplay[statusType] != 1)
                    m_byStatusDisplay[statusType] = 2;
            if (m_byStatusDisplay[statusType] != 0)
            {
                int iX, iY, iPosY = 0;
                m_byStatusDisplayCounter[statusType] = (sbyte)(m_byStatusDisplayCounter[statusType] + 1);
                switch (m_byStatusDisplay[statusType])
                {
                    case 1:
                        iPosY = -((10 - m_byStatusDisplayCounter[statusType]) * Actor.aniData[26].modules[15].Height) / 10;
                        if (m_byStatusDisplayCounter[statusType] > 10)
                        {
                            m_byStatusDisplayCounter[statusType] = 0;
                            m_byStatusDisplay[statusType] = (sbyte)(m_byStatusDisplay[statusType] + 1);
                        }
                        break;

                    case 3:
                        iPosY = -(m_byStatusDisplayCounter[statusType] * Actor.aniData[26].modules[15].Height) / 10;
                        if (m_byStatusDisplayCounter[statusType] > 10)
                        {
                            m_byStatusDisplayCounter[statusType] = 0;
                            m_byStatusDisplay[statusType] = (sbyte)(m_byStatusDisplay[statusType] + 1);
                        }
                        break;
                    
                    case 2:
                        if (m_byStatusDisplayCounter[statusType] > 30)
                        {
                            m_byStatusDisplayCounter[statusType] = 0;
                            m_byStatusDisplay[statusType] = (sbyte)(m_byStatusDisplay[statusType] + 1);
                        }
                        break;
                }
                m_byStatusDisplay[statusType] = (sbyte)(m_byStatusDisplay[statusType] % 4);
                m_bBackgroundUsed = false;
                switch (statusType)
                {
                    case 0:
                        iX = 3;
                        iY = 3 + iPosY;
                        Actor.drawModule(GameMidlet.Instance_Game.m_gameMenu_pData, 15, iX, iY, 0, GameMidlet.Instance_Game.g_graBackBuffer);
                        iX += Actor.aniData[26].modules[15].Width + 3;
                        Actor.drawModule(GameMidlet.Instance_Game.m_gameMenu_pData, 17 + GameMidlet.Instance_Game.m_gameFrame_nEnergy, iX, iY, 0, GameMidlet.Instance_Game.g_graBackBuffer);
                        iY += Actor.aniData[26].modules[17].Height + 3 + Actor.aniData[26].modules[0].Height - Actor.aniData[26].modules[11].Height;
                        Actor.drawModule(GameMidlet.Instance_Game.m_gameMenu_pData, 11, iX, iY, 0, GameMidlet.Instance_Game.g_graBackBuffer);
                        iX += Actor.aniData[26].modules[11].Width + 3;
                        iY += Actor.aniData[26].modules[11].Height - Actor.aniData[26].modules[0].Height;
                        Status_DrawNumber(GameMidlet.Instance_Game.m_gameFrame_nLife, iX, iY, false);
                        break;
                    
                    case 2:
                        iX = Resolution.X - 3 - Actor.aniData[26].modules[16].Width;
                        iY = 3 + iPosY;
                        Actor.drawModule(GameMidlet.Instance_Game.m_gameMenu_pData, 16, iX, iY, 0, GameMidlet.Instance_Game.g_graBackBuffer);
                        iX -= 3;
                        iX = Status_DrawNumber(GameMidlet.Instance_Game.s_iLumsTotal & 0xFF, iX, iY, true) - Actor.aniData[26].modules[10].Width;
                        Actor.drawModule(GameMidlet.Instance_Game.m_gameMenu_pData, 10, iX, iY, 0, GameMidlet.Instance_Game.g_graBackBuffer);
                        Status_DrawNumber(GameMidlet.Instance_Game.s_iLumsTaken & 0xFF, iX, 3 + iPosY, true);
                        break;
                    
                    case 1:
                        iY = 3 + iPosY;
                        switch (m_byStatusDisplay[2])
                        {
                            case 1:
                                iY += m_byStatusDisplayCounter[2] * Actor.aniData[26].modules[16].Height / 10;
                                break;
                            case 3:
                                iY += (10 - m_byStatusDisplayCounter[2]) * Actor.aniData[26].modules[16].Height / 10;
                                break;
                            case 2:
                                iY += Actor.aniData[26].modules[16].Height;
                                break;
                        }
                        iX = Resolution.X - 3 - Actor.aniData[26].modules[12].Width;
                        Actor.drawModule(GameMidlet.Instance_Game.m_gameMenu_pData, 12, iX, iY, 0, GameMidlet.Instance_Game.g_graBackBuffer);
                        iX -= 3;
                        iX = Status_DrawNumber(GameMidlet.Instance_Game.s_iCageTotal, iX, iY, true) - Actor.aniData[26].modules[10].Width;
                        Actor.drawModule(GameMidlet.Instance_Game.m_gameMenu_pData, 10, iX, iY, 0, GameMidlet.Instance_Game.g_graBackBuffer);
                        Status_DrawNumber(GameMidlet.Instance_Game.s_iCageOpened, iX, iY, true);
                        break;
                }
            }
        }
        m_bBackgroundUsed = bBackgroundState;
    }

    public void IngameTextbox_Draw()
    {
        bool bBackgroundState = m_bBackgroundUsed;
        m_bBackgroundUsed = false;
        if (pRayman.actorReference is { objType: OBJECT_TYPE.LEVEL_SIGN })
        {
            if (pRayman.actorReference.anim.curAction == 2 && pRayman.anim.curAction != 37 && pRayman.anim.curAction != 9)
            {
                string str1, str2;
                int levelID = pRayman.actorReference.V[0];
                string sTitle = RM.GetString(0x2D007F) + levelID;
                
                if (s_synopsis[levelID, 1] != -1)
                    str1 = $" {s_synopsis[levelID, 0]}/{s_synopsis[levelID, 1]}";
                else
                    str1 = RM.GetString(0x2D0086);

                if (s_synopsis[levelID, 3] != -1)
                    str2 = $" {s_synopsis[levelID, 2]}/{s_synopsis[levelID, 3]}";
                else
                    str2 = RM.GetString(0x2D0086);
                
                int iW = 230;
                int iH = 23 + Actor.aniData[26].modules[12].Height + 2 - 5;
                int iX = 5;
                int iY = 320 - iH - 240;

                g_graBackBuffer.setClip(iX, iY, iW + 1, iH + 1);
                DrawParchment(iX, iY, iW, iH, true);
                iX += 2;
                iY += 2;
                iW -= 4;
                iH -= 4;
                g_graBackBuffer.setColor(0x330099);
                int iPosX = 120;
                int iPosY = iY - 2;
                // TODO: Implement clipping text to fit box
                g_graBackBuffer.setClip(iX, iY, iW, iH);
                string sSignTip = RM.GetString(0x2D0074);
                if ((sbyte)levelID == m_gameFrame_unlockedLevel)
                {
                    if (m_iGlobalTicker % 20 < 10)
                        g_graBackBuffer.drawString(sTitle, iPosX, iPosY, ANCHOR.HCENTER | ANCHOR.TOP);
                    else
                        g_graBackBuffer.drawString(sSignTip, iPosX, iPosY, ANCHOR.HCENTER | ANCHOR.TOP);
                }
                else
                {
                    g_graBackBuffer.drawString(sTitle, iPosX, iPosY, ANCHOR.HCENTER | ANCHOR.TOP);
                }
                iPosX = iX + 5;
                iPosY = iY + iH - Actor.aniData[26].modules[16].Height;
                Actor.drawModule(m_gameMenu_pData, 16, iPosX, iPosY, 0, g_graBackBuffer);
                iPosX += Actor.aniData[26].modules[16].Width;
                iPosY = iY + iH;
                g_graBackBuffer.setClip(iX, iY, iW, iH);
                g_graBackBuffer.drawString(str1, iPosX, iPosY, ANCHOR.LEFT | ANCHOR.BOTTOM);
                iPosX = 130;
                iPosY = iY + iH - Actor.aniData[26].modules[12].Height;
                Actor.drawModule(m_gameMenu_pData, 12, iPosX, iPosY, 0, g_graBackBuffer);
                iPosX += Actor.aniData[26].modules[12].Width;
                iPosY = iY + iH;
                g_graBackBuffer.setClip(iX, iY, iW, iH);
                g_graBackBuffer.drawString(str2, iPosX, iPosY, ANCHOR.LEFT | ANCHOR.BOTTOM);
            }
            else if (pRayman.actorReference.anim.curAction == 3)
            {
                int tutorialID = pRayman.actorReference.V[0];
                int iID = s_iTutorialArray[tutorialID][0];
                int iNbrStrings = s_iTutorialArray[tutorialID][1];
                if (m_iGlobalTicker >= (iNbrStrings + 3 - 1) * 21)
                    m_iGlobalTicker = 0;
                int iSkipLines = iNbrStrings <= 2 ? 0 : m_iGlobalTicker / 21 - 2;
                int i;
                for (i = 0; i < iSkipLines; i++)
                    iID = RM.NextStringID(iID);
                string[] sTutorial = new string[3];
                for (i = 0; i < 3; i++)
                {
                    if (0 <= iSkipLines + i && iSkipLines + i < iNbrStrings && iID != -1)
                    {
                        sTutorial[i] = RM.GetString(iID);
                        iID = RM.NextStringID(iID);
                    }
                    else
                    {
                        sTutorial[i] = String.Empty;
                    }
                }

                int c = 230;
                sbyte b2 = 41;
                sbyte b1 = 5;
                int k = 320 - b2 - 240;
                g_graBackBuffer.setClip(b1, k, c, b2);
                DrawParchment(b1, k, c, b2, true);
                int m = k + (iNbrStrings <= 2 ? 2 : -(m_iGlobalTicker % 21));
                g_graBackBuffer.setColor(3342489);
                for (int j = 0; j < 3; j++)
                {
                    g_graBackBuffer.drawString(sTutorial[j], 120, m, ANCHOR.HCENTER | ANCHOR.TOP);
                    m += 21;
                }
            }
        }

        m_bBackgroundUsed = bBackgroundState;
    }
}