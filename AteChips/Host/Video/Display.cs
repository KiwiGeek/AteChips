using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using GameWindow = OpenTK.Windowing.Desktop.GameWindow;
using IDrawable = AteChips.Shared.Interfaces.IDrawable;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ClearBufferMask = OpenTK.Graphics.OpenGL4.ClearBufferMask;
using GL = OpenTK.Graphics.OpenGL4.GL;
using AteChips.Shared.Interfaces;
using AteChips.Shared.Settings;
using OpenTK.Graphics.OpenGL4;
using AteChips.Host.UI.ImGui;
using AteChips.Core.Video;

namespace AteChips.Host.Video;

partial class Display : IVisualizable, IDrawable
{
    private readonly ImGuiFrontEnd _imgui;
    private readonly ImGuiBackend _imGuiRenderer;
    private VideoOutputSignal _connectedSignal;

    private bool _gpuInitialized = false;

    private bool _fullscreen;
    private Vector2i _savedClientSize;
    private Vector2i _savedPosition;

    private int _vao, _vbo, _shader;


    public GameWindow Window { get; }

    private double _renderAccumulator;
    private double _messagePumpAccumulator;

    private const double RENDER_HZ = 60;
    private const double MESSAGE_PUMP_HZ = 60;
    private const double RENDER_INTERVAL = 1.0 / RENDER_HZ;
    private const double MESSAGE_PUMP_INTERVAL = 1.0 / MESSAGE_PUMP_HZ;

    private readonly float _pixelAspectRatio; 

    private void InitializeGL(int windowWidth, int windowHeight)
    {

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, 64, 32, 0, PixelFormat.Red, PixelType.UnsignedByte, nint.Zero);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        _shader = CreateShader();
        SetupFullscreenQuad();
    }


    public Display(float pixelAspectRatio = 1.0f)
    {
        _pixelAspectRatio = pixelAspectRatio;

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

        _imGuiRenderer = new ImGuiBackend(Window);

        GL.ClearColor(0f, 0f, 0f, 1f);
        _imgui = new ImGuiFrontEnd(Window.Size.X, Window.Size.Y);

        InitializeGL(Window.Size.X, Window.Size.Y);
    }

    public void Connect(VideoOutputSignal signal)
    {
        _connectedSignal = signal;
    }

    public struct Viewport
    {
        public int X, Y, Width, Height;
        public Viewport(int x, int y, int width, int height)
            => (X, Y, Width, Height) = (x, y, width, height);
    }

    public Viewport CalculateViewport(int windowWidth, int windowHeight)
    {
        if (_connectedSignal?.Surface == null)
            return new Viewport(0, 0, windowWidth, windowHeight); // fallback to fullscreen


        int logicalWidth = _connectedSignal.Surface.Width;
        int logicalHeight = _connectedSignal.Surface.Height;
        float chipAspect = logicalWidth / (logicalHeight * _pixelAspectRatio);
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

        if (!_gpuInitialized)
        {
            _gpuInitialized = true;

            Window.MakeCurrent(); // extra safe
        }


        GL.Clear(ClearBufferMask.ColorBufferBit);
        GLFW.GetFramebufferSize(Window.WindowPtr, out int fbWidth, out int fbHeight);

        // calculate aspect-ratio corrected viewport
        Viewport viewport = CalculateViewport(fbWidth, fbHeight);
        RenderSurface(viewport.X, viewport.Y, viewport.Width, viewport.Height);

        if (Settings.ShowImGui)
        {
            GL.Viewport(0, 0, fbWidth, fbHeight);
            _imGuiRenderer.Update(Window, args.Time);
            _imgui.RenderUi();
            _imGuiRenderer.Render();
        }

        Window.SwapBuffers();
    }

    private void RenderSurface(int x, int y, int width, int height)
    {
        if (_connectedSignal?.IsConnected != true)
            return;

        IRenderSurface surface = _connectedSignal.Surface;

        GL.BindTexture(TextureTarget.Texture2D, (int)surface.TextureId);
        GL.Viewport(x, y, width, height);
        GL.UseProgram(_shader);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
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

    private void SetupFullscreenQuad()
    {
        float[] vertices = {
            //   X      Y       U     V
            -1f, -1f,   0f, 1f,  // bottom-left
            1f, -1f,   1f, 1f,  // bottom-right
            1f,  1f,   1f, 0f,  // top-right

            -1f, -1f,   0f, 1f,  // bottom-left
            1f,  1f,   1f, 0f,  // top-right
            -1f,  1f,   0f, 0f   // top-left
        };

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        // Position (location = 0)
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // TexCoord (location = 1)
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
    }

    private int CreateShader()
    {
        string vertexSource = @"
        #version 330 core

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;

void main()
{
    gl_Position = vec4(aPosition, 0.0, 1.0);
    vTexCoord = aTexCoord;
}";

        string fragmentSource = @"
        #version 330 core

in vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uTexture;

void main()
{
    float pixel = texture(uTexture, vTexCoord).r;
    FragColor = vec4(vec3(pixel), 1.0); // white for on-pixel, black for off
}";

        int vertex = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertex, vertexSource);
        GL.CompileShader(vertex);
        Console.WriteLine(GL.GetShaderInfoLog(vertex));

        int fragment = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragment, fragmentSource);
        GL.CompileShader(fragment);
        Console.WriteLine(GL.GetShaderInfoLog(fragment));

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);
        GL.LinkProgram(program);
        Console.WriteLine(GL.GetProgramInfoLog(program));

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        return program;
    }


}
