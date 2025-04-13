using System;

namespace AteChips.Host.Video;

public interface IRenderSurface
{
    int Width { get; }
    int Height { get; }

    IntPtr TextureId { get; }      // For ImGui
}