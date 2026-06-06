using System;

namespace GbaMonoGame.Rayman3.J2ME;

// TODO: Adjust for widescreen
public partial class Game
{
    public Actor pFocusActor { get; set; }

    public void Camera_Tick()
    {
        Actor pActor = pFocusActor;
        if (pActor == null || pRayman == null)
        {
            if (m_bBackgroundUsed)
                setCameraPos(0, 0, null);
            return;
        }

        int tx = (int)((pActor.x >> 8) + -(IntegerResolution.X * (pRayman.xDirectionConfirmed ? 1 : 2)) / 3);
        int ty = (int)((pActor.y >> 8) + -213);

        switch (pRayman.anim.curAction)
        {
            case 10:
            case 11:
                if (pRayman.y > pRayman.V[16] << 8)
                    ty = (int)(pActor.y >> 8) + pActor.colBox.Top;
                else
                    ty = pActor.V[16] + -213;
                break;
            
            case 7:
            case 8:
                ty = pActor.V[16] + -213;
                break;
            
            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
            case 24:
            case 25:
                ty = (int)((pActor.y >> 8) + -213L);
                break;
            
            case 3:
            case 4:
            case 5:
                ty = (int)(pActor.y >> 8) + pActor.colBox.Top - 80;
                break;
            
            case 9:
            case 34:
            case 35:
            case 36:
                if (pRayman.V[7] == -1)
                {
                    if (pActor.dy <= 0)
                        ty = (int)((pActor.y >> 8) + -213L - 16L);
                    else if (pActor.dy > 0)
                        ty = (int)((pActor.y >> 8) + -213L + 16L);

                    tx = (int)((pActor.x >> 8) - 120L);
                    if (pActor.xDirectionConfirmed)
                        tx += 16;
                    else
                        tx -= 16;
                }
                else
                {
                    if (pRayman.y > (pRayman.V[16] << 8))
                        ty = (int)(pActor.y >> 8) + pActor.colBox.Top;
                    else if (pActor.yDirectionConfirmed == 0)
                        ty = pActor.V[16] + -213;
                    else if (pActor.yDirectionConfirmed == 1)
                        ty = pActor.V[16] + pActor.colBox.Top - 16;
                    else if (pActor.yDirectionConfirmed == -1)
                        ty = pActor.V[16] - IntegerResolution.Y;
                }
                break;

            case 27:
                tx = pActor.V[13] + ((pActor.colBox.Left + pActor.colBox.Right) >> 1) + -(IntegerResolution.X * (pRayman.xDirectionConfirmed ? 1 : 2)) / 3;
                ty = pActor.V[14] + ((pActor.colBox.Top + pActor.colBox.Bottom) >> 1) + -213;
                break;
        }

        int dx = (tx - m_iBackgroundX) >> 1;
        int dy = ty - m_iBackgroundY;

        if (pRayman.anim.curAction != 11)
            dy >>= 1;
        if (pRayman.anim.curAction == 9)
            dy >>= 1;
        
        if (dx < -8 && pRayman.anim.curAction != 27)
            dx = -8;
        else if (dx > 8 && pRayman.anim.curAction != 27)
            dx = 8;

        if (dy < -8)
            dy = -8;
        else if (dy > 8)
            dy = 8;

        setCameraPos(m_iBackgroundX + dx, m_iBackgroundY + dy, pRayman);
    }

    public void setCameraPos(int sx, int sy, Actor actor)
    {
        sx = Math.Min(sx, (m_sBackgroundWidth << 4) - IntegerResolution.X - 1);
        sy = Math.Min(sy, (m_sBackgroundHeight << 4) - IntegerResolution.Y - 1);

        if (actor != null && actor.anim.curAction != 27)
        {
            int posX = (int)(actor.x >> 8);
            int posY = (int)(actor.y >> 8);
        
            if (sx > posX + actor.colBox.Left)
                sx = posX + actor.colBox.Left;
            else if (sx + IntegerResolution.X < posX + actor.colBox.Right)
                sx = posX + actor.colBox.Right - IntegerResolution.X;

            if (sy + IntegerResolution.Y < posY + actor.colBox.Bottom)
                sy = posY + actor.colBox.Bottom - IntegerResolution.Y;

            sx = Math.Min(sx, (m_sBackgroundWidth << 4) - IntegerResolution.X - 1);
            sy = Math.Min(sy, (m_sBackgroundHeight << 4) - IntegerResolution.Y - 1);
        }

        if (sx < 0)
            sx = 0;
        if (sy < 0)
            sy = 0;
        
        m_iBackgroundX = sx;
        m_iBackgroundY = sy;
    }

    public bool Camera_IsVisible(Actor pActor)
    {
        return pActor.GameObj_isCollideBox(m_iBackgroundX, m_iBackgroundY, IntegerResolution.X, IntegerResolution.Y);
    }
}