using System;

namespace GbaMonoGame.Rayman3.J2ME;

public partial class Game
{
    public const int CAMERA_MAX_SPEED = 8;

    public Actor pFocusActor { get; set; }

    // Custom helpers
    private int ScaleXValue(int value) => value +
                                          (IntegerResolution.X - GameMidlet.OriginalIntegerResolution.X) / 2;
    private int ScaleYValue(int value) => value +
                                          (IntegerResolution.Y - GameMidlet.OriginalIntegerResolution.Y) / 2;

    public void Camera_Tick()
    {
        Actor pActor = pFocusActor;
        if (pActor == null || pRayman == null)
        {
            if (m_bBackgroundUsed)
                setCameraPos(0, 0, null);
            return;
        }

        int cameraYOffset = ScaleYValue(213);

        int tx = (int)((pActor.x >> 8) + -(IntegerResolution.X * (pRayman.xDirectionConfirmed ? 1 : 2)) / 3);
        int ty = (int)((pActor.y >> 8) + -cameraYOffset);

        switch (pRayman.anim.curAction)
        {
            case 10:
            case 11:
                if (pRayman.y > pRayman.V[16] << 8)
                    ty = (int)(pActor.y >> 8) + pActor.colBox.Top;
                else
                    ty = pActor.V[16] + -cameraYOffset;
                break;
            
            case 7:
            case 8:
                ty = pActor.V[16] + -cameraYOffset;
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
                ty = (int)((pActor.y >> 8) + -cameraYOffset);
                break;
            
            case 3:
            case 4:
            case 5:
                ty = (int)(pActor.y >> 8) + pActor.colBox.Top - ScaleYValue(80);
                break;
            
            case 9:
            case 34:
            case 35:
            case 36:
                if (pRayman.V[7] == -1)
                {
                    if (pActor.dy <= 0)
                        ty = (int)((pActor.y >> 8) + -cameraYOffset - 16L);
                    else if (pActor.dy > 0)
                        ty = (int)((pActor.y >> 8) + -cameraYOffset + 16L);

                    tx = (int)((pActor.x >> 8) - IntegerResolution.X / 2);
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
                        ty = pActor.V[16] + -cameraYOffset;
                    else if (pActor.yDirectionConfirmed == 1)
                        ty = pActor.V[16] + pActor.colBox.Top - 16;
                    else if (pActor.yDirectionConfirmed == -1)
                        ty = pActor.V[16] - IntegerResolution.Y;
                }
                break;

            case 27:
                tx = pActor.V[13] + ((pActor.colBox.Left + pActor.colBox.Right) >> 1) + -(IntegerResolution.X * (pRayman.xDirectionConfirmed ? 1 : 2)) / 3;
                ty = pActor.V[14] + ((pActor.colBox.Top + pActor.colBox.Bottom) >> 1) + -cameraYOffset;
                break;
        }

        int dx = (tx - m_iBackgroundX) >> 1;
        int dy = ty - m_iBackgroundY;

        if (pRayman.anim.curAction != 11)
            dy >>= 1;
        if (pRayman.anim.curAction == 9)
            dy >>= 1;
        
        if (dx < -CAMERA_MAX_SPEED && pRayman.anim.curAction != 27)
            dx = -CAMERA_MAX_SPEED;
        else if (dx > CAMERA_MAX_SPEED && pRayman.anim.curAction != 27)
            dx = CAMERA_MAX_SPEED;

        if (dy < -CAMERA_MAX_SPEED)
            dy = -CAMERA_MAX_SPEED;
        else if (dy > CAMERA_MAX_SPEED)
            dy = CAMERA_MAX_SPEED;

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
        // Optionally fix determining if an actor is visible by checking the render box rather than collision box
        if (Engine.Settings.Local.J2ME.FixBugs)
            return pActor.GameObj_isCollideBox(pActor.anim.GetRenderBox(pActor.stateFlag & (ACTOR_STATE.FLIP_X | ACTOR_STATE.FLIP_Y)), m_iBackgroundX, m_iBackgroundY, IntegerResolution.X, IntegerResolution.Y);
        else
            return pActor.GameObj_isCollideBox(pActor.colBox, m_iBackgroundX, m_iBackgroundY, IntegerResolution.X, IntegerResolution.Y);
    }
}