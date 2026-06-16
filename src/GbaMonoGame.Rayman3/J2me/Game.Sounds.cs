using System;
using System.Collections.Generic;
using System.IO;
using MeltySynth;

namespace GbaMonoGame.Rayman3.J2me;

public partial class Game
{
    public const int SOUNDS_START_INDEX = 23;
    public const int SOUNDS_COUNT = 22;

    public const int LOOP_NONE = 1;
    public const int LOOP_INFINITE = 255; // NOTE: This is a bug in the og game code - should be -1

    public Player[] m_SoundPlayer { get; } = new Player[SOUNDS_COUNT]; // Unused in Readvanced
    public bool m_bSoundPlaying { get; set; } // Unused - only ever set
    public SOUND_INDEX m_iCurrentSound { get; set; } // Unused in Readvanced
    public long m_lLastSoundTime { get; set; } // Unused - only ever set

    // Custom for Readvanced MIDI playback
    public SoundFont SoundFont { get; set; }
    public MidiFile[] MidiFiles { get; } = new MidiFile[SOUNDS_COUNT];
    public List<MidiSoundInstance> SoundInstances { get; } = []; // TODO: Pause sounds when pausing game

    public void InitSound()
    {
        m_iCurrentSound = SOUND_INDEX.INVALID;
        m_bSoundPlaying = false;
    }

    public void LoadSound(int iPackage)
    {
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_MENU_MOVE);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_MENU_SELECT);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_MUSIC_SPLASH);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_CAGE_BREAK);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_CAGE_HIT);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_ENEMY_DEATH);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_ENEMY_HIT);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_ENTER_LEVEL);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_LUMS);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_LUMS_RED);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_LUMS_WHITE);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_MUSIC_GAMEOVER);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_MUSIC_LEVELDONE);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_MUSIC_MAP);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_PUNCH_CHARGE);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_PUNCH_RELEASED);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_RAYMAN_CROUCH);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_RAYMAN_DEATH);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_RAYMAN_HELICO);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_RAYMAN_HIT);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_RAYMAN_JUMP);
        RM.LoadData<MidiResource>(RESOURCE_ID_DATA_SOUND_RAYMAN_WATER);
        RM.Synchronize();
    }

    // Unused
    public void FreeSound()
    {
        for (int i = 0; i < SOUNDS_COUNT; i++)
        {
            if (m_SoundPlayer[i] != null)
            {
                m_SoundPlayer[i].close();
                m_SoundPlayer[i] = null;
            }
        }

        // Readvanced
        SoundFont = null;
        Array.Clear(MidiFiles);

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

    public void InitSounds()
    {
        // Custom for Readvanced MIDI playback
        SoundFont = new SoundFont($"{Paths.AssetsDirectoryName}/{Assets.BaseName}/J2me/GeneralUser-GS.sf2");

        for (int i = 0; i < SOUNDS_COUNT; i++)
        {
            if (RM.GetData<MidiResource>(SOUNDS_START_INDEX + i) is { Data: { } midi })
            {
                // Readvanced code
                if (true)
                {
                    MidiFiles[i] = new MidiFile(new MemoryStream(midi));
                }
                // Original game code
                else
                {
                    using MemoryStream soundIStream = new(midi);
                    m_SoundPlayer[i] = new Player(soundIStream, "audio/midi");
                    m_SoundPlayer[i].addPlayerListener(this);
                    m_SoundPlayer[i].prefetch();
                    m_SoundPlayer[i].realize();
                    System.gc();
                }
            }
        }
    }

    public void PlaySound(SOUND_INDEX iSoundIndex, bool bStopCurrent)
    {
        PlaySound(iSoundIndex, bStopCurrent, LOOP_NONE);
    }

    public void PlaySound(SOUND_INDEX iSoundIndex, bool bStopCurrent, int iLoop)
    {
        // Validate sound and that volume isn't off
        if (SoundVolume == VOL_OFF || iSoundIndex == SOUND_INDEX.INVALID)
            return;

        // Get the sound ID
        int iID = (int)(iSoundIndex - SOUNDS_START_INDEX);

        // Readvanced code
        if (true)
        {
            bool loop = iLoop switch
            {
                LOOP_NONE => false,
                LOOP_INFINITE => true,
                _ => throw new ArgumentOutOfRangeException(nameof(iLoop), iLoop, null)
            };

            // If the sound loops then we only want to play if not already playing
            if (loop)
            {
                foreach (MidiSoundInstance soundInstance in SoundInstances)
                    if (soundInstance.SoundIndex == iSoundIndex)
                        return; 
            }

            MidiSoundInstance sndInstance = new(SoundFont, MidiFiles[iID], iSoundIndex);
            sndInstance.SetVolume(SoundVolume / 100f * GetMasterVolume(iSoundIndex));
            sndInstance.Play(loop);
            SoundInstances.Add(sndInstance);
        }
        // Original game code
        else
        {
            // Don't stop current sound if in the menu or worldmap since music is playing there
            if (m_gameFrame_curLevel <= LEVEL_WORLD_MAP)
                bStopCurrent = false;

            // Validate not null
            if (m_SoundPlayer[iID] == null)
                return;

            // If we don't stop current sound...
            if (!bStopCurrent)
            {
                // Ignore if already playing
                if (m_SoundPlayer[iID].getState() == PLAYER_STATE.STARTED)
                    return;

                // Ignore if not music
                if (iSoundIndex is not (SOUND_INDEX.music_map or SOUND_INDEX.music_splash or SOUND_INDEX.menu_select or SOUND_INDEX.music_leveldone))
                    return;
            }

            // Stop sounds
            StopSound();

            m_SoundPlayer[iID].setLoopCount(iLoop);
            m_SoundPlayer[iID].start();
            m_iCurrentSound = iSoundIndex;
            m_bSoundPlaying = true;
            m_lLastSoundTime = System.currentTimeMillis();
        }
    }

    // Unused in the original game
    public void StopSound(SOUND_INDEX iSoundIndex)
    {
        // Readvanced code
        if (true)
        {
            MidiSoundInstance foundInstance = null;
            foreach (MidiSoundInstance soundInstance in SoundInstances)
            {
                if (soundInstance.SoundIndex == iSoundIndex)
                {
                    foundInstance = soundInstance;
                    break;
                }
            }

            if (foundInstance != null)
            {
                foundInstance.Stop();
                foundInstance.Dispose();
                SoundInstances.Remove(foundInstance);
            }
        }
        // Original game code
        else
        {
            if (iSoundIndex == m_iCurrentSound)
                StopSound();
        }
    }

    public void StopSound()
    {
        // Readvanced code
        if (true)
        {
            foreach (MidiSoundInstance soundInstance in SoundInstances)
            {
                soundInstance.Stop();
                soundInstance.Dispose();
            }
            SoundInstances.Clear();
        }
        // Original game code
        else
        {
            for (int i = 0; i < SOUNDS_COUNT; i++)
            {
                if (m_SoundPlayer[i] != null)
                    m_SoundPlayer[i].stop();
            }
            m_bSoundPlaying = false;
        }
    }

    public void setSoundVolume(int soundlevel)
    {
        // Readvanced code
        if (true)
        {
            foreach (MidiSoundInstance soundInstance in SoundInstances)
                soundInstance.SetVolume(SoundVolume / 100f * GetMasterVolume(soundInstance.SoundIndex));
        }
        // Original game code
        else
        {
            for (int i = 0; i < SOUNDS_COUNT; i++)
            {
                if (m_SoundPlayer[i] != null && m_SoundPlayer[i].getState() != PLAYER_STATE.UNREALIZED)
                    m_SoundPlayer[i].setVolumeLevel(soundlevel);
            }
        }
    }

    // Custom in Readvanced
    public float GetMasterVolume(SOUND_INDEX iSoundIndex)
    {
        if (iSoundIndex is SOUND_INDEX.music_gameover or SOUND_INDEX.music_leveldone or SOUND_INDEX.music_map or SOUND_INDEX.music_splash)
            return Engine.Settings.Local.Sound.MusicVolume;
        else
            return Engine.Settings.Local.Sound.SfxVolume;
    }
    public void UpdateSounds()
    {
        // Dispose stopped sounds and remove from list
        foreach (MidiSoundInstance soundInstance in SoundInstances)
            if (soundInstance.EndOfSequence)
                soundInstance.Dispose();
        SoundInstances.RemoveAll(static soundInstance => soundInstance.IsDisposed);
    }
}