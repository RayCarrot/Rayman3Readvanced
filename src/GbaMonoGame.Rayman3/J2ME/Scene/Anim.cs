using System;

namespace GbaMonoGame.Rayman3.J2ME;

public class Anim
{
    public OBJECT_TYPE type { get; set; }
    public int actionFrame { get; set; }
    public int frameId { get; set; }
    public int frameTick { get; set; }
    public int frameDuration { get; set; }
    public int oldAction { get; set; }
    public int curAction { get; set; }
    public int newAction { get; set; }
    public static ANIM_EVENT_FLAGS aniEvent_flag { get; set; }
    public static MM_TYPE aniEvent_mmtype { get; set; }
    public static short[] aniEvent_mmpar { get; } = new short[6];
    public static CollisionBox aniEvent_pColBoxData { get; set; }

    public void setFrame(int frame)
    {
        sbyte[] frameData = Actor.aniData[(sbyte)type].frames[frame];
        frameDuration = frameData[1] & 0xFF;
        frameTick = 0;
        aniEvent_flag |= ANIM_EVENT_FLAGS.LOADED_COLLISION_BOX;
        aniEvent_pColBoxData = new CollisionBox()
        {
            Left = frameData[2],
            Top = frameData[3],
            Right = frameData[4],
            Bottom = frameData[5],
        };
        frameId = frame;
    }

    public void build(OBJECT_TYPE t, int newAct)
    {
        type = t;
        newAction = newAct;
        initNewAction(true);
    }
    
    public void initNewAction(bool bInitMModel)
    {
        AnimData data = Actor.aniData[(sbyte)type];
        sbyte[] mmParam = data.mmParam[newAction];
        int nbFrames = data.actions[newAction][0] & 0xFF;
        aniEvent_flag = ANIM_EVENT_FLAGS.NONE;
        if (bInitMModel && (data.flag & ANIM_DATA_FLAGS.HAS_MECH_MODEL) != 0)
        {
            aniEvent_flag |= ANIM_EVENT_FLAGS.LOADED_MECH_MODEL;
            aniEvent_mmtype = (MM_TYPE)((mmParam[0] & 0xF0) >> 4);
            for (int i = 0; i < 6; i++)
            {
                if (i < mmParam.Length - 1)
                {
                    int param = mmParam[1 + i];
                    if (param < 0)
                        aniEvent_mmpar[i] = (short)-(-param << 5);
                    else
                        aniEvent_mmpar[i] = (short)(param << 5);
                }
            }
        }
        actionFrame = 0;
        curAction = newAction;

        if (nbFrames > 0)
            setFrame(data.actions[newAction][1] & 0xFF);
        else
            frameId = -1;
    }

    public void step(bool bInitMModel)
    {
        frameTick++;
        if (newAction != curAction)
        {
            initNewAction(bInitMModel);
        }
        else if (frameTick == frameDuration)
        {
            sbyte[] actionData = Actor.aniData[(sbyte)type].actions[newAction];
            actionFrame++;
            if (actionFrame == (actionData[0] & 0xFF))
                actionFrame = 0;
            if ((actionData[0] & 0xFF) > 0)
                setFrame(actionData[actionFrame + 1] & 0xFF);
        }
    }
    
    public void draw(int nx, int ny, ACTOR_STATE nflag)
    {
        if (frameId < 0)
            return;

        AnimData data = Actor.aniData[(sbyte)type];
        sbyte[] frameData = data.frames[frameId];
        int frameOffset = 0;
        int nbSprite = frameData[0] & 0xFF;
        
        if (type == OBJECT_TYPE.RAYMAN)
            Game.raymanAnim = 0;

        for (int i = 0; i < nbSprite; i++)
        {
            if (i == nbSprite - 1 && 
                type == OBJECT_TYPE.RAYMAN && 
                Game.pFist[0].anim.curAction != 0 && 
                Game.pRayman.anim.curAction != 10 && 
                Game.pRayman.anim.curAction != 36)
                break;

            bool flag = !(i == 0 && type == OBJECT_TYPE.RAYMAN && Game.pFist[1].anim.curAction != 0);
            sbyte[] pModule = data.modules[frameData[6 + frameOffset] & SByte.MaxValue];
            
            int nsx;
            int geflag;
            if ((nflag & ACTOR_STATE.FLIP_X) != 0)
            {
                nsx = nx - frameData[6 + frameOffset + 1] - (pModule[2] & 0xFF);
                geflag = frameData[6 + frameOffset + 0] < 0 ? 0 : 0x4000;
            }
            else
            {
                nsx = nx + frameData[6 + frameOffset + 1];
                geflag = frameData[6 + frameOffset + 0] < 0 ? 0x4000 : 0;
            }
            
            int nsy = ny + frameData[6 + frameOffset + 2];
            
            if (type == OBJECT_TYPE.RAYMAN)
                Game.raymanAnim = nsx;

            if (flag)
            {
                GameMidlet.Instance_Game.drawImageEx(
                    dstx: nsx, 
                    dsty: nsy, 
                    w: pModule[2] & 0xFF, 
                    h: pModule[3] & 0xFF, 
                    iImageIndex: data.resID, 
                    sx: pModule[0] & 0xFF, 
                    sy: pModule[1] & 0xFF, 
                    flag: geflag);
            }

            frameOffset += 3;
        }
    }
}