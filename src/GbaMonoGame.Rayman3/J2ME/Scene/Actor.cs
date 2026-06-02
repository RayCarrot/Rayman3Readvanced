using System;
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable

namespace GbaMonoGame.Rayman3.J2ME;

// TODO: Helpers for button input checks to avoid casts?
public class Actor
{
    public Actor(sbyte[] data)
    {
        Actor_ctor(data);

        int nOff = 6;
        switch (objType)
        {
            case OBJECT_TYPE.RAYMAN:
                V = new int[17];
                V[1] = 0;
                V[12] = 0;
                V[0] |= 0x2;
                actorReference = null;
                m_bPHBTable = new PHB_TYPE[6, 5];
                m_sPHBTableReferenceTileX = -20;
                m_sPHBTableReferenceTileY = -20;
                m_sTableRefIndexX = 2;
                m_sTableRefIndexY = 4;
                xDirectionConfirmed = true;
                break;

            case OBJECT_TYPE.FIST:
                stateFlag |= ACTOR_STATE.DEAD;
                anim.newAction = 0;
                actorReference = null;
                break;
            
            case OBJECT_TYPE.CAGE:
                V = new int[1];
                V[0] = 2;
                if (data[nOff++] != 0)
                    anim.newAction = 5;
                break;
            
            case OBJECT_TYPE.PIRATE:
                V = new int[6];
                V[0] = (byte)data[nOff++];
                V[1] = (byte)data[nOff++];
                V[2] = (byte)data[nOff++];
                V[2] <<= 16;
                V[2] |= (byte)data[nOff++];
                V[2] |= V[2] << 8;
                V[5] = (byte)data[nOff++];
                V[3] = 240;
                V[4] = 0;
                break;

            case OBJECT_TYPE.BULLET:
                stateFlag |= ACTOR_STATE.DEAD;
                break;
            
            case OBJECT_TYPE.PLATFORM_1:
            case OBJECT_TYPE.PLATFORM_2:
                V = new int[2];
                V[0] = data[nOff++];
                m_bPHBTable = new PHB_TYPE[1, 1];
                m_sPHBTableReferenceTileX = -10;
                m_sPHBTableReferenceTileY = -10;
                m_sTableRefIndexX = 0;
                m_sTableRefIndexY = 0;
                break;

            case OBJECT_TYPE.TENTACLE:
                V = new int[2];
                V[0] = 30;
                break;
            
            case OBJECT_TYPE.LEVEL_POST:
                stateFlag |= ACTOR_STATE.LEFT_TO_DIE;
                break;
            
            case OBJECT_TYPE.LEVEL_SIGN:
                V = new int[1];
                V[0] = (byte)data[nOff++];
                break;
        }

        if ((stateFlag & ACTOR_STATE.LEFT_TO_DIE) != 0)
            GameMidlet.Instance_Game.s_iLeftToDie++;

        m_lInitX = x;
        m_lInitY = y;
        m_iInitAction = anim.newAction;
        m_sInitStateFlag = stateFlag;
        
        if (V != null)
        {
            m_iInitV = new int[V.Length];
            System.arraycopy(V, 0, m_iInitV, 0, V.Length);
        }
    }

    public const int FISTS_COUNT = 2;
    public const int ANIM_DATAS_COUNT = 28;

    public static AnimData[] aniData { get; } = new AnimData[ANIM_DATAS_COUNT];
    public Actor actorReference { get; set; }
    public Anim anim { get; } = new();
    public OBJECT_TYPE objType { get; set; }
    public Box colBox { get; set; }
    public int[] V { get; set; } // TODO: Custom class per object type?
    public int[] m_iInitV { get; set; }
    public long x { get; set; }
    public long y { get; set; }
    public short dx { get; set; }
    public short dy { get; set; }
    public MM_TYPE mmodel_type { get; set; }
    public short mmodel_vX { get; set; }
    public short mmodel_vY { get; set; }
    public short mmodel_aX { get; set; }
    public short mmodel_aY { get; set; }
    public short mmodel_fX { get; set; }
    public short mmodel_fY { get; set; }
    public ACTOR_STATE stateFlag { get; set; }
    public long m_lInitX { get; set; }
    public long m_lInitY { get; set; }
    public int m_iInitAction { get; set; }
    public ACTOR_STATE m_sInitStateFlag { get; set; }
    public PHB_TYPE[,] m_bPHBTable { get; set; }
    public short m_sPHBTableReferenceTileX { get; set; }
    public short m_sPHBTableReferenceTileY { get; set; }
    public short m_sTableRefIndexX { get; set; }
    public short m_sTableRefIndexY { get; set; }
    public bool xDirectionConfirmed { get; set; }
    public int xDirectionConfirmationCounter { get; set; }
    public int yDirectionConfirmed { get; set; }
    public int yDirectionConfirmationCounter { get; set; }
    public int[] fist_time { get; } = new int[FISTS_COUNT];
    public int fist_top { get; set; }
    public static int[] fist_energy { get; } = new int[FISTS_COUNT];
    public bool bWasPHBStop { get; set; }

    public bool GameObj_checkCollsion(Actor that)
    {
        if (that == null)
            return false;

        return GameObj_isCollideBox(
            (int)((that.x >> 8) + that.colBox.Left), 
            (int)((that.y >> 8) + that.colBox.Top),
            that.colBox.Right - that.colBox.Left, 
            that.colBox.Bottom - that.colBox.Top);
    }

    public bool GameObj_isCollideBox(int l, int t, int w, int h)
    {
        int me = (int)(x >> 8) + colBox.Right;
        int it = l;
        if (me < it)
            return false;

        me = (int)(x >> 8) + colBox.Left;
        it = l + w;
        if (it < me)
            return false;
        
        me = (int)(y >> 8) + colBox.Top;
        it = t + h;
        if (it < me)
            return false;
        
        me = (int)(y >> 8) + colBox.Bottom;
        it = t;
        if (me < it)
            return false;
        
        return true;
    }

    public void MModel_Init(MM_TYPE mmtype, short[] mmpar)
    {
        if (mmtype == MM_TYPE.NONE)
            return;

        mmodel_type = mmtype;
        switch (mmodel_type)
        {
            case MM_TYPE.RESET:
                mmodel_vX = mmodel_vY = 0;
                mmodel_aX = mmodel_aY = 0;
                break;

            case MM_TYPE.SET_SPEED_XY:
                mmodel_vX = mmpar[0];
                mmodel_vY = mmpar[1];
                break;
            
            case MM_TYPE.SET_SPEED_X_RESET_SPEED_Y:
                mmodel_vX = mmpar[0];
                mmodel_vY = 0;
                break;
            
            case MM_TYPE.SET_SPEED_Y_RESET_SPEED_X:
                mmodel_vY = mmpar[0];
                mmodel_vX = 0;
                break;
            
            case MM_TYPE.SET_SPEED_X:
                mmodel_vX = mmpar[0];
                break;
            
            case MM_TYPE.SET_SPEED_Y:
                mmodel_vY = mmpar[0];
                break;
            
            case MM_TYPE.SET_ACCELERATION_XY_SET_TARGET_SPEED_XY:
                mmodel_fX = mmpar[0];
                mmodel_aX = mmpar[1];
                mmodel_fY = mmpar[2];
                mmodel_aY = mmpar[3];
                break;
            
            case MM_TYPE.SET_ACCELERATION_X_SET_TARGET_SPEED_X:
                mmodel_fX = mmpar[0];
                mmodel_aX = mmpar[1];
                break;
            
            case MM_TYPE.SET_ACCELERATION_Y_SET_TARGET_SPEED_Y:
                mmodel_fY = mmpar[0];
                mmodel_aY = mmpar[1];
                break;

            case MM_TYPE.SET_SPEED_XY_SET_ACCELERATION_XY_SET_TARGET_SPEED_XY:
                mmodel_vX = mmpar[0];
                mmodel_vY = mmpar[1];
                mmodel_fX = mmpar[2];
                mmodel_aX = mmpar[3];
                mmodel_fY = mmpar[4];
                mmodel_aY = mmpar[5];
                mmodel_type = MM_TYPE.SET_ACCELERATION_XY_SET_TARGET_SPEED_XY;
                break;

            case MM_TYPE.SET_SPEED_X_SET_ACCELERATION_X_SET_TARGET_SPEED_X:
                mmodel_vX = mmpar[0];
                mmodel_fX = mmpar[1];
                mmodel_aX = mmpar[2];
                mmodel_type = MM_TYPE.SET_ACCELERATION_X_SET_TARGET_SPEED_X;
                break;

            case MM_TYPE.SET_SPEED_Y_SET_ACCELERATION_Y_SET_TARGET_SPEED_Y:
                mmodel_vY = mmpar[0];
                mmodel_fY = mmpar[1];
                mmodel_aY = mmpar[2];
                mmodel_type = MM_TYPE.SET_ACCELERATION_Y_SET_TARGET_SPEED_Y;
                break;
        }
    }

    public void MModel_Tick()
    {
        if (mmodel_type is MM_TYPE.SET_ACCELERATION_XY_SET_TARGET_SPEED_XY or MM_TYPE.SET_ACCELERATION_X_SET_TARGET_SPEED_X && mmodel_aX != 0)
        {
            mmodel_vX = (short)(mmodel_vX + mmodel_aX);
            if ((mmodel_fX > 0 && mmodel_vX > mmodel_fX) ||
                (mmodel_fX < 0 && mmodel_vX < mmodel_fX))
            {
                mmodel_vX = mmodel_fX;
                mmodel_aX = 0;
            }
        }

        if (mmodel_type is MM_TYPE.SET_ACCELERATION_XY_SET_TARGET_SPEED_XY or MM_TYPE.SET_ACCELERATION_Y_SET_TARGET_SPEED_Y && mmodel_aY != 0)
        {
            mmodel_vY = (short)(mmodel_vY + mmodel_aY);
            if ((mmodel_fY > 0 && mmodel_vY > mmodel_fY) ||
                (mmodel_fY < 0 && mmodel_vY < mmodel_fY))
            {
                mmodel_vY = mmodel_fY;
                mmodel_aY = 0;
            }
        }
    }

    public static void AniLoad(int aniResIndex, int resID)
    {
        byte[] buffer = GameMidlet.Instance_Game.RM.Array_Data[aniResIndex];
        if (buffer == null)
            return;

        int type = buffer[0] & SByte.MaxValue;
        if (aniData[type] != null)
        {
            aniData[type].flag |= ANIM_DATA_FLAGS.LOADED;
            return;
        }

        AnimData data = new();
        aniData[type] = data;
        data.resID = (sbyte)resID;
        data.flag = (buffer[0] & 0x80) != 0 ? ANIM_DATA_FLAGS.HAS_MECH_MODEL : ANIM_DATA_FLAGS.NONE;
        data.nbModule = buffer[1];
        data.nbFrame = buffer[2];
        data.nbAction = buffer[3];
        data.flag |= ANIM_DATA_FLAGS.LOADED;
        int count = data.nbModule;
        data.modules = new AnimModule[count];
        int offset = 4;
        for (int i = 0; i < count; i++)
        {
            data.modules[i] = new AnimModule
            {
                X = buffer[offset + 0],
                Y = buffer[offset + 1],
                Width = buffer[offset + 2],
                Height = buffer[offset + 3]
            };
            offset += 4;
        }

        count = data.nbFrame;
        data.frames = new AnimFrame[count];
        for (int j = 0; j < count; j++)
        {
            data.frames[j] = new AnimFrame
            {
                SpritesCount = buffer[offset + 0],
                FrameDuration = buffer[offset + 1],
                Box = new Box()
                {
                    Left = (sbyte)buffer[offset + 2],
                    Top = (sbyte)buffer[offset + 3],
                    Right = (sbyte)buffer[offset + 4],
                    Bottom = (sbyte)buffer[offset + 5],
                },
                Frames = new AnimFrameSprite[buffer[offset + 0]]
            };
            offset += 6;

            for (int frameIndex = 0; frameIndex < data.frames[j].SpritesCount; frameIndex++)
            {
                data.frames[j].Frames[frameIndex] = new AnimFrameSprite
                {
                    Module = (sbyte)buffer[offset + 0],
                    X = (sbyte)buffer[offset + 1],
                    Y = (sbyte)buffer[offset + 2]
                };
                offset += 3;
            }
        }

        count = data.nbAction;
        data.actions = new Action[count];
        data.mmParam = new MechModelParams[count];
        for (int k = 0; k < count; k++)
        {
            data.actions[k] = new Action
            {
                FramesCount = buffer[offset + 0],
                Frames = new byte[buffer[offset + 0]]
            };
            offset++;
            System.arraycopy(buffer, offset, data.actions[k].Frames, 0, data.actions[k].FramesCount);
            offset += data.actions[k].FramesCount;

            data.mmParam[k] = new MechModelParams()
            {
                ParamsCount = (byte)(buffer[offset + 0] & 0xF),
                Type = (MM_TYPE)((buffer[offset + 0] & 0xF0) >> 4),
                Params = new sbyte[buffer[offset + 0]]
            };
            offset++;
            System.arraycopy(buffer, offset, data.mmParam[k].Params, 0, data.mmParam[k].ParamsCount);
            offset += data.mmParam[k].ParamsCount;
        }
    }

    public bool Ani_CheckEnd()
    {
        if (anim.newAction != anim.curAction)
            return false;

        return anim.frameTick + 1 >= anim.frameDuration &&
               anim.actionFrame + 1 >= aniData[(sbyte)anim.type].actions[anim.newAction].FramesCount;
    }

    public static void drawModule(AnimData pData, int idMod, int nx, int ny, int nflag, Graphics g)
    {
        AnimModule module = pData.modules[idMod];
        GameMidlet.Instance_Game.drawImageEx(
            dstx: nx, 
            dsty: ny, 
            w: (byte)module.Width, 
            h: (byte)module.Height, 
            iImageIndex: pData.resID,
            sx: (byte)module.X, 
            sy: (byte)module.Y, 
            flag: nflag);
    }

    public void step()
    {
        x += dx;
        y += dy;
        Anim.aniEvent_flag = ANIM_EVENT_FLAGS.NONE;
        anim.step(true);
        if ((Anim.aniEvent_flag & ANIM_EVENT_FLAGS.LOADED_MECH_MODEL) != 0)
        {
            MModel_Init(Anim.aniEvent_mmtype, Anim.aniEvent_mmpar);
            stateFlag |= ACTOR_STATE.USE_MECH_MODEL;
        }

        if ((Anim.aniEvent_flag & ANIM_EVENT_FLAGS.LOADED_COLLISION_BOX) != 0)
        {
            if ((stateFlag & ACTOR_STATE.FLIP_X) != 0)
            {
                colBox = colBox with
                {
                    Left = (sbyte)-Anim.aniEvent_pColBoxData.Right,
                    Right = (sbyte)-Anim.aniEvent_pColBoxData.Left
                };
            }
            else
            {
                colBox = colBox with
                {
                    Left = Anim.aniEvent_pColBoxData.Left,
                    Right = Anim.aniEvent_pColBoxData.Right
                };
            }

            if ((stateFlag & ACTOR_STATE.FLIP_Y) != 0)
            {
                colBox = colBox with
                {
                    Top = (sbyte)-Anim.aniEvent_pColBoxData.Bottom,
                    Bottom = (sbyte)-Anim.aniEvent_pColBoxData.Top
                };
            }
            else
            {
                colBox = colBox with
                {
                    Top = Anim.aniEvent_pColBoxData.Top,
                    Bottom = Anim.aniEvent_pColBoxData.Bottom
                };
            }
        }

        if ((stateFlag & ACTOR_STATE.USE_MECH_MODEL) != 0 && (stateFlag & ACTOR_STATE.FLIP_X) != 0)
            dx = (short)-mmodel_vX;
        else
            dx = mmodel_vX;

        if ((stateFlag & ACTOR_STATE.USE_MECH_MODEL) != 0 && (stateFlag & ACTOR_STATE.FLIP_Y) != 0)
            dy = (short)-mmodel_vY;
        else
            dy = mmodel_vY;

        if ((stateFlag & ACTOR_STATE.USE_MECH_MODEL) != 0)
            MModel_Tick();
    }

    public void draw()
    {
        if ((objType == OBJECT_TYPE.RAYMAN && (V[1] & 0x1) == 1) || (objType == OBJECT_TYPE.PIRATE && (V[4] & 0x1) == 1))
        {
            GameMidlet.Instance_Game.raymanDraw = 2;
            return;
        }

        if (objType == OBJECT_TYPE.RAYMAN)
            GameMidlet.Instance_Game.raymanNull = anim.frameId;

        anim.draw((int)(x >> 8), (int)(y >> 8), stateFlag & (ACTOR_STATE.FLIP_X | ACTOR_STATE.FLIP_Y));

        if (objType == OBJECT_TYPE.RAYMAN)
            GameMidlet.Instance_Game.raymanDraw = (int)(y >> 8);
    }

    public void Actor_ctor(sbyte[] data)
    {
        objType = (OBJECT_TYPE)data[1];

        x = GameMidlet.Instance_Game.ReadUnsignedShort(data, 2);
        x *= 2L;
        x <<= 8;

        y = GameMidlet.Instance_Game.ReadUnsignedShort(data, 4);
        y *= 2L;
        y <<= 8;

        short firstAction = (short)(data[0] & 0xF);
        stateFlag = (ACTOR_STATE)((data[0] & 0xF0) >> 4);
        mmodel_vX = mmodel_vY = 0;
        mmodel_aX = mmodel_aY = 0;
        mmodel_fX = mmodel_fY = 0;
        Anim.aniEvent_flag = ANIM_EVENT_FLAGS.NONE;
        anim.build(objType, firstAction);

        if ((Anim.aniEvent_flag & ANIM_EVENT_FLAGS.LOADED_MECH_MODEL) != 0)
        {
            MModel_Init(Anim.aniEvent_mmtype, Anim.aniEvent_mmpar);
            stateFlag |= ACTOR_STATE.USE_MECH_MODEL;
        }

        if ((Anim.aniEvent_flag & ANIM_EVENT_FLAGS.LOADED_COLLISION_BOX) != 0)
        {
            if ((stateFlag & ACTOR_STATE.FLIP_X) != 0)
            {
                colBox = colBox with
                {
                    Left = (sbyte)-Anim.aniEvent_pColBoxData.Right,
                    Right = (sbyte)-Anim.aniEvent_pColBoxData.Left
                };
            }
            else
            {
                colBox = colBox with
                {
                    Left = Anim.aniEvent_pColBoxData.Left,
                    Right = Anim.aniEvent_pColBoxData.Right
                };
            }

            if ((stateFlag & ACTOR_STATE.FLIP_Y) != 0)
            {
                colBox = colBox with
                {
                    Top = (sbyte)-Anim.aniEvent_pColBoxData.Bottom,
                    Bottom = (sbyte)-Anim.aniEvent_pColBoxData.Top
                };
            }
            else
            {
                colBox = colBox with
                {
                    Top = Anim.aniEvent_pColBoxData.Top,
                    Bottom = Anim.aniEvent_pColBoxData.Bottom
                };
            }
        }

        dx = 0;
        dy = 0;
    }

    public bool checkFloor()
    {
        if (dy < 0)
            return false;

        int tilePosX = (int)((x + dx) >> 8 >> 4);
        int tilePosY = (int)((y + dy) >> 8 >> 4);

        PHB_TYPE phb = Actor_GetPHB(tilePosX, tilePosY);
        if (phb == PHB_TYPE.NONE)
        {
            tilePosY = (int)((y + dy + Math.Abs(dx)) >> 8 >> 4);
            if ((phb = Actor_GetPHB(tilePosX, tilePosY)) == 0)
                return false;
        }

        bool bHasTouchedFloor = false;
        bWasPHBStop = false;
        while (phb is PHB_TYPE.SOLID or PHB_TYPE.HANG)
        {
            bHasTouchedFloor = true;
            dy = (short)(int)((tilePosY << 8 << 4) - y);
            tilePosY--;
            phb = Actor_GetPHB(tilePosX, tilePosY);
            if (phb == PHB_TYPE.TYPE_28)
                bWasPHBStop = true;
        }

        if (phb is 
            PHB_TYPE.TYPE_2 or 
            PHB_TYPE.TYPE_4 or 
            PHB_TYPE.TYPE_5 or 
            PHB_TYPE.TYPE_6 or 
            PHB_TYPE.TYPE_7 or 
            PHB_TYPE.TYPE_8 or 
            PHB_TYPE.TYPE_9)
        {
            int displacement = GameMidlet.Instance_Game.PF_getSlopeDisp(phb, (int)((((x + dx) >> 8) & 0xFL) / 2L));
            dy = (short)(int)((tilePosY << 8 << 4) + displacement * 2 - y);
            return true;
        }

        return bHasTouchedFloor;
    }

    public bool checkWall()
    {
        int j = dx <= 0 ? colBox.Left : colBox.Right;
        int tilePosX = ((int)((x + dx) >> 8) + j) >> 4;
        int tileLowerPosY = ((int)((y + dy) >> 8) + colBox.Bottom - 1) >> 4;
        int tileUpperPosY = ((int)((y + dy) >> 8) + colBox.Top + 1) >> 4;
        for (int i = tileUpperPosY; i <= tileLowerPosY; i++)
        {
            PHB_TYPE phb = Actor_GetPHB(tilePosX, i);
            if (phb is PHB_TYPE.SOLID or PHB_TYPE.HANG)
            {
                phb = dx < 0 ? Actor_GetPHB(tilePosX + 1, i) : Actor_GetPHB(tilePosX - 1, i);

                if (dx < 0)
                {
                    int otherSideTilePosX = ((int)((x + dx) >> 8) + colBox.Right) >> 4;
                    for (int k = tilePosX; k < otherSideTilePosX; k++)
                    {
                        phb = Actor_GetPHB(k + 1, i);
                        if (phb is
                            PHB_TYPE.TYPE_2 or
                            PHB_TYPE.TYPE_4 or
                            PHB_TYPE.TYPE_5 or
                            PHB_TYPE.TYPE_6 or
                            PHB_TYPE.TYPE_7 or
                            PHB_TYPE.TYPE_8 or
                            PHB_TYPE.TYPE_9)
                            return false;
                    }
                }
                else
                {
                    int k = ((int)((x + dx) >> 8) + colBox.Left) >> 4;
                    for (int m = k; m > tilePosX; m--)
                    {
                        phb = Actor_GetPHB(m - 1, i);
                        if (phb is
                            PHB_TYPE.TYPE_2 or
                            PHB_TYPE.TYPE_4 or
                            PHB_TYPE.TYPE_5 or
                            PHB_TYPE.TYPE_6 or
                            PHB_TYPE.TYPE_7 or
                            PHB_TYPE.TYPE_8 or
                            PHB_TYPE.TYPE_9)
                            return false;
                    }
                }

                dx = (short)((16 - (char)(int)((x >> 8) + j - (tilePosX << 4))) << 8);
                return true;
            }
        }

        return false;
    }

    public void Spike_ai()
    {
        int action = anim.curAction;
        switch (action)
        {
            case 0:
                for (int f = 0; f < FISTS_COUNT; f++)
                {
                    if (GameMidlet.Instance_Game.pFist[f].Fist_CheckCollision(this))
                    {
                        anim.newAction = 1;
                        break;
                    }
                }

                if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman))
                    GameMidlet.Instance_Game.pRayman.doDamage();
                break;
            
            case 1:
                if (Ani_CheckEnd() && (GameMidlet.Instance_Game.pFist[0].stateFlag & ACTOR_STATE.DEAD) != 0 && (GameMidlet.Instance_Game.pFist[1].stateFlag & ACTOR_STATE.DEAD) != 0)
                    anim.newAction = 0;

                if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman))
                    GameMidlet.Instance_Game.pRayman.doDamage();
                break;
        }
    }

    public void Bomb_ai()
    {
        int action = anim.curAction;
        switch (action)
        {
            case 0:
                if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman))
                {
                    GameMidlet.Instance_Game.pRayman.doDamage();
                    anim.newAction = 1;
                }

                for (int f = 0; f < FISTS_COUNT; f++)
                {
                    if (GameMidlet.Instance_Game.pFist[f].Fist_CheckCollision(this))
                    {
                        anim.newAction = 1;
                        GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.enemy_hit, true);
                        break;
                    }
                }
                break;

            case 1:
                if (Ani_CheckEnd())
                    stateFlag |= ACTOR_STATE.DEAD;
                break;
        }
    }

    public void SeaUrchin_ai()
    {
        int action = anim.curAction;
        switch (action)
        {
            case 0:
                for (int f = 0; f < FISTS_COUNT; f++)
                {
                    if (GameMidlet.Instance_Game.pFist[f].Fist_CheckCollision(this))
                    {
                        anim.newAction = 1;
                        break;
                    }
                }

                if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman))
                    GameMidlet.Instance_Game.pRayman.doDamage();
                break;

            case 1:
                if (Ani_CheckEnd())
                    anim.newAction = 0;

                if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman))
                    GameMidlet.Instance_Game.pRayman.doDamage();
                break;
        }
    }

    public void Tentacle_ai()
    {
        int action = anim.curAction;
        switch (action)
        {
            case 0:
            case 1:
                V[1] += 1;
                if (V[1] + 1 > V[0] && Math.Abs(GameMidlet.Instance_Game.pRayman.x - x) < 0x7800 && Math.Abs(GameMidlet.Instance_Game.pRayman.y - y) < 0xA000)
                {
                    if (action == 1)
                    {
                        anim.newAction = 5;
                        break;
                    }

                    anim.newAction = 2;
                }
                break;

            case 2:
            case 5:
                if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman))
                    GameMidlet.Instance_Game.pRayman.doDamage();
                for (int f = 0; f < FISTS_COUNT; f++)
                {
                    if (GameMidlet.Instance_Game.pFist[f].Fist_CheckCollision(this))
                    {
                        anim.newAction = action == 5 ? 7 : 4;
                        V[1] = 0;
                        break;
                    }
                }

                if (Ani_CheckEnd())
                    anim.newAction = action == 5 ? 6 : 3;
                break;

            case 3:
            case 6:
                if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman))
                    GameMidlet.Instance_Game.pRayman.doDamage();
                for (int f = 0; f < FISTS_COUNT; f++)
                {
                    if (GameMidlet.Instance_Game.pFist[f].Fist_CheckCollision(this))
                    {
                        V[1] = 0;
                        anim.newAction = action == 6 ? 7 : 4;
                        GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.enemy_hit, true);
                        break;
                    }
                }
                break;

            case 4:
            case 7:
                if (Ani_CheckEnd())
                    anim.newAction = action == 7 ? 1 : 0;
                break;
        }
    }

    public void Fly_ai()
    {
        int action = anim.curAction;
        switch (action)
        {
            case 0:
                if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman))
                    GameMidlet.Instance_Game.pRayman.doDamage();
                for (int f = 0; f < FISTS_COUNT; f++)
                {
                    if (GameMidlet.Instance_Game.pFist[f].Fist_CheckCollision(this))
                    {
                        anim.newAction = 2;
                        GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.enemy_hit, true);
                        break;
                    }
                }

                if (Math.Abs(GameMidlet.Instance_Game.pRayman.x - x) < 30720L && Math.Abs(GameMidlet.Instance_Game.pRayman.y - y) < 0x5000)
                {
                    anim.newAction = 1;
                    mmodel_vX = (short)(int)((GameMidlet.Instance_Game.pRayman.x - x) / 0x20);
                    mmodel_vY = (short)(int)((GameMidlet.Instance_Game.pRayman.y - y) / 0x20);
                }
                break;

            case 1:
                if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman))
                    GameMidlet.Instance_Game.pRayman.doDamage();
                for (int f = 0; f < FISTS_COUNT; f++)
                {
                    if (GameMidlet.Instance_Game.pFist[f].Fist_CheckCollision(this))
                    {
                        anim.newAction = 2;
                        GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.enemy_hit, true);
                        break;
                    }
                }
                break;

            case 2:
                if (Ani_CheckEnd())
                    stateFlag |= ACTOR_STATE.DEAD;
                break;
        }
    }

    public void Bonus_ai()
    {
        if (objType == OBJECT_TYPE.SWING_LUM)
        {
            for (int f = 0; f < FISTS_COUNT; f++)
            {
                Actor pFist = GameMidlet.Instance_Game.pFist[f];
                if ((pFist.stateFlag & ACTOR_STATE.DEAD) == 0 && GameObj_checkCollsion(pFist))
                {
                    GameMidlet.Instance_Game.pFist[0].stateFlag |= ACTOR_STATE.DEAD;
                    GameMidlet.Instance_Game.pFist[1].stateFlag |= ACTOR_STATE.DEAD;
                    fist_top = 0;
                    GameMidlet.Instance_Game.pFist[0].anim.curAction = 0;
                    GameMidlet.Instance_Game.pFist[1].anim.curAction = 0;
                    GameMidlet.Instance_Game.pRayman.actorReference = null;
                    GameMidlet.Instance_Game.pRayman.anim.newAction = 27;
                    GameMidlet.Instance_Game.pRayman.step();
                    GameMidlet.Instance_Game.pRayman.x = x;
                    GameMidlet.Instance_Game.pRayman.y = y;
                    GameMidlet.Instance_Game.pRayman.V[13] = (int)x >> 8;
                    GameMidlet.Instance_Game.pRayman.V[14] = (int)y >> 8;
                }
            }
        }
        else if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman))
        {
            stateFlag |= ACTOR_STATE.DEAD;
            switch (objType)
            {
                case OBJECT_TYPE.LUM:
                    GameMidlet.Instance_Game.s_iLumsTaken++;
                    GameMidlet.Instance_Game.Status_Show(2);
                    GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.lums, true);
                    break;
                
                case OBJECT_TYPE.LIFE:
                    GameMidlet.Instance_Game.m_gameFrame_nLife = (sbyte)(GameMidlet.Instance_Game.m_gameFrame_nLife + (GameMidlet.Instance_Game.m_gameFrame_nLife < 99 ? 1 : 0));
                    GameMidlet.Instance_Game.Status_Show(0);
                    GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.lums_white, true);
                    break;
                
                case OBJECT_TYPE.ENERGY:
                    GameMidlet.Instance_Game.m_gameFrame_nEnergy = (sbyte)(GameMidlet.Instance_Game.m_gameFrame_nEnergy + (GameMidlet.Instance_Game.m_gameFrame_nEnergy < 5 ? 1 : 0));
                    GameMidlet.Instance_Game.Status_Show(0);
                    GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.lums_red, true);
                    break;
                
                case OBJECT_TYPE.BLUE_LUM:
                    GameMidlet.Instance_Game.pRayman.V[7] = -1;
                    GameMidlet.Instance_Game.pRayman.anim.newAction = 9;
                    GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.lums, true);
                    break;

                case OBJECT_TYPE.CHECKPOINT:
                    GameMidlet.Instance_Game.s_actorCheckpoint = this;
                    GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.lums, true);
                    break;
            }
        }
    }

    public void Cage_ai()
    {
        int action = anim.curAction;
        switch (action)
        {
            case 0:
                for (int f = 0; f < FISTS_COUNT; f++)
                {
                    if (GameMidlet.Instance_Game.pFist[f].anim.curAction == 1 && GameMidlet.Instance_Game.pFist[f].Fist_CheckCollision(this))
                    {
                        GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.cage_hit, true);
                        anim.newAction = 2;
                        break;
                    }
                }
                break;
            
            case 2:
                if (Ani_CheckEnd())
                {
                    V[0] -= 1;
                    if (V[0] - 1 != 0)
                    {
                        anim.newAction = 0;
                        break;
                    }

                    GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.cage_break, true);
                    GameMidlet.Instance_Game.s_iCageOpened++;
                    anim.newAction = 4;
                    GameMidlet.Instance_Game.Status_Show(1);
                }
                break;

            case 4:
                if (Ani_CheckEnd() || !GameMidlet.Instance_Game.Camera_IsVisible(this))
                    stateFlag |= ACTOR_STATE.DEAD;
                break;
            
            case 5:
                for (int i = 0; i < FISTS_COUNT; i++)
                {
                    if (GameMidlet.Instance_Game.pFist[i].anim.curAction == 1 && GameMidlet.Instance_Game.pFist[i].Fist_CheckCollision(this))
                    {
                        GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.cage_hit, true);
                        anim.newAction = 7;
                        break;
                    }
                }
                break;

            case 7:
                if (Ani_CheckEnd())
                {
                    V[0] -= 1;
                    if (V[0] - 1 != 0)
                    {
                        anim.newAction = 5;
                        break;
                    }

                    GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.cage_break, true);
                    GameMidlet.Instance_Game.s_iCageOpened++;
                    anim.newAction = 9;
                    GameMidlet.Instance_Game.Status_Show(1);
                }
                break;

            case 9:
                if (Ani_CheckEnd() || !GameMidlet.Instance_Game.Camera_IsVisible(this))
                    stateFlag |= ACTOR_STATE.DEAD;
                break;
        }
    }

    public void Platform_ai()
    {
        int action = anim.curAction;
        bool attached = GameMidlet.Instance_Game.pRayman.anim.curAction != 27 && GameMidlet.Instance_Game.pRayman.Rayman_checkAttachPlatform(this);
        switch (action)
        {
            case 0:
                if (attached)
                    anim.newAction = V[0];
                break;

            case 5:
                if (!GameMidlet.Instance_Game.Camera_IsVisible(this))
                    Actor_Reset();
                break;
            
            case 10:
                V[1] -= 1;
                if (V[1] > 0)
                    break;
                goto default;
            
            default:
                int nx = (int)(x >> 8 >> 4);
                int ny = (int)(y >> 8 >> 4);
                Actor_SetReferencePoint(nx, ny);
                PHB_TYPE phb = Actor_GetPHB(nx, ny);
                switch (phb)
                {
                    case PHB_TYPE.TYPE_28:
                        anim.newAction = 5;
                        break;

                    case PHB_TYPE.TYPE_20:
                        if (action == 4)
                        {
                            V[1] = 20;
                            anim.newAction = 10;
                        }
                        else
                        {
                            anim.newAction = 6;
                        }
                        break;
                    
                    case PHB_TYPE.TYPE_21:
                        if (action == 6)
                        {
                            V[1] = 20;
                            anim.newAction = 10;
                        }
                        else
                        {
                            anim.newAction = 4;
                        }
                        break;
                    
                    case PHB_TYPE.TYPE_22:
                        anim.newAction = 8;
                        break;
                    
                    case PHB_TYPE.TYPE_23:
                        anim.newAction = 2;
                        break;
                    
                    case PHB_TYPE.TYPE_24:
                        anim.newAction = 9;
                        break;
                    
                    case PHB_TYPE.TYPE_25:
                        anim.newAction = 7;
                        break;
                    
                    case PHB_TYPE.TYPE_26:
                        anim.newAction = 3;
                        break;
                    
                    case PHB_TYPE.TYPE_27:
                        anim.newAction = 1;
                        break;
                }
                break;
        }

        for (int f = 0; f < FISTS_COUNT; f++)
        {
            if (GameMidlet.Instance_Game.pFist[f].anim.curAction == 1 &&
                GameMidlet.Instance_Game.pFist[f].Fist_CheckCollision(this))
                GameMidlet.Instance_Game.pFist[f].anim.newAction = 3;
        }
    }

    public void Fist_checkHitBackEnd()
    {
        Actor pRayman = GameMidlet.Instance_Game.pRayman;
        if (actorReference != null)
        {
            dx = 0;
            anim.newAction = 5;
        }
        else
        {
            int ndx = (int)(x - pRayman.x);
            if (ndx < 0)
                ndx *= -1;
            if (ndx < 0xC00 || ((stateFlag & ACTOR_STATE.FLIP_X) == 0 && x > pRayman.x) ||
                ((stateFlag & ACTOR_STATE.FLIP_X) != 0 && x < pRayman.x) || ndx > 0x1E000)
            {
                stateFlag |= ACTOR_STATE.DEAD;
                anim.curAction = 0;
            }
            else
            {
                int ndy = (int)(pRayman.y - y);
                ndy = ndy * 0x400 / ndx;
                dy = (short)(dy + ndy);
            }
        }
    }

    public void Fist_ai()
    {
        int action = anim.curAction;
        switch (action)
        {
            case 1:
                if (actorReference != null)
                {
                    dx = 0;
                    anim.newAction = 3;
                }
                else if (mmodel_vX >= 0)
                {
                    anim.newAction = 4;
                }
                else if (Actor_GetPHB((int)(x >> 8 >> 4), (int)(((y >> 8) + colBox.Top) >> 4)) == PHB_TYPE.SOLID)
                {
                    anim.newAction = 3;
                }
                break;

            case 2:
                if (Ani_CheckEnd())
                    anim.newAction = 4;
                break;
            
            case 4:
                Fist_checkHitBackEnd();
                break;
            
            case 3:
                Fist_checkHitBackEnd();
                if (Ani_CheckEnd())
                    anim.newAction = 4;
                break;
            
            case 5:
                if (Ani_CheckEnd())
                    anim.newAction = 4;
                break;
        }

        actorReference = null;
    }

    public void Fist_launch(ACTOR_STATE ndir, int energy)
    {
        int offsetX;
        Actor pRayman = GameMidlet.Instance_Game.pRayman;
        stateFlag &= ~(ACTOR_STATE.FLIP_X | ACTOR_STATE.FLIP_Y);
        stateFlag |= ndir;
        if ((pRayman.stateFlag & ACTOR_STATE.FLIP_X) != 0)
        {
            pRayman.xDirectionConfirmed = true;
            pRayman.xDirectionConfirmationCounter = 2;
            offsetX = 0x400;
        }
        else
        {
            pRayman.xDirectionConfirmed = false;
            pRayman.xDirectionConfirmationCounter = -2;
            offsetX = -0x400;
        }

        if (energy > 0x2D00)
            energy = 0x2D00;

        mmodel_vX = (short)-energy;
        const int n_damage = 1;
        const int energy_factor = 0x12C0 / n_damage;
        fist_energy[fist_top] = 2 + (energy - 0x1A40) / energy_factor;
        dx = dy = 0;
        x = pRayman.x + pRayman.dx + offsetX;
        y = pRayman.y - pRayman.dy;
        stateFlag &= ~ACTOR_STATE.DEAD;
        anim.newAction = 1;
    }

    public bool Fist_CheckCollision(Actor des)
    {
        if ((stateFlag & ACTOR_STATE.DEAD) != 0)
            return false;

        if (!GameObj_checkCollsion(des)) 
            return false;
        
        actorReference = des;
        return true;
    }

    public int getAvailableFist()
    {
        if ((GameMidlet.Instance_Game.pFist[0].stateFlag & ACTOR_STATE.DEAD) != 0)
            return 0;
        if ((GameMidlet.Instance_Game.pFist[1].stateFlag & ACTOR_STATE.DEAD) != 0)
            return 1;
        return -1;
    }

    public void LevelSign_ai()
    {
        int action = anim.curAction;
        switch (action)
        {
            case 0:
            case 2:
                if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman))
                    GameMidlet.Instance_Game.pRayman.actorReference = this;
                else if (GameMidlet.Instance_Game.pRayman.actorReference == this)
                    GameMidlet.Instance_Game.pRayman.actorReference = null;
                break;
           
            case 1:
                if ((V[0] <= 8 && V[0] <= GameMidlet.Instance_Game.m_gameFrame_unlockedLevel) ||
                    (GameMidlet.Instance_Game.s_iLumsTaken >= 140 && GameMidlet.Instance_Game.s_iCageOpened >= 12))
                    anim.newAction = 4;
                break;
            
            case 4:
                if (Ani_CheckEnd())
                    anim.newAction = 2;
                break;
            
            case 3:
                int raymanAction = GameMidlet.Instance_Game.pRayman.anim.curAction;
                if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman) && raymanAction is 0 or 2 or 4 or 3 or 5 or 6 or 15 or 16 or 13 or 39 or 40)
                {
                    if (GameMidlet.Instance_Game.pRayman.actorReference != this)
                    {
                        GameMidlet.Instance_Game.pRayman.actorReference = this;
                        GameMidlet.Instance_Game.m_iGlobalTicker = 21;
                    }
                }
                else if (GameMidlet.Instance_Game.pRayman.actorReference == this)
                {
                    GameMidlet.Instance_Game.pRayman.actorReference = null;
                }
                break;
        }
    }

    public void LevelPost_ai()
    {
        int action = anim.curAction;
        switch (action)
        {
            case 0:
                if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman) && (GameMidlet.Instance_Game.pRayman.anim.curAction == 0 || GameMidlet.Instance_Game.pRayman.anim.curAction == 2))
                    Actor_Death();
                break;
        }
    }

    public void Bullet_ai()
    {
        if (!GameMidlet.Instance_Game.Camera_IsVisible(this))
            stateFlag |= ACTOR_STATE.DEAD;

        if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman))
        {
            GameMidlet.Instance_Game.pRayman.doDamage();
            stateFlag |= ACTOR_STATE.DEAD;
        }
    }

    public void Pirate_ai()
    {
        if (checkFloor())
        {
            mmodel_type = 0;
        }
        else
        {
            mmodel_fY = 0x700;
            mmodel_aY = 0x100;
            mmodel_type = MM_TYPE.SET_ACCELERATION_Y_SET_TARGET_SPEED_Y;
        }

        switch (anim.curAction)
        {
            case 0:
                if ((GameMidlet.Instance_Game.pRayman.stateFlag & ACTOR_STATE.DEAD) == 0)
                {
                    if (V[3] % 15 == 0)
                    {
                        if ((V[2] & 0x10000) != 0)
                        {
                            anim.newAction = 1;
                            V[3] -= 15;
                        }
                        else if ((V[2] & 0x1) != 0)
                        {
                            anim.newAction = 3;
                            V[3] -= 15;
                        }
                        else
                        {
                            V[3] -= 1;
                        }

                        V[2] >>= 1;
                    }
                    else
                    {
                        V[3] -= 1;
                    }

                    if (V[3] <= 0)
                    {
                        V[2] = m_iInitV[2];
                        V[3] = m_iInitV[3];
                    }
                }
                break;

            case 1:
                if (Ani_CheckEnd())
                {
                    Actor bullet = GameMidlet.Instance_Game.actors[V[0]];
                    if (((stateFlag & ACTOR_STATE.FLIP_X) == 0 && bullet.mmodel_vX > 0) ||
                        ((stateFlag & ACTOR_STATE.FLIP_X) != 0 && bullet.mmodel_vX < 0))
                        bullet.mmodel_vX = (short)-bullet.mmodel_vX;
                    bullet.x = x - 0x2800 * ((stateFlag & ACTOR_STATE.FLIP_X) == 0 ? 1 : -1) + dx;
                    bullet.y = y - 0x2E00 + dy;
                    bullet.stateFlag &= ~ACTOR_STATE.DEAD;
                    V[4] = 0;
                    anim.newAction = 2;
                }
                break;

            case 2:
                if (Ani_CheckEnd())
                    anim.newAction = 0;
                break;

            case 3:
                if (Ani_CheckEnd())
                {
                    Actor bullet = GameMidlet.Instance_Game.actors[V[0]];
                    if (((stateFlag & ACTOR_STATE.FLIP_X) == 0 && bullet.mmodel_vX > 0) ||
                        ((stateFlag & ACTOR_STATE.FLIP_X) != 0 && bullet.mmodel_vX < 0))
                        bullet.mmodel_vX = (short)-bullet.mmodel_vX;
                    bullet.x = x - 0x2800 * ((stateFlag & ACTOR_STATE.FLIP_X) == 0 ? 1 : -1) + dx;
                    bullet.y = y - 0x1400 + dy;
                    bullet.stateFlag &= ~ACTOR_STATE.DEAD;
                    V[4] = 0;
                    anim.newAction = 4;
                }
                break;

            case 4:
                if (Ani_CheckEnd())
                    anim.newAction = 0;
                break;
            
            case 5:
                if (Ani_CheckEnd())
                {
                    while (V[3] % 15 != 0)
                        V[3] -= 1;
                    for (int i = 0; i < 16; i++)
                    {
                        if ((V[2] & 0x10000) != 0 || (V[2] & 0x1) != 0)
                            break;
                        V[2] >>= 1;
                        V[3] -= 15;
                        if (V[3] == 0)
                        {
                            V[2] = m_iInitV[2];
                            V[3] = m_iInitV[3];
                        }
                    }

                    anim.newAction = 0;
                }
                break;

            case 6:
                if (Ani_CheckEnd())
                {
                    if ((stateFlag & ACTOR_STATE.OVERRIDE_ON_DEATH) != 0)
                        stateFlag |= ACTOR_STATE.OVERRIDEN;
                    else
                        Actor_Death();
                }
                return;
        }

        bool pirateHit = false;
        if (V[4] == 0)
        {
            for (int f = 0; f < 2; f++)
            {
                pirateHit = GameMidlet.Instance_Game.pFist[f].Fist_CheckCollision(this);
                if (pirateHit)
                    break;
            }
        }
        else
        {
            V[4] -= 1;
        }

        if (pirateHit)
        {
            V[1] -= fist_energy[fist_top];
            if (V[1] > 0)
            {
                anim.newAction = 5;
                V[4] = 50;
                GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.enemy_hit, true);
            }
            else
            {
                anim.newAction = 6;
                GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.enemy_death, true);
            }
        }
        else if (GameObj_checkCollsion(GameMidlet.Instance_Game.pRayman))
        {
            GameMidlet.Instance_Game.pRayman.doDamage();
            if (((stateFlag & ACTOR_STATE.FLIP_X) == 0 && (GameMidlet.Instance_Game.pRayman.stateFlag & ACTOR_STATE.FLIP_X) != 0) ||
                ((stateFlag & ACTOR_STATE.FLIP_X) != 0 && (GameMidlet.Instance_Game.pRayman.stateFlag & ACTOR_STATE.FLIP_X) == 0))
            {
                GameMidlet.Instance_Game.pRayman.mmodel_vX = 0;
                GameMidlet.Instance_Game.pRayman.dx = 0;
            }
        }
    }

    public bool checkCeilingStandUp()
    {
        int nx = (int)(x >> 8 >> 4);
        int ny = (int)(y >> 8 >> 4);
        int height = aniData[0].frames[aniData[0].actions[0].Frames[0]].Box.Top;
        int heightInTiles = Math.Abs(height / 16);
        if (height % 16 != 0)
            heightInTiles++;
        for (int i = 1; i <= heightInTiles; i++)
        {
            PHB_TYPE phb = Actor_GetPHB(nx, ny - i);
            if (phb is PHB_TYPE.SOLID or PHB_TYPE.HANG)
                return true;
        }

        return false;
    }

    public void jumpUp(int vX, int deltaX)
    {
        if (actorReference is { objType: OBJECT_TYPE.PLATFORM_2 or OBJECT_TYPE.PLATFORM_1 })
        {
            actorReference = null;
            dy = (short)(dy - 0x200);
        }

        if (vX > 0)
        {
            if (checkClimb())
                mmodel_vX = (short)-(vX - deltaX);
            else
                mmodel_vX = (short)(mmodel_vX - vX - deltaX);

            stateFlag &= ~ACTOR_STATE.FLIP_Y;
            stateFlag |= ACTOR_STATE.FLIP_X;
        }
        else if (vX < 0)
        {
            if (checkClimb())
                mmodel_vX = (short)(vX + deltaX);
            else
                mmodel_vX = (short)(mmodel_vX + vX + deltaX);

            stateFlag &= ~ACTOR_STATE.FLIP_Y;
            stateFlag &= ~ACTOR_STATE.FLIP_X;
        }
        else
        {
            mmodel_vX = (short)((stateFlag & (ACTOR_STATE.FLIP_X | ACTOR_STATE.FLIP_Y)) != 0 ? -deltaX : deltaX);
        }

        anim.newAction = 7;
        if (mmodel_vX > 0x700)
            mmodel_vX = 0x700;
        else if (mmodel_vX < -0x700)
            mmodel_vX = -0x700;

        V[16] = (int)(y >> 8);
        V[15] = 0;
        mmodel_aX = 0;
    }

    public void startFly(int vX)
    {
        if ((V[0] & 0x2) != 2)
            return;

        anim.newAction = 9;
        mmodel_aX = 0;
        if (vX > 0)
        {
            mmodel_vX = (short)-vX;
            stateFlag &= ~ACTOR_STATE.FLIP_Y;
            stateFlag |= ACTOR_STATE.FLIP_X;
        }
        else if (vX < 0)
        {
            mmodel_vX = (short)vX;
            stateFlag &= ~ACTOR_STATE.FLIP_Y;
            stateFlag &= ~ACTOR_STATE.FLIP_X;
        }

        V[6] = 0;
        V[0] = (int)(V[0] & 0xFFFFFFFD);
    }

    public void handleKeyEvent()
    {
        int action = anim.curAction;
        int bug = -1;
        try
        {
            switch (action)
            {
                case 17:
                    if (anim.oldAction is 11 or 9 or 7 or 8 or 10)
                    {
                        yDirectionConfirmed = 0;
                        yDirectionConfirmationCounter = 0;
                    }

                    if (((GAME_KEY)V[4] & GAME_KEY.Middle) != 0 && 
                        (fist_top = getAvailableFist()) != -1 &&
                        (GameMidlet.Instance_Game.pFist[0].stateFlag & ACTOR_STATE.DEAD) != 0)
                    {
                        anim.newAction = 25;
                        fist_time[fist_top] = 0;
                        return;
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Up) != 0)
                    {
                        bool canJump = checkClimbJump(4);
                        if (((GAME_KEY)V[4] & GAME_KEY.Up) != 0)
                        {
                            if (canJump)
                            {
                                y -= 0x1000L;
                                jumpUp(0, 0);
                            }
                        }
                        else if (!canJump)
                        {
                            anim.newAction = 18;
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Down) != 0)
                    {
                        bool canJump = checkClimbJump(8);
                        if (((GAME_KEY)V[4] & GAME_KEY.Down) != 0)
                        {
                            if (canJump)
                            {
                                anim.newAction = 11;
                                V[15] = 0;
                                y += 0x1800L;
                            }
                        }
                        else if (!canJump)
                        {
                            anim.newAction = 20;
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Left) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        bool canJump = checkClimbJump(1);
                        if (((GAME_KEY)V[4] & GAME_KEY.Left) != 0)
                        {
                            if (canJump)
                                xDirectionConfirmationCounter -= 2;
                        }
                        else if (!canJump)
                        {
                            anim.newAction = 22;
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Right) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        bool canJump = checkClimbJump(2);
                        if (((GAME_KEY)V[4] & GAME_KEY.Right) != 0)
                        {
                            if (canJump)
                                xDirectionConfirmationCounter += 2;
                        }
                        else if (!canJump)
                        {
                            anim.newAction = 22;
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.UpRight) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        bool canJump = checkClimbJump(6);
                        if (((GAME_KEY)V[4] & GAME_KEY.UpRight) != 0)
                        {
                            if (canJump)
                            {
                                y -= 0x1000L;
                                x += 0x1000L;
                                if (x + (colBox.Right << 8) > GameMidlet.Instance_Game.m_sBackgroundWidth << 4 << 8)
                                {
                                    x = ((GameMidlet.Instance_Game.m_sBackgroundWidth << 4) - colBox.Right + 2) << 8;
                                    jumpUp(0, 0);
                                }
                                else
                                {
                                    jumpUp(1792, 0);
                                }
                            }
                        }
                        else if (!canJump)
                        {
                            anim.newAction = 19;
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.UpLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        bool canJump = checkClimbJump(5);
                        if (((GAME_KEY)V[4] & GAME_KEY.UpLeft) != 0)
                        {
                            if (canJump)
                            {
                                x -= 0x1000L;
                                y -= 0x1000L;
                                if (x + (colBox.Left << 8) < 0L)
                                {
                                    x = -colBox.Left << 8;
                                    jumpUp(0, 0);
                                }
                                else
                                {
                                    jumpUp(-0x700, 0);
                                }
                            }
                        }
                        else if (!canJump)
                        {
                            anim.newAction = 19;
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.DownRight) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        bool canFall = checkClimbJump(8);
                        bool canMoveSide = checkClimbJump(2);
                        if (((GAME_KEY)V[4] & GAME_KEY.DownRight) != 0)
                        {
                            if (canFall)
                            {
                                anim.newAction = 11;
                                V[15] = 0;
                                y += 0x1800L;
                            }
                        }
                        else if (!(canFall | canMoveSide))
                        {
                            anim.newAction = 21;
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.DownLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        bool canFall = checkClimbJump(8);
                        bool canMoveSide = checkClimbJump(1);
                        if (((GAME_KEY)V[4] & GAME_KEY.DownLeft) != 0)
                        {
                            if (canFall)
                            {
                                anim.newAction = 11;
                                V[15] = 0;
                                y += 0x1800L;
                            }
                        }
                        else if (!(canFall | canMoveSide))
                        {
                            anim.newAction = 21;
                        }
                    }
                    break;

                case 18:
                    if (((GAME_KEY)V[3] & (GAME_KEY.Up | GAME_KEY.UpLeft | GAME_KEY.UpRight)) != 0 && checkClimbJump(4))
                    {
                        if (((GAME_KEY)V[4] & GAME_KEY.Up) != 0)
                            jumpUp(0, 0);
                        else if (((GAME_KEY)V[4] & GAME_KEY.UpLeft) != 0)
                            jumpUp(-0x700, 0);
                        else if (((GAME_KEY)V[4] & GAME_KEY.UpRight) != 0)
                            jumpUp(0x700, 0);
                        else
                            anim.newAction = 17;
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Up) == 0)
                        anim.newAction = 17;
                    break;

                case 20:
                    const GAME_KEY allDownKeys = GAME_KEY.DownLeft | GAME_KEY.Down | GAME_KEY.DownRight;
                    if (((GAME_KEY)V[3] & allDownKeys) != 0 && checkClimbJump(8))
                    {
                        if (((GAME_KEY)V[4] & allDownKeys) != 0)
                        {
                            anim.newAction = 11;
                            V[15] = 0;
                            y += 0x1000L;
                        }
                        else
                        {
                            anim.newAction = 17;
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Down) == 0)
                        anim.newAction = 17;
                    break;

                case 19:
                    if (((GAME_KEY)V[3] & GAME_KEY.UpLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        bool canJumpUp = checkClimbJump(4);
                        bool canJumpLeft = checkClimbJump(1);
                        if (canJumpUp && !canJumpLeft)
                            mmodel_vY = 0;
                        else if (!canJumpUp && canJumpLeft)
                            mmodel_vX = 0;
                        else if (canJumpUp && canJumpLeft)
                            anim.newAction = 17;
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.UpRight) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        bool canJumpUp = checkClimbJump(4);
                        bool canJumpRight = checkClimbJump(2);
                        if (canJumpUp && !canJumpRight)
                            mmodel_vY = 0;
                        else if (!canJumpUp && canJumpRight)
                            mmodel_vX = 0;
                        else if (canJumpUp && canJumpRight)
                            anim.newAction = 17;
                    }

                    if (((GAME_KEY)V[3] & ((stateFlag & ACTOR_STATE.FLIP_X) == 0 ? GAME_KEY.UpLeft : GAME_KEY.UpRight)) == 0)
                        anim.newAction = 17;
                    break;

                case 21:
                    if (((GAME_KEY)V[3] & GAME_KEY.DownLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        bool canJumpDown = checkClimbJump(8);
                        bool canJumpLeft = checkClimbJump(1);
                        
                        if (canJumpDown && !canJumpLeft)
                            mmodel_vY = 0;
                        else if (!canJumpDown && canJumpLeft)
                            mmodel_vX = 0;
                        else if (canJumpDown && canJumpLeft)
                            anim.newAction = 17;
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.DownRight) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        bool canJumpDown = checkClimbJump(8);
                        bool canJumpRight = checkClimbJump(2);
                        if (canJumpDown && !canJumpRight)
                            mmodel_vY = 0;
                        else if (!canJumpDown && canJumpRight)
                            mmodel_vX = 0;
                        else if (canJumpDown && canJumpRight)
                            anim.newAction = 17;
                    }

                    if (((GAME_KEY)V[3] & ((stateFlag & ACTOR_STATE.FLIP_X) == 0 ? GAME_KEY.DownLeft : GAME_KEY.DownRight)) == 0)
                        anim.newAction = 17;
                    break;

                case 22:
                    if (((GAME_KEY)V[3] & GAME_KEY.Left) != 0)
                    {
                        bool canJump = checkClimbJump(1);
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        if (canJump)
                            anim.newAction = 17;
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Right) != 0)
                    {
                        bool canJump = checkClimbJump(2);
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        if (canJump)
                            anim.newAction = 17;
                    }

                    if (((GAME_KEY)V[3] & ((stateFlag & ACTOR_STATE.FLIP_X) == 0 ? GAME_KEY.Left : GAME_KEY.Right)) == 0)
                        anim.newAction = 17;
                    break;
                
                case 27:
                    if (((GAME_KEY)V[4] & GAME_KEY.Up) != 0)
                        jumpUp(0, 0);
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpRight) != 0)
                        jumpUp(0x700, 0);
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpLeft) != 0)
                        jumpUp(-0x700, 0);
                    else
                        return;

                    x = V[13] + ((colBox.Left + colBox.Right) >> 1);
                    y = V[14] + ((colBox.Top + colBox.Bottom) >> 1);
                    x <<= 8;
                    y <<= 8;
                    step();
                    actorReference = null;
                    break;

                case 0:
                case 12:
                case 39:
                case 40:
                    if ((GAME_KEY)V[3] == GAME_KEY.None && (GAME_KEY)V[4] == GAME_KEY.None)
                        yDirectionConfirmationCounter = yDirectionConfirmed = 0;
                    const GAME_KEY allUpKeys = GAME_KEY.UpLeft | GAME_KEY.Up | GAME_KEY.UpRight;
                    if (((GAME_KEY)V[3] & allUpKeys) != 0 && ((GAME_KEY)V[4] & allUpKeys) != 0)
                    {
                        if (((GAME_KEY)V[4] & GAME_KEY.Up) != 0)
                            jumpUp(0, 0);
                        else if (((GAME_KEY)V[4] & GAME_KEY.UpLeft) != 0)
                            jumpUp(-0x700, 0);
                        else if (((GAME_KEY)V[4] & GAME_KEY.UpRight) != 0)
                            jumpUp(0x700, 0);
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Down) != 0)
                        anim.newAction = 3;
                    
                    if (((GAME_KEY)V[3] & GAME_KEY.Left) != 0)
                    {
                        if ((stateFlag & ACTOR_STATE.FLIP_X) == 0 && xDirectionConfirmationCounter < -1)
                        {
                            anim.newAction = 2;
                        }
                        else
                        {
                            stateFlag &= ~ACTOR_STATE.FLIP_Y;
                            stateFlag &= ~ACTOR_STATE.FLIP_X;
                            if (actorReference is { objType: OBJECT_TYPE.PLATFORM_2 or OBJECT_TYPE.PLATFORM_1 } &&
                                actorReference.dy != 0 && actorReference.dx == 0)
                            {
                                xDirectionConfirmed = false;
                                xDirectionConfirmationCounter = -2;
                            }
                            else
                            {
                                xDirectionConfirmationCounter--;
                            }
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Right) != 0)
                    {
                        if ((stateFlag & ACTOR_STATE.FLIP_X) != 0 && xDirectionConfirmationCounter > 1)
                        {
                            anim.newAction = 2;
                        }
                        else
                        {
                            stateFlag &= ~ACTOR_STATE.FLIP_Y;
                            stateFlag |= ACTOR_STATE.FLIP_X;
                            if (actorReference is { objType: OBJECT_TYPE.PLATFORM_2 or OBJECT_TYPE.PLATFORM_1 } &&
                                actorReference.dy != 0 && actorReference.dx == 0)
                            {
                                xDirectionConfirmed = true;
                                xDirectionConfirmationCounter = 2;
                            }
                            else
                            {
                                xDirectionConfirmationCounter++;
                            }
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.DownLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        anim.newAction = 3;
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.DownRight) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        anim.newAction = 3;
                    }

                    if (((GAME_KEY)V[4] & GAME_KEY.Middle) != 0)
                    {
                        if (actorReference is { objType: OBJECT_TYPE.LEVEL_SIGN } &&
                            actorReference.anim.curAction is 2 or 0)
                        {
                            if (actorReference.anim.curAction is 2 or 0)
                                GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.lums_red, true);
                            anim.newAction = 37;
                        }
                        else if ((fist_top = getAvailableFist()) != -1)
                        {
                            anim.newAction = 15;
                            fist_time[fist_top] = 0;
                        }
                    }

                    if ((GAME_KEY)V[4] == GAME_KEY.None)
                        V[10]++;
                    else
                        V[10] = 0;
                    break;

                case 2:
                    if (((GAME_KEY)V[3] & GAME_KEY.Left) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                    }
                    else if (((GAME_KEY)V[3] & GAME_KEY.Right) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                    }

                    if (((GAME_KEY)V[4] & GAME_KEY.Up) != 0)
                    {
                        jumpUp(0, 0);
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.Down) != 0)
                    {
                        anim.newAction = 3;
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpRight) != 0)
                    {
                        jumpUp(0x700, -mmodel_vX);
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpLeft) != 0)
                    {
                        jumpUp(-0x700, -mmodel_vX);
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.DownLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        anim.newAction = 3;
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.DownRight) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        anim.newAction = 3;
                    }
                    else if (((GAME_KEY)V[3] & GAME_KEY.Right) == 0 && ((GAME_KEY)V[3] & GAME_KEY.Left) == 0)
                    {
                        anim.newAction = 0;
                    }

                    if (((GAME_KEY)V[4] & GAME_KEY.Middle) != 0 && (fist_top = getAvailableFist()) != -1)
                    {
                        anim.newAction = 15;
                        fist_time[fist_top] = 0;
                    }
                    break;

                case 9:
                    if (V[7] != 0)
                    {
                        if (((GAME_KEY)V[3] & GAME_KEY.Up) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpLeft) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpRight) != 0)
                        {
                            mmodel_vY = -0x200;
                        }
                        else
                        {
                            if (mmodel_vY < 0)
                                mmodel_vY = 0;
                            mmodel_vY = (short)(mmodel_vY + 0x100);
                            if (mmodel_vY > 0x200)
                                mmodel_vY = 0x200;
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Left) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpLeft) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        if (mmodel_vX > -0x380)
                        {
                            mmodel_vX = (short)(mmodel_vX - 0x100);
                            if (mmodel_vX < -0x380)
                                mmodel_vX = -0x380;
                        }
                    }
                    else if (((GAME_KEY)V[3] & GAME_KEY.Right) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpRight) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownRight) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        if (mmodel_vX > -0x380)
                        {
                            mmodel_vX = (short)(mmodel_vX - 0x100);
                            if (mmodel_vX < -0x380)
                                mmodel_vX = -0x380;
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Left) == 0 && ((GAME_KEY)V[3] & GAME_KEY.UpLeft) == 0 && ((GAME_KEY)V[3] & GAME_KEY.DownLeft) == 0 &&
                        ((GAME_KEY)V[3] & GAME_KEY.Right) == 0 && ((GAME_KEY)V[3] & GAME_KEY.UpRight) == 0 && ((GAME_KEY)V[3] & GAME_KEY.DownRight) == 0)
                        mmodel_vX = 0;

                    if (((GAME_KEY)V[3] & GAME_KEY.Up) == 0 && ((GAME_KEY)V[3] & GAME_KEY.UpLeft) == 0 && ((GAME_KEY)V[3] & GAME_KEY.UpRight) == 0)
                    {
                        if (V[7] != 0)
                        {
                            V[8]++;
                            if (V[8] < 5)
                            {
                                mmodel_vY = 0;
                                dy = 0;
                            }
                        }
                    }
                    else
                    {
                        V[8] = 0;
                    }

                    if (((GAME_KEY)V[4] & GAME_KEY.Middle) != 0 && (fist_top = getAvailableFist()) != -1)
                    {
                        if (V[7] != 0)
                        {
                            anim.newAction = 34;
                        }
                        else
                        {
                            anim.newAction = 10;
                            GameMidlet.Instance_Game.pFist[fist_top].Fist_launch(stateFlag & ACTOR_STATE.FLIP_X, 0x21C0);
                        }
                    }
                    break;

                case 34:
                case 35:
                    if (((GAME_KEY)V[5] & GAME_KEY.Middle) != 0)
                    {
                        anim.newAction = 36;
                        fist_top = getAvailableFist();
                        if (fist_top != -1)
                            GameMidlet.Instance_Game.pFist[fist_top].Fist_launch(stateFlag & ACTOR_STATE.FLIP_X, 8640);
                    }
                    break;

                case 30:
                    if (((GAME_KEY)V[3] & GAME_KEY.Left) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        anim.newAction = 31;
                    }
                    else if (((GAME_KEY)V[3] & GAME_KEY.Right) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpRight) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        anim.newAction = 31;
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Down) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownLeft) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownRight) != 0)
                    {
                        yDirectionConfirmed = 1;
                        yDirectionConfirmationCounter = 5;
                        anim.newAction = 11;
                        y += 0x1000;
                    }
                    break;

                case 31:
                    if (((GAME_KEY)V[3] & GAME_KEY.Left) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                    }
                    else if (((GAME_KEY)V[3] & GAME_KEY.Right) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpRight) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                    }

                    if (((GAME_KEY)V[5] & GAME_KEY.Down) != 0 || ((GAME_KEY)V[5] & GAME_KEY.DownLeft) != 0 || ((GAME_KEY)V[5] & GAME_KEY.DownRight) != 0)
                    {
                        yDirectionConfirmed = 1;
                        yDirectionConfirmationCounter = 5;
                        anim.newAction = 11;
                        y += 4096L;
                        break;
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Left) == 0 && ((GAME_KEY)V[3] & GAME_KEY.UpLeft) == 0 && 
                        ((GAME_KEY)V[3] & GAME_KEY.Right) == 0 && ((GAME_KEY)V[3] & GAME_KEY.UpRight) == 0)
                        anim.newAction = 30;
                    break;

                case 11:
                    if (((GAME_KEY)V[4] & GAME_KEY.Up) != 0 || ((GAME_KEY)V[4] & GAME_KEY.UpRight) != 0 || ((GAME_KEY)V[4] & GAME_KEY.UpLeft) != 0)
                        startFly(0);
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpRight) != 0)
                        startFly(0x700);
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpLeft) != 0)
                        startFly(-0x700);

                    if (((GAME_KEY)V[3] & GAME_KEY.Left) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpLeft) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        if (mmodel_vX > -0x700)
                        {
                            mmodel_vX = (short)(mmodel_vX - 0x100);
                            if (mmodel_vX < -0x700)
                                mmodel_vX = -0x700;
                        }
                    }
                    else if (((GAME_KEY)V[3] & GAME_KEY.Right) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpRight) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownRight) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        if (mmodel_vX > -0x700)
                        {
                            mmodel_vX = (short)(mmodel_vX - 0x100);
                            if (mmodel_vX < -0x700)
                                mmodel_vX = -0x700;
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Left) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpLeft) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownLeft) != 0 ||
                        ((GAME_KEY)V[3] & GAME_KEY.Right) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpRight) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownRight) == 0)
                    {

                    }
                    if (((GAME_KEY)V[4] & GAME_KEY.Middle) != 0 && (fist_top = getAvailableFist()) != -1)
                    {
                        anim.newAction = 10;
                        GameMidlet.Instance_Game.pFist[fist_top].Fist_launch(stateFlag & ACTOR_STATE.FLIP_X, 0x21C0);
                    }
                    break;
                
                case 7:
                    if (anim.oldAction is 28 or 29)
                    {
                        mmodel_vY = (short)(mmodel_vY - 0x100);
                        V[9] = 0x7FFFFFF;
                    }
                    else
                    {
                        V[9] = 0;
                    }
                    break;

                case 8:
                    if (((GAME_KEY)V[4] & GAME_KEY.Up) != 0)
                    {
                        if (mmodel_vX < 0)
                            mmodel_vX = 0;
                        mmodel_vY = 0;
                        mmodel_aY = 0;
                        startFly(0);
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpRight) != 0)
                    {
                        mmodel_vY = 0;
                        mmodel_aY = 0;
                        startFly(0x700);
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpLeft) != 0)
                    {
                        mmodel_vY = 0;
                        mmodel_aY = 0;
                        startFly(-0x700);
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Up) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpLeft) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpRight) != 0)
                    {
                        V[9] += 1;
                        if (V[9] < 8)
                            mmodel_vY = (short)(mmodel_vY - 0x99);
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Left) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpLeft) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        if (mmodel_vX > -0x700)
                        {
                            mmodel_vX = (short)(mmodel_vX - 0x100);
                            if (mmodel_vX < -0x700)
                                mmodel_vX = -0x700;
                        }
                    }
                    else if (((GAME_KEY)V[3] & GAME_KEY.Right) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpRight) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownRight) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        if (mmodel_vX > -0x700)
                        {
                            mmodel_vX = (short)(mmodel_vX - 0x100);
                            if (mmodel_vX < -0x700)
                                mmodel_vX = -0x700;
                        }
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Left) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpLeft) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownLeft) != 0 ||
                        ((GAME_KEY)V[3] & GAME_KEY.Right) != 0 || ((GAME_KEY)V[3] & GAME_KEY.UpRight) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownRight) == 0)
                    {

                    }
                    if (((GAME_KEY)V[4] & GAME_KEY.Middle) != 0 && (fist_top = getAvailableFist()) != -1)
                    {
                        anim.newAction = 10;
                        GameMidlet.Instance_Game.pFist[fist_top].Fist_launch(stateFlag & ACTOR_STATE.FLIP_X, 0x21C0);
                    }
                    break;

                case 4:
                    if (((GAME_KEY)V[3] & GAME_KEY.Left) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        anim.newAction = 5;
                    }
                    else if (((GAME_KEY)V[3] & GAME_KEY.Right) != 0 || ((GAME_KEY)V[3] & GAME_KEY.DownRight) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        anim.newAction = 5;
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Down) == 0 && ((GAME_KEY)V[3] & GAME_KEY.DownLeft) == 0 && ((GAME_KEY)V[3] & GAME_KEY.DownRight) == 0)
                        if (!checkCeilingStandUp())
                            anim.newAction = 6;
                    break;
                case 5:
                    if (((GAME_KEY)V[3] & GAME_KEY.DownLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                    }
                    else if (((GAME_KEY)V[3] & GAME_KEY.DownRight) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                    }
                    else if (((GAME_KEY)V[3] & GAME_KEY.Down) != 0)
                    {
                        anim.newAction = 4;
                    }

                    if (((GAME_KEY)V[3] & GAME_KEY.Down) == 0 && ((GAME_KEY)V[3] & GAME_KEY.DownLeft) == 0 && ((GAME_KEY)V[3] & GAME_KEY.DownRight) == 0)
                    {
                        if (!checkCeilingStandUp())
                            anim.newAction = 6;
                        else
                            anim.newAction = 4;
                    }
                    break;

                case 6:
                    if (((GAME_KEY)V[4] & GAME_KEY.Middle) != 0 && !checkCeilingStandUp())
                    {
                        anim.newAction = 15;
                        if (fist_top != -1)
                            fist_time[fist_top] = 0;
                    }
                    break;
                
                case 13:
                    if (((GAME_KEY)V[4] & GAME_KEY.Up) != 0)
                    {
                        jumpUp(0, 0);
                    }
                    else if (((GAME_KEY)V[3] & GAME_KEY.Down) != 0)
                    {
                        anim.newAction = 3;
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.Left) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        anim.newAction = 2;
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.Right) != 0)
                    {
                        anim.newAction = 2;
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpRight) != 0)
                    {
                        jumpUp(1792, 0);
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpLeft) != 0)
                    {
                        jumpUp(-1792, 0);
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.DownLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                        anim.newAction = 3;
                    }
                    // NOTE: Bug! Should be DownRight
                    else if (((GAME_KEY)V[4] & GAME_KEY.DownLeft) != 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                        anim.newAction = 3;
                    }
                    break;

                case 15:
                    if (((GAME_KEY)V[5] & GAME_KEY.Middle) != 0)
                    {
                        anim.newAction = 13;
                        fist_top = getAvailableFist();
                        if (fist_top != -1)
                            GameMidlet.Instance_Game.pFist[fist_top].Fist_launch(stateFlag & ACTOR_STATE.FLIP_X, fist_time[fist_top] * 0xF0 + 0x1A40);
                    }
                    break;

                case 16:
                    if (((GAME_KEY)V[5] & GAME_KEY.Middle) != 0)
                    {
                        anim.newAction = 13;
                        fist_top = getAvailableFist();
                        if (fist_top != -1)
                            GameMidlet.Instance_Game.pFist[fist_top].Fist_launch(stateFlag & ACTOR_STATE.FLIP_X, fist_time[fist_top] * 0xF0 + 0x1A40);
                    }
                    bug = 8;
                    break;

                case 25:
                    if (((GAME_KEY)V[5] & GAME_KEY.Middle) != 0)
                    {
                        anim.newAction = 23;
                        fist_top = getAvailableFist();
                        if (fist_top != -1)
                            GameMidlet.Instance_Game.pFist[fist_top].Fist_launch(stateFlag & ACTOR_STATE.FLIP_X, fist_time[fist_top] * 0xF0 + 0x1A40);
                    }
                    break;
                
                case 28:
                    yDirectionConfirmed = 0;
                    yDirectionConfirmationCounter = 0;
                    if (((GAME_KEY)V[4] & GAME_KEY.Up) != 0)
                    {
                        if ((stateFlag & ACTOR_STATE.FLIP_X) == 0)
                            jumpUp(-0x100, 0);
                        else
                            jumpUp(0x100, 0);
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.Down) != 0)
                    {
                        y += 0x1000L;
                        if ((stateFlag & ACTOR_STATE.FLIP_X) == 0)
                            x += 0x200L;
                        else
                            x -= 0x200L;

                        anim.newAction = 11;
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpRight) != 0)
                    {
                        jumpUp(0x100, 0);
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpLeft) != 0)
                    {
                        jumpUp(-0x100, 0);
                    }
                    break;

                case 29:
                    if (((GAME_KEY)V[4] & GAME_KEY.Up) != 0)
                    {
                        if ((stateFlag & ACTOR_STATE.FLIP_X) == 0)
                            jumpUp(-0x100, 0);
                        else
                            jumpUp(0x100, 0);
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.Down) != 0)
                    {
                        y += 0x1000L;
                        if ((stateFlag & ACTOR_STATE.FLIP_X) == 0)
                            x += 0x200L;
                        else
                            x -= 0x200L;

                        anim.newAction = 11;
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpRight) != 0)
                    {
                        jumpUp(0x100, 0);
                    }
                    else if (((GAME_KEY)V[4] & GAME_KEY.UpLeft) != 0)
                    {
                        jumpUp(-0x100, 0);
                    }
                    break;
            }

            GAME_KEY allKeys = GAME_KEY.UpLeft | GAME_KEY.Left | GAME_KEY.DownLeft;
            if (((GAME_KEY)V[4] & allKeys) != 0)
                xDirectionConfirmationCounter--;
            allKeys = GAME_KEY.UpRight | GAME_KEY.Right | GAME_KEY.DownRight;
            if (((GAME_KEY)V[4] & allKeys) != 0)
                xDirectionConfirmationCounter++;
        }
        catch (Exception e)
        {
            System.println($" action {action}  exception {e} bug {bug}");
            System.println($"Game.pFist {GameMidlet.Instance_Game.pFist} fist_time {fist_time}");
            System.println($"fist_top {fist_top} fist_top {fist_top}");
            System.println($"Game.pFist[fist_top] {GameMidlet.Instance_Game.pFist[fist_top]}");
            System.println($" fist_time[fist_top] {fist_time[fist_top]}");
        }
    }

    public bool checkCeilingAir()
    {
        int nx = (int)(x >> 8 >> 4);
        int ny = (int)((((y + dy) >> 8) + colBox.Top) >> 4);
        PHB_TYPE phb = Actor_GetPHB(nx, ny);
        if (phb == PHB_TYPE.SOLID)
        {
            while (phb == PHB_TYPE.SOLID)
                phb = Actor_GetPHB(nx, ++ny);
            y = ((ny << 4) - colBox.Top + 2) << 8;
            dy = 0;
            return true;
        }

        return false;
    }

    public bool canHangRoof()
    {
        int nx = (int)((x + dx) >> 8 >> 4);
        int ny = (int)((((y + dy) >> 8) - 58) >> 4);
        PHB_TYPE phb = Actor_GetPHB(nx, ny);
        if (phb == PHB_TYPE.HANG_ROOF)
        {
            y = ((ny << 4) + 58 + 8) << 8;
            dy = 0;
            return true;
        }

        return false;
    }

    public bool checkClimb()
    {
        int tileX = (int)((x + dx) >> 8 >> 4);
        int topTileY = (int)((((y + dy) >> 8) + colBox.Top) >> 4);
        int bottomTileY = (int)((((y + dy) >> 8) + colBox.Bottom) >> 4);
        if (Actor_GetPHB(tileX, topTileY) == PHB_TYPE.CLIMB && Actor_GetPHB(tileX, topTileY + 1) == PHB_TYPE.CLIMB)
        {
            if (Actor_GetPHB(tileX + 1, topTileY) != PHB_TYPE.CLIMB)
                x = (((tileX << 4) + 16 - colBox.Right) << 8);
            else if (Actor_GetPHB(tileX - 1, topTileY) != PHB_TYPE.CLIMB)
                x = (((tileX << 4) - colBox.Left) << 8);

            if (dy > 0 && Actor_GetPHB(tileX, topTileY - 1) != PHB_TYPE.CLIMB)
                y = (((topTileY << 4) - colBox.Top) << 8);
            else if (dy < 0 && Actor_GetPHB(tileX, topTileY + 2) != PHB_TYPE.CLIMB)
                y = (((topTileY << 4) - colBox.Top) << 8);

            dx = dy = 0;
            V[15] = 1;
        }
        else
        {
            V[15] = 0;
        }

        return V[15] == 1;
    }

    public bool checkClimbJump(int sides)
    {
        int tileX = -100;
        int tileY = -100;
        if ((sides & 0x1) != 0)
            tileX = (int)((((x + dx) >> 8) + colBox.Right) >> 4);
        if ((sides & 0x2) != 0)
            tileX = (int)((((x + dx) >> 8) + colBox.Left - 1L) >> 4);
        if ((sides & 0x4) != 0)
            tileY = (int)((((y + dy) >> 8) + colBox.Top + 8L) >> 4);
        if ((sides & 0x8) != 0)
            tileY = (int)((((y + dy) >> 8) + colBox.Top) >> 4);
        if (tileX == -100)
            tileX = (int)((x + dx) >> 8 >> 4);
        if (tileY == -100)
            tileY = (int)((((y + dy) >> 8) + colBox.Top) >> 4);
        if ((sides & 0x1) != 0)
            tileX--;
        if ((sides & 0x2) != 0)
            tileX++;
        if ((sides & 0x4) != 0)
            tileY--;
        if ((sides & 0x8) != 0)
            tileY += 2;
        if (Actor_GetPHB(tileX, tileY) == PHB_TYPE.NONE)
            return true;
        return false;
    }

    public bool canHangOnLedge()
    {
        bool bFaceLeft = (stateFlag & ACTOR_STATE.FLIP_X) == 0;
        int posX = (int)(x + dx) >> 8;
        int posY = (int)(y + dy) >> 8;
        if (bFaceLeft)
        {
            posX += colBox.Left - 8;
        }
        else
        {
            posX += colBox.Right + 8;
        }

        int tileX = posX >> 4;
        int tileY = (int)(((y + dy) >> 8) + colBox.Top + 16L) >> 4;
        PHB_TYPE phb = Actor_GetPHB(tileX, tileY);
        int behindtileX = tileX;
        if (bFaceLeft)
            behindtileX++;
        else
            behindtileX--;

        PHB_TYPE behindPhb = Actor_GetPHB(behindtileX, tileY);
        if (phb == PHB_TYPE.HANG || behindPhb == PHB_TYPE.HANG)
        {
            anim.newAction = 28;
            if (behindPhb == PHB_TYPE.HANG)
                x = behindtileX << 4;
            else
                x = tileX << 4;

            y = (tileY << 4) - colBox.Top - 8;
            if (bFaceLeft)
                x -= colBox.Left - 16 + 4;
            else
                x -= colBox.Right - 4;

            dx = dy = 0;
            x <<= 8;
            y <<= 8;
            return true;
        }

        return false;
    }

    public void doDamage()
    {
        if (V[12] > 0 || (stateFlag & ACTOR_STATE.DEAD) != 0)
            return;

        GameMidlet.Instance_Game.m_gameFrame_nEnergy = (sbyte)(GameMidlet.Instance_Game.m_gameFrame_nEnergy - (GameMidlet.Instance_Game.m_gameFrame_nEnergy > 0 ? 1 : 0));

        if (GameMidlet.Instance_Game.m_gameFrame_nEnergy == 0)
        {
            anim.newAction = 33;
            GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.rayman_death, true);
        }
        else
        {
            V[1] = 1;
            V[12] = 1;
            GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.rayman_hit, true);
        }

        GameMidlet.Instance_Game.Status_Show(0);
    }

    public bool checkDamage()
    {
        for (int i = (int)(y + (colBox.Top << 8)); i <= y + (colBox.Bottom << 8); i += 0x1000)
        {
            PHB_TYPE phb = Actor_GetPHB((int)(x >> 8 >> 4), i >> 8 >> 4);
            if (phb == PHB_TYPE.TYPE_18)
            {
                anim.newAction = 33;
                GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.rayman_death, true);
                return true;
            }

            if (phb == PHB_TYPE.TYPE_29 || bWasPHBStop)
            {
                anim.newAction = 32;
                bWasPHBStop = false;
                GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.rayman_water, true);
                return true;
            }

            if (phb == PHB_TYPE.TYPE_17)
            {
                doDamage();
                return true;
            }
        }

        if (GameMidlet.Instance_Game.m_gameFrame_nEnergy <= 0)
        {
            anim.newAction = 33;
            GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.rayman_death, true);
            return true;
        }

        return false;
    }

    public bool Rayman_checkAttachPlatform(Actor rpAttach)
    {
        if (actorReference != null || dy < 0)
            return false;

        int nx = (int)(x >> 8);
        int ny = (int)(y >> 8) + 5;
        int l = (int)(rpAttach.colBox.Left + (rpAttach.x >> 8));
        int t = (int)(rpAttach.colBox.Top + (rpAttach.y >> 8));
        int r = (int)(rpAttach.colBox.Right + (rpAttach.x >> 8));
        int b = (int)(rpAttach.colBox.Bottom + (rpAttach.y >> 8));
        if (nx >= l && nx < r && ny >= t && ny < b)
        {
            actorReference = rpAttach;
            dx = dy = 0;
            mmodel_aX = mmodel_aY = mmodel_vX = mmodel_vY = 0;
            y = rpAttach.y + (rpAttach.colBox.Top << 8) + rpAttach.dy;
            anim.newAction = 12;
            return true;
        }

        return false;
    }

    public void Rayman_doPlatform()
    {
        int nx = (int)(x >> 8);
        int l = (int)(actorReference.colBox.Left + (actorReference.x >> 8));
        int r = (int)(actorReference.colBox.Right + (actorReference.x >> 8));
        if (nx < l || nx >= r)
        {
            anim.newAction = 11;
            if (actorReference.dy == 0 && actorReference.dx != 0)
            {
                y += (actorReference.colBox.Bottom - actorReference.colBox.Top + 2) << 8;
                if ((stateFlag & ACTOR_STATE.FLIP_X) == 0)
                    x -= 0x800L;
                else
                    x += 0x800L;
            }

            actorReference = null;
        }
        else
        {
            y = actorReference.y + (actorReference.colBox.Top << 8);
            dy = actorReference.dy;
            dx = (short)(dx + actorReference.dx);
        }
    }

    public void Rayman_ai()
    {
        int action = anim.curAction;
        Actor_SetReferencePoint((int)(x >> 8 >> 4), (int)(y >> 8 >> 4));
        V[4] = (short)GameMidlet.Instance_Game.pressedKey;
        V[5] = (short)GameMidlet.Instance_Game.releasedKey;
        V[3] = (short)GameMidlet.Instance_Game.currentKey;
        if (GameMidlet.Instance_Game.m_gameFrame_curLevel > 0 && GameMidlet.Instance_Game.s_iLeftToDie == 0 && (action is 0 or 2 or 5 or 4 || GameMidlet.Instance_Game.pRayman.anim.curAction == 12))
        {
            V[3] = (short)GAME_KEY.None;
            V[4] = (short)GAME_KEY.None;
            V[5] = (short)GAME_KEY.None;
            anim.newAction = 38;
        }

        if (V[12] > 0)
        {
            V[12] += 1;
            if (V[12] == 25)
                V[12] = 0;
            if (V[1] > 0)
            {
                V[1] += 1;
                if (V[1] == 15)
                    V[1] = 0;
            }
        }

        if (action is 15 or 16 or 25 or 35 && fist_top != -1) 
            fist_time[fist_top]++;

        if (Ani_CheckEnd())
        {
            switch (action)
            {
                case 39:
                case 40:
                    anim.newAction = 0;
                    V[10] = 0;
                    break;
          
                case 27:
                    if ((stateFlag & ACTOR_STATE.FLIP_X) == 0)
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag |= ACTOR_STATE.FLIP_X;
                    }
                    else
                    {
                        stateFlag &= ~ACTOR_STATE.FLIP_Y;
                        stateFlag &= ~ACTOR_STATE.FLIP_X;
                    }

                    step();
                    anim.newAction = 27;
                    break;

                case 34:
                    anim.newAction = 35;
                    break;
                
                case 23:
                    anim.newAction = 17;
                    break;
                
                case 36:
                    if (V[7] != 0)
                    {
                        anim.newAction = 9;
                        break;
                    }

                    anim.newAction = 8;
                    break;
                
                case 10:
                    anim.newAction = 8;
                    break;
                
                case 7:
                    anim.newAction = 8;
                    break;
                
                case 6:
                case 12:
                    anim.newAction = 0;
                    break;
                
                case 3:
                    anim.newAction = 4;
                    break;
                
                case 28:
                    anim.newAction = 29;
                    break;
                
                case 38:
                    if (GameMidlet.Instance_Game.m_gameFrame_unlockedLevel <= GameMidlet.Instance_Game.m_gameFrame_curLevel)
                        GameMidlet.Instance_Game.m_gameFrame_unlockedLevel = (sbyte)(GameMidlet.Instance_Game.m_gameFrame_curLevel + 1);
                    anim.newAction = 0;
                    GameMidlet.Instance_Game.m_gameStateStep = 0;
                    GameMidlet.Instance_Game.GameFrame_PostMessage(MESSAGE_ID.CHANGE_LEVEL, 0);
                    break;
                
                case 37:
                    m_lInitX = actorReference.x;
                    m_lInitY = actorReference.y;
                    anim.newAction = 0;
                    stateFlag |= ACTOR_STATE.DEAD;
                    GameMidlet.Instance_Game.m_gameStateStep = 0;
                    GameMidlet.Instance_Game.GameFrame_PostMessage(MESSAGE_ID.CHANGE_LEVEL, actorReference.V[0]);
                    break;
                
                case 32:
                case 33:
                    stateFlag |= ACTOR_STATE.DEAD;
                    GameMidlet.Instance_Game.GameFrame_PostMessage(MESSAGE_ID.RAYMAN_DEATH, 0);
                    if (GameMidlet.Instance_Game.s_iLeftToDie == 0)
                        GameMidlet.Instance_Game.s_iLeftToDie = 1;
                    return;
                
                case 15:
                    anim.newAction = 16;
                    break;
                
                case 13:
                    anim.newAction = 0;
                    break;
            }
        }

        if (action is 8 or 7 && mmodel_vY > 0) 
            anim.newAction = 11;
        
        handleKeyEvent();
        checkCamera();
        
        if (V[10] > 75)
        {
            GameMidlet.Instance_Game.Status_ShowLock();
            V[11]++;
            if ((V[11] & 0x1) == 0)
            {
                anim.newAction = 39;
            }
            else
            {
                anim.newAction = 40;
                V[11] = 0;
            }

            V[10] = 0;
        }

        if (actorReference is { objType: OBJECT_TYPE.PLATFORM_2 or OBJECT_TYPE.PLATFORM_1 })
            Rayman_doPlatform();

        if (action is 9 or 8 or 7 or 10)
            checkCeilingAir();

        if (action != 27 && checkWall())
            dx = 0;
        
        if (action == 27)
        {
            dx = dy = 0;
        }
        else if (V[15] == 1 || checkClimb())
        {
            switch (action)
            {
                case 8:
                case 9:
                case 10:
                case 11:
                    anim.newAction = 17;
                    break;
            }
        }
        else if (canHangRoof())
        {
            switch (action)
            {
                case 7:
                case 8:
                case 9:
                case 11:
                    anim.newAction = 30;
                    break;
            }
        }
        else
        {
            switch (action)
            {
                case 31:
                    anim.newAction = 11;
                    y += 0x800L;
                    break;
                case 7:
                case 8:
                    if (checkCeilingAir())
                        anim.newAction = 11;
                    break;
            }
        }

        if (anim.newAction != 8 && anim.newAction != 7)
        {
            if (checkFloor())
            {
                switch (action)
                {
                    case 9:
                    case 10:
                    case 11:
                        if (!bWasPHBStop)
                            anim.newAction = 12;
                        break;
                }
            }
            else if (actorReference is { objType: OBJECT_TYPE.PLATFORM_2 or OBJECT_TYPE.PLATFORM_1 })
            {
                switch (action)
                {
                    case 9:
                    case 28:
                    case 29:
                        anim.newAction = 12;
                        break;
                }
            }
            else
            {
                switch (action)
                {
                    case 0:
                    case 2:
                    case 4:
                    case 5:
                        anim.newAction = 11;
                        break;
                    
                    case 9:
                        if (canHangOnLedge())
                            break;
                        V[6]++;
                        if (V[7] == 0 && V[6] > 25)
                        {
                            anim.newAction = 11;
                            V[0] = (int)(V[0] & 0xFFFFFFFD);
                        }
                        break;

                    case 10:
                    case 11:
                        if (canHangOnLedge())
                        {

                        }
                        break;
                }
            }
        }

        if (anim.newAction is 28 or 12 or 30 or 27 or 7 or >= 17 and <= 22)
            V[0] |= 0x2;

        if (action != 33 && action != 32)
            checkDamage();
        
        if (action == 11 && anim.oldAction is 2 or 0 or 5 or 4)
        {
            if (mmodel_vX < 0)
                mmodel_vX = -512;
            else if (mmodel_vX > 0)
                mmodel_vX = 512;
        }

        if (anim.curAction != anim.newAction)
        {
            anim.oldAction = anim.curAction;
            if (anim.curAction == 9)
            {

            }

            switch (anim.newAction)
            {
                case 16:
                case 25:
                case 35:
                    GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.punch_charge, true, 255);
                    break;

                case 9:
                    GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.rayman_helico, true);
                    break;

                case 11:
                    if (anim.curAction != 7 && anim.curAction != 8)
                    {
                        V[16] = (int)(y >> 8);
                        if (mmodel_vX < 0)
                        {
                            mmodel_vX = -768;
                            break;
                        }

                        if (mmodel_vX > 0)
                            mmodel_vX = 768;
                    }
                    break;

                case 7:
                    GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.rayman_jump, true);
                    break;

                case 10:
                case 13:
                case 23:
                    GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.punch_released, true);
                    break;

                case 3:
                    GameMidlet.Instance_Game.PlaySound(SOUND_INDEX.rayman_crouch, true);
                    break;
            }
        }
    }

    public void ai()
    {
        switch (objType)
        {
            case OBJECT_TYPE.SEA_URCHIN:
                SeaUrchin_ai();
                break;

            case OBJECT_TYPE.BOMB:
                Bomb_ai();
                break;
            
            case OBJECT_TYPE.SPIKE:
                Spike_ai();
                break;

            case OBJECT_TYPE.RAYMAN:
                Rayman_ai();
                break;

            case OBJECT_TYPE.FIST:
                Fist_ai();
                break;

            case OBJECT_TYPE.LUM:
            case OBJECT_TYPE.LIFE:
            case OBJECT_TYPE.BLUE_LUM:
            case OBJECT_TYPE.ENERGY:
            case OBJECT_TYPE.SWING_LUM:
            case OBJECT_TYPE.CHECKPOINT:
                Bonus_ai();
                break;

            case OBJECT_TYPE.CAGE:
                Cage_ai();
                break;

            case OBJECT_TYPE.LEVEL_SIGN:
                LevelSign_ai();
                break;

            case OBJECT_TYPE.PIRATE:
                Pirate_ai();
                break;

            case OBJECT_TYPE.BULLET:
                Bullet_ai();
                break;
            
            case OBJECT_TYPE.FLY:
                Fly_ai();
                break;

            case OBJECT_TYPE.TENTACLE:
                Tentacle_ai();
                break;

            case OBJECT_TYPE.LEVEL_POST:
                LevelPost_ai();
                break;

            case OBJECT_TYPE.PLATFORM_1:
            case OBJECT_TYPE.PLATFORM_2:
                Platform_ai();
                break;
        }
    }

    public void Actor_Reset()
    {
        switch (objType)
        {
            case OBJECT_TYPE.RAYMAN:
            case OBJECT_TYPE.FIST:
            case OBJECT_TYPE.BLUE_LUM:
            case OBJECT_TYPE.ENERGY:
            case OBJECT_TYPE.LEVEL_SIGN:
            case OBJECT_TYPE.LEVEL_POST:
            case OBJECT_TYPE.PLATFORM_1:
            case OBJECT_TYPE.PLATFORM_2:
            case OBJECT_TYPE.SEA_URCHIN:
            case OBJECT_TYPE.SPIKE:
            case OBJECT_TYPE.BOMB:
            case OBJECT_TYPE.FLY:
            case OBJECT_TYPE.TENTACLE:
            case OBJECT_TYPE.PIRATE:
            case OBJECT_TYPE.CHECKPOINT:
                x = m_lInitX;
                y = m_lInitY;
                anim.newAction = m_iInitAction;
                stateFlag = m_sInitStateFlag;
                if (m_iInitV != null)
                    System.arraycopy(m_iInitV, 0, V, 0, V.Length);
                dx = 0;
                dy = 0;
                m_sPHBTableReferenceTileX = -10;
                m_sPHBTableReferenceTileY = -10;
                if (m_bPHBTable != null)
                {
                    for (int i = 0; i < m_bPHBTable.GetLength(0); i++)
                    {
                        for (int j = 0; j < m_bPHBTable.GetLength(1); j++)
                        {
                            m_bPHBTable[i, j] = 0;
                        }
                    }
                }

                break;
        }

        if (objType == OBJECT_TYPE.RAYMAN)
        {
            if (GameMidlet.Instance_Game.s_actorCheckpoint != null)
            {
                x = GameMidlet.Instance_Game.s_actorCheckpoint.x;
                y = GameMidlet.Instance_Game.s_actorCheckpoint.y;
                stateFlag &= ~ACTOR_STATE.FLIP_Y;
                stateFlag |= ACTOR_STATE.FLIP_X;
                xDirectionConfirmed = true;
            }

            xDirectionConfirmed = ((stateFlag & ACTOR_STATE.FLIP_X) != 0);
            GameMidlet.Instance_Game.pFist[0].anim.curAction = 0;
            GameMidlet.Instance_Game.pFist[1].anim.curAction = 0;
            xDirectionConfirmationCounter = xDirectionConfirmed ? 2 : -2;
        }
    }

    public void Actor_SetReferencePoint(int tileX, int tileY)
    {
        short offsetX = (short)(tileX - m_sPHBTableReferenceTileX);
        short offsetY = (short)(tileY - m_sPHBTableReferenceTileY);
        int lowRangeX = -m_sTableRefIndexX;
        int highRangeX = m_bPHBTable.GetLength(1) - 1 - m_sTableRefIndexX;
        int lowRangeY = -m_sTableRefIndexY;
        int highRangeY = m_bPHBTable.GetLength(0) - 1 - m_sTableRefIndexY;
        if (offsetX < 0)
        {
            if (offsetY < 0)
            {
                int arrayY = m_bPHBTable.GetLength(0) - 1;
                for (int i = tileY + highRangeY; i >= tileY + lowRangeY; i--)
                {
                    int arrayX = (m_bPHBTable.GetLength(1)) - 1;
                    for (int j = tileX + highRangeX; j >= tileX + lowRangeX; j--)
                    {
                        m_bPHBTable[arrayY, arrayX] = Actor_GetPHB(j, i);
                        arrayX--;
                    }

                    arrayY--;
                }
            }
            else
            {
                sbyte b = 0;
                for (int i = tileY + lowRangeY; i <= tileY + highRangeY; i++)
                {
                    int k = (m_bPHBTable.GetLength(1)) - 1;
                    for (int j = tileX + highRangeX; j >= tileX + lowRangeX; j--)
                    {
                        m_bPHBTable[b, k] = Actor_GetPHB(j, i);
                        k--;
                    }

                    b++;
                }
            }
        }
        else if (offsetY < 0)
        {
            int j = m_bPHBTable.GetLength(0) - 1;
            for (int i = tileY + highRangeY; i >= tileY + lowRangeY; i--)
            {
                sbyte b = 0;
                for (int k = tileX + lowRangeX; k <= tileX + highRangeX; k++)
                {
                    m_bPHBTable[j, b] = Actor_GetPHB(k, i);
                    b++;
                }

                j--;
            }
        }
        else
        {
            sbyte b = 0;
            for (int i = tileY + lowRangeY; i <= tileY + highRangeY; i++)
            {
                sbyte b1 = 0;
                for (int j = tileX + lowRangeX; j <= tileX + highRangeX; j++)
                {
                    m_bPHBTable[b, b1] = Actor_GetPHB(j, i);
                    b1++;
                }

                b++;
            }
        }

        m_sPHBTableReferenceTileX = (short)tileX;
        m_sPHBTableReferenceTileY = (short)tileY;
    }

    public PHB_TYPE Actor_GetPHB(int tileX, int tileY)
    {
        if (m_bPHBTable != null)
        {
            if (tileX >= m_sPHBTableReferenceTileX + -m_sTableRefIndexX &&
                tileX <= m_sPHBTableReferenceTileX + m_bPHBTable.GetLength(1) - 1 - m_sTableRefIndexX &&
                tileY >= m_sPHBTableReferenceTileY + -m_sTableRefIndexY && tileY <=
                m_sPHBTableReferenceTileY + m_bPHBTable.GetLength(0) - 1 - m_sTableRefIndexY)
            {
                return m_bPHBTable[
                    tileY - m_sPHBTableReferenceTileY + m_sTableRefIndexY, 
                    tileX - m_sPHBTableReferenceTileX + m_sTableRefIndexX];
            }
        }

        return GameMidlet.Instance_Game.PF_getPHBI(tileX, tileY);
    }

    public bool Actor_Death()
    {
        if (objType != OBJECT_TYPE.LEVEL_POST)
            stateFlag |= ACTOR_STATE.DEAD;
        else if (GameMidlet.Instance_Game.s_iLeftToDie != 1)
            return false;

        if ((stateFlag & ACTOR_STATE.LEFT_TO_DIE) != 0)
            GameMidlet.Instance_Game.s_iLeftToDie--;

        return true;
    }

    public void checkCamera()
    {
        if (anim.curAction != 8 && anim.curAction != 7 &&
            ((anim.curAction != 11 && anim.curAction != 10) || y >> 8 > V[16]) &&
            (anim.curAction != 9 || V[7] == -1 || y >> 8 > V[16]))
        {
            if (yDirectionConfirmed == 0)
            {
                if (dy < 0)
                {
                    yDirectionConfirmationCounter--;
                }
                else if (dy > 0)
                {
                    yDirectionConfirmationCounter++;
                }

                if (yDirectionConfirmationCounter > 4)
                {
                    yDirectionConfirmationCounter += 2;
                }
                else if (yDirectionConfirmationCounter < -4)
                {
                    yDirectionConfirmed = -1;
                }
            }
            else if (yDirectionConfirmed == 1)
            {
                if (dy < 0)
                    yDirectionConfirmationCounter--;
                if (yDirectionConfirmationCounter < -1)
                    yDirectionConfirmed = -1;
            }
            else if (yDirectionConfirmed == -1)
            {
                if (dy > 0)
                    yDirectionConfirmationCounter++;
                if (yDirectionConfirmationCounter > 1)
                    yDirectionConfirmed = 1;
            }
        }

        if (xDirectionConfirmed)
        {
            if (dx < 0 || actorReference is { objType: OBJECT_TYPE.PLATFORM_2 or OBJECT_TYPE.PLATFORM_1, dx: < 0 })
                xDirectionConfirmationCounter--;

            if (xDirectionConfirmationCounter < -1)
                xDirectionConfirmed = false;
        }
        else
        {
            if (dx > 0 || actorReference is { objType: OBJECT_TYPE.PLATFORM_2 or OBJECT_TYPE.PLATFORM_1, dx: > 0 })
                xDirectionConfirmationCounter++;

            if (xDirectionConfirmationCounter > 1)
                xDirectionConfirmed = true;
        }

        if (anim.curAction is 16 or 35 or 34 or 10 or 25 or 24 or 23)
        {
            xDirectionConfirmed = (stateFlag & ACTOR_STATE.FLIP_X) != 0;
            if (xDirectionConfirmed)
                xDirectionConfirmationCounter = 4;
            else
                xDirectionConfirmationCounter = -4;
        }

        for (int f = 0; f < FISTS_COUNT; f++)
        {
            if (GameMidlet.Instance_Game.pFist[f].anim.curAction != 0)
            {
                if (GameMidlet.Instance_Game.pFist[f].x < x)
                {
                    xDirectionConfirmed = false;
                    xDirectionConfirmationCounter = -4;
                }
                else if (GameMidlet.Instance_Game.pFist[f].x > x)
                {
                    xDirectionConfirmed = true;
                    xDirectionConfirmationCounter = 4;
                }
            }
        }
    }
}