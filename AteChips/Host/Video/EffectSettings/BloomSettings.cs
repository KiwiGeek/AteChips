namespace AteChips.Host.Video.EffectSettings;

public class BloomSettings
{
    public bool IsEnabled { get; set; } = true;
    public float Intensity { get; set; } = 1.0f;
    public float Threshold { get; set; } = 0.8f;
}