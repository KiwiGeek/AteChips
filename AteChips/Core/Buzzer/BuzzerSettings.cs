namespace AteChips.Core;

public record BuzzerSettings
{
    public bool IsMuted { get; set; } = false;
    public float Pitch { get; set; } = 440.0f;
    public float Volume { get; set; } = 0.4f;
    public float PulseDutyCycle { get; set; } = 0.25f;
    public float RoundedSharpness { get; set; } = 15.0f;
    public int StairSteps { get; set; } = 8;
    public WaveformType Waveform { get; set; } = WaveformType.Square;
}