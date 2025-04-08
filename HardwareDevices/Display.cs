using AteChips.Video.ImGui;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using AteChips.Interfaces;
using GameWindow = OpenTK.Windowing.Desktop.GameWindow;
using IDrawable = AteChips.Interfaces.IDrawable;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using OpenTK.Graphics.ES20;
using ClearBufferMask = OpenTK.Graphics.OpenGL4.ClearBufferMask;
using GL = OpenTK.Graphics.OpenGL4.GL;

namespace AteChips;

class Display : VisualizableHardware, IDrawable
{
    private GameWindow _window;
    private ImGuiController _imgui;
    private ImGuiRenderer _imGuiRenderer = null!;
    private bool _loaded = false;
    private Gpu _gpu = null!; // Assuming Gpu is defined elsewhere
    private FrameBuffer _frameBuffer = null!; // Assuming FrameBuffer is defined elsewhere
    public GameWindow Window => _window;

    private double _renderAccumulator = 0;
    private double _messagePumpAccumulator = 0;
    private const double RenderHz = 60;
    private const double MessagePumpHz = 60;
    private const double RenderInterval = 1.0 / RenderHz;
    private const double MessagePumpInterval = 1.0 / MessagePumpHz;

    public Display(Gpu gpu)
    {
        var nativeWindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(1280, 720),
            Title = "Emulator Display",
            Flags = ContextFlags.ForwardCompatible,
            IsEventDriven = false
        };

        _window = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);
        _window.IsEventDriven = false;
        _window.VSync = VSyncMode.Off;

        _window.Resize += OnResize;
        _window.Unload += OnUnload;
        _window.Closing += OnWindowClosing;

        _gpu = gpu; // Assuming Gpu is defined elsewhere

        _imGuiRenderer = new ImGuiRenderer(_window);

        GL.ClearColor(0f, 0f, 0f, 1f);
        _imgui = new ImGuiController(_window.Size.X, _window.Size.Y);
        InitAudio();
        //_frameBuffer = frameBuffer;
        _gpu.Init(_window.Size.X, _window.Size.Y);
    }

    public struct Viewport
    {
        public int X, Y, Width, Height;
        public Viewport(int x, int y, int width, int height)
            => (X, Y, Width, Height) = (x, y, width, height);
    }

    public static Viewport CalculateChip8Viewport(int windowWidth, int windowHeight)
    {
        const int chip8Width = 64;
        const int chip8Height = 32;
        float chipAspect = chip8Width / (float)chip8Height;
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
        GLFW.GetFramebufferSize(_window.WindowPtr, out int fbWidth, out int fbHeight);

        // calculate aspect-ratio corrected viewport
        var viewport = CalculateChip8Viewport(fbWidth, fbHeight);
        _gpu.Render(args.Time, viewport.X, viewport.Y, viewport.Width, viewport.Height);

        GL.Viewport(0, 0, fbWidth, fbHeight);
        _imGuiRenderer.Update(_window, args.Time);
        _imgui.RenderUi();
        _imGuiRenderer.Render();

        _window.SwapBuffers();
    }


    private bool _fullscreen = false;
    private Vector2i _savedClientSize;
    private Vector2i _savedPosition;


    public void ToggleFullScreen()
    {

        if (_fullscreen)
        {
            _window.WindowBorder = WindowBorder.Resizable;
            _window.WindowState = WindowState.Normal;
            // Restore client size and position
            _window.ClientSize = _savedClientSize;
            _window.Location = _savedPosition;
            _fullscreen = false;
        }
        else
        {

            _savedClientSize = _window.ClientSize;
            _savedPosition = _window.Location;

            var monitor = _window.CurrentMonitor;
            var width = monitor.HorizontalResolution;
            var height = monitor.VerticalResolution;

            _window.WindowBorder = WindowBorder.Hidden;
            _window.WindowState = WindowState.Normal;
            _window.Location = new Vector2i(0, 0);
            _window.Size = new Vector2i(monitor.HorizontalResolution, monitor.VerticalResolution); 
            _fullscreen = true;
        }

    }

    public void Draw(double delta)
    {

        _renderAccumulator += delta;
        _messagePumpAccumulator += delta;

        if (_messagePumpAccumulator >= MessagePumpInterval)
        {
            _window.ProcessEvents(0);
            _messagePumpAccumulator = 0;
        }

        while (_renderAccumulator >= RenderInterval)
        {
            OnRenderFrame(new FrameEventArgs(RenderInterval));
            _renderAccumulator = 0;
        }

    }

    private void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
        _imGuiRenderer.WindowResized(e.Width, e.Height);
    }

    private void OnUnload()
    {
        // Cleanup audio, ImGui, and GPU resources
    }

    private void InitAudio()
    {
        //var device = ALC.OpenDevice(null);
        //var context = ALC.CreateContext(device, (int[])null);
        //ALC.MakeContextCurrent(context);
        // Stub: init streaming, buffer setup, etc.
    }

    private void OnWindowClosing(System.ComponentModel.CancelEventArgs e)
    {
        Environment.Exit(0); // Ensures full app shutdown
    }

    public override void RenderVisual()
    {
        //throw new NotImplementedException();
    }

}
