namespace GbaMonoGame.Rayman3.J2ME;

public partial class Game
{
    public Actor pRayman { get; set; }
    public Actor[] pFist { get; } = new Actor[2];
    public int Fist_num { get; set; }
    public int s_iLeftToDie { get; set; }
    public short m_sectors_width { get; set; }
    public short m_sectors_height { get; set; }
    public sbyte m_sectors_nbrWidth { get; set; }
    public sbyte m_sectors_nbrHeight { get; set; }
    public sbyte[] m_sectors_activeSector { get; set; }
    public sbyte[][] m_sectors_actorIds { get; set; }
    public Actor[] actors { get; set; }
    public int m_actors_1stAlwaysActive { get; set; }
    public int raymanNull { get; set; }
    public int raymanAnim { get; set; }
    public int raymanDraw { get; set; }

    public short ReadUnsignedShort(sbyte[] buf, int nOff)
    {
        return (short)((byte)buf[nOff] | ((byte)buf[nOff + 1] << 8));
    }

    public Actor Actor_create(sbyte[] data)
    {
        Actor obj = new(data);

        switch (obj.objType)
        {
            case OBJECT_TYPE.RAYMAN:
                pRayman = obj;
                break;

            case OBJECT_TYPE.FIST:
                pFist[Fist_num] = obj;
                Fist_num++;
                if (Fist_num == Actor.FISTS_COUNT)
                    Fist_num = 0;
                break;

            case OBJECT_TYPE.PIRATE:
                if (obj.V[5] != 0 && obj.V[5] != 22)
                {
                    for (int i = 1; i < 4; i++)
                    {
                        flagActorType(obj.V[5], i);
                        RM.Synchronize();
                    }

                    sbyte[] morphData = new sbyte[11];
                    morphData[0] = 0;
                    morphData[1] = (sbyte)obj.V[5];
                    morphData[2] = data[2];
                    morphData[3] = data[3];
                    morphData[4] = data[4];
                    morphData[5] = data[5];
                    obj.actorReference = new Actor(morphData);
                    obj.stateFlag |= ACTOR_STATE.OVERRIDE_ON_DEATH;
                    obj.m_sInitStateFlag |= ACTOR_STATE.OVERRIDE_ON_DEATH;
                }
                break;

            case OBJECT_TYPE.CAGE:
                s_iCageTotal++;
                break;

            case OBJECT_TYPE.LUM:
                s_iLumsTotal++;
                break;
        }
        return obj;
    }

    void Scene_Load(int iSceneIndex)
    {
        s_iCageTotal = 0;
        s_iLumsTotal = 0;
        s_iLeftToDie = 0;
        m_gameFrame_nEnergy = 5;
        AM_actorFactory(RM.Array_Data[iSceneIndex], 0);
        AM_initSectors();
        GameFrame_LoadLevelInfo(m_gameFrame_curLevel);
    }

    void AM_initSectors()
    {
        m_sectors_width = (short)Resolution.X;
        m_sectors_height = (short)Resolution.Y;
        m_sectors_nbrWidth = (sbyte)((m_sBackgroundWidth << 4) / m_sectors_width + 1);
        m_sectors_nbrHeight = (sbyte)((m_sBackgroundHeight << 4) / m_sectors_height + 1);
        m_sectors_activeSector = new sbyte[4];
        
        sbyte[] sectorLength = new sbyte[m_sectors_nbrWidth * m_sectors_nbrHeight];
        for (sbyte actorIndex = 0; actorIndex < m_actors_1stAlwaysActive; actorIndex = (sbyte)(actorIndex + 1))
        {
            int x = (int)(actors[actorIndex].x >> 8);
            int y = (int)(actors[actorIndex].y >> 8);
            int sectorIndex = AM_getSector(x, y);
            sectorLength[sectorIndex] = (sbyte)(sectorLength[sectorIndex] + 1);
        }
        
        m_sectors_actorIds = new sbyte[m_sectors_nbrWidth * m_sectors_nbrHeight][];
        for (int i = 0; i < m_sectors_actorIds.Length; i++)
        {
            m_sectors_actorIds[i] = new sbyte[sectorLength[i]];
            sectorLength[i] = 0;
        }
        
        for (sbyte actorIndex = 0; actorIndex < m_actors_1stAlwaysActive; actorIndex = (sbyte)(actorIndex + 1))
        {
            int j = (int)(actors[actorIndex].x >> 8);
            int k = (int)(actors[actorIndex].y >> 8);
            int m = AM_getSector(j, k);
            m_sectors_actorIds[m][sectorLength[m]] = actorIndex;
            sectorLength[m] = (sbyte)(sectorLength[m] + 1);
        }
    }

    int AM_getSector(int x, int y)
    {
        int sectorRow = x / m_sectors_width;
        int sectorLine = y / m_sectors_height;
        int bgwidth_pix = m_sBackgroundWidth << 4;
        return sectorLine * m_sectors_nbrWidth + sectorRow;
    }

    void AM_setCurrentSector(int camX, int camY)
    {
        m_sectors_activeSector[0] = (sbyte)AM_getSector(camX, camY);
        m_sectors_activeSector[1] = (sbyte)(m_sectors_activeSector[0] + 1);
        m_sectors_activeSector[2] = (sbyte)(m_sectors_activeSector[0] + m_sectors_nbrWidth);
        m_sectors_activeSector[3] = (sbyte)(m_sectors_activeSector[2] + 1);
    }

    void AM_step(int camX, int camY, int camW, int camH)
    {
        raymanNull = 0;
        AM_setCurrentSector(camX, camY);
        for (int j = 0; j < m_sectors_activeSector.Length; j++)
        {
            int sector = m_sectors_activeSector[j];
            if (m_sectors_actorIds.Length > sector && m_sectors_actorIds[sector] != null)
            {
                for (int k = m_sectors_actorIds[sector].Length - 1; k >= 0; k--)
                {
                    Actor actor = actors[m_sectors_actorIds[sector][k]];
                    if ((actor.stateFlag & ACTOR_STATE.OVERRIDEN) != 0)
                        actor = actor.actorReference;
                    if (actor != null && (actor.stateFlag & ACTOR_STATE.DEAD) == 0)
                    {
                        if (!m_gameFrame_paused)
                        {
                            actor.step();
                            actor.ai();
                        }

                        if (Camera_IsVisible(actor))
                            actor.draw();
                    }
                }
            }
        }

        raymanDraw = 1;
        
        for (int i = m_actors_1stAlwaysActive; i < actors.Length; i++)
        {
            Actor actor = actors[i];
            if ((actor.stateFlag & ACTOR_STATE.OVERRIDEN) != 0)
                actor = actor.actorReference;
            if (actor != null && (actor.stateFlag & ACTOR_STATE.DEAD) == 0)
            {
                if (!m_gameFrame_paused)
                {
                    actor.step();
                    actor.ai();
                }

                if (Camera_IsVisible(actor))
                    actor.draw();
            }
        }
    }

    void AM_actorFactory(byte[] pBuf, int nStartOffset)
    {
        RM.Load(ResourceId.Create(0, RESOURCE_TYPE.DATA, 0));
        RM.Synchronize();
        s_actorCheckpoint = null;
        pRayman = null;
        pFist[0] = null;
        pFist[1] = null;
        if (actors != null)
            for (int i = 0; i < actors.Length; i++)
                actors[i] = null;
        actors = null;
        int actorCount = pBuf[nStartOffset++];
        actors = new Actor[actorCount];
        sbyte[][] tempData = new sbyte[actorCount][];
        int Offset_Parameters = nStartOffset + actorCount * 7;
        sbyte b;
        for (b = 0; b < actorCount; b++)
        {
            int NbParameters = pBuf[nStartOffset];
            tempData[b] = new sbyte[6 + NbParameters];
            tempData[b][0] = (sbyte)pBuf[nStartOffset + actorCount * 1];
            tempData[b][1] = (sbyte)pBuf[nStartOffset + actorCount * 2];
            tempData[b][2] = (sbyte)pBuf[nStartOffset + actorCount * 3];
            tempData[b][3] = (sbyte)pBuf[nStartOffset + actorCount * 4];
            tempData[b][4] = (sbyte)pBuf[nStartOffset + actorCount * 5];
            tempData[b][5] = (sbyte)pBuf[nStartOffset + actorCount * 6];
            for (int j = 0; j < NbParameters; j++)
                tempData[b][6 + j] = (sbyte)pBuf[Offset_Parameters++];
            nStartOffset++;
        }
        for (b = 0; b < 28; b++)
            flagActorType(b, 0);
        for (b = 1; b < 4; b++)
        {
            for (sbyte b1 = 0; b1 < actorCount; b1++)
                flagActorType(tempData[b1][1], b);
            flagActorType(26, b);
            RM.Synchronize();
        }
        for (b = 0; b < 28; b++)
        {
            if (Actor.aniData[b] != null && (Actor.aniData[b].flag & ANIM_DATA_FLAGS.LOADED) == 0)
                Actor.aniData[b] = null;
        }
        System.gc();

        int firstFree = 0;
        int lastFree = actorCount - 1;
        for (b = 0; b < actorCount; b++)
        {
            Actor actor = Actor_create(tempData[b]);
            switch ((OBJECT_TYPE)tempData[b][1])
            {
                case OBJECT_TYPE.RAYMAN:
                case OBJECT_TYPE.FIST:
                case OBJECT_TYPE.PLATFORM_1:
                case OBJECT_TYPE.PLATFORM_2:
                case OBJECT_TYPE.BULLET:
                    actors[lastFree--] = actor;
                    break;

                default:
                    actors[firstFree++] = actor;
                    break;
            }
            tempData[b] = null;
        }

        m_actors_1stAlwaysActive = lastFree + 1;
        for (b = 0; b < actors.Length; b++)
        {
            switch (actors[b].objType)
            {
                case OBJECT_TYPE.PIRATE:
                    int actorRef = actors[b].V[0];
                    for (short b1 = 0; b1 < actors.Length; b1++)
                    {
                        if (actors[b1].objType == OBJECT_TYPE.BULLET && --actorRef < 0)
                        {
                            actors[b].V[0] = b1;
                            actors[b].m_iInitV[0] = b1;
                            break;
                        }
                    }
                    break;
            }
        }

        RM.Free(0x60000100);
    }

    void flagActorType(int iActorType, int iLoadState)
    {
        byte[] actorArray = RM.Array_Data[0];
        int offset = 11 * iActorType;
        int kImage_ResourceID = ReadInt(actorArray, offset + 0);
        int kImage_Index = (sbyte)actorArray[offset + 4];
        int kData_ResourceID = ReadInt(actorArray, offset + 5);
        int kData_Index = (sbyte)actorArray[offset + 9];
        bool bCreateDataImage = ((sbyte)actorArray[offset + 10] == 1);
        
        if (kImage_ResourceID == -1 || kData_ResourceID == -1 || kImage_Index == -1 || kData_Index == -1)
            return;

        switch (iLoadState)
        {
            case 0:
                RM.Free(kImage_ResourceID);
                RM.Free(kData_ResourceID);
                if (Actor.aniData[iActorType] != null)
                    Actor.aniData[iActorType].flag &= ~ANIM_DATA_FLAGS.LOADED;
                break;
            case 1:
                RM.Load(kImage_ResourceID);
                break;
            case 2:
                RM.Load(kData_ResourceID);
                break;
            case 3:
                RM.Free(kData_ResourceID);
                Actor.AniLoad(kData_Index, kImage_Index);
                break;
        }
    }

}