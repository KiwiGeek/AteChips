using System.Text.Json.Serialization;

namespace AteChips.Host.Video;

public record VideoSettings
{

    public enum PresetPhosphorColor
    {
        Amber,
        Green,
        White
    }

    public record PhosphorColor
    {
        public float Red { get; set; } = 1.0f;
        public float Green { get; set; } = 1.0f;
        public float Blue { get; set; } = 1.0f;
    }

    public bool MaintainAspectRatio { get; set; } = true;
    public bool FullScreen { get; set; } = false;
    public PresetPhosphorColor? PhosphorColorType { get; set; } = PresetPhosphorColor.Amber;
    public PhosphorColor? CustomPhosphorColor { get; set; }

    [JsonIgnore]
    public PhosphorColor RenderPhosphorColor =>
        PhosphorColorType switch
        {
            PresetPhosphorColor.Amber => new PhosphorColor { Red = 1.0f, Green = 0.5f, Blue = 0.2f },
            PresetPhosphorColor.Green => new PhosphorColor { Red = 0.2f, Green = 1.0f, Blue = 0.2f },
            PresetPhosphorColor.White => new PhosphorColor { Red = 1.0f, Green = 1.0f, Blue = 1.0f },
            _ => CustomPhosphorColor!
        };
}