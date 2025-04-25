using System.Text.Json.Serialization;
using AteChips.Core;
using AteChips.Host.Audio;

namespace AteChips.Shared.Settings;

public record AudioSettings
{
    public BuzzerSettings BuzzerSettings { get; set; } = new();
    public StereoSpeakersSetting StereoSpeakersSettings { get; set; } = new();
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AudioSettings))]
public partial class AudioSettingsJsonContext : JsonSerializerContext
{
}
