using AteChips.Interfaces;
using AteChips.Video.ImGui;
using AteChips.Video;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using AteChips.EffectSettings;
using IDrawable = AteChips.Interfaces.IDrawable;

namespace AteChips;

public partial class Display : VisualizableHardware, IDrawable
{
    private SpriteBatch _spriteBatch = null!;
    private RenderTarget2D _chip8RenderTarget = null!;
    private ImGuiRenderer _imGuiRenderer = null!;
    private Texture2D _chip8PixelTexture = null!;
    private PostProcessor _postProcessor = null!;
    private RenderTarget2D _scaledRenderTarget = null!;
    private ImGuiController _imGuiController = null!;
    private GraphicsDevice _graphics = null!;
    private readonly FrameBuffer _frameBuffer;


    private CurvatureSettings _curvature = null!;
    private ScanlineSettings _scanlines = null!;

    public Display(FrameBuffer frameBuffer)
    {
        _frameBuffer = frameBuffer;
    }

    private static bool _useCustomColor;
    private static int _selectedPhosphorIndex = 2; // default to green
    private static Vector3 _customColor = new(0.1f, 1.0f, 0.1f);

    public void LoadContent(GraphicsDevice graphics, ContentManager content, Game game)
    {
        //_game = Machine.Instance.Chip8;
        _graphics = graphics;
        _spriteBatch = new SpriteBatch(graphics);
        _chip8RenderTarget = new RenderTarget2D(graphics, _frameBuffer.Width, _frameBuffer.Height);
        _imGuiRenderer = new ImGuiRenderer(game);
        float imguiScale = 1.0f; // Try 1.5 to 3.0 depending on your resolution

        ImGui.GetIO().FontGlobalScale = 1.0f; // optional; usually leave at 1
        ImGui.GetStyle().ScaleAllSizes(imguiScale);
        _imGuiRenderer.RebuildFontAtlas(); // Important!

        _chip8RenderTarget = new RenderTarget2D(
            graphics,
            _frameBuffer.Width,
            _frameBuffer.Height,
            false,
            SurfaceFormat.Color,
            DepthFormat.None
        );

        _chip8PixelTexture = new Texture2D(graphics, _frameBuffer.Width, _frameBuffer.Height);

        _postProcessor = new PostProcessor(graphics, _spriteBatch, content);
        _curvature = _postProcessor.CurvatureSettings;
        _scanlines = _postProcessor.ScanlineSettings;
        _imGuiController = new ImGuiController();

        _scaledRenderTarget = new RenderTarget2D(
            graphics,
            graphics.PresentationParameters.BackBufferWidth,
            graphics.PresentationParameters.BackBufferHeight,
            false,
            SurfaceFormat.Color,
            DepthFormat.None
        );

    }

    public void Draw(GameTime gameTime)
    {
        // 1. Update CHIP-8 texture
        _chip8PixelTexture.SetData(_frameBuffer!.Pixels);

        // 2. Draw to internal CHIP-8 render target (logical resolution, no effects)
        _graphics.SetRenderTarget(_chip8RenderTarget);
        _graphics.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_chip8PixelTexture, Vector2.Zero, Color.White);
        _spriteBatch.End();
        _graphics.SetRenderTarget(null);

        // 3. Scale to full screen-sized buffer (no aspect ratio logic here)
        _graphics.SetRenderTarget(_scaledRenderTarget);
        _graphics.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_chip8RenderTarget, _scaledRenderTarget.Bounds, Color.White);
        _spriteBatch.End();
        _graphics.SetRenderTarget(null);

        // 4. Post-process scaled image (pass correct height for scanlines)
        RenderTarget2D finalOutput = _postProcessor.Render(_scaledRenderTarget, gameTime, _graphics.Viewport.Width);

        // 5. Output to screen (stretch to full screen, use linear filter for better CRT quality)
        _graphics.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        Rectangle targetRect = Settings.MaintainAspectRatio
            ? GetLetterboxedDestination(_graphics, _scaledRenderTarget.Width, _scaledRenderTarget.Height)
            : _graphics.Viewport.Bounds;

        _spriteBatch.Draw(finalOutput, targetRect, Color.White);
        _spriteBatch.End();

        // 6. ImGui overlay
        if (Settings.ShowImGui)
        {
            _imGuiRenderer.BeforeLayout(gameTime);
            _imGuiController.RenderUi();
            _imGuiRenderer.AfterLayout();
        }
    }


    static Rectangle GetLetterboxedDestination(GraphicsDevice device, int sourceWidth, int sourceHeight)
    {
        int screenWidth = device.Viewport.Width;
        int screenHeight = device.Viewport.Height;

        float targetAspect = (float)sourceWidth / sourceHeight;
        float screenAspect = (float)screenWidth / screenHeight;

        int width, height;

        if (screenAspect > targetAspect)
        {
            // Window is wider than target
            height = screenHeight;
            width = (int)(height * targetAspect);
        }
        else
        {
            // Window is taller than target
            width = screenWidth;
            height = (int)(width / targetAspect);
        }

        int x = (screenWidth - width) / 2;
        int y = (screenHeight - height) / 2;

        return new Rectangle(x, y, width, height);
    }


}