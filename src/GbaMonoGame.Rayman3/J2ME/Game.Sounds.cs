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
        RM.Load(0x6000011F);
        RM.Load(0x60000120);
        RM.Load(0x60000124);
        RM.Load(0x60000117);
        RM.Load(0x60000118);
        RM.Load(0x60000119);
        RM.Load(0x6000011A);
        RM.Load(0x6000011B);
        RM.Load(0x6000011C);
        RM.Load(0x6000011D);
        RM.Load(0x6000011E);
        RM.Load(0x60000121);
        RM.Load(0x60000122);
        RM.Load(0x60000123);
        RM.Load(0x60000125);
        RM.Load(0x60000126);
        RM.Load(0x60000127);
        RM.Load(0x60000128);
        RM.Load(0x60000129);
        RM.Load(0x6000012A);
        RM.Load(0x6000012B);
        RM.Load(0x6000012C);
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
        RM.Free(0x6000011F);
        RM.Free(0x60000120);
        RM.Free(0x60000124);
        RM.Free(0x60000117);
        RM.Free(0x60000118);
        RM.Free(0x60000119);
        RM.Free(0x6000011A);
        RM.Free(0x6000011B);
        RM.Free(0x6000011C);
        RM.Free(0x6000011D);
        RM.Free(0x6000011E);
        RM.Free(0x60000121);
        RM.Free(0x60000122);
        RM.Free(0x60000123);
        RM.Free(0x60000125);
        RM.Free(0x60000126);
        RM.Free(0x60000127);
        RM.Free(0x60000128);
        RM.Free(0x60000129);
        RM.Free(0x6000012A);
        RM.Free(0x6000012B);
        RM.Free(0x6000012C);
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
        if (m_gameFrame_curLevel <= 0)
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