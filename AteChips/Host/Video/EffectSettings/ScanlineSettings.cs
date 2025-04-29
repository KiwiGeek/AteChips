namespace AteChips.Host.Video.EffectSettings;
public class ScanlineSettings
{
    public float Intensity { get; set; } = 1.0f;
    public float Sharpness { get; set; } = 0.8f;
    public float BleedAmount { get; set; } = 0.2f;
    public float FlickerStrength { get; set; } = 0.05f;
    public float MaskStrength { get; set; } = 0.0f;
    public float SlotSharpness { get; set; } = 0.8f;
    public bool IsEnabled { get; set; } = true;
}
