using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using GameWindow = OpenTK.Windowing.Desktop.GameWindow;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ClearBufferMask = OpenTK.Graphics.OpenGL4.ClearBufferMask;
using GL = OpenTK.Graphics.OpenGL4.GL;
using AteChips.Shared.Settings;
using OpenTK.Graphics.OpenGL4;
using AteChips.Host.UI.ImGui;
using AteChips.Core.Shared.Interfaces;
using AteChips.Shared.Video;
using Shared.Settings;

namespace AteChips.Host.Video;

/// <summary>
/// Main video output manager. Responsible for rendering the emulated display
/// using OpenGL, integrating ImGui, and managing the window lifecycle.
/// </summary>
partial class Display : IVisualizable, IDrawable, ISettingsChangedNotifier
{
    /// <summary>
    /// The main ImGui front-end UI renderer.
    /// </summary>
    private readonly ImGuiFrontEnd _imGuiFrontEnd;

    /// <summary>
    /// The backend that handles ImGui and OpenGL context interaction.
    /// </summary>
    private readonly ImGuiBackend _imGuiBackEnd;

    /// <summary>
    /// The currently connected video signal source.
    /// </summary>
    private VideoOutputSignal? _connectedSignal;

    private VideoSettings _videoSettings;

    // todo: move window size to settings
    /// <summary>
    /// Saves the window size when toggling to fullscreen.
    /// </summary>
    private Vector2i _savedClientSize;


    /// <summary>
    /// Saves the window position when toggling to fullscreen.
    /// </summary>
    private Vector2i _savedPosition;

    /// <summary>
    /// OpenGL Vertex Array Object ID.
    /// </summary>
    private int _vao;

    /// <summary>
    /// OpenGL Vertex Buffer Object ID.
    /// </summary>
    private int _vbo;

    /// <summary>
    /// OpenGL shader program ID.
    /// </summary>
    private int _shader;

    /// <summary>
    /// The OpenTK window and OpenGL context host.
    /// </summary>
    public GameWindow Window { get; }

    // The message pump is 60Hz. There's no point doing it more often
    // than we render. But for separation of concerns, we'll track it
    // separately from the render loop.
    private const double MESSAGE_PUMP_HZ = 60;
    private const double MESSAGE_PUMP_INTERVAL = 1.0 / MESSAGE_PUMP_HZ;

    // The render loop occurs at 60Hz. Effectively this sets the display
    // up to pretend to be a monitor with a refresh rate of 60Hz.
    private const double RENDER_HZ = 60;
    private const double RENDER_INTERVAL = 1.0 / RENDER_HZ;


    /// <summary>
    /// Tracks whether the window should close.
    /// </summary>
    private bool _shouldClose;

    /// <summary>
    /// The desired pixel aspect ratio requested by the emulated machine.
    /// </summary>
    private readonly float _pixelAspectRatio;

    /// <summary>
    /// Constructs the display system and sets up the rendering pipeline.
    /// </summary>
    /// <param name="machine">The emulated machine providing display specs.</param>
    public Display(IEmulatedMachine machine, VideoSettings videoSettings)
    {
        _videoSettings = videoSettings;

        _pixelAspectRatio = machine.DisplaySpec.PixelAspectRatio;

        NativeWindowSettings nativeWindowSettings = new()
        {
            ClientSize = new Vector2i(1280, 720),
            Title = "AteChips",
            Flags = ContextFlags.ForwardCompatible,
            IsEventDriven = false,
            Vsync = VSyncMode.Off
        };

        Window = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);

        Window.Resize += OnResize;
        Window.Closing += _ => _shouldClose = true;

        _imGuiBackEnd = new ImGuiBackend(Window);

        GL.ClearColor(0f, 0f, 0f, 1f);
        _imGuiFrontEnd = new ImGuiFrontEnd(machine, [this]);

        InitializeGl();
    }

    /// <summary>
    /// Initializes the OpenGL texture, shader, and quad geometry.
    /// </summary>
    private void InitializeGl()
    {
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, 64, 32, 0, PixelFormat.Red, PixelType.UnsignedByte, nint.Zero);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        _shader = CreateDefaultShader();
        SetupFullscreenQuad();
    }

    /// <summary>
    /// Connects a video signal to this display for rendering.
    /// </summary>
    /// <param name="signal">The signal to connect to the display.</param>
    public void Connect(VideoOutputSignal signal)
    {
        _connectedSignal = signal;
    }

    /// <summary>
    /// Describes a rectangular viewport region.
    /// </summary>
    public struct Viewport
    {
        public int X, Y, Width, Height;
        public Viewport(int x, int y, int width, int height)
            => (X, Y, Width, Height) = (x, y, width, height);
    }

    /// <summary>
    /// Calculates the viewport for rendering based on the window size
    /// and the pixel aspect ratio of the connected signal.
    /// </summary>
    public Viewport CalculateViewport(int windowWidth, int windowHeight)
    {
        if (_connectedSignal?.Surface == null)
        {
            return new Viewport(0, 0, windowWidth, windowHeight);
        }


        int logicalWidth = _connectedSignal.Surface.Width;
        int logicalHeight = _connectedSignal.Surface.Height;
        float chipAspect = logicalWidth / (logicalHeight * _pixelAspectRatio);
        float windowAspect = windowWidth / (float)windowHeight;

        if (!SettingsManager.Current.Display.VideoSettings.MaintainAspectRatio)
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

    /// <summary>
    /// Main render loop, called on a fixed interval to draw the emulator screen.
    /// </summary>
    private unsafe void RenderFrame(FrameEventArgs args)
    {

        GL.Clear(ClearBufferMask.ColorBufferBit);
        GLFW.GetFramebufferSize(Window.WindowPtr, out int fbWidth, out int fbHeight);

        // calculate aspect-ratio corrected viewport
        Viewport viewport = CalculateViewport(fbWidth, fbHeight);
        RenderSurface(viewport.X, viewport.Y, viewport.Width, viewport.Height);

        if (Settings.ShowImGui)
        {
            GL.Viewport(0, 0, fbWidth, fbHeight);
            _imGuiBackEnd.Update(Window, args.Time);
            _imGuiFrontEnd.RenderUi();
            _imGuiBackEnd.Render();
        }

        Window.SwapBuffers();
    }

    /// <summary>
    /// Draws the connected render surface to the screen using the full-screen quad.
    /// </summary>
    private void RenderSurface(int x, int y, int width, int height)
    {
        if (_connectedSignal?.IsConnected != true) { return; }

        IRenderSurface surface = _connectedSignal.Surface;

        GL.BindTexture(TextureTarget.Texture2D, (int)surface.TextureId);
        GL.Viewport(x, y, width, height);
        GL.UseProgram(_shader);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    /// <summary>
    /// Toggles fullscreen mode, saving and restoring window state as needed.
    /// </summary>
    public void ToggleFullScreen()
    {

        if (_videoSettings.FullScreen)
        {
            Window.WindowBorder = WindowBorder.Resizable;
            Window.WindowState = WindowState.Normal;
            Window.ClientSize = _savedClientSize;
            Window.Location = _savedPosition;
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
        }
        _videoSettings.FullScreen ^= true;
        SettingsChanged?.Invoke();
    }

    private double _lastGameTime = 0;
    private double _lastRenderTime = 0;
    private double _lastDrawTime = 0;
    private double _messagePumpAccumulator = 0;

    public bool ReadyToDraw(double gameTime) => (gameTime - _lastDrawTime) >= RENDER_INTERVAL;

    /// <summary>
    /// Called once per host frame to update the emulator window and render output.
    /// </summary>
    public bool Draw(double gameTime)
    {
        // Always process events on a regular interval
        double deltaTime = gameTime - _lastGameTime;
        _messagePumpAccumulator += deltaTime;

        if (_messagePumpAccumulator >= MESSAGE_PUMP_INTERVAL)
        {
            Window.ProcessEvents(0);
            _messagePumpAccumulator = 0;
        }

        // Only draw when it's time
        if (gameTime - _lastRenderTime >= RENDER_INTERVAL)
        {
            RenderFrame(new FrameEventArgs(RENDER_INTERVAL));
            _lastRenderTime = gameTime;
        }

        _lastGameTime = gameTime;
        return _shouldClose;
    }



    /// <summary>
    /// Handles window resize events and updates the GL viewport and ImGui layout.
    /// </summary>
    private void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
        _imGuiBackEnd.WindowResized(e.Width, e.Height);
    }

    /// <summary>
    /// Creates and uploads a full-screen quad to the GPU as two triangles.
    /// This is used to draw the emulator's framebuffer texture on screen.
    /// </summary>
    private void SetupFullscreenQuad()
    {
        // Vertex format: [X, Y, U, V]
        // 
        // - X, Y → screen-space position in Normalized Device Coordinates (range: -1 to +1)
        // - U, V → texture coordinates in UV space (range: 0 to 1)
        //
        //   U = 0.0 is the left side of the texture, U = 1.0 is the right
        //   V = 0.0 is the top of the texture,   V = 1.0 is the bottom
        //
        // Each group of 4 floats defines a vertex for the fullscreen quad.
        // The quad is composed of two triangles covering the entire screen.
        float[] vertices =
        [
            //   X      Y      U      V
                -1f,   -1f,    0f,    1f,  // bottom-left
                 1f,   -1f,    1f,    1f,  // bottom-right
                 1f,    1f,    1f,    0f,  // top-right

                -1f,   -1f,    0f,    1f,  // bottom-left
                 1f,    1f,    1f,    0f,  // top-right
                -1f,    1f,    0f,    0f   // top-left
        ];

        // Generate a VAO (Vertex Array Object) to store the vertex format and buffer bindings
        _vao = GL.GenVertexArray();

        // Generate a VBO (Vertex Buffer Object) to hold the vertex data
        _vbo = GL.GenBuffer();

        // Bind the VAO so we can define how vertex attributes are interpreted
        GL.BindVertexArray(_vao);

        // Bind the VBO and upload the vertex data
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        // Tell OpenGL how to interpret the vertex data (stride: 4 floats per vertex)

        // Attribute 0: vec2 position (X, Y) – starts at offset 0
        GL.VertexAttribPointer(
            0, 
            2, 
            VertexAttribPointerType.Float, 
            false, 
            4 * sizeof(float), 
            0
        );
        GL.EnableVertexAttribArray(0);

        // Attribute 1: vec2 texCoords (U, V) – starts after 2 floats
        GL.VertexAttribPointer(
            1, 
            2, 
            VertexAttribPointerType.Float, 
            false, 
            4 * sizeof(float), 
            2 * sizeof(float)
        );
        GL.EnableVertexAttribArray(1);
    }

    /// <summary>
    /// Creates and compiles the default shader used for drawing the framebuffer.
    /// This program includes a vertex shader (positions screen quad)
    /// and a fragment shader (samples the framebuffer texture).
    /// </summary>
    /// <returns>OpenGL shader program ID.</returns>
    private static int CreateDefaultShader()
    {
        // Create and compile the vertex shader
        int vertex = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertex, LoadEmbeddedShader("Basic.vert.glsl"));
        GL.CompileShader(vertex);

        // Create and compile the fragment shader
        int fragment = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragment, LoadEmbeddedShader("Basic.frag.glsl"));
        GL.CompileShader(fragment);

        // Create new shader program and attach both shaders to it, and then link
        int program = GL.CreateProgram();
        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);
        GL.LinkProgram(program);

        // Shaders are now part of the program, so we can delete the raw handles
        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        // Return the linked shader program ID
        return program;
    }

    /// <summary>
    /// Loads an embedded shader resource by filename (case-insensitive match).
    /// </summary>
    /// <param name="resourceName">The name of the shader file (e.g., "Basic.vert.glsl").</param>
    /// <returns>The contents of the shader source as a string.</returns>
    public static string LoadEmbeddedShader(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string fullResourceName = assembly
            .GetManifestResourceNames()
            .First(f => f.Contains(resourceName, StringComparison.InvariantCultureIgnoreCase));

        using Stream stream = assembly.GetManifestResourceStream(fullResourceName)!;
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }


    public double FrequencyHz => 60;
    public byte UpdatePriority => 0;
    public event Action? SettingsChanged;
}
