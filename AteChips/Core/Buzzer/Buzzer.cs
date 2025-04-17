using System;
using System.Collections.Generic;
using System.Numerics;
using AteChips.Core.Shared.Base;
using AteChips.Shared.Sound;

namespace AteChips.Core;

public partial class Buzzer : VisualizableHardware, IBuzzer
{
    private readonly IAudioOutputSignal _output;
    private double _phase;
    private const double FREQUENCY = 440.0; // A4 pitch
    private const double TAU = Math.PI * 2;
    public int SampleRate => 44100;
    public int Channels => 2;
    private readonly CrystalTimer _timer;
    private float _currentAmplitude = 0f;

    public Buzzer(CrystalTimer timer)
    {
        _output = new AudioOutputSignal(this);
        _timer = timer;
    }

    public void Reset()
    {
    }

    public bool IsMuted { get; set; } = false;
    public float Pitch { get; set; } = 440;
    public float Volume { get; set; } = 0.6f;
    public bool TestTone { get; set; } = false;
    public WaveformTypes Waveform { get; set; } = WaveformTypes.Square;

    public IEnumerable<IAudioOutputSignal> GetOutputs() => [_output];

    public IAudioOutputSignal GetPrimaryOutput() => _output;

    public int GetSamples(float[] buffer, int offset, int count)
    {
        bool makingSound = (_timer.SoundTimer > 0 && !IsMuted) || TestTone;

        double frequency = Pitch;
        double phaseIncrement = TAU * frequency / SampleRate;

        float targetAmplitude = makingSound ? Volume : 0f;
        float fadeSpeed = 1f / (SampleRate * 0.01f); // fade over 10ms

        for (int i = 0; i < count; i += Channels)
        {
            // Smoothly approach target amplitude
            _currentAmplitude += (targetAmplitude - _currentAmplitude) * fadeSpeed;

            float sample = GetWaveform(_phase) * _currentAmplitude;

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

    protected float GetWaveform(double phase)
    {
        return Waveform switch
        {
            WaveformTypes.Square => SquareWave(phase),
            WaveformTypes.Sawtooth => SawtoothWave(phase),
            WaveformTypes.Triangle => TriangleWave(phase),
            WaveformTypes.Sine => SineWave(phase),
            _ => throw new ArgumentOutOfRangeException(nameof(Waveform), "Invalid waveform type")
        };
    }

    public static float SineWave(double phase) => (float) Math.Sin(phase);
    public static float SquareWave(double phase) => Math.Sin(phase) >= 0 ? 1f : -1f;
    public static float TriangleWave(double phase) => (float)(2.0 / Math.PI * Math.Asin(Math.Sin(phase)));
    public static float SawtoothWave(double phase) => (float)(2.0 * (phase / TAU) - 1.0);
}