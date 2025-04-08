using System;
using AteChips.Shared.Interfaces;

namespace AteChips.Core.Framebuffer;
public class FrameBuffer : IHardware, IResettable
{
    public string Name => GetType().Name;

    private const int DEFAULT_WIDTH = 64;
    private const int DEFAULT_HEIGHT = 32;

    public bool[] Pixels { get; }

    public int Width { get; }
    public int Height { get; }

    public bool this[int x, int y]
    {
        get => Pixels[y * Width + x];
        set => Pixels[y * Width + x] = value;
    }

    //public void ClearPixel(int x, int y) => this[x, y] = false;
    //public void AssertPixel(int x, int y) => this[x, y] = true;
    public void TogglePixel(int x, int y) => this[x, y] ^= true;
    public bool TogglePixel(int x, int y, out bool wasErased)
    {
        TogglePixel(x,y);
        wasErased = !this[x, y];
        return wasErased;
    }

    public void Reset()
    {
        Array.Fill(Pixels, false);
    }

    public FrameBuffer()
    {
        Pixels = new bool[DEFAULT_WIDTH * DEFAULT_HEIGHT];
        Width = DEFAULT_WIDTH;
        Height = DEFAULT_HEIGHT;
    }

    public FrameBuffer(int width, int height)
    {
        Pixels = new bool[width * height];
        Width = width;
        Height = height;
    }
}
