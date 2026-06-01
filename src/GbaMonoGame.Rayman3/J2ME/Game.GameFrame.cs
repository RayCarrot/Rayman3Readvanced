using System;

namespace GbaMonoGame.Rayman3.J2ME;

public partial class Game
{
    public const int DEFAULT_LIVES = 3;
    public const int DEFAULT_ENERGY = 5;

    public const string GAME_SAVE_NAME = "RaymanSave";
    public const string RECORD_FLAG_SAVE_NAME = "RECORDFLAG";
    public const string SOUND_SAVE_NAME = "SOUND";

    public int[] actorsLen { get; } = [32, 64, 71, 5, 55, 69, 74, 5, 58, 40, 35];
    public int s_iCageTotal { get; set; }
    public int s_iCageOpened { get; set; }
    public int s_iLumsTotal { get; set; }
    public int s_iLumsTaken { get; set; }
    public Actor s_actorCheckpoint { get; set; }
    public sbyte[,] s_synopsis { get; set; } // TODO: Structs
    public sbyte[] m_RecordUsedFlag { get; } = new sbyte[11]; // TODO: Bools?
    public GAME_FRAME_STATE m_gameFrame_prevState { get; set; }
    public GAME_FRAME_STATE m_gameFrame_curState { get; set; }
    public bool m_gameFrame_paused { get; set; }
    public MESSAGE_ID m_gameFrame_msgId { get; set; }
    public int m_gameFrame_msgPar { get; set; }
    public sbyte m_gameFrame_curLevel { get; set; } // TODO: Consts for -1 and 0
    public sbyte m_gameFrame_unlockedLevel { get; set; }
    public sbyte m_gameFrame_nbLevels { get; set; }
    public sbyte m_gameFrame_nLife { get; set; }
    public sbyte m_gameFrame_nEnergy { get; set; }
    public int m_iPrevLevel { get; set; }
    public int m_gameStateStep { get; set; }

    public bool GameFrame_PhysicalInitI()
    {
        m_gameStateStep = 0;
        lStartMillForKeyDelay = System.currentTimeMillis();
        m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
        GameFrame_InitNewGame();
        return true;
    }

    public void GameFrame_InitNewGame()
    {
        m_gameFrame_curLevel = -1;
        m_gameFrame_unlockedLevel = 1;
        m_gameFrame_nLife = DEFAULT_LIVES;
        m_gameFrame_nEnergy = DEFAULT_ENERGY;
    }

    public int ReadInt(byte[] data, int offset)
    {
        return ((data[offset++] & 0xFF) << 24) | ((data[offset++] & 0xFF) << 16) | ((data[offset++] & 0xFF) << 8) | (data[offset++] & 0xFF);
    }

    public bool GameFrame_loadLevel(int iLevel)
    {
        System.gc();

        try
        {
            // If loading a new level...
            if (iLevel != m_iPrevLevel)
            {
                // Load scene map data
                RM.Load(ResourceId.Create(19, RESOURCE_TYPE.DATA, 2));
                RM.Synchronize();

                m_bBackgroundUsed = false;
                byte[] sceneMapData = RM.Array_Data[68];
                m_gameFrame_nbLevels = (sbyte)(sceneMapData.Length / 15 - 1);

                // TODO: Why load every level?
                int offset = 0;
                for (int i = 0; i < sceneMapData.Length / 15; i++)
                {
                    RM.Free(ReadInt(sceneMapData, offset + 0));
                    RM.Free(ReadInt(sceneMapData, offset + 5));
                    RM.Free(ReadInt(sceneMapData, offset + 10));
                    offset += 15;
                }
                RM.Synchronize();

                offset = 15 * iLevel;

                // Load current level
                if (iLevel != -1)
                {
                    StopSound();
                    Menu_Free(true);
                    RM.Load(ReadInt(sceneMapData, offset + 0));
                    RM.Load(ReadInt(sceneMapData, offset + 5));
                    RM.Load(ReadInt(sceneMapData, offset + 10));
                    Status_ShowAll();
                    RM.Synchronize();
                    CreateTiledBackground(sceneMapData[offset + 14], sceneMapData[offset + 4]);
                    setFastMode(true, 240, 320);
                    Scene_Load(sceneMapData[offset + 9]);
                    RM.Free(ReadInt(sceneMapData, offset + 5));
                    
                    if (iLevel is 0 or 1)
                    {
                        RM.Load(ResourceId.Create(48, RESOURCE_TYPE.DATA, 0));
                        RM.Load(0x60000130);
                    }
                    else
                    {
                        RM.Free(0x60000130);
                    }
                }
                // Load menu
                else
                {
                    Actor.AniLoad(7, 5);
                    m_gameMenu_pData = Actor.aniData[26];
                    if (RM.Array_Image[18] == null)
                        Menu_LoadMain();
                    Menu_SetCurrentPage(MENU_PAGE.MAIN);
                }
                RM.Free(0x60000913);
                RM.Synchronize();
                m_iPrevLevel = iLevel;
            }

            if (iLevel >= 0 && actors != null)
            {
                // Reset actors
                for (int iActorLoop = 0; iActorLoop < actors.Length; iActorLoop++)
                {
                    actors[iActorLoop].Actor_Reset();
                    if (actors[iActorLoop].actorReference != null)
                        actors[iActorLoop].actorReference.Actor_Reset();
                }

                // Reset camera
                pFocusActor = pRayman;
                setCameraPos((int)((pRayman.x >> 8) - 40L), (int)((pRayman.y >> 8) - 80L), pFocusActor);
                
                // Initialize background drawing
                Fast_Init();
            }
        }
        catch (Exception e)
        {
            System.println($"GameFrame_loadLevel Exception : {e}");
            // e.printStackTrace();
        }

        return true;
    }

    public void GameCore()
    {
        // Draw background
        if (m_gameFrame_curLevel >= 0)
            fastDraw(g_graBackBuffer, Resolution.X, Resolution.Y);
        
        // Step and draw actors
        if (m_bBackgroundUsed)
            AM_step(m_iBackgroundX, m_iBackgroundY, Resolution.X, Resolution.Y);
        
        // Draw status
        if (!m_gameFrame_paused)
        {
            Status_Draw();
            IngameTextbox_Draw();
        }

        // Draw soft key pause indicator
        if (m_gameStateStep != 0 && (m_gameFrame_curState == GAME_FRAME_STATE.DEFAULT || curState == 4))
        {
            bool bBackgroundState = m_bBackgroundUsed;
            m_bBackgroundUsed = false;
            Actor.drawModule(Actor.aniData[26], 23, Resolution.X - Actor.aniData[26].modules[23].Width, Resolution.Y - Actor.aniData[26].modules[23].Height, 0, g_graBackBuffer);
            m_bBackgroundUsed = bBackgroundState;
        }
    }

    public void GameFrame_StateRun()
    {
        // Paused
        if (m_gameFrame_paused || m_gameFrame_curLevel < 0)
        {
            // Draw background
            if (m_gameFrame_curLevel >= 0)
                fastDraw(g_graBackBuffer, Resolution.X, Resolution.Y);

            // Draw menu
            Menu_Draw();

            if (!m_gameFrame_paused)
                PlaySound(SOUND_INDEX.music_splash, false, 255);

            // Draw soft key pause indicator
            Actor.drawModule(Actor.aniData[26], 23, Resolution.X - Actor.aniData[26].modules[23].Width, Resolution.Y - Actor.aniData[26].modules[23].Height - Menu_GetVArrowPos(), 0, g_graBackBuffer);
        }
        // In-game
        else
        {
            Camera_Tick();
            GameCore();
        }

        // Process message
        switch (m_gameFrame_msgId)
        {
            case MESSAGE_ID.RAYMAN_DEATH:
                if (m_gameFrame_nLife > 0)
                {
                    m_gameFrame_nLife = (sbyte)(m_gameFrame_nLife - 1);
                    m_gameFrame_nEnergy = 5;
                    m_gameStateStep = 0;
                    m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                    m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
                }
                else
                {
                    m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                    m_gameFrame_curState = GAME_FRAME_STATE.GAME_OVER;
                }
                break;

            case MESSAGE_ID.CHANGE_LEVEL:
                if (m_gameFrame_msgPar != m_gameFrame_curLevel)
                {
                    m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                    m_gameFrame_curState = GAME_FRAME_STATE.EXITING_LEVEL;
                }
                break;

            case MESSAGE_ID.EXIT:
                m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                m_gameFrame_curState = GAME_FRAME_STATE.CONFIRM_EXIT;
                break;

            case MESSAGE_ID.EXIT_TO_MENU:
                m_gameFrame_curLevel = -1;
                m_gameStateStep = 0;
                m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
                m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
                break;
        }

        m_gameFrame_msgId = MESSAGE_ID.NONE;
    }

    public void GameFrame_StateInit()
    {
        m_gameFrame_paused = false;
        if (m_gameStateStep == 0)
        {
            StopSound();
            if (m_iPrevLevel == -1 && RM.GetImage(18) != null)
            {
                g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
                g_graBackBuffer.drawImage(RM.GetImage(18), Resolution.X / 2, Resolution.Y / 2, ANCHOR.HCENTER | ANCHOR.VCENTER);
            }
            else if (m_bClearBackMenu)
            {
                fastDraw(g_graBackBuffer, Resolution.X, Resolution.Y);
                m_bClearBackMenu = false;
            }
            g_graBackBuffer.setFont(m_fontGeneral);
            if (m_iPrevLevel == -1)
            {
                if (m_gameMenu_idCurPage == MENU_PAGE.PAUSE)
                    Menu_DrawString(RM.GetString(2949259), (Resolution.X - Menu_GetStringWidth(RM.GetString(2949259))) >> 1, 110, 0);
                else
                    Menu_DrawString(RM.GetString(2949259), (Resolution.X - Menu_GetStringWidth(RM.GetString(2949259))) >> 1, 263, 0);
            }
            else
            {
                Menu_DrawString(RM.GetString(2949259), (Resolution.X - Menu_GetStringWidth(RM.GetString(2949259))) >> 1, 110, 0);
            }
        }
        else
        {
            bool isLoadSuccess = GameFrame_loadLevel(m_gameFrame_curLevel);
            if (isLoadSuccess)
            {
                if (m_gameFrame_curLevel == 0)
                    PlaySound(SOUND_INDEX.music_map, false, 255);
                else
                    PlaySound(SOUND_INDEX.enter_level, true);

                Status_ShowAll();
                currentKey = GAME_KEY.None;
                pressedKey = GAME_KEY.None;
                releasedKey = GAME_KEY.None;
                m_keys = 0;
                pRayman.V[5] = (ushort)GAME_KEY.None;
                pRayman.V[4] = (ushort)GAME_KEY.None;
                pRayman.V[3] = (ushort)GAME_KEY.None;
                lStartMillForKeyDelay = System.currentTimeMillis();
                m_gameFrame_prevState = GAME_FRAME_STATE.LOADING;
                m_gameFrame_curState = GAME_FRAME_STATE.DEFAULT;
            }
        }

        m_gameStateStep++;
    }

    public void GameFrame_StateNewGame()
    {
        if (m_gameStateStep == 0)
        {
            g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
            g_graBackBuffer.drawImage(RM.GetImage(18), Resolution.X / 2, Resolution.Y / 2, ANCHOR.HCENTER | ANCHOR.VCENTER);
            Menu_DrawString(RM.GetString(2949259), (Resolution.X - Menu_GetStringWidth(RM.GetString(2949259))) >> 1, 263, 0);
        }
        else if (m_gameStateStep == 1)
        {
            if (GameFrame_IsSavedFileExists())
            {
                s_synopsis = null;
                for (int i = 0; i < 11; i++)
                    m_RecordUsedFlag[i] = 0;
                GameFrame_SaveRecordFlag();
            }
            else
            {
                GameFrame_CreateSaveGame();
                GameFrame_LoadRecordFlag();
                for (int i = 0; i < 11; i++)
                    m_RecordUsedFlag[i] = 0;
                GameFrame_SaveRecordFlag();
            }
        }
        else if (m_gameStateStep == 2)
        {
            Status_ShowAll();
            const int iLeftLevel = -1;
            m_gameFrame_curLevel = 0;
            GameFrame_Save(iLeftLevel);
            GameFrame_SaveRecordFlag();
            m_gameStateStep = 0;
            m_gameFrame_prevState = GAME_FRAME_STATE.DEFAULT;
            m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
        }

        m_gameStateStep++;
    }

    public GAME_FRAME_STATE GameFrame_doLoop()
    {
        switch (m_gameFrame_curState)
        {
            case GAME_FRAME_STATE.LOADING:
                // TODO: Why does the game sleep here?
                //try
                //{
                //    Thread.sleep(300L);
                //}
                //catch (Exception e) { }
                GameFrame_StateInit();
                return m_gameFrame_curState;

            case GAME_FRAME_STATE.NEW_GAME:
                GameFrame_StateNewGame();
                return m_gameFrame_curState;

            case GAME_FRAME_STATE.LOAD_GAME:
                if (m_gameStateStep == 0)
                {
                    g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
                    g_graBackBuffer.drawImage(RM.GetImage(18), Resolution.X / 2, Resolution.Y / 2, ANCHOR.HCENTER | ANCHOR.VCENTER);
                    Menu_DrawString(RM.GetString(2949259), (Resolution.X - Menu_GetStringWidth(RM.GetString(2949259))) >> 1, 263, 0);
                }
                else if (m_gameStateStep == 1)
                {
                    GameFrame_LoadRecordFlag();
                    m_iPrevLevel = -1;
                    GameFrame_LoadSynopsis(true);
                    m_gameStateStep = 0;
                    m_gameFrame_prevState = GAME_FRAME_STATE.LOAD_GAME;
                    m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
                }
                m_gameStateStep++;
                return m_gameFrame_curState;

            case GAME_FRAME_STATE.CONFIRM_RESTART:
                if (bConfirmExit)
                {
                    if (m_gameStateStep == 0)
                    {
                        fastDraw(g_graBackBuffer, Resolution.X, Resolution.Y);
                        Menu_DrawString(RM.GetString(2949259), (Resolution.X - Menu_GetStringWidth(RM.GetString(2949259))) >> 1, 110, 0);
                    }
                    else if (m_gameStateStep == 1)
                    {
                        s_actorCheckpoint = null;
                        m_iPrevLevel = -1;
                        GameFrame_LoadSynopsis(true);
                        m_gameStateStep = 0;
                        m_gameFrame_prevState = GAME_FRAME_STATE.CONFIRM_RESTART;
                        m_gameFrame_curState = GAME_FRAME_STATE.LOADING;
                    }
                    m_gameStateStep++;
                }
                return m_gameFrame_curState;

            case GAME_FRAME_STATE.EXITING_LEVEL:
                if (m_gameStateStep == 0)
                {
                    if (pRayman.anim.curAction == 37)
                    {
                        Menu_DrawString(RM.GetString(2949259), (Resolution.X - Menu_GetStringWidth(RM.GetString(2949259))) >> 1, 110, 0);
                    }
                    else
                    {
                        GameCore();
                        m_gameFrame_paused = false;
                    }
                }
                else if (m_gameStateStep == 2)
                {
                    Status_ShowAll();
                    int iLeftLevel = m_gameFrame_curLevel;
                    m_gameFrame_curLevel = (sbyte)m_gameFrame_msgPar;
                    GameFrame_Save(iLeftLevel);
                    GameFrame_SaveRecordFlag();
                    m_gameFrame_prevState = GAME_FRAME_STATE.EXITING_LEVEL;
                    m_gameFrame_curState = GAME_FRAME_STATE.EXITED_LEVEL;
                }
                m_gameStateStep++;
                return m_gameFrame_curState;
        }

        GameFrame_StateRun();
        return m_gameFrame_curState;
    }

    public void GameFrame_PostMessage(MESSAGE_ID id, int par)
    {
        m_gameFrame_msgId = id;
        m_gameFrame_msgPar = par;
    }

    public bool GameFrame_IsSavedFileExists()
    {
        bool bRes = false;
        bool bRes1 = false;
        bool bRes2 = false;
        
        try
        {
            string[] sRecordList = RecordStore.listRecordStores();
            if (sRecordList != null)
            {
                for (int i = 0; i < sRecordList.Length; i++)
                {
                    if (sRecordList[i] == GAME_SAVE_NAME)
                        bRes = true;
                    if (sRecordList[i] == RECORD_FLAG_SAVE_NAME)
                        bRes1 = true;
                    if (sRecordList[i] == SOUND_SAVE_NAME)
                        bRes2 = true;
                }
            }
        }
        catch (Exception e) { }

        return bRes && bRes1 && bRes2;
    }

    public void GameFrame_CreateSaveGame()
    {
        try
        {
            RecordStore rs = RecordStore.openRecordStore("RaymanSave", true);
            for (int i = 0; i < actorsLen.Length; i++)
            {
                byte[] data = new byte[actorsLen[i]];
                rs.addRecord(data, 0, data.Length);
                m_RecordUsedFlag[i] = 0;
            }
            rs.closeRecordStore();
        }
        catch (Exception e) { }
    }

    public void GameFrame_LoadRecordFlag()
    {
        try
        {
            RecordStore rs;
            bool isFileExists = false;
            string[] sRecordList = RecordStore.listRecordStores();
            if (sRecordList != null)
            {
                for (int i = 0; i < sRecordList.Length; i++)
                {
                    if (sRecordList[i] == RECORD_FLAG_SAVE_NAME)
                    {
                        isFileExists = true;
                        break;
                    }
                }
            }

            byte[] data = null;
            if (isFileExists)
            {
                rs = RecordStore.openRecordStore(RECORD_FLAG_SAVE_NAME, false);
                data = rs.getRecord(1);
            }
            else
            {
                rs = RecordStore.openRecordStore(RECORD_FLAG_SAVE_NAME, true);
                if (rs.getNumRecords() == 0)
                    rs.addRecord(data, 0, 0);
            }

            if (data != null)
            {
                for (int i = 0; i < 11; i++)
                    m_RecordUsedFlag[i] = (sbyte)data[i];
            }
            rs.closeRecordStore();
        }
        catch (Exception e) { }
    }

    public void GameFrame_SaveRecordFlag()
    {
        try
        {
            RecordStore rs = RecordStore.openRecordStore(RECORD_FLAG_SAVE_NAME, false);
            byte[] data = new byte[m_RecordUsedFlag.Length];
            for (int i = 0; i < m_RecordUsedFlag.Length; i++)
                data[i] = (byte)m_RecordUsedFlag[i];
            rs.setRecord(1, data, 0, data.Length);
            rs.closeRecordStore();
        }
        catch (Exception e) { }
    }

    public void GameFrame_ClearSaveGame()
    {
        try
        {
            s_synopsis = null;
            RecordStore rs = RecordStore.openRecordStore(GAME_SAVE_NAME, false);
            int iRecordId = 1;
            while (iRecordId <= m_RecordUsedFlag.Length)
            {
                byte[] data = rs.getRecord(iRecordId);
                if (data != null)
                    m_RecordUsedFlag[iRecordId - 1] = 0;
                iRecordId++;
            }
            rs.closeRecordStore();
        }
        catch (Exception e) { }
    }

    void GameFrame_DeleteSaveGame()
    {
        s_synopsis = null;
        try
        {
            RecordStore.deleteRecordStore(GAME_SAVE_NAME);
        }
        catch (Exception e) { }
    }

    public void GameFrame_SaveSound()
    {
        try
        {
            RecordStore rs = RecordStore.openRecordStore(SOUND_SAVE_NAME, false);
            byte[] data = BitConverter.GetBytes(SoundVolume);
            rs.setRecord(1, data, 0, data.Length);
            rs.closeRecordStore();
        }
        catch (Exception e) { }
    }

    public void GameFrame_LoadSound()
    {
        try
        {
            RecordStore rs;
            bool isSaveSoundFileExists = false;
            string[] sRecordList = RecordStore.listRecordStores();
            if (sRecordList != null)
            {
                for (int i = 0; i < sRecordList.Length; i++)
                {
                    if (sRecordList[i] == SOUND_SAVE_NAME)
                        isSaveSoundFileExists = true;
                }
            }

            byte[] data = null;
            if (isSaveSoundFileExists)
            {
                rs = RecordStore.openRecordStore(SOUND_SAVE_NAME, false);
                data = rs.getRecord(1);
            }
            else
            {
                rs = RecordStore.openRecordStore(SOUND_SAVE_NAME, true);
                if (rs.getNumRecords() == 0)
                    rs.addRecord(data, 0, 0);
            }

            if (data != null)
            {
                // TODO: Implement loading sound?
                // Do nothing
            }
            rs.closeRecordStore();
        }
        catch (Exception e) { }
    }

    public void GameFrame_Save(int p_iLeftLevel)
    {
        try
        {
            GameFrame_SaveLevelInfo(p_iLeftLevel);

            if (s_synopsis == null)
                GameFrame_LoadSynopsis(false);
            
            if (p_iLeftLevel is > 0 and <= 9)
            {
                s_synopsis[p_iLeftLevel, 0] = (sbyte)s_iLumsTaken;
                s_iLumsTaken = s_synopsis[0, 0] = (sbyte)(s_synopsis[0, 0] + (sbyte)s_iLumsTaken);
                s_synopsis[p_iLeftLevel, 1] = (sbyte)s_iLumsTotal;
                s_iLumsTotal = s_synopsis[0, 1] = (sbyte)(s_synopsis[0, 1] + (sbyte)s_iLumsTotal);
                s_synopsis[p_iLeftLevel, 2] = (sbyte)s_iCageOpened;
                s_iCageOpened = s_synopsis[0, 2] = (sbyte)(s_synopsis[0, 2] + (sbyte)s_iCageOpened);
                s_synopsis[p_iLeftLevel, 3] = (sbyte)s_iCageTotal;
                s_iCageTotal = s_synopsis[0, 3] = (sbyte)(s_synopsis[0, 3] + (sbyte)s_iCageTotal);
            }
            else if (p_iLeftLevel == 0)
            {
                if (s_synopsis[m_gameFrame_curLevel, 1] == -1 || s_synopsis[m_gameFrame_curLevel, 3] == -1)
                {
                    s_synopsis[m_gameFrame_curLevel, 1] = 0;
                    s_synopsis[m_gameFrame_curLevel, 3] = 0;
                }
                s_synopsis[0, 0] = (sbyte)(s_synopsis[0, 0] - (s_iLumsTaken = s_synopsis[m_gameFrame_curLevel, 0]));
                s_synopsis[0, 1] = (sbyte)(s_synopsis[0, 1] - (s_iLumsTotal = s_synopsis[m_gameFrame_curLevel, 1]));
                s_synopsis[0, 2] = (sbyte)(s_synopsis[0, 2] - (s_iCageOpened = s_synopsis[m_gameFrame_curLevel, 2]));
                s_synopsis[0, 3] = (sbyte)(s_synopsis[0, 3] - (s_iCageTotal = s_synopsis[m_gameFrame_curLevel, 3]));
            }
            GameFrame_SaveSynopsis();
        }
        catch (Exception e) { }
    }

    public void GameFrame_SaveSynopsis()
    {
        try
        {
            RecordStore rs = RecordStore.openRecordStore(GAME_SAVE_NAME, false);
            byte[] data = new byte[s_synopsis.GetLength(0) * 4 + 3];
            int offset = 0;
            for (int i = 0; i < s_synopsis.GetLength(0); i++)
            {
                data[offset++] = (byte)s_synopsis[i, 0];
                data[offset++] = (byte)s_synopsis[i, 1];
                data[offset++] = (byte)s_synopsis[i, 2];
                data[offset++] = (byte)s_synopsis[i, 3];
            }
            data[offset++] = (byte)m_gameFrame_curLevel;
            data[offset++] = (byte)m_gameFrame_unlockedLevel;
            data[offset++] = (byte)m_gameFrame_nLife;
            rs.setRecord(10, data, 0, data.Length);
            m_RecordUsedFlag[9] = 1;
            rs.closeRecordStore();
        }
        catch (Exception e) { }
    }

    public void GameFrame_LoadSynopsis(bool bGetGameInfo)
    {
        try
        {
            int offset = 0;
            s_synopsis = new sbyte[10, 4];
            RecordStore rs = RecordStore.openRecordStore(GAME_SAVE_NAME, false);
            byte[] data = rs.getRecord(10);
            if (data != null && m_RecordUsedFlag[9] == 1)
            {
                for (int i = 0; i < s_synopsis.GetLength(0); i++)
                {
                    s_synopsis[i, 0] = (sbyte)data[offset++];
                    s_synopsis[i, 1] = (sbyte)data[offset++];
                    s_synopsis[i, 2] = (sbyte)data[offset++];
                    s_synopsis[i, 3] = (sbyte)data[offset++];
                }
                if (bGetGameInfo)
                {
                    m_gameFrame_curLevel = (sbyte)data[offset++];
                    m_gameFrame_unlockedLevel = (sbyte)data[offset++];
                    m_gameFrame_nLife = (sbyte)data[offset++];
                    s_iLumsTaken = s_synopsis[m_gameFrame_curLevel, 0];
                    s_iLumsTotal = s_synopsis[m_gameFrame_curLevel, 1];
                    s_iCageOpened = s_synopsis[m_gameFrame_curLevel, 2];
                    s_iCageTotal = s_synopsis[m_gameFrame_curLevel, 3];
                }
                data = null;
            }
            else
            {
                s_synopsis[0, 3] = 0;
                s_synopsis[0, 2] = 0;
                s_synopsis[0, 1] = 0;
                s_synopsis[0, 0] = 0;
                for (sbyte b = 1; b < s_synopsis.GetLength(0); b++)
                {
                    s_synopsis[b, 2] = 0;
                    s_synopsis[b, 0] = 0;
                    s_synopsis[b, 3] = -1;
                    s_synopsis[b, 1] = -1;
                }
            }
            rs.closeRecordStore();
        }
        catch (Exception e) { }
    }

    public void GameFrame_SaveLevelInfo(int pLevel)
    {
        try
        {
            RecordStore rs = RecordStore.openRecordStore(GAME_SAVE_NAME, false);
            int offset = 0;
            if (0 < pLevel && pLevel <= m_gameFrame_nbLevels)
            {
                byte[] data = new byte[actors.Length];
                for (int i = 0; i < actors.Length;)
                {
                    byte buffer = 0;
                    int mask = 1;
                    do
                    {
                        if ((actors[i].stateFlag & ACTOR_STATE.DEAD) == 0)
                            continue;
                        buffer = (byte)(buffer | mask);
                    } while (++i < actors.Length && (mask <<= 1) < 256);
                    data[offset++] = buffer;
                }
                rs.setRecord(pLevel, data, 0, data.Length);
                m_RecordUsedFlag[pLevel - 1] = 1;
            }
            else if (pLevel == 0)
            {
                byte[] data = new byte[16 + actors.Length];
                long tempV = pRayman.m_lInitX;
                data[offset] = (byte)(0xFFL & (tempV >> 56));
                data[offset + 1] = (byte)(0xFFL & (tempV >> 48));
                data[offset + 2] = (byte)(0xFFL & (tempV >> 40));
                data[offset + 3] = (byte)(0xFFL & (tempV >> 32));
                data[offset + 4] = (byte)(0xFFL & (tempV >> 24));
                data[offset + 5] = (byte)(0xFFL & (tempV >> 16));
                data[offset + 6] = (byte)(0xFFL & (tempV >> 8));
                data[offset + 7] = (byte)(0xFFL & tempV);
                offset += 8;
                tempV = pRayman.m_lInitY;
                data[offset] = (byte)(0xFFL & (tempV >> 56));
                data[offset + 1] = (byte)(0xFFL & (tempV >> 48));
                data[offset + 2] = (byte)(0xFFL & (tempV >> 40));
                data[offset + 3] = (byte)(0xFFL & (tempV >> 32));
                data[offset + 4] = (byte)(0xFFL & (tempV >> 24));
                data[offset + 5] = (byte)(0xFFL & (tempV >> 16));
                data[offset + 6] = (byte)(0xFFL & (tempV >> 8));
                data[offset + 7] = (byte)(0xFFL & tempV);
                offset += 8;
                byte mask = 1;
                for (int i = 0; i < actors.Length; i++)
                {
                    byte buffer = 0;
                    if ((actors[i].stateFlag & ACTOR_STATE.DEAD) != 0)
                        buffer = (byte)(buffer | mask);
                    data[offset++] = buffer;
                }
                rs.setRecord(11, data, 0, data.Length);
                m_RecordUsedFlag[10] = 1;
            }
            rs.closeRecordStore();
        }
        catch (Exception e) { }
    }

    public void GameFrame_LoadLevelInfo(int pLevel)
    {
        try
        {
            RecordStore rs = RecordStore.openRecordStore(GAME_SAVE_NAME, false);
            int offset = 0;
            if (0 < pLevel && pLevel <= m_gameFrame_nbLevels)
            {
                byte[] data = new byte[actors.Length];
                data = rs.getRecord(pLevel);
                if (data != null && m_RecordUsedFlag[pLevel - 1] == 1)
                {
                    for (int i = 0; i < actors.Length;)
                    {
                        byte buffer = data[offset++];
                        int mask = 1;
                        do
                        {
                            if ((buffer & mask) == 0)
                                continue;
                            actors[i].stateFlag |= ACTOR_STATE.DEAD;
                        } while (++i < actors.Length && (mask <<= 1) < 256);
                    }
                    data = null;
                }
            }
            else if (pLevel == 0)
            {
                byte[] data = rs.getRecord(11);
                if (data != null && m_RecordUsedFlag[10] == 1)
                {
                    byte a = data[offset++];
                    byte b = data[offset++];
                    byte c = data[offset++];
                    byte d = data[offset++];
                    byte e = data[offset++];
                    byte f = data[offset++];
                    byte g = data[offset++];
                    byte h = data[offset++];
                    pRayman.m_lInitX = ((long)(a & 0xFF) << 56) | ((long)(b & 0xFF) << 48) | ((long)(c & 0xFF) << 40) | ((long)(d & 0xFF) << 32) | ((long)(e & 0xFF) << 24) | ((long)(f & 0xFF) << 16) | ((long)(g & 0xFF) << 8) | (long)(h & 0xFF);
                    a = data[offset++];
                    b = data[offset++];
                    c = data[offset++];
                    d = data[offset++];
                    e = data[offset++];
                    f = data[offset++];
                    g = data[offset++];
                    h = data[offset++];
                    pRayman.m_lInitY = ((long)(a & 0xFF) << 56) | ((long)(b & 0xFF) << 48) | ((long)(c & 0xFF) << 40) | ((long)(d & 0xFF) << 32) | ((long)(e & 0xFF) << 24) | ((long)(f & 0xFF) << 16) | ((long)(g & 0xFF) << 8) | (long)(h & 0xFF);
                    const sbyte mask = 1;
                    for (int i = 0; i < actors.Length; i++)
                    {
                        byte buffer = data[offset++];
                        if ((buffer & mask) != 0)
                            actors[i].stateFlag |= ACTOR_STATE.DEAD;
                    }
                }
            }
            rs.closeRecordStore();
        }
        catch (Exception e) { }

        s_iLumsTaken = s_synopsis[pLevel, 0];
        s_iCageOpened = s_synopsis[pLevel, 2];

        if (pLevel == 0)
        {
            s_iLumsTotal = s_synopsis[0, 1];
            s_iCageTotal = s_synopsis[0, 3];
        }
        else
        {
            s_synopsis = null;
        }
    }
}