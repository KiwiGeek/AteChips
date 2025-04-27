using AteChips.Host.Video;
using System.Collections.Generic;
using AteChips.Core.Shared.Interfaces;
using AteChips.Shared.Video;
using AteChips.Core.Shared.Timing;
using AteChips.Shared.Runtime;

namespace AteChips.Core;
public class FrameBufferVideoCard : IVideoCard, IUpdatable
{
    private readonly GpuTextureSurface _renderSurface;
    private readonly VideoOutputSignal _output;
    private readonly byte[] _pixelBuffer;
    private readonly FrameBuffer _framebuffer;

    public FrameBufferVideoCard(FrameBuffer frameBuffer)
    {
        _framebuffer = frameBuffer;
        _pixelBuffer = new byte[frameBuffer.Width * frameBuffer.Height]; // RGBA format
        _renderSurface = new GpuTextureSurface(frameBuffer.Width, frameBuffer.Height); // You might already have something like this
        _output = new VideoOutputSignal("Main", _renderSurface);
    }

    private static void ConvertFramebufferToMono(bool[] source, byte[] dest)
    {
        for (int i = 0; i < source.Length; i++)
        {
            dest[i] = source[i] ? (byte)255 : (byte)0;
        }
    }

    public IEnumerable<VideoOutputSignal> GetOutputs() => [_output];
    public VideoOutputSignal GetPrimaryOutput() => _output;

    public void Update()
    {
        ConvertFramebufferToMono(_framebuffer.Pixels, _pixelBuffer);
        _renderSurface.Update(_pixelBuffer);
    }

    public double FrequencyHz => 60; // or whatever makes sense

    public byte UpdatePriority => UpdatePriorities.Gpu; // optional; lower = higher priority

    public bool Update(double deltaTime)
    {
        Update(); // Call your existing update logic
        return false;
    }

    public string Name => GetType().Name;

    private IHostBridge? _hostBridge;

    public virtual void SetHostBridge(IHostBridge hostBridge)
    {
        _hostBridge = hostBridge;
    }
}
