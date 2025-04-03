using AteChips.Interfaces;
using AteChips.Video.ImGui;
using AteChips.Video;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using AteChips.EffectSettings;

namespace AteChips;

public class Display : VisualizableHardware
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

    public override void RenderVisual()
    {
        ImGui.Begin("Visual Settings##Main", ImGuiWindowFlags.NoDocking);

        ImGuiHelpers.Checkbox("Maintain Aspect Ratio", () => Settings.MaintainAspectRatio,
            value => Settings.MaintainAspectRatio = value);

        PhosphorColor();

        ImGui.SeparatorText("Visual Effects");

        ImGui.Checkbox("CRT Scanlines", ref _postProcessor.EnableScanlines);
        if (_postProcessor.EnableScanlines &&
            ImGui.CollapsingHeader("Scanline Settings", ImGuiTreeNodeFlags.None))
        {
            ImGui.SliderFloat("Scanline Intensity", ref _scanlines.ScanlineIntensity, 0.0f, 1.0f);
            ImGui.SliderFloat("Scanline Sharpness", ref _scanlines.ScanlineSharpness, 0.1f, 10.0f);
            ImGui.SliderFloat("Bleed Amount", ref _scanlines.BleedAmount, 0.0f, 1.0f);
            ImGui.SliderFloat("Flicker Strength", ref _scanlines.FlickerStrength, 0.0f, 0.2f);
            ImGui.SliderFloat("RGB Mask Strength", ref _scanlines.MaskStrength, 0.0f, 1.0f);
            ImGui.SliderFloat("Slot Sharpness", ref _scanlines.SlotSharpness, 1.0f, 20.0f);
        }

        ImGui.Checkbox("Bloom", ref _postProcessor.EnableBloom);

        ImGui.Checkbox("Curvature", ref _postProcessor.EnableCurvature);
        if (_postProcessor.EnableCurvature &&
            ImGui.CollapsingHeader("Curvature Settings", ImGuiTreeNodeFlags.None))
        {
            ImGui.SliderFloat("Curvature Amount", ref _curvature.CurvatureAmount, 0.0f, 1.5f);
            ImGui.SliderFloat("Edge Fade Strength", ref _curvature.EdgeFadeStrength, 0.1f, 5.0f);
            ImGui.SliderFloat("Flicker Strength", ref _curvature.FlickerStrength, 0.0f, 0.1f);
            ImGui.SliderFloat("Flicker Speed", ref _curvature.FlickerSpeed, 1.0f, 120.0f);
            ImGui.SliderFloat("RGB Warp Amount", ref _curvature.WarpAmount, 0.0f, 0.01f);
        }

        ImGui.Checkbox("Vignette", ref _postProcessor.EnableVignette);
        ImGui.Checkbox("Chromatic Aberration", ref _postProcessor.EnableChromatic);

        PhosphorDecay();


        ImGui.End();
    }

    private void PhosphorDecay()
    {
        ImGui.Checkbox("Phosphor Decay", ref _postProcessor.EnablePhosphor);

        if (_postProcessor.EnablePhosphor)
        {
            ImGui.SliderFloat("Decay Amount", ref _postProcessor.PhosphorSettings.Decay, 0.90f, 1.0f, "%.3f");
        }
    }


    private void PhosphorColor()
    {
        ImGui.SeparatorText("Phosphor Color");

        // Preset dropdown
        string[] colorOptions = ["White", "Amber", "Green"];

        ImGui.Checkbox("Use Custom Color", ref _useCustomColor);

        if (_useCustomColor)
        {
            ImGui.SliderFloat("Red", ref _customColor.X, 0.0f, 1.0f);
            ImGui.SliderFloat("Green", ref _customColor.Y, 0.0f, 1.0f);
            ImGui.SliderFloat("Blue", ref _customColor.Z, 0.0f, 1.0f);
            _postProcessor.PhosphorColor = _customColor;
        }
        else
        {
            if (ImGui.Combo("Preset", ref _selectedPhosphorIndex, colorOptions, colorOptions.Length))
            {
                _customColor = _selectedPhosphorIndex switch
                {
                    0 => new Vector3(1.0f, 1.0f, 1.0f), // White
                    1 => new Vector3(1.0f, 0.64f, 0.1f), // Amber
                    2 => new Vector3(0.1f, 1.0f, 0.1f), // Green
                    _ => new Vector3(1.0f, 1.0f, 1.0f)
                };
                _postProcessor.PhosphorColor = _customColor;
            }
        }
    }

}