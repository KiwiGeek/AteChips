using System;
using System.Collections.Generic;
using AteChips.Shared.Sound;

namespace AteChips.Core;

public partial class Buzzer : IBuzzer
{
    private readonly IAudioOutputSignal _output;
    private float _phase;
    private const float TAU = (float)(Math.PI * 2);
    private static readonly Random Rng = new();
    private long _totalSamplesGenerated;

    public int SampleRate => 44100;
    public int Channels => 2;
    private readonly CrystalTimer _timer;
    private float _currentAmplitude;

    public Buzzer(CrystalTimer timer)
    {
        _output = new AudioOutputSignal(this);
        _timer = timer;

        VisualizerInit();
    }

    public void Reset()
    {
        // No need to flush anything; just let the host produce silence when needed.
        // All the settings are host related, so they shouldn't be reset when the device
        // is reset.
    }

    public bool IsMuted { get; set; } = false;
    public float Pitch { get; set; } = 440.0f;
    public float Volume { get; set; } = 0.4f;
    public bool TestTone { get; set; } = false;
    public float PulseDutyCycle { get; set; } = 0.25f;
    public float RoundedSharpness { get; set; } = 15.0f;
    public int StairSteps { get; set; } = 8;

    public WaveformType Waveform { get; set; } = WaveformType.Square;

    public IEnumerable<IAudioOutputSignal> GetOutputs() => [_output];

    public IAudioOutputSignal GetPrimaryOutput() => _output;

    public int GetSamples(float[] buffer, int offset, int count)
    {
        bool makingSound = (_timer.SoundTimer > 0 && !IsMuted) || TestTone;

        float frequency = Pitch;
        float phaseIncrement = TAU * frequency / SampleRate;

        float targetAmplitude = makingSound ? Volume : 0f;
        float fadeSpeed = 1f / (SampleRate * 0.01f);

        for (int i = 0; i < count; i += Channels)
        {
            // Smoothly approach target amplitude
            _currentAmplitude += (targetAmplitude - _currentAmplitude) * fadeSpeed;

            float sample = GetWaveformSample(_phase, _totalSamplesGenerated) * _currentAmplitude * GetNormalizationFactor(Waveform);

            buffer[offset + i] = sample;       // Left
            buffer[offset + i + 1] = sample;   // Right

            _phase += phaseIncrement;
            if (_phase >= TAU)
            {
                _phase -= TAU;
            }
        }

        _totalSamplesGenerated += count / Channels;
        return count;
    }



    public static float GetNormalizationFactor(WaveformType type) => 0.1f;

    protected float GetWaveformSample(float phase, float time)
    {
        return Waveform switch
        {
            WaveformType.Square => SquareWave(phase),
            WaveformType.Sawtooth => SawtoothWave(phase),
            WaveformType.Triangle => TriangleWave(phase),
            WaveformType.Pulse => PulseWave(phase),
            WaveformType.Noise => NoiseWave(phase),
            WaveformType.Sine => SineWave(phase),
            WaveformType.HalfSine => HalfSineWave(phase),
            WaveformType.RoundedSquare => RoundedSquareWave(phase),
            WaveformType.Staircase => StaircaseWave(phase),
            WaveformType.ChipTuneLead => ChipTuneLead(phase),
            WaveformType.StaticBuzz => StaticBuzzWave(phase),
            WaveformType.DirtyBass => DirtyBassWave(phase),
            WaveformType.LunarPad => LunarPadWave(phase),
            WaveformType.RetroLaser => RetroLaserWave(phase),
            WaveformType.SolarRamp => SolarRampWave(phase),
            WaveformType.MorphPulse => MorphPulseWave(phase),
            WaveformType.DetuneTwin => DeTuneTwin(phase),
            WaveformType.RingByte => RingByteWave(phase),
            WaveformType.BitBuzz => BitBuzzWave(phase),
            WaveformType.FormantVox => FormantVoxWave(phase),
            _ => throw new InvalidOperationException("Invalid waveform type")
        };
    }

    private static float NextFloat() => (float)Rng.NextDouble();

    float SquareWave(float phase) => phase < 0.5f ? 1f : -1f;
    float SawtoothWave(float phase) => 2f * (phase % 1f) - 1f;
    float TriangleWave(float phase) => 4f * MathF.Abs(phase - 0.5f) - 1f;
    float PulseWave(float phase) => (phase % 1f) < (PulseDutyCycle / TAU) ? 1f : -1f;
    float NoiseWave(float _) => NextFloat() * 2.0f - 1.0f;
    float SineWave(float phase) => MathF.Sin(TAU * phase);
    float HalfSineWave(float phase) => MathF.Sin(phase * TAU * 0.5f);
    float RoundedSquareWave(float phase) => MathF.Tanh(MathF.Sin(phase * TAU) * RoundedSharpness);
    float StaircaseWave(float phase) => (StairSteps <= 1) ? 0f : ((2f * (int)(MathF.Min((phase % 1f), 0.99999994f) * StairSteps) + 1f - StairSteps) / (StairSteps - 1f));
    float ChipTuneLead(float phase) => (0.6f * MathF.Sign(MathF.Sin(phase * TAU))) + (0.4f * (2f * MathF.Abs(2f * (phase % 1f) - 1f) - 1f));
    float StaticBuzzWave(float phase) => 0.7f * ((phase % 1f) < (PulseDutyCycle /TAU) ? 1f : -1f) + 0.3f * (NextFloat() * 2f - 1f);
    float DirtyBassWave(float phase) => 0.5f * (2f * (phase - 0.5f)) + 0.5f * (2f * MathF.Abs(2f * phase - 1f) - 1f);
    float LunarPadWave(float phase) => ((0.8f * TriangleWave(phase)) + (0.2f * NoiseWave(phase)));
    float RetroLaserWave(float phase) =>  MathF.Sin(TAU * phase) * 0.8f - 0.2f + ((phase % 1f) < (PulseDutyCycle / TAU) ? 0.4f : 0f);
    float SolarRampWave(float phase) => MathF.Pow(phase % 1f, 2.5f) * 2f - 1f;
    float MorphPulseWave(float phase) => MathF.Sin(phase) + ((((phase %1f)  < (PulseDutyCycle /TAU)) ? 1f : -1f) - MathF.Sin(phase)) * 0.4f;
    float DeTuneTwin(float phase) => 0.5f * (MathF.Sin(TAU * phase) + (2f * MathF.Abs(2f * (((phase) * 0.975f) % 1f) - 1f) - 1f)) *1.5f;
    float RingByteWave(float phase) => MathF.Sign(MathF.Sin(TAU * phase)) * (MathF.Abs(MathF.Sin(TAU * phase * 8f)) > 0.5f ? 1f : -1f);
    float BitBuzzWave(float phase) => MathF.Floor(MathF.Sin(TAU * phase) * 4) / 4 + 0.1f;
    float FormantVoxWave(float phase) => 0.5f * (MathF.Sin(TAU * phase) + MathF.Sin(TAU * phase * 2.5f));
}