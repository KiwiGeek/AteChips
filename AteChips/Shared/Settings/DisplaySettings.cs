using System.Text.Json.Serialization;
using AteChips.Host.Video;

namespace AteChips.Shared.Settings;

public record DisplaySettings
{
    public VideoSettings VideoSettings { get; set; } = new();
    public ImGuiSettings VisualizerLayout { get; set; } = new();
}


[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DisplaySettings))]
public partial class DisplaySettingsJsonContext : JsonSerializerContext
{
}
