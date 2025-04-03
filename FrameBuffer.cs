using System;
using AteChips.Interfaces;
using Microsoft.Xna.Framework;

namespace AteChips;
public class FrameBuffer : Hardware, IResettable
{
    private const int DEFAULT_WIDTH = 64;
    private const int DEFAULT_HEIGHT = 32;

    public Color[] Pixels { get; }

    public int Width { get; }
    public int Height { get; }

    public bool this[int x, int y]
    {
        get => Pixels[(y * Width) + x] == Color.White;
        set => Pixels[(y * Width) + x] = value ? Color.White : Color.Black;
    }

    //public void SetPixel(int x, int y, bool on) => this[x, y] = on;
    public void TogglePixel(int x, int y) => this[x, y] = !this[x, y];
    //public void ClearPixel(int x, int y) => this[x, y] = false;
    //public void AssertPixel(int x, int y) => this[x, y] = true;

    public void Reset() => Array.Fill(Pixels, Color.Black);

    public FrameBuffer()
    {
        Pixels = new Color[DEFAULT_WIDTH * DEFAULT_HEIGHT];
        Width = DEFAULT_WIDTH;
        Height = DEFAULT_HEIGHT;
    }

    public FrameBuffer(int width, int height)
    {
        Pixels = new Color[width * height];
        Width = width;
        Height = height;
    }
}
