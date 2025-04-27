using System;
using OpenTK.Graphics.OpenGL4;

namespace AteChips.Host.Video;
public class GpuTextureSurface : IRenderSurface, IDisposable
{
    public int Width { get; }
    public int Height { get; }

    private byte[] _pixelBuffer;

    private int _glTextureId = -1;

    public GpuTextureSurface(int width, int height)
    {
        Width = width;
        Height = height;
        _pixelBuffer = new byte[width * height];
    }

    public IntPtr PixelData
    {
        get
        {
            unsafe
            {
                fixed (byte* ptr = _pixelBuffer)
                {
                    return (IntPtr)ptr;
                }
            }
        }
    }

    public void Update(byte[] pixelData)
    {
        if (pixelData.Length != _pixelBuffer.Length)
            throw new InvalidOperationException("Pixel data size mismatch");

        Array.Copy(pixelData, _pixelBuffer, pixelData.Length);
    }

    public void Dispose()
    {
        if (_glTextureId != -1)
        {
            GL.DeleteTexture(_glTextureId);
        }
    }
}
