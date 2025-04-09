using AteChips.Host.Video;
using System.Collections.Generic;
using AteChips.Core.Framebuffer;

namespace AteChips.Core.Video;
public class FrameBufferRenderer : IVideoCard
{
    private readonly GpuTextureSurface _renderSurface;
    private readonly VideoOutputSignal _output;
    private readonly byte[] _pixelBuffer;
    private readonly FrameBuffer _framebuffer;

    public FrameBufferRenderer(FrameBuffer frameBuffer)
    {
        _framebuffer = frameBuffer;
        _pixelBuffer = new byte[frameBuffer.Width * frameBuffer.Height * 4]; // RGBA format
        _renderSurface = new GpuTextureSurface(frameBuffer.Width, frameBuffer.Height); // You might already have something like this
        _output = new VideoOutputSignal("Main", _renderSurface);
    }

    private static void ConvertFramebufferToRgba(bool[] source, byte[] dest, byte r, byte g, byte b)
    {
        for (int i = 0; i < source.Length; i++)
        {
            int offset = i * 4;
            if (source[i])
            {
                dest[offset + 0] = r;
                dest[offset + 1] = g;
                dest[offset + 2] = b;
                dest[offset + 3] = 255;
            }
            else
            {
                dest[offset + 0] = 0;
                dest[offset + 1] = 0;
                dest[offset + 2] = 0;
                dest[offset + 3] = 255; // You can also make this 0 for transparency
            }
        }
    }
    public IEnumerable<VideoOutputSignal> GetOutputs() => [_output];

    public void Update()
    {
        ConvertFramebufferToRgba(_framebuffer.Pixels, _pixelBuffer, 255, 255, 255); 
        _renderSurface.Update(_pixelBuffer);
    }

}
