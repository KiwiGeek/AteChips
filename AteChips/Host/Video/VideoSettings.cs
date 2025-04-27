namespace AteChips.Host.Video;

public record VideoSettings
{
    public bool MaintainAspectRatio { get; set; } = true;
    public bool FullScreen { get; set; } = false;
}