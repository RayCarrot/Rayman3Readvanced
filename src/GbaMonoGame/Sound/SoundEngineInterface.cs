using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

// Useful links for reverse engineering the GBA MP2k sound engine:
// https://loveemu.github.io/vgmdocs/Summary_of_GBA_Standard_Sound_Driver_MusicPlayer2000.html
// https://github.dev/pret/pokeruby/blob/master/include/gba/m4a_internal.h
// https://github.com/SAT-R/sa2/blob/main/src/lib/m4a/m4a.c

// In the game this interfaces with the MP2K MIDI engine
public class SoundEngineInterface
{
    public SoundEngineInterface()
    {
        // Only implemented on GBA
        if (Rom.Platform != Platform.GBA)
            return;

        // Create the midi interface
        Midi = new MidiInterface();

        // Set the default midi parameters
        MidiParams = new MidiParams
        {
            ReverbValue = 0,
            NbOfVoices = 7,
            MasterVolume = MaxVolume,
            Freq = 0x70000
        };
        Midi.SetMidiParams(MidiParams);
    }

    public const sbyte MinPan = -128;
    public const sbyte MaxPan = 127;
    public const byte MaxVolume = 128;

    public MidiInterface Midi { get; }
    public MidiParams MidiParams { get; }

    public void SetReverb(byte newReverbValue)
    {
        // Only implemented on GBA
        if (Rom.Platform != Platform.GBA)
            return;

        Debug.Assert(newReverbValue <= 127);
        MidiParams.ReverbValue = newReverbValue;
        Midi.SetMidiParams(MidiParams);
    }

    public void SetNbVoices(byte newNbOfVoices)
    {
        // Only implemented on GBA
        if (Rom.Platform != Platform.GBA) 
            return;

        Debug.Assert(newNbOfVoices is >= 1 and <= 12);
        MidiParams.NbOfVoices = newNbOfVoices;
        Midi.SetMidiParams(MidiParams);
    }

    public void SetMasterVolume(byte newMasterVol)
    {
        // Only implemented on GBA
        if (Rom.Platform != Platform.GBA) 
            return;

        Debug.Assert(newMasterVol <= MaxVolume);
        MidiParams.MasterVolume = newMasterVol;
        Midi.SetMidiParams(MidiParams);
    }

    public void SetFreq(byte newFreq)
    {
        // Only implemented on GBA
        if (Rom.Platform != Platform.GBA) 
            return;

        MidiParams.Freq = newFreq;
        Midi.SetMidiParams(MidiParams);
    }
}