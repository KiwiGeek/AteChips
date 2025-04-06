using AteChips.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

namespace AteChips;

public partial class Buzzer : VisualizableHardware, IBuzzer
{

    private Ram _ram;

    public Buzzer(Ram ram)
    {
        _ram = ram;
    }

    private SoundEffect _buzzerSound = null!;
    private SoundEffectInstance _buzzerInstance = null!;

    private const int SampleRate = 44100;
    private const float PreviewSeconds = 0.02f; // 20ms
    private const int LoopCycleCount = 12; // Loopable cycles
    private static readonly Random _rng = new();

    private bool _isMuted = false;

    private float _roundedSharpness = 15f;
    private int _stairSteps = 8;

    public float PulseDutyCycle { get; set; } = 0.25f;

    public enum WaveformType
    {
        Square,
        Triangle,
        Sawtooth,
        Pulse,
        Sine,
        HalfSine,
        RoundedSquare,
        Staircase,
        ChipTuneLead,
        StaticBuzz,
        DirtyBass,
        LunarPad,
        RetroLaser,
        SolarRamp,
        MorphPulse,
        DetuneTwin,
        RingByte,
        BitBuzz,
        FormantVox,
        Noise
    }

    public float Pitch { get; set; } = 440f;
    public float Volume { get; set; } = 0.25f;

    private WaveformType _waveform = WaveformType.Square;
    public WaveformType Waveform
    {
        get => _waveform;
        set
        {
            if (_waveform != value)
            {
                _waveform = value;
                GenerateSoundWave(); // regenerate audio buffer on change
            }
        }
    }

    private byte SoundTimer
    {
        get => _ram.GetByte(Ram.SOUND_TIMER_ADDR);
        set => _ram.SetByte(Ram.SOUND_TIMER_ADDR, value);
    }

    private void GenerateAudioBuffer()
    {
        int samplesPerCycle = (int)(SampleRate / Pitch);
        int sampleCount = samplesPerCycle * LoopCycleCount;
        short[] samples = new short[sampleCount];
        float perceptualVolume = MathF.Pow(Volume, 2.0f);
        short amp = _isMuted ? (short)0 : (short)(short.MaxValue * MathF.Pow(Volume, 2f));

        for (int i = 0; i < sampleCount; i++)
        {
            float phase = (i % samplesPerCycle) / (float)samplesPerCycle;
            float t = i / (float)SampleRate;
            float value = GetWaveformSample(t, phase);
            samples[i] = (short)(value * amp);
        }

        byte[] buffer = new byte[sampleCount * sizeof(short)];
        Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
        _buzzerSound = new SoundEffect(buffer, SampleRate, AudioChannels.Mono);
    }

    private float GetWaveformSample(float t, float phase)
    {
        return Waveform switch
        {
            WaveformType.Square => MathF.Sign(MathF.Sin(2 * MathF.PI * Pitch * t)),
            WaveformType.Triangle => 2f * MathF.Abs(2f * (t * Pitch - MathF.Floor(t * Pitch + 0.5f))) - 1f,
            WaveformType.Sawtooth => 2f * (t * Pitch - MathF.Floor(t * Pitch + 0.5f)),
            WaveformType.Pulse => phase < PulseDutyCycle ? 1f : -1f,
            WaveformType.Noise => (float)(_rng.NextDouble() * 2.0 - 1.0),
            WaveformType.Sine => MathF.Sin(2 * MathF.PI * Pitch * t),
            WaveformType.HalfSine => MathF.Abs(MathF.Sin(2 * MathF.PI * Pitch * t)),
            WaveformType.RoundedSquare => MathF.Tanh(MathF.Sin(2 * MathF.PI * phase) * _roundedSharpness),
            WaveformType.Staircase => MathF.Floor(phase * MathF.Max(1, _stairSteps)) / (MathF.Max(1, _stairSteps) - 1f) * 2f - 1f,
            WaveformType.ChipTuneLead => (0.6f * MathF.Sign(MathF.Sin(2 * MathF.PI * Pitch * t))) + (0.4f * (2f * MathF.Abs(2f * (t * Pitch - MathF.Floor(t * Pitch + 0.5f))) - 1f)),
            WaveformType.StaticBuzz => (0.7f * (phase < PulseDutyCycle ? 1f : -1f)) + (0.3f * ((float)(_rng.NextDouble() * 2.0 - 1.0))),
            WaveformType.DirtyBass => (0.5f * (2f * (t * Pitch - MathF.Floor(t * Pitch + 0.5f)))) + (0.5f * (2f * MathF.Abs(2f * (t * Pitch - MathF.Floor(t * Pitch + 0.5f))) - 1f)),
            WaveformType.LunarPad => (0.8f * (2f * MathF.Abs(2f * (t * Pitch - MathF.Floor(t * Pitch + 0.5f))) - 1f)) + (0.2f * ((float)(_rng.NextDouble() * 2.0 - 1.0))),
            WaveformType.RetroLaser => (0.6f * (MathF.Sin(2 * MathF.PI * Pitch * t))) + (0.4f * (phase < PulseDutyCycle ? 1f : -1f)),
            WaveformType.SolarRamp => (MathF.Pow(phase, 2.5f) * 2f) - 1f,
            WaveformType.MorphPulse => Lerp(MathF.Sin(2 * MathF.PI * phase), phase < PulseDutyCycle ? 1f : -1f,0.4f),
            WaveformType.DetuneTwin => 0.5f * (MathF.Sin(2 * MathF.PI * Pitch * t) + MathF.Sin(2 * MathF.PI * Pitch * 0.985f * t)),
            WaveformType.RingByte => MathF.Sin(2 * MathF.PI * Pitch * t) * MathF.Sin(2 * MathF.PI * Pitch * 2f * t),
            WaveformType.BitBuzz => MathF.Floor(MathF.Sin(2 * MathF.PI * phase) * 8) / 8,
            WaveformType.FormantVox => 0.5f * (MathF.Sin(2 * MathF.PI * Pitch * t) + MathF.Sin(2 * MathF.PI * Pitch * 2.5f * t)),
            _ => 0f
        };
    }
    private static float Lerp(float a, float b, float t) => a + (b - a) * t;

    private void GenerateSoundWave()
    {
        bool wasPlaying = _buzzerInstance != null && _buzzerInstance.State == SoundState.Playing;
        if (wasPlaying)
        {
            _buzzerInstance.Stop();
        }

        // Generate loop-safe audio buffer
        GenerateAudioBuffer();

        // Generate time-based waveform preview
        if (VisualShown) { GeneratePreviewBuffer(); }

        // Create the sound effect instance for playback
        _buzzerInstance = _buzzerSound.CreateInstance();
        _buzzerInstance.IsLooped = true;

        if (wasPlaying)
        {
            _buzzerInstance.Play();
        }
    }

    public void Reset()
    {
        GenerateSoundWave();
        _buzzerInstance = _buzzerSound.CreateInstance();
        _buzzerInstance.IsLooped = true;

        if (Machine.Instance.TryGet(out Ram? ram)) { _ram = ram!; }
    }

   

    public void Update(double gameTime)
    {


        if (SoundTimer > 0)
        {
            SoundTimer--;

            if (_buzzerInstance.State != SoundState.Playing)
            {
                _buzzerInstance.Play();
            }
        }
        else if (_buzzerInstance.State == SoundState.Playing)
        {
            _buzzerInstance.Stop();
        }

    }

    public byte UpdatePriority => 1;
}