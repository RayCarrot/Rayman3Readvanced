using BinarySerializer.Gameloft.J2me;

namespace GbaMonoGame.Rayman3.J2me;

public class Anim
{
    public ACTOR_TYPE type { get; set; }
    public int actionFrame { get; set; }
    public int frameId { get; set; }
    public int frameTick { get; set; }
    public int frameDuration { get; set; }
    public int oldAction { get; set; }
    public int curAction { get; set; }
    public int newAction { get; set; }
    public static ANIM_EVENT_FLAGS aniEvent_flag { get; set; }
    public static MechModelType aniEvent_mmtype { get; set; }
    public static short[] aniEvent_mmpar { get; } = new short[6];
    public static Box aniEvent_pColBoxData { get; set; }

    public void setFrame(int frame)
    {
        AnimationFrame frameData = Actor.aniData[(sbyte)type].frames[frame];
        frameDuration = frameData.FrameDuration;
        frameTick = 0;
        aniEvent_flag |= ANIM_EVENT_FLAGS.LOADED_COLLISION_BOX;
        aniEvent_pColBoxData = new Box
        {
            Left = frameData.Box.Left,
            Top = frameData.Box.Top,
            Right = frameData.Box.Right,
            Bottom = frameData.Box.Bottom
        };
        frameId = frame;
    }

    public void build(ACTOR_TYPE t, int newAct)
    {
        type = t;
        newAction = newAct;
        initNewAction(true);
    }
    
    public void initNewAction(bool bInitMModel)
    {
        AnimData data = Actor.aniData[(sbyte)type];
        MechModelParams mmParam = data.mmParam[newAction];
        int nbFrames = data.actions[newAction].FramesCount;
        aniEvent_flag = ANIM_EVENT_FLAGS.NONE;
        if (bInitMModel && (data.flag & ANIM_DATA_FLAGS.HAS_MECH_MODEL) != 0)
        {
            aniEvent_flag |= ANIM_EVENT_FLAGS.LOADED_MECH_MODEL;
            aniEvent_mmtype = mmParam.Type;
            for (int i = 0; i < 6; i++)
            {
                if (i < mmParam.ParamsCount)
                {
                    int param = mmParam.Params[i];
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
            setFrame(data.actions[newAction].Frames[0]);
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
            Action actionData = Actor.aniData[(sbyte)type].actions[newAction];
            actionFrame++;
            if (actionFrame == actionData.FramesCount)
                actionFrame = 0;
            if (actionData.FramesCount > 0)
                setFrame(actionData.Frames[actionFrame]);
        }
    }
    
    public void draw(int nx, int ny, ACTOR_STATE nflag)
    {
        if (frameId < 0)
            return;

        AnimData data = Actor.aniData[(sbyte)type];
        AnimationFrame frameData = data.frames[frameId];
        int frameOffset = 0;
        int nbSprite = frameData.SpritesCount;
        
        if (type == ACTOR_TYPE.RAYMAN)
            GameMidlet.Instance_Game.raymanAnim = 0;

        for (int i = 0; i < nbSprite; i++)
        {
            if (i == nbSprite - 1 && 
                type == ACTOR_TYPE.RAYMAN &&
                GameMidlet.Instance_Game.pFist[0].anim.curAction != 0 &&
                GameMidlet.Instance_Game.pRayman.anim.curAction != 10 &&
                GameMidlet.Instance_Game.pRayman.anim.curAction != 36)
                break;

            bool flag = !(i == 0 && type == ACTOR_TYPE.RAYMAN && GameMidlet.Instance_Game.pFist[1].anim.curAction != 0);
            int modId = frameData.Sprites[frameOffset].Module;
            AnimationModule pModule = data.modules[modId];
            
            int nsx;
            int geflag;
            if ((nflag & ACTOR_STATE.FLIP_X) != 0)
            {
                nsx = nx - frameData.Sprites[frameOffset].XPosition - pModule.Width;
                geflag = frameData.Sprites[frameOffset].FlipX ? 0 : 0x4000;
            }
            else
            {
                nsx = nx + frameData.Sprites[frameOffset].XPosition;
                geflag = frameData.Sprites[frameOffset].FlipX ? 0x4000 : 0;
            }
            
            int nsy = ny + frameData.Sprites[frameOffset].YPosition;
            
            if (type == ACTOR_TYPE.RAYMAN)
                GameMidlet.Instance_Game.raymanAnim = nsx;

            if (flag)
            {
                GameMidlet.Instance_Game.drawImageEx(
                    dstx: nsx,
                    dsty: nsy,
                    w: pModule.Width,
                    h: pModule.Height,
                    iImageIndex: data.resID,
                    sx: pModule.XPosition,
                    sy: pModule.YPosition,
                    flag: geflag);
            }

            frameOffset++;
        }
    }

    // Custom helper
    public Box GetRenderBox(ACTOR_STATE nflag)
    {
        if (frameId < 0)
            return new Box();

        AnimData data = Actor.aniData[(sbyte)type];
        AnimationFrame frameData = data.frames[frameId];
        int frameOffset = 0;
        int nbSprite = frameData.SpritesCount;

        int? minX = null;
        int? minY = null;
        int? maxX = null;
        int? maxY = null;

        for (int i = 0; i < nbSprite; i++)
        {
            if (i == nbSprite - 1 &&
                type == ACTOR_TYPE.RAYMAN &&
                GameMidlet.Instance_Game.pFist[0].anim.curAction != 0 &&
                GameMidlet.Instance_Game.pRayman.anim.curAction != 10 &&
                GameMidlet.Instance_Game.pRayman.anim.curAction != 36)
                break;

            bool flag = !(i == 0 && type == ACTOR_TYPE.RAYMAN && GameMidlet.Instance_Game.pFist[1].anim.curAction != 0);
            AnimationModule pModule = data.modules[frameData.Sprites[frameOffset].Module];

            int nsx;
            if ((nflag & ACTOR_STATE.FLIP_X) != 0)
                nsx = -frameData.Sprites[frameOffset].XPosition - pModule.Width;
            else
                nsx = frameData.Sprites[frameOffset].XPosition;

            int nsy = frameData.Sprites[frameOffset].YPosition;

            if (flag)
            {
                if (minX == null || nsx < minX)
                    minX = nsx;
                if (minY == null || nsy < minY)
                    minY = nsy;
                if (maxX == null || nsx + pModule.Width > maxX)
                    maxX = nsx + pModule.Width;
                if (maxY == null || nsy + pModule.Height > maxY)
                    maxY = nsy + pModule.Height;
            }

            frameOffset++;
        }

        return new Box
        {
            Left = minX ?? 0,
            Top = minY ?? 0,
            Right = maxX ?? 0,
            Bottom = maxY ?? 0
        };
    }
}