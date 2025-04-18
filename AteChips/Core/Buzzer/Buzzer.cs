using System;
using System.Collections.Generic;
using System.Numerics;
using AteChips.Core.Shared.Base;
using AteChips.Shared.Sound;

namespace AteChips.Core;

public partial class Buzzer : VisualizableHardware, IBuzzer
{
    private readonly IAudioOutputSignal _output;
    private float _phase;
    private const float TAU = (float)(Math.PI * 2);
    private Random _rng = new ();
    private long _totalSamplesGenerated = 0;

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
    public float PulseDutyCycle { get; set; } = 0.25f;
    public float RoundedSharpness { get; set; } = 15.0f;
    public int StairSteps { get; set; } = 8;

    public WaveformTypes Waveform { get; set; } = WaveformTypes.Square;

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



    public static float GetNormalizationFactor(WaveformTypes type) => type switch
    {
        // todo: RMS this bitch
        _ => 1.0f
    };

    protected float GetWaveformSample(float phase, float time)
    {
        return Waveform switch
        {
            WaveformTypes.Square => SquareWave(phase),
            WaveformTypes.Sawtooth => SawtoothWave(phase),
            WaveformTypes.Triangle => TriangleWave(phase),
            WaveformTypes.Pulse => PulseWave(phase), 
            WaveformTypes.Noise => NoiseWave(phase),
            WaveformTypes.Sine => SineWave(phase),
            WaveformTypes.HalfSine => HalfSineWave(phase),
            WaveformTypes.RoundedSquare => RoundedSquareWave(phase), 
            WaveformTypes.Staircase => StaircaseWave(phase),
            WaveformTypes.ChipTuneLead => ChipTuneLead(phase),
            WaveformTypes.StaticBuzz => StaticBuzzWave(phase),
            WaveformTypes.DirtyBass => DirtyBaseWave(phase),
            WaveformTypes.LunarPad => LunarPadWave(phase),
            WaveformTypes.RetroLaser => RetroLaserWave(phase),
            WaveformTypes.SolarRamp => (MathF.Pow((phase / TAU), 2.5f) * 2f) - 1f,
            WaveformTypes.MorphPulse => Lerp(MathF.Sin(TAU * (phase / TAU)), (phase / TAU) < PulseDutyCycle ? 1f : -1f, 0.4f),
            WaveformTypes.DetuneTwin => 0.5f * (MathF.Sin(TAU * Pitch * time) + MathF.Sin(TAU * Pitch * 0.985f * time)),
            WaveformTypes.RingByte => MathF.Sin(TAU * Pitch * time) * MathF.Sin(TAU * Pitch * 2f * time),
            WaveformTypes.BitBuzz => MathF.Floor(MathF.Sin(TAU * (phase / TAU)) * 8) / 8,
            WaveformTypes.FormantVox => 0.5f * (MathF.Sin(TAU * Pitch * time) + MathF.Sin(TAU * Pitch * 2.5f * time)),
            WaveformTypes.LaraCroftsNinetiesBoobies => LaraCroftsNinetiesBoobsWave(phase),
            WaveformTypes.LaraCroftsModernBoobies => LaraCroftsModernBoobies(phase),
            _ => throw new InvalidOperationException("Invalid waveform type")
        };
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;


    float SquareWave(float phase) => Math.Sin(phase) >= 0 ? 1f : -1f;
    float SawtoothWave(float phase) => (float)((2.0 * (phase / TAU)) - 1.0);
    float TriangleWave(float phase) => (float)(2.0 / Math.PI * Math.Asin(Math.Sin(phase)));
    float PulseWave(float phase) => (phase / TAU) < PulseDutyCycle ? 1f : -1f;
    float NoiseWave(float phase) => (float)(_rng.NextDouble() * 2.0 - 1.0);
    float SineWave(float phase) => (float) Math.Sin(phase);
    float HalfSineWave(float phase) => (float)((Math.Abs(Math.Sin(phase)) - 0.5) * 2);
    float RoundedSquareWave(float phase) => MathF.Tanh(MathF.Sin((float)phase) * RoundedSharpness);
    float StaircaseWave(float phase) => MathF.Floor((phase / TAU) * MathF.Max(1, StairSteps)) / (MathF.Max(1, StairSteps) - 1f) * 2f - 1f;
    float ChipTuneLead(float phase) => (0.6f * MathF.Sign(MathF.Sin(TAU * phase))) + (0.4f * (2f * MathF.Abs(2f * phase - 1f) - 1f));
    float StaticBuzzWave(float phase) => (0.7f * ((phase / TAU) < PulseDutyCycle ? 1f : -1f)) + (0.3f * ((float)(_rng.NextDouble() * 2.0 - 1.0)));
    float DirtyBaseWave(float phase) => 0.5f * (2f * (phase - 0.5f)) + 0.5f * (2f * MathF.Abs(2f * phase - 1f) - 1f);
    float LunarPadWave(float phase) => (float)((0.8* TriangleWave(phase)) + (0.2f * NoiseWave(phase)));
    float RetroLaserWave(float phase) => (0.6f * MathF.Sin(TAU * phase)) + (0.4f * MathF.Sign(PulseDutyCycle - phase));
    float LaraCroftsNinetiesBoobsWave(float phase) => (phase % TAU < TAU * 0.5) ? (float)(-1.0f + (2 * (Math.Max(0.0, 1.0 - (Math.Abs((phase % TAU) - (TAU * 0.125)) / (TAU * 0.2 / 2.0))) + Math.Max(0.0, 1.0 - (Math.Abs((phase % TAU) - (TAU * 0.375)) / (TAU * 0.2 / 2.0)))))) : -1.0f;
    float LaraCroftsModernBoobies(float phase) => phase switch { < TAU / 4 => (2 * SineWave(2 * phase)) - 1.0f, < TAU / 2 => (2 * SineWave(2 * (phase - (TAU / 4)))) - 1.0f, _ => -1.0f };
}