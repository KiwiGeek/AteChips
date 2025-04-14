using System;
using System.Collections.Generic;
using AteChips.Shared.Sound;

namespace AteChips.Core;

public partial class Buzzer : IBuzzer
{
    private IAudioOutputSignal _output;
    private double _phase;
    private const double Frequency = 440.0; // A4 pitch
    private const double TwoPi = Math.PI * 2;
    private readonly Ram _ram;
    private byte SoundTimer
    {
        get => _ram.GetByte(Ram.SOUND_TIMER_ADDR);
        set => _ram.SetByte(Ram.SOUND_TIMER_ADDR, value);
    }


    public Buzzer(Ram ram)
    {
        _output = new AudioOutputSignal(this);
        _ram = ram;
    }

    public bool VisualShown { get; set; }
    public string Name { get; }
    public void Reset()
    {
    }

    public double FrequencyHz { get; }
    public byte UpdatePriority { get; }
    public IEnumerable<IAudioOutputSignal> GetOutputs() => [_output];

    public IAudioOutputSignal GetPrimaryOutput() => _output;

    public int SampleRate => 44100;
    public int Channels => 1;
    public int GetSamples(float[] buffer, int offset, int count)
    {

        bool makingSound = SoundTimer > 0;

        double phaseIncrement = TwoPi * Frequency / SampleRate;

        for (int i = 0; i < count; i += Channels)
        {

            float sample = makingSound ? (float)Math.Sin(_phase) : 0;

            buffer[offset + i] = sample;       // Left
            //buffer[offset + i + 1] = sample;   // Right

            _phase += phaseIncrement;
            if (_phase >= TwoPi)
                _phase -= TwoPi;
        }

        return count;
    }
}