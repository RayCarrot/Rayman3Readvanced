namespace GbaMonoGame;

public class MidiParams
{
    public byte ReverbValue { get; set; }
    public byte NbOfVoices { get; set; } // maxChans in MP2k
    public byte MasterVolume { get; set; }
    public uint Freq { get; set; }
}