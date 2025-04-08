using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using GameWindow = OpenTK.Windowing.Desktop.GameWindow;
using IDrawable = AteChips.Shared.Interfaces.IDrawable;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ClearBufferMask = OpenTK.Graphics.OpenGL4.ClearBufferMask;
using GL = OpenTK.Graphics.OpenGL4.GL;
using AteChips.Core.Framebuffer;
using AteChips.Host.UI;
using AteChips.Shared.Interfaces;
using AteChips.Shared.Settings;
using AteChips.Host.Video;

namespace AteChips;

partial class Display : VisualizableHardware, IDrawable
{
    private readonly ImGuiController _imgui;
    private readonly ImGuiRenderer _imGuiRenderer;
    private readonly Gpu _gpu;
    private readonly FrameBuffer _frameBuffer;


    private bool _fullscreen;
    private Vector2i _savedClientSize;
    private Vector2i _savedPosition;



    public GameWindow Window { get; }

    private double _renderAccumulator;
    private double _messagePumpAccumulator;

    private const double RENDER_HZ = 60;
    private const double MESSAGE_PUMP_HZ = 60;
    private const double RENDER_INTERVAL = 1.0 / RENDER_HZ;
    private const double MESSAGE_PUMP_INTERVAL = 1.0 / MESSAGE_PUMP_HZ;
    private const float PIXEL_ASPECT_RATIO = 1.5f;

    public Display(Gpu gpu, FrameBuffer frameBuffer)
    {
        NativeWindowSettings nativeWindowSettings = new()
        {
            ClientSize = new Vector2i(1280, 720),
            Title = "AteChips",
            Flags = ContextFlags.ForwardCompatible,
            IsEventDriven = false
        };

        Window = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);
        Window.IsEventDriven = false;
        Window.VSync = VSyncMode.Off;

        Window.Resize += OnResize;
        Window.Closing += OnWindowClosing;

        _gpu = gpu; // Assuming Gpu is defined elsewhere

        _imGuiRenderer = new ImGuiRenderer(Window);

        GL.ClearColor(0f, 0f, 0f, 1f);
        _imgui = new ImGuiController(Window.Size.X, Window.Size.Y);
        _frameBuffer = frameBuffer;
        _gpu.Init(Window.Size.X, Window.Size.Y);
    }

    public struct Viewport
    {
        public int X, Y, Width, Height;
        public Viewport(int x, int y, int width, int height)
            => (X, Y, Width, Height) = (x, y, width, height);
    }

    public Viewport CalculateChip8Viewport(int windowWidth, int windowHeight)
    {
        int logicalWidth = _frameBuffer.Width;
        int logicalHeight = _frameBuffer.Height;
        float chipAspect = logicalWidth / (logicalHeight * PIXEL_ASPECT_RATIO);
        float windowAspect = windowWidth / (float)windowHeight;

        if (!Settings.MaintainAspectRatio)
        {
            return new Viewport(0, 0, windowWidth, windowHeight);
        }

        if (windowAspect > chipAspect)
        {
            // Window is wider than CHIP-8
            int height = windowHeight;
            int width = (int)(height * chipAspect);
            int x = (windowWidth - width) / 2;
            return new Viewport(x, 0, width, height);
        }
        else
        {
            // Window is taller or same as CHIP-8
            int width = windowWidth;
            int height = (int)(width / chipAspect);
            int y = (windowHeight - height) / 2;
            return new Viewport(0, y, width, height);
        }
    }


    private unsafe void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GLFW.GetFramebufferSize(Window.WindowPtr, out int fbWidth, out int fbHeight);

        // calculate aspect-ratio corrected viewport
        Viewport viewport = CalculateChip8Viewport(fbWidth, fbHeight);
        _gpu.Render(args.Time, viewport.X, viewport.Y, viewport.Width, viewport.Height);

        if (Settings.ShowImGui)
        {
            GL.Viewport(0, 0, fbWidth, fbHeight);
            _imGuiRenderer.Update(Window, args.Time);
            _imgui.RenderUi();
            _imGuiRenderer.Render();
        }

        Window.SwapBuffers();
    }




    public void ToggleFullScreen()
    {

        if (_fullscreen)
        {
            Window.WindowBorder = WindowBorder.Resizable;
            Window.WindowState = WindowState.Normal;
            Window.ClientSize = _savedClientSize;
            Window.Location = _savedPosition;
            _fullscreen = false;
        }
        else
        {

            _savedClientSize = Window.ClientSize;
            _savedPosition = Window.Location;

            MonitorInfo? monitor = Window.CurrentMonitor;
            Window.WindowBorder = WindowBorder.Hidden;
            Window.WindowState = WindowState.Normal;
            Window.Location = new Vector2i(0, 0);
            Window.Size = new Vector2i(monitor.HorizontalResolution, monitor.VerticalResolution);
            _fullscreen = true;
        }

    }

    public void Draw(double delta)
    {

        _renderAccumulator += delta;
        _messagePumpAccumulator += delta;

        if (_messagePumpAccumulator >= MESSAGE_PUMP_INTERVAL)
        {
            Window.ProcessEvents(0);
            _messagePumpAccumulator = 0;
        }

        while (_renderAccumulator >= RENDER_INTERVAL)
        {
            OnRenderFrame(new FrameEventArgs(RENDER_INTERVAL));
            _renderAccumulator = 0;
        }

    }

    private void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
        _imGuiRenderer.WindowResized(e.Width, e.Height);
    }

    private void OnWindowClosing(System.ComponentModel.CancelEventArgs e)
    {
        Environment.Exit(0); // Ensures full app shutdown
    }

}
