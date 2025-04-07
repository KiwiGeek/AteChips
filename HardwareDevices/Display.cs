using AteChips.Video.ImGui;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using AteChips.Interfaces;
using GameWindow = OpenTK.Windowing.Desktop.GameWindow;
using IDrawable = AteChips.Interfaces.IDrawable;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

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

    private unsafe void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GLFW.GetFramebufferSize(_window.WindowPtr, out int fbWidth, out int fbHeight);
        _gpu.Render(0, fbWidth, fbHeight);
        _imGuiRenderer.Update(_window, args.Time);
        _imgui.RenderUi();
        _imGuiRenderer.Render();

        _window.SwapBuffers();
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
        GL.Viewport(0, 0, _window.Size.X, _window.Size.Y);
        //_imgui.WindowResized(_window.Size.X, _window.Size.Y);
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
