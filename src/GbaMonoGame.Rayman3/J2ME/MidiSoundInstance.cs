using System;
using System.Runtime.InteropServices;
using MeltySynth;
using Microsoft.Xna.Framework.Audio;

namespace GbaMonoGame.Rayman3.J2ME;

// Custom Readvanced class, using MeltySynth to play MIDI audio
public sealed class MidiSoundInstance : IDisposable
{
    public MidiSoundInstance(SoundFont soundFont, MidiFile midiFile, SOUND_INDEX soundIndex)
    {
        SoundFont = soundFont;
        MidiFile = midiFile;
        SoundIndex = soundIndex;

        _sequencer = new MidiFileSequencer(new Synthesizer(soundFont, SAMPLE_RATE));

        _dynamicSound = new DynamicSoundEffectInstance(SAMPLE_RATE, AudioChannels.Stereo);
        _buffer = new byte[4 * (int)(SAMPLE_RATE * BUFFER_LENGTH)];

        _dynamicSound.BufferNeeded += (_, _) => SubmitBuffer();
    }

    private const float BUFFER_LENGTH = 0.1f; // 100 ms
    private const int SAMPLE_RATE = 48000;

    private readonly MidiFileSequencer _sequencer;
    private readonly DynamicSoundEffectInstance _dynamicSound;
    private readonly byte[] _buffer;

    public SoundFont SoundFont { get; }
    public MidiFile MidiFile { get; }
    public SOUND_INDEX SoundIndex { get; }
    public bool EndOfSequence => _sequencer.EndOfSequence;
    public SoundState State => _dynamicSound.State;
    public bool IsDisposed => _dynamicSound.IsDisposed;

    private void SubmitBuffer()
    {
        _sequencer.RenderInterleavedInt16(MemoryMarshal.Cast<byte, short>(_buffer.AsSpan()));
        _dynamicSound.SubmitBuffer(_buffer, 0, _buffer.Length);
    }

    public void Play(bool loop)
    {
        if (_dynamicSound.IsDisposed)
            throw new ObjectDisposedException(nameof(_dynamicSound));

        _sequencer.Play(MidiFile, loop);
        SubmitBuffer();
        _dynamicSound.Play();
    }

    public void Stop()
    {
        if (_dynamicSound.IsDisposed)
            throw new ObjectDisposedException(nameof(_dynamicSound));

        _dynamicSound.Stop();
        _sequencer.Stop();
    }

    public void SetVolume(float volume)
    {
        if (_dynamicSound.IsDisposed)
            throw new ObjectDisposedException(nameof(_dynamicSound));

        _dynamicSound.Volume = volume;
    }

    public void Dispose()
    {
        if (_dynamicSound.IsDisposed)
            return;

        Stop();
        _dynamicSound.Dispose();
    }
}