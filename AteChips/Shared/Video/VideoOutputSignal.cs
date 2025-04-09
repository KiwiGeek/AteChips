using AteChips.Host.Video;

namespace AteChips.Shared.Video;
public class VideoOutputSignal
{
    public string Name { get; init; } = "Main";
    public IRenderSurface Surface { get; init; }
    public bool IsConnected { get; set; } = true;

    public VideoOutputSignal(string name, IRenderSurface surface)
    {
        Name = name;
        Surface = surface;
    }
}
