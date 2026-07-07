using System.Diagnostics;
using BinarySerializer.Ubisoft.GbaEngine;

namespace GbaMonoGame;

// Only implemented on GBA
public class MidiInterface
{
    public MidiInterface()
    {
        Debug.Assert(Rom.Platform == Platform.GBA);
    }

    public void SetMidiParams(MidiParams midiParams)
    {
        uint newMasterVol = 0x7f;
        if (midiParams.MasterVolume != 0x80)
            newMasterVol = midiParams.MasterVolume;
        newMasterVol >>= 3;

        Debug.Assert(newMasterVol <= 15);

        uint mode = (uint)(midiParams.ReverbValue + 0x80) | (uint)(midiParams.NbOfVoices << 8) | (newMasterVol << 0xc) | midiParams.Freq;

        // NOTE: Game sets the mode in the MP2K engine here
    }
}