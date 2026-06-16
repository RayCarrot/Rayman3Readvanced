using System;
using System.Diagnostics.CodeAnalysis;
// ReSharper disable RedundantAssignment
// ReSharper disable UselessBinaryOperation

namespace GbaMonoGame.Rayman3.J2me;

public partial class Game
{
    public const int DEFAULT_LIVES = 3;
    public const int DEFAULT_ENERGY = 5;

    public const int LEVEL_INVALID = -2;
    public const int LEVEL_MENU = -1;
    public const int LEVEL_WORLD_MAP = 0;
    public const int LEVEL_FIRST = 1;
    public const int LEVEL_FINAL = 8;
    public const int LEVELS_COUNT = 10;

    public const string GAME_SAVE_NAME = "RaymanSave";
    public const string RECORD_FLAG_SAVE_NAME = "RECORDFLAG";
    public const string SOUND_SAVE_NAME = "SOUND";

    public int[] actorsLen { get; } = [32, 64, 71, 5, 55, 69, 74, 5, 58, 40, 35];
    public int s_iCageTotal { get; set; }
    public int s_iCageOpened { get; set; }
    public int s_iLumsTotal { get; set; }
    public int s_iLumsTaken { get; set; }
    public Actor s_actorCheckpoint { get; set; }
    public Synopsis[] s_synopsis { get; set; }
    public bool[] m_RecordUsedFlag { get; } = new bool[11];
    public GAME_FRAME_STATE m_gameFrame_prevState { get; set; }
    public GAME_FRAME_STATE m_gameFrame_curState { get; set; }
    public int m_gameStateStep { get; set; }
    public bool m_gameFrame_paused { get; set; }
    public MESSAGE_ID m_gameFrame_msgId { get; set; }
    public int m_gameFrame_msgPar { get; set; }
    public int m_iPrevLevel { get; set; }
    public sbyte m_gameFrame_curLevel { get; set; }
    public sbyte m_gameFrame_unlockedLevel { get; set; }
    public sbyte m_gameFrame_nbLevels { get; set; }
    public sbyte m_gameFrame_nLife { get; set; }
    public sbyte m_gameFrame_nEnergy { get; set; }

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
        m_gameFrame_curLevel = LEVEL_MENU;
        m_gameFrame_unlockedLevel = LEVEL_FIRST;
        m_gameFrame_nLife = DEFAULT_LIVES;
        m_gameFrame_nEnergy = DEFAULT_ENERGY;
    }

    public int ReadInt(byte[] data, int offset)
    {
        return (data[offset++] << 24) | (data[offset++] << 16) | (data[offset++] << 8) | data[offset++];
    }

    public bool GameFrame_loadLevel(int iLevel)
    {
        System.gc();

        // If loading a new level...
        if (iLevel != m_iPrevLevel)
        {
            // Load scene map data
            RM.LoadData<SceneMapResource>(RESOURCE_ID_DATA_SCENE_MAP);
            RM.Synchronize();

            m_bBackgroundUsed = false;
            SceneMapResource sceneMapData = RM.GetData<SceneMapResource>(RESOURCE_ID_DATA_SCENE_MAP);
            m_gameFrame_nbLevels = (sbyte)(sceneMapData.Entries.Length - 1);

            // Unload previous levels
            foreach (SceneMapEntry entry in sceneMapData.Entries)
            {
                RM.Free(entry.BackgroundResourceId);
                RM.Free(entry.SceneResourceId);
                RM.Free(entry.ImageResourceId);
            }
            RM.Synchronize();

            // Custom - clear screens
            Gfx.ClearScreens();

            // Load current level
            if (iLevel != LEVEL_MENU)
            {
                SceneMapEntry entry = sceneMapData.Entries[iLevel];
                StopSound();
                Menu_Free(true);
                RM.LoadData<DirectTwinVQResource>(entry.BackgroundResourceId);
                RM.LoadData<SceneResource>(entry.SceneResourceId);
                RM.LoadImage(entry.ImageResourceId);
                Status_ShowAll();
                RM.Synchronize();
                CreateTiledBackground(entry.ImageDataIndex, entry.BackgroundDataIndex);
                setFastMode(true, Resolution.X, Resolution.Y);
                Scene_Load(entry.SceneDataIndex);
                RM.Free(entry.SceneResourceId);

                if (iLevel is LEVEL_WORLD_MAP or LEVEL_FIRST)
                    RM.LoadData<TextBankResource>(RESOURCE_ID_DATA_TEXTBANK_HELP);
                else
                    RM.Free(RESOURCE_ID_DATA_TEXTBANK_HELP);

                Graphics.SetMaxResolution(m_sBackgroundWidth * TILE_SIZE, m_sBackgroundHeight * TILE_SIZE);
            }
            // Load menu
            else
            {
                Actor.AniLoad(RM.ResourceID_To_Index(RESOURCE_ID_DATA_ANIM_FONT), RM.ResourceID_To_Index(RESOURCE_ID_IMG_FONT));
                m_gameMenu_pData = Actor.aniData[(sbyte)OBJECT_TYPE.FONT];
                if (RM.Array_Image[RM.ResourceID_To_Index(RESOURCE_ID_IMG_SPLASH_SCREEN)] == null)
                    Menu_LoadMain();
                Menu_SetCurrentPage(MENU_PAGE.MAIN);
                Graphics.ForceOriginalResolution();
            }
            RM.Free(RESOURCE_ID_DATA_SCENE_MAP);
            RM.Synchronize();
            m_iPrevLevel = iLevel;
        }

        if (iLevel >= LEVEL_WORLD_MAP && actors != null)
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

        return true;
    }

    public void GameCore()
    {
        // Draw background
        if (m_gameFrame_curLevel >= LEVEL_WORLD_MAP)
            fastDraw(g_graBackBuffer, Resolution.X, Resolution.Y);
        
        // Step and draw actors
        if (m_bBackgroundUsed)
            AM_step(m_iBackgroundX, m_iBackgroundY, IntegerResolution.X, IntegerResolution.Y);
        
        // Draw status
        if (!m_gameFrame_paused)
        {
            Status_Draw();
            IngameTextbox_Draw();
        }

        // Draw soft key pause indicator
        if (m_gameStateStep != 0 && (m_gameFrame_curState == GAME_FRAME_STATE.DEFAULT || curState == SYS_FRAME_STATE.GAME_OVER))
        {
            bool bBackgroundState = m_bBackgroundUsed;
            m_bBackgroundUsed = false;
            Actor.drawModule(Actor.aniData[(sbyte)OBJECT_TYPE.FONT], 23, Resolution.X - Actor.aniData[(sbyte)OBJECT_TYPE.FONT].modules[23].Width, Resolution.Y - Actor.aniData[(sbyte)OBJECT_TYPE.FONT].modules[23].Height, 0, g_graBackBuffer);
            m_bBackgroundUsed = bBackgroundState;
        }
    }

    public void GameFrame_StateRun()
    {
        // Paused
        if (m_gameFrame_paused || m_gameFrame_curLevel < LEVEL_WORLD_MAP)
        {
            // Draw background
            if (m_gameFrame_curLevel >= LEVEL_WORLD_MAP)
                fastDraw(g_graBackBuffer, Resolution.X, Resolution.Y);

            // Draw menu
            Menu_Draw();

            if (!m_gameFrame_paused)
                PlaySound(SOUND_INDEX.music_splash, false, LOOP_INFINITE);

            // Draw soft key pause indicator
            Actor.drawModule(Actor.aniData[(sbyte)OBJECT_TYPE.FONT], 23, Resolution.X - Actor.aniData[(sbyte)OBJECT_TYPE.FONT].modules[23].Width, Resolution.Y - Actor.aniData[(sbyte)OBJECT_TYPE.FONT].modules[23].Height - Menu_GetVArrowPos(), 0, g_graBackBuffer);
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
                m_gameFrame_curLevel = LEVEL_MENU;
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
            if (m_iPrevLevel == LEVEL_MENU && RM.GetImage(RESOURCE_ID_IMG_SPLASH_SCREEN) != null)
            {
                g_graBackBuffer.setClip(0, 0, Resolution.X, Resolution.Y);
                g_graBackBuffer.drawImage(RM.GetImage(RESOURCE_ID_IMG_SPLASH_SCREEN), Resolution.X / 2, Resolution.Y / 2, ANCHOR.HCENTER | ANCHOR.VCENTER);
            }
            else if (m_bClearBackMenu)
            {
                fastDraw(g_graBackBuffer, Resolution.X, Resolution.Y);
                m_bClearBackMenu = false;
            }
            g_graBackBuffer.setFont(m_fontGeneral);
            if (m_iPrevLevel == LEVEL_MENU)
            {
                if (m_gameMenu_idCurPage == MENU_PAGE.PAUSE)
                    Menu_DrawString(RM.GetString(STRING_ID_LOADING), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_LOADING))) / 2, 110, 0);
                else
                    Menu_DrawString(RM.GetString(STRING_ID_LOADING), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_LOADING))) / 2, 263, 0);
            }
            else
            {
                Menu_DrawString(RM.GetString(STRING_ID_LOADING), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_LOADING))) / 2, 110, 0);
            }
        }
        else
        {
            bool isLoadSuccess = GameFrame_loadLevel(m_gameFrame_curLevel);
            if (isLoadSuccess)
            {
                if (m_gameFrame_curLevel == LEVEL_WORLD_MAP)
                    PlaySound(SOUND_INDEX.music_map, false, LOOP_INFINITE);
                else
                    PlaySound(SOUND_INDEX.enter_level, true);

                Status_ShowAll();
                currentKey = GAME_KEY.NONE;
                pressedKey = GAME_KEY.NONE;
                releasedKey = GAME_KEY.NONE;
                m_keys = 0;
                pRayman.V[5] = (ushort)GAME_KEY.NONE;
                pRayman.V[4] = (ushort)GAME_KEY.NONE;
                pRayman.V[3] = (ushort)GAME_KEY.NONE;
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
            g_graBackBuffer.drawImage(RM.GetImage(RESOURCE_ID_IMG_SPLASH_SCREEN), Resolution.X / 2, Resolution.Y / 2, ANCHOR.HCENTER | ANCHOR.VCENTER);
            Menu_DrawString(RM.GetString(STRING_ID_LOADING), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_LOADING))) / 2, 263, 0);
        }
        else if (m_gameStateStep == 1)
        {
            if (GameFrame_IsSavedFileExists())
            {
                s_synopsis = null;
                for (int i = 0; i < 11; i++)
                    m_RecordUsedFlag[i] = false;
                GameFrame_SaveRecordFlag();
            }
            else
            {
                GameFrame_CreateSaveGame();
                GameFrame_LoadRecordFlag(); // Why is this called here?
                for (int i = 0; i < 11; i++)
                    m_RecordUsedFlag[i] = false;
                GameFrame_SaveRecordFlag();
            }
        }
        else if (m_gameStateStep == 2)
        {
            Status_ShowAll();
            const int iLeftLevel = -1;
            m_gameFrame_curLevel = LEVEL_WORLD_MAP;
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
                    g_graBackBuffer.drawImage(RM.GetImage(RESOURCE_ID_IMG_SPLASH_SCREEN), Resolution.X / 2, Resolution.Y / 2, ANCHOR.HCENTER | ANCHOR.VCENTER);
                    Menu_DrawString(RM.GetString(STRING_ID_LOADING), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_LOADING))) / 2, 263, 0);
                }
                else if (m_gameStateStep == 1)
                {
                    GameFrame_LoadRecordFlag();
                    m_iPrevLevel = LEVEL_MENU;
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
                        Menu_DrawString(RM.GetString(STRING_ID_LOADING), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_LOADING))) / 2, 110, 0);
                    }
                    else if (m_gameStateStep == 1)
                    {
                        s_actorCheckpoint = null;
                        m_iPrevLevel = LEVEL_MENU;
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
                        Menu_DrawString(RM.GetString(STRING_ID_LOADING), (Resolution.X - Menu_GetStringWidth(RM.GetString(STRING_ID_LOADING))) / 2, 110, 0);
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
        try
        {
            bool bRes = RecordStore.recordStoreExists(GAME_SAVE_NAME);
            bool bRes1 = RecordStore.recordStoreExists(RECORD_FLAG_SAVE_NAME);
            bool bRes2 = RecordStore.recordStoreExists(SOUND_SAVE_NAME);

            return bRes && bRes1 && bRes2;
        }
        catch
        {
            return false;
        }
    }

    public void GameFrame_CreateSaveGame()
    {
        try
        {
            using (RecordStore rs = RecordStore.openRecordStore(GAME_SAVE_NAME, true))
            {
                for (int i = 0; i < actorsLen.Length; i++)
                {
                    byte[] data = new byte[actorsLen[i]];
                    rs.addRecord(data, 0, data.Length);
                    m_RecordUsedFlag[i] = false;
                }
            }

            Rayman3.Save.ShowPopup();
        }
        catch (Exception ex)
        {
            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when saving the game.",
                header: "Error saving game");
        }
    }

    public void GameFrame_LoadRecordFlag()
    {
        try
        {
            bool isFileExists = RecordStore.recordStoreExists(RECORD_FLAG_SAVE_NAME);

            byte[] data = null;
            using RecordStore rs = RecordStore.openRecordStore(RECORD_FLAG_SAVE_NAME, !isFileExists);
            if (isFileExists)
            {
                data = rs.getRecord(1);
            }
            else
            {
                if (rs.getNumRecords() == 0)
                    rs.addRecord(null, 0, 0);
            }

            if (data != null)
            {
                for (int i = 0; i < 11; i++)
                    m_RecordUsedFlag[i] = data[i] != 0;
            }
        }
        catch (Exception ex)
        {
            Array.Clear(m_RecordUsedFlag);

            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when reading the save.",
                header: "Error reading game save");
        }
    }

    public void GameFrame_SaveRecordFlag()
    {
        try
        {
            using (RecordStore rs = RecordStore.openRecordStore(RECORD_FLAG_SAVE_NAME, false))
            {
                byte[] data = new byte[m_RecordUsedFlag.Length];
                for (int i = 0; i < m_RecordUsedFlag.Length; i++)
                    data[i] = (byte)(m_RecordUsedFlag[i] ? 1 : 0);
                rs.setRecord(1, data, 0, data.Length);
            }

            Rayman3.Save.ShowPopup();
        }
        catch (Exception ex)
        {
            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when saving the game.",
                header: "Error saving game");
        }
    }

    // Unused
    public void GameFrame_ClearSaveGame()
    {
        try
        {
            s_synopsis = null;
            using (RecordStore rs = RecordStore.openRecordStore(GAME_SAVE_NAME, false))
            {
                int iRecordId = 1;
                while (iRecordId <= m_RecordUsedFlag.Length)
                {
                    byte[] data = rs.getRecord(iRecordId);
                    if (data != null)
                        m_RecordUsedFlag[iRecordId - 1] = false;
                    iRecordId++;
                }
            }
            
            Rayman3.Save.ShowPopup();
        }
        catch (Exception ex)
        {
            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when clearing the save.",
                header: "Error clearing game save");
        }
    }

    // Unused
    void GameFrame_DeleteSaveGame()
    {
        try
        {
            s_synopsis = null;
            RecordStore.deleteRecordStore(GAME_SAVE_NAME);

            Rayman3.Save.ShowPopup();
        }
        catch (Exception ex)
        {
            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when deleting the save.",
                header: "Error deleting game save");
        }
    }

    public void GameFrame_SaveSound()
    {
        try
        {
            using RecordStore rs = RecordStore.openRecordStore(SOUND_SAVE_NAME, false);
            byte[] data = BitConverter.GetBytes(SoundVolume);
            rs.setRecord(1, data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when saving the game.",
                header: "Error saving game");
        }
    }

    public void GameFrame_LoadSound()
    {
        try
        {
            bool isSaveSoundFileExists = RecordStore.recordStoreExists(SOUND_SAVE_NAME);
            byte[] data = null;
            using RecordStore rs = RecordStore.openRecordStore(SOUND_SAVE_NAME, !isSaveSoundFileExists);
            if (isSaveSoundFileExists)
            {
                data = rs.getRecord(1);
            }
            else
            {
                if (rs.getNumRecords() == 0)
                    rs.addRecord(null, 0, 0);
            }

            if (data != null)
            {
                // Do nothing
            }
        }
        catch (Exception ex)
        {
            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when reading the save.",
                header: "Error reading game save");
        }
    }

    public void GameFrame_Save(int p_iLeftLevel)
    {
        GameFrame_SaveLevelInfo(p_iLeftLevel);

        if (s_synopsis == null)
            GameFrame_LoadSynopsis(false);

        // Level
        if (p_iLeftLevel is > LEVEL_WORLD_MAP and < LEVELS_COUNT)
        {
            // Save for level
            s_synopsis[p_iLeftLevel] = new Synopsis
            {
                LumsTaken = (sbyte)s_iLumsTaken,
                LumsTotal = (sbyte)s_iLumsTotal,
                CageOpened = (sbyte)s_iCageOpened,
                CageTotal = (sbyte)s_iCageTotal
            };

            // Update total
            s_synopsis[LEVEL_WORLD_MAP] = new Synopsis
            {
                LumsTaken = (sbyte)(s_synopsis[LEVEL_WORLD_MAP].LumsTaken + (sbyte)s_iLumsTaken),
                LumsTotal = (sbyte)(s_synopsis[LEVEL_WORLD_MAP].LumsTotal + (sbyte)s_iLumsTotal),
                CageOpened = (sbyte)(s_synopsis[LEVEL_WORLD_MAP].CageOpened + (sbyte)s_iCageOpened),
                CageTotal = (sbyte)(s_synopsis[LEVEL_WORLD_MAP].CageTotal + (sbyte)s_iCageTotal)
            };
            s_iLumsTaken = s_synopsis[LEVEL_WORLD_MAP].LumsTaken;
            s_iLumsTotal = s_synopsis[LEVEL_WORLD_MAP].LumsTotal;
            s_iCageOpened = s_synopsis[LEVEL_WORLD_MAP].CageOpened;
            s_iCageTotal = s_synopsis[LEVEL_WORLD_MAP].CageTotal;
        }
        // Worldmap
        else if (p_iLeftLevel == LEVEL_WORLD_MAP)
        {
            if (s_synopsis[m_gameFrame_curLevel].LumsTotal == -1 || s_synopsis[m_gameFrame_curLevel].CageTotal == -1)
            {
                s_synopsis[m_gameFrame_curLevel] = s_synopsis[m_gameFrame_curLevel] with
                {
                    LumsTotal = 0,
                    CageTotal = 0
                };
            }

            s_iLumsTaken = s_synopsis[m_gameFrame_curLevel].LumsTaken;
            s_iLumsTotal = s_synopsis[m_gameFrame_curLevel].LumsTotal;
            s_iCageOpened = s_synopsis[m_gameFrame_curLevel].CageOpened;
            s_iCageTotal = s_synopsis[m_gameFrame_curLevel].CageTotal;

            s_synopsis[LEVEL_WORLD_MAP] = new Synopsis
            {
                LumsTaken = (sbyte)(s_synopsis[LEVEL_WORLD_MAP].LumsTaken - (sbyte)s_iLumsTaken),
                LumsTotal = (sbyte)(s_synopsis[LEVEL_WORLD_MAP].LumsTotal - (sbyte)s_iLumsTotal),
                CageOpened = (sbyte)(s_synopsis[LEVEL_WORLD_MAP].CageOpened - (sbyte)s_iCageOpened),
                CageTotal = (sbyte)(s_synopsis[LEVEL_WORLD_MAP].CageTotal - (sbyte)s_iCageTotal)
            };
        }
        GameFrame_SaveSynopsis();
    }

    public void GameFrame_SaveSynopsis()
    {
        try
        {
            using (RecordStore rs = RecordStore.openRecordStore(GAME_SAVE_NAME, false))
            {
                byte[] data = new byte[s_synopsis.Length * 4 + 3];
                int offset = 0;
                for (int i = 0; i < s_synopsis.Length; i++)
                {
                    data[offset++] = (byte)s_synopsis[i].LumsTaken;
                    data[offset++] = (byte)s_synopsis[i].LumsTotal;
                    data[offset++] = (byte)s_synopsis[i].CageOpened;
                    data[offset++] = (byte)s_synopsis[i].CageTotal;
                }

                data[offset++] = (byte)m_gameFrame_curLevel;
                data[offset++] = (byte)m_gameFrame_unlockedLevel;
                data[offset++] = (byte)m_gameFrame_nLife;
                rs.setRecord(10, data, 0, data.Length);
                m_RecordUsedFlag[9] = true;
            }

            Rayman3.Save.ShowPopup();
        }
        catch (Exception ex)
        {
            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when saving the game.",
                header: "Error saving game");
        }
    }

    [MemberNotNull(nameof(s_synopsis))]
    public void GameFrame_LoadSynopsis(bool bGetGameInfo)
    {
        try
        {
            int offset = 0;
            s_synopsis = new Synopsis[10];
            using RecordStore rs = RecordStore.openRecordStore(GAME_SAVE_NAME, false);
            byte[] data = rs.getRecord(10);
            if (data != null && m_RecordUsedFlag[9])
            {
                for (int i = 0; i < s_synopsis.Length; i++)
                {
                    s_synopsis[i] = new Synopsis
                    {
                        LumsTaken = (sbyte)data[offset++],
                        LumsTotal = (sbyte)data[offset++],
                        CageOpened = (sbyte)data[offset++],
                        CageTotal = (sbyte)data[offset++]
                    };
                }
                if (bGetGameInfo)
                {
                    m_gameFrame_curLevel = (sbyte)data[offset++];
                    m_gameFrame_unlockedLevel = (sbyte)data[offset++];
                    m_gameFrame_nLife = (sbyte)data[offset++];
                    s_iLumsTaken = s_synopsis[m_gameFrame_curLevel].LumsTaken;
                    s_iLumsTotal = s_synopsis[m_gameFrame_curLevel].LumsTotal;
                    s_iCageOpened = s_synopsis[m_gameFrame_curLevel].CageOpened;
                    s_iCageTotal = s_synopsis[m_gameFrame_curLevel].CageTotal;
                }
                data = null;
            }
            else
            {
                s_synopsis[LEVEL_WORLD_MAP] = new Synopsis
                {
                    CageTotal = 0,
                    CageOpened = 0,
                    LumsTotal = 0,
                    LumsTaken = 0,
                };
                for (sbyte b = 1; b < s_synopsis.Length; b++)
                {
                    s_synopsis[b] = new Synopsis
                    {
                        CageOpened = 0,
                        LumsTaken = 0,
                        CageTotal = -1,
                        LumsTotal = -1,
                    };
                }
            }
        }
        catch (Exception ex)
        {
            s_synopsis[LEVEL_WORLD_MAP] = new Synopsis
            {
                CageTotal = 0,
                CageOpened = 0,
                LumsTotal = 0,
                LumsTaken = 0,
            };
            for (sbyte b = 1; b < s_synopsis.Length; b++)
            {
                s_synopsis[b] = new Synopsis
                {
                    CageOpened = 0,
                    LumsTaken = 0,
                    CageTotal = -1,
                    LumsTotal = -1,
                };
            }

            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when reading the save.",
                header: "Error reading game save");
        }
    }

    public void GameFrame_SaveLevelInfo(int pLevel)
    {
        try
        {
            using (RecordStore rs = RecordStore.openRecordStore(GAME_SAVE_NAME, false))
            {
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
                    m_RecordUsedFlag[pLevel - 1] = true;
                }
                else if (pLevel == LEVEL_WORLD_MAP)
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
                    m_RecordUsedFlag[10] = true;
                }
            }

            Rayman3.Save.ShowPopup();
        }
        catch (Exception ex)
        {
            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when saving the game.",
                header: "Error saving game");
        }
    }

    public void GameFrame_LoadLevelInfo(int pLevel)
    {
        try
        {
            using RecordStore rs = RecordStore.openRecordStore(GAME_SAVE_NAME, false);
            int offset = 0;
            if (pLevel > LEVEL_WORLD_MAP && pLevel <= m_gameFrame_nbLevels)
            {
                byte[] data = rs.getRecord(pLevel);
                if (data != null && m_RecordUsedFlag[pLevel - 1])
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
                }
            }
            else if (pLevel == LEVEL_WORLD_MAP)
            {
                byte[] data = rs.getRecord(11);
                if (data != null && m_RecordUsedFlag[10])
                {
                    byte a = data[offset++];
                    byte b = data[offset++];
                    byte c = data[offset++];
                    byte d = data[offset++];
                    byte e = data[offset++];
                    byte f = data[offset++];
                    byte g = data[offset++];
                    byte h = data[offset++];
                    pRayman.m_lInitX = ((long)a << 56) | ((long)b << 48) | ((long)c << 40) | ((long)d << 32) |
                                       ((long)e << 24) | ((long)f << 16) | ((long)g << 8) | (long)h;
                    a = data[offset++];
                    b = data[offset++];
                    c = data[offset++];
                    d = data[offset++];
                    e = data[offset++];
                    f = data[offset++];
                    g = data[offset++];
                    h = data[offset++];
                    pRayman.m_lInitY = ((long)a << 56) | ((long)b << 48) | ((long)c << 40) | ((long)d << 32) |
                                       ((long)e << 24) | ((long)f << 16) | ((long)g << 8) | (long)h;
                    const sbyte mask = 1;
                    for (int i = 0; i < actors.Length; i++)
                    {
                        byte buffer = data[offset++];
                        if ((buffer & mask) != 0)
                            actors[i].stateFlag |= ACTOR_STATE.DEAD;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Engine.Messages.EnqueueExceptionMessage(
                ex: ex,
                text: "An error occurred when reading the save.",
                header: "Error reading game save");
        }

        s_iLumsTaken = s_synopsis[pLevel].LumsTaken;
        s_iCageOpened = s_synopsis[pLevel].CageOpened;

        if (pLevel == 0)
        {
            s_iLumsTotal = s_synopsis[LEVEL_WORLD_MAP].LumsTotal;
            s_iCageTotal = s_synopsis[LEVEL_WORLD_MAP].CageTotal;
        }
        else
        {
            s_synopsis = null;
        }
    }
}