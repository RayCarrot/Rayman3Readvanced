using System;

namespace GbaMonoGame.Rayman3.J2ME;

public partial class Game
{
    //  Player[] m_SoundPlayer = new Player[22]; // TODO: Implement
    public bool m_bSoundPlaying { get; set; }
    public int m_iCurrentSound { get; set; }
    public long m_lLastSoundTime { get; set; }

    public void InitSound()
    {
        m_iCurrentSound = -1;
        m_bSoundPlaying = false;
    }

    public void LoadSound(int iPackage)
    {
        RM.Load(RESOURCE_ID_DATA_SOUND_MENU_MOVE);
        RM.Load(RESOURCE_ID_DATA_SOUND_MENU_SELECT);
        RM.Load(RESOURCE_ID_DATA_SOUND_MUSIC_SPLASH);
        RM.Load(RESOURCE_ID_DATA_SOUND_CAGE_BREAK);
        RM.Load(RESOURCE_ID_DATA_SOUND_CAGE_HIT);
        RM.Load(RESOURCE_ID_DATA_SOUND_ENEMY_DEATH);
        RM.Load(RESOURCE_ID_DATA_SOUND_ENEMY_HIT);
        RM.Load(RESOURCE_ID_DATA_SOUND_ENTER_LEVEL);
        RM.Load(RESOURCE_ID_DATA_SOUND_LUMS);
        RM.Load(RESOURCE_ID_DATA_SOUND_LUMS_RED);
        RM.Load(RESOURCE_ID_DATA_SOUND_LUMS_WHITE);
        RM.Load(RESOURCE_ID_DATA_SOUND_MUSIC_GAMEOVER);
        RM.Load(RESOURCE_ID_DATA_SOUND_MUSIC_LEVELDONE);
        RM.Load(RESOURCE_ID_DATA_SOUND_MUSIC_MAP);
        RM.Load(RESOURCE_ID_DATA_SOUND_PUNCH_CHARGE);
        RM.Load(RESOURCE_ID_DATA_SOUND_PUNCH_RELEASED);
        RM.Load(RESOURCE_ID_DATA_SOUND_RAYMAN_CROUCH);
        RM.Load(RESOURCE_ID_DATA_SOUND_RAYMAN_DEATH);
        RM.Load(RESOURCE_ID_DATA_SOUND_RAYMAN_HELICO);
        RM.Load(RESOURCE_ID_DATA_SOUND_RAYMAN_HIT);
        RM.Load(RESOURCE_ID_DATA_SOUND_RAYMAN_JUMP);
        RM.Load(RESOURCE_ID_DATA_SOUND_RAYMAN_WATER);
        RM.Synchronize();
    }

    public void FreeSound()
    {
        // TODO: Implement
        //for (int i = 0; i < 22; i++)
        //{
        //    if (this.m_SoundPlayer[i] != null)
        //    {
        //        this.m_SoundPlayer[i].close();
        //        this.m_SoundPlayer[i] = null;
        //    }
        //}
        RM.Free(RESOURCE_ID_DATA_SOUND_MENU_MOVE);
        RM.Free(RESOURCE_ID_DATA_SOUND_MENU_SELECT);
        RM.Free(RESOURCE_ID_DATA_SOUND_MUSIC_SPLASH);
        RM.Free(RESOURCE_ID_DATA_SOUND_CAGE_BREAK);
        RM.Free(RESOURCE_ID_DATA_SOUND_CAGE_HIT);
        RM.Free(RESOURCE_ID_DATA_SOUND_ENEMY_DEATH);
        RM.Free(RESOURCE_ID_DATA_SOUND_ENEMY_HIT);
        RM.Free(RESOURCE_ID_DATA_SOUND_ENTER_LEVEL);
        RM.Free(RESOURCE_ID_DATA_SOUND_LUMS);
        RM.Free(RESOURCE_ID_DATA_SOUND_LUMS_RED);
        RM.Free(RESOURCE_ID_DATA_SOUND_LUMS_WHITE);
        RM.Free(RESOURCE_ID_DATA_SOUND_MUSIC_GAMEOVER);
        RM.Free(RESOURCE_ID_DATA_SOUND_MUSIC_LEVELDONE);
        RM.Free(RESOURCE_ID_DATA_SOUND_MUSIC_MAP);
        RM.Free(RESOURCE_ID_DATA_SOUND_PUNCH_CHARGE);
        RM.Free(RESOURCE_ID_DATA_SOUND_PUNCH_RELEASED);
        RM.Free(RESOURCE_ID_DATA_SOUND_RAYMAN_CROUCH);
        RM.Free(RESOURCE_ID_DATA_SOUND_RAYMAN_DEATH);
        RM.Free(RESOURCE_ID_DATA_SOUND_RAYMAN_HELICO);
        RM.Free(RESOURCE_ID_DATA_SOUND_RAYMAN_HIT);
        RM.Free(RESOURCE_ID_DATA_SOUND_RAYMAN_JUMP);
        RM.Free(RESOURCE_ID_DATA_SOUND_RAYMAN_WATER);
        RM.Synchronize();
    }

    public void PlaySound(SOUND_INDEX iSoundIndex, bool bStopCurrent)
    {
        PlaySound(iSoundIndex, bStopCurrent, 1);
    }

    public void InitSounds()
    {
        try
        {
            for (int i = 0; i < 22; i++)
            {
                if (RM.Array_Data[23 + i] != null)
                {
                    // TODO: Implement
                    // InputStream soundIStream = new sbyteArrayInputStream(RM.Array_Data[23 + i]);
                    // m_SoundPlayer[i] = Manager.createPlayer(soundIStream, "audio/midi");
                    // m_SoundPlayer[i].addPlayerListener(this);
                    // m_SoundPlayer[i].prefetch();
                    // m_SoundPlayer[i].realize();
                    // soundIStream = null;
                    // System.gc();
                }
            }
        }
        catch (Exception e)
        {
            System.println($"InitSounds error {e}");
        }
    }

    public void PlaySound(SOUND_INDEX iSoundIndex, bool bStopCurrent, int iLoop)
    {
        if (SoundVolume == 0 || (int)iSoundIndex == -1)
            return;

        int iID = (int)(iSoundIndex - 23);
        if (m_gameFrame_curLevel <= LEVEL_WORLD_MAP)
            bStopCurrent = false;

        // TODO: Implement
        //if (!bStopCurrent && m_SoundPlayer[iID] != null)
        //{
        //    if (m_SoundPlayer[iID].getState() == 400)
        //        return;
        //    if (iSoundIndex != 35 && iSoundIndex != 36 && iSoundIndex != 32 && iSoundIndex != 34)
        //        return;
        //}

        //if (m_SoundPlayer[iID] != null)
        //    StopSound();

        //if (m_SoundPlayer[iID] != null)
        //{
        //    try
        //    {
        //        m_SoundPlayer[iID].setLoopCount(iLoop);
        //        m_SoundPlayer[iID].start();
        //        m_iCurrentSound = iSoundIndex;
        //        m_bSoundPlaying = true;
        //        m_lLastSoundTime = System.currentTimeMillis();
        //    }
        //    catch (Exception e)
        //    {
        //        System.println($"playsound() Exception: {e}");
        //    }
        //}
    }

    public void StopSound(int iSoundIndex)
    {
        if (iSoundIndex == m_iCurrentSound)
            StopSound();
    }

    public void StopSound()
    {
        try
        {
            // TODO: Implement
            //for (int i = 0; i < 22; i++)
            //{
            //    if (m_SoundPlayer[i] != null)
            //        m_SoundPlayer[i].stop();
            //}
            m_bSoundPlaying = false;
        }
        catch (Exception e)
        {
            System.println($"stopsound exception: {e}");
        }
    }

    public void setSoundVolume(int soundlevel)
    {
        try
        {
            // TODO: Implement
            //for (int i = 0; i < 22; i++)
            //{
            //    if (m_SoundPlayer[i] != null && m_SoundPlayer[i].getState() != 100)
            //        ((VolumeControl)m_SoundPlayer[i].getControl("VolumeControl")).setLevel(soundlevel);
            //}
        }
        catch (Exception e)
        {
            System.println(e.ToString());
        }
    }
}