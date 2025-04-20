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
    }

    public bool IsMuted { get; set; } = false;
    public float Pitch { get; set; } = 440.0f;
    public float Volume { get; set; } = 0.6f;
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
            WaveformType.DirtyBass => DirtyBaseWave(phase),
            WaveformType.LunarPad => LunarPadWave(phase),
            WaveformType.RetroLaser => RetroLaserWave(phase),
            WaveformType.SolarRamp => SolarRampWave(phase),
            WaveformType.MorphPulse => MorphPulseWave(phase),
            WaveformType.DetuneTwin => DeTuneTwin(phase),
            WaveformType.RingByte => RingByteWave(phase),
            WaveformType.BitBuzz => BitBuzzWave(phase),
            WaveformType.FormantVox => FormantVoxWave(phase),
            WaveformType.LaraCroftsNinetiesBoobies => LaraCroftsNinetiesBoobsWave(phase),
            WaveformType.LaraCroftsModernBoobies => LaraCroftsModernBoobies(phase),
            _ => throw new InvalidOperationException("Invalid waveform type")
        };
    }

    private static float NextFloat() => (float)Rng.NextDouble();

    float SquareWave(float phase) => MathF.Sin(phase) >= 0.0f ? 1.0f : -1.0f;
    float SawtoothWave(float phase) => (2.0f * phase - 1.0f);
    float TriangleWave(float phase) => (2.0f / MathF.PI * MathF.Asin(MathF.Sin(phase)));
    float PulseWave(float phase) => phase < PulseDutyCycle ? 1f : -1f;
    float NoiseWave(float _) => NextFloat() * 2.0f - 1.0f;
    float SineWave(float phase) => MathF.Sin(phase);
    float HalfSineWave(float phase) => (MathF.Abs(MathF.Sin(phase)) - 0.5f) * 2f;
    float RoundedSquareWave(float phase) => MathF.Tanh(MathF.Sin(phase) * RoundedSharpness);
    float StaircaseWave(float phase) => MathF.Floor(phase * MathF.Max(1, StairSteps)) / (MathF.Max(1, StairSteps) - 1f) * 2f - 1f;
    float ChipTuneLead(float phase) => (0.6f * MathF.Sign(MathF.Sin(phase))) + (0.4f * (2f * MathF.Abs(2f * phase - 1f) - 1f));
    float StaticBuzzWave(float phase) => (0.7f * (phase < PulseDutyCycle ? 1f : -1f)) + (0.3f * (NextFloat() * 2.0f - 1.0f));
    float DirtyBaseWave(float phase) => 0.5f * (2f * (phase - 0.5f)) + 0.5f * (2f * MathF.Abs(2f * phase - 1f) - 1f);
    float LunarPadWave(float phase) => ((0.8f * TriangleWave(phase)) + (0.2f * NoiseWave(phase)));
    float RetroLaserWave(float phase) => (MathF.Sin(phase) * .8f) - .2f + (phase < PulseDutyCycle ? 0.4f : 0f);
    float SolarRampWave(float phase) => (MathF.Pow(phase / TAU, 2.5f) * 2f) - 1f;
    float MorphPulseWave(float phase) => MathF.Sin(phase) + (((phase < PulseDutyCycle) ? 1f : -1f) - MathF.Sin(phase)) * 0.4f;
    float DeTuneTwin(float phase) => 0.75f * (MathF.Sin(phase) + (2f * MathF.Abs(2f * (phase * 0.96f / TAU % 1f) - 1f) - 1f));
    float RingByteWave(float phase) => MathF.Sin(phase) * MathF.Sin(phase * 2f);
    float BitBuzzWave(float phase) => MathF.Floor(MathF.Sin(phase) * 4) / 4 + 0.1f;
    float FormantVoxWave(float phase) => 0.5f * (MathF.Sin(phase) + MathF.Sin(phase * 2.5f));
    float LaraCroftsNinetiesBoobsWave(float phase) => (phase % TAU < TAU * 0.5) ? (float)(-1.0f + (2 * (Math.Max(0.0, 1.0 - (Math.Abs((phase % TAU) - (TAU * 0.125)) / (TAU * 0.2 / 2.0))) + Math.Max(0.0, 1.0 - (Math.Abs((phase % TAU) - (TAU * 0.375)) / (TAU * 0.2 / 2.0)))))) : -1.0f;
    float LaraCroftsModernBoobies(float phase) => phase switch { < TAU / 4 => (2 * SineWave(2 * phase)) - 1.0f, < TAU / 2 => (2 * SineWave(2 * (phase - (TAU / 4)))) - 1.0f, _ => -1.0f };
}