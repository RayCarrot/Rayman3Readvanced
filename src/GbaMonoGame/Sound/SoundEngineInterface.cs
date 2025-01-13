using System;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

// Useful links for reverse engineering the GBA MP2k sound engine:
// https://loveemu.github.io/vgmdocs/Summary_of_GBA_Standard_Sound_Driver_MusicPlayer2000.html
// https://github.dev/pret/pokeruby/blob/master/include/gba/m4a_internal.h

// In the game this interfaces with the MP2K MIDI engine
public static class SoundEngineInterface
{
    #region Constant Fields

    public const float MinPan = -128;
    public const float MaxPan = 127;
    public const float MaxVolume = 128;

    #endregion

    #region Public Methods

    public static void Load()
    {
        // Load the Midi interface on GBA
        if (Rom.Platform == Platform.GBA)
        {
            MidiInterface.MidiParams = new MidiParams
            {
                ReverbValue = 0,
                NbOfVoices = 7,
                MasterVolume = MaxVolume,
                Freq = 0x70000,
            };
            MidiInterface.SetMidiParams();
        }
    }

    public static void SetNbVoices(byte newNbOfVoices)
    {
        // Only implemented on GBA
        if (Rom.Platform == Platform.GBA)
        {
            if (newNbOfVoices is < 1 or > 12)
                throw new Exception("Invalid number of voices. Must be a value between 1 and 12.");

            MidiInterface.MidiParams.NbOfVoices = newNbOfVoices;
            MidiInterface.SetMidiParams();
        }
    }

    #endregion
}