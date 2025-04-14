using System;
using System.Collections.Generic;
using AteChips.Shared.Sound;

namespace AteChips.Core;

public partial class Buzzer : IBuzzer
{
    private readonly IAudioOutputSignal _output;
    private double _phase;
    private const double FREQUENCY = 440.0; // A4 pitch
    private const double TAU = Math.PI * 2;
    public int SampleRate => 44100;
    public int Channels => 2;
    private readonly CrystalTimer _timer;


    public Buzzer(CrystalTimer timer)
    {
        _output = new AudioOutputSignal(this);
        _timer = timer;
    }

    public bool VisualShown { get; set; }
    public string Name => nameof(Buzzer);
    public void Reset()
    {
    }

    public IEnumerable<IAudioOutputSignal> GetOutputs() => [_output];

    public IAudioOutputSignal GetPrimaryOutput() => _output;

    public int GetSamples(float[] buffer, int offset, int count)
    {

        bool makingSound = _timer.SoundTimer > 0;

        double phaseIncrement = TAU * FREQUENCY / SampleRate;

        for (int i = 0; i < count; i += Channels)
        {

            float sample = makingSound ? (float)Math.Sin(_phase) : 0;

            buffer[offset + i] = sample;       // Left
            buffer[offset + i + 1] = sample;   // Right

            _phase += phaseIncrement;
            if (_phase >= TAU)
            {
                _phase -= TAU;
            }
        }

        return count;
    }
}