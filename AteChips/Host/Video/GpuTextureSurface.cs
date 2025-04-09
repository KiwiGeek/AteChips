using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;

namespace AteChips.Host.Video;
public class GpuTextureSurface : IRenderSurface, IDisposable
{
    public int Width { get; }
    public int Height { get; }

    public IntPtr TextureId => (IntPtr)_glTextureId;
    public object NativeHandle => TextureId;             // For generality

    private int _glTextureId = -1;
    private bool _isInitialized = false;

    public GpuTextureSurface(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void Update(byte[] pixelData)
    {
        if (!_isInitialized)
        {
            Initialize();
            _isInitialized = true;
        }

        GL.BindTexture(TextureTarget.Texture2D, _glTextureId);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Width, Height,
            PixelFormat.Rgba, PixelType.UnsignedByte, pixelData);
    }

    private void Initialize()
    {

        _glTextureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _glTextureId);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
            Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }

    public void Dispose()
    {
        if (_glTextureId != -1)
        {
            GL.DeleteTexture(_glTextureId);
        }
    }
}
