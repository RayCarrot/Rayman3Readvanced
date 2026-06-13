namespace GbaMonoGame.Rayman3.J2me;

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
    public static Box aniEvent_pColBoxData { get; set; }

    public void setFrame(int frame)
    {
        AnimFrame frameData = Actor.aniData[(sbyte)type].frames[frame];
        frameDuration = frameData.FrameDuration;
        frameTick = 0;
        aniEvent_flag |= ANIM_EVENT_FLAGS.LOADED_COLLISION_BOX;
        aniEvent_pColBoxData = frameData.Box;
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
        AnimFrame frameData = data.frames[frameId];
        int frameOffset = 0;
        int nbSprite = frameData.SpritesCount;
        
        if (type == OBJECT_TYPE.RAYMAN)
            GameMidlet.Instance_Game.raymanAnim = 0;

        for (int i = 0; i < nbSprite; i++)
        {
            if (i == nbSprite - 1 && 
                type == OBJECT_TYPE.RAYMAN &&
                GameMidlet.Instance_Game.pFist[0].anim.curAction != 0 &&
                GameMidlet.Instance_Game.pRayman.anim.curAction != 10 &&
                GameMidlet.Instance_Game.pRayman.anim.curAction != 36)
                break;

            bool flag = !(i == 0 && type == OBJECT_TYPE.RAYMAN && GameMidlet.Instance_Game.pFist[1].anim.curAction != 0);
            int modId = frameData.Frames[frameOffset].Module & 0x7F;
            AnimModule pModule = data.modules[modId];
            
            int nsx;
            int geflag;
            if ((nflag & ACTOR_STATE.FLIP_X) != 0)
            {
                nsx = nx - frameData.Frames[frameOffset].X - pModule.Width;
                geflag = frameData.Frames[frameOffset].Module < 0 ? 0 : 0x4000;
            }
            else
            {
                nsx = nx + frameData.Frames[frameOffset].X;
                geflag = frameData.Frames[frameOffset].Module < 0 ? 0x4000 : 0;
            }
            
            int nsy = ny + frameData.Frames[frameOffset].Y;
            
            if (type == OBJECT_TYPE.RAYMAN)
                GameMidlet.Instance_Game.raymanAnim = nsx;

            if (flag)
            {
                GameMidlet.Instance_Game.drawImageEx(
                    dstx: nsx,
                    dsty: nsy,
                    w: pModule.Width,
                    h: pModule.Height,
                    img: data.ModuleTextures[modId],
                    iImageIndex: data.resID,
                    sx: pModule.X,
                    sy: pModule.Y,
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
        AnimFrame frameData = data.frames[frameId];
        int frameOffset = 0;
        int nbSprite = frameData.SpritesCount;

        int? minX = null;
        int? minY = null;
        int? maxX = null;
        int? maxY = null;

        for (int i = 0; i < nbSprite; i++)
        {
            if (i == nbSprite - 1 &&
                type == OBJECT_TYPE.RAYMAN &&
                GameMidlet.Instance_Game.pFist[0].anim.curAction != 0 &&
                GameMidlet.Instance_Game.pRayman.anim.curAction != 10 &&
                GameMidlet.Instance_Game.pRayman.anim.curAction != 36)
                break;

            bool flag = !(i == 0 && type == OBJECT_TYPE.RAYMAN && GameMidlet.Instance_Game.pFist[1].anim.curAction != 0);
            AnimModule pModule = data.modules[frameData.Frames[frameOffset].Module & 0x7F];

            int nsx;
            if ((nflag & ACTOR_STATE.FLIP_X) != 0)
                nsx = -frameData.Frames[frameOffset].X - pModule.Width;
            else
                nsx = frameData.Frames[frameOffset].X;

            int nsy = frameData.Frames[frameOffset].Y;

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