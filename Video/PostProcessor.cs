using AteChips.EffectSettings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AteChips.Video;

public class PostProcessor
{
    private readonly GraphicsDevice _graphics;
    private readonly SpriteBatch _spriteBatch;

    // Effects
    private readonly Effect _fxScanlines;
    private readonly Effect _fxBloom;
    private readonly Effect _fxCurvature;
    private readonly Effect _fxVignette;
    private readonly Effect _fxChromatic;
    private readonly Effect _fxPhosphor;
    private readonly Effect _fxPhosphorTint;
    private readonly Effect _fxAntialias;

    // Parameters
    public bool EnableScanlines = true;
    public bool EnableBloom = false;
    public bool EnableCurvature = true;
    public bool EnableVignette = false;
    public bool EnableChromatic = false;
    public bool EnablePhosphor = false;

    public CurvatureSettings CurvatureSettings = new();
    public ScanlineSettings ScanlineSettings = new();
    public PhosphorDecaySettings PhosphorSettings = new();

    public Vector3 PhosphorColor = new(0.1f, 1.0f, 0.1f); // default: green


    // Targets
    private RenderTarget2D _rtTemp1 = null!;
    private RenderTarget2D _rtTemp2 = null!;
    private RenderTarget2D _rtPhosphorFeedback = null!;

    public PostProcessor(GraphicsDevice graphics, SpriteBatch spriteBatch, ContentManager content)
    {
        _graphics = graphics;
        _spriteBatch = spriteBatch;

        _fxScanlines = content.Load<Effect>("CrtScanlines");
        _fxBloom = content.Load<Effect>("Bloom");
        _fxCurvature = content.Load<Effect>("Curvature");
        _fxVignette = content.Load<Effect>("Vignette");
        _fxChromatic = content.Load<Effect>("ChromaticAberration");
        _fxPhosphor = content.Load<Effect>("PhosphorDecay");
        _fxPhosphorTint = content.Load<Effect>("PhosphorTint");

        _fxCurvature.Parameters["edgeFadeStrength"]?.SetValue(1.5f);
        _fxCurvature.Parameters["flickerStrength"]?.SetValue(0.02f);
        _fxCurvature.Parameters["flickerSpeed"]?.SetValue(60.0f);
        _fxCurvature.Parameters["warpAmount"]?.SetValue(0.002f);

        _fxAntialias = content.Load<Effect>("Antialias");


        ResizeRenderTargets();
    }

    public void ResizeRenderTargets()
    {
        int w = _graphics.PresentationParameters.BackBufferWidth;
        int h = _graphics.PresentationParameters.BackBufferHeight;

        _rtTemp1 = new RenderTarget2D(_graphics, w, h);
        _rtTemp2 = new RenderTarget2D(_graphics, w, h);
        _rtPhosphorFeedback = new RenderTarget2D(_graphics, w, h);
    }

    private RenderTarget2D Apply(Effect fx, RenderTarget2D source, RenderTarget2D dest)
    {
        _graphics.SetRenderTarget(dest);
        _graphics.Clear(Color.Black);

        _spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, fx);
        _spriteBatch.Draw(source, _graphics.Viewport.Bounds, Color.White);
        _spriteBatch.End();

        _graphics.SetRenderTarget(null);
        return dest;
    }

    public RenderTarget2D Render(RenderTarget2D source, GameTime gameTime, int width)
    {

        RenderTarget2D ping = _rtTemp1;
        RenderTarget2D pong = _rtTemp2;
        RenderTarget2D current = source;

        // === PHOSPHOR DECAY (MUST happen BEFORE tint) ===
        if (EnablePhosphor)
        {
            // Copy current unmodified source to feedback
            _graphics.SetRenderTarget(_rtPhosphorFeedback);
            _spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp);
            _spriteBatch.Draw(current, _graphics.Viewport.Bounds, Color.White);
            _spriteBatch.End();
            _graphics.SetRenderTarget(null);

            // Apply decay
            _fxPhosphor.Parameters["CurrentFrame"]?.SetValue(current);
            _fxPhosphor.Parameters["PreviousFrame"]?.SetValue(_rtPhosphorFeedback);
            _fxPhosphor.Parameters["decayFactor"]?.SetValue(PhosphorSettings.Decay); // ensure this is defined

            current = Apply(_fxPhosphor, current, ping);
            Swap(ref ping, ref pong);
        }

        // === PHOSPHOR TINT (AFTER decay, always-on) ===
        _fxPhosphorTint.Parameters["phosphorColor"]?.SetValue(PhosphorColor);
        current = Apply(_fxPhosphorTint, current, ping);
        Swap(ref ping, ref pong);

        // === Optional Effects ===
        if (EnableCurvature)
        {
            _fxCurvature.Parameters["time"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            _fxCurvature.Parameters["curvatureAmount"]?.SetValue(CurvatureSettings.CurvatureAmount);
            _fxCurvature.Parameters["edgeFadeStrength"]?.SetValue(CurvatureSettings.EdgeFadeStrength);
            _fxCurvature.Parameters["flickerStrength"]?.SetValue(CurvatureSettings.FlickerStrength);
            _fxCurvature.Parameters["flickerSpeed"]?.SetValue(CurvatureSettings.FlickerSpeed);
            _fxCurvature.Parameters["warpAmount"]?.SetValue(CurvatureSettings.WarpAmount);
            current = Apply(_fxCurvature, current, ping);
            Swap(ref ping, ref pong);
        }

        if (EnableScanlines)
        {
            _fxScanlines.Parameters["time"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            _fxScanlines.Parameters["scanlineIntensity"]?.SetValue(ScanlineSettings.ScanlineIntensity);
            _fxScanlines.Parameters["scanlineSharpness"]?.SetValue(ScanlineSettings.ScanlineSharpness);
            _fxScanlines.Parameters["bleedAmount"]?.SetValue(ScanlineSettings.BleedAmount);
            _fxScanlines.Parameters["flickerStrength"]?.SetValue(ScanlineSettings.FlickerStrength);
            _fxScanlines.Parameters["maskStrength"]?.SetValue(ScanlineSettings.MaskStrength);
            _fxScanlines.Parameters["slotSharpness"]?.SetValue(ScanlineSettings.SlotSharpness);
            _fxScanlines.Parameters["screenWidth"]?.SetValue((float)width);
            current = Apply(_fxScanlines, current, ping);
            Swap(ref ping, ref pong);
        }

        if (EnableBloom)
        {
            current = Apply(_fxBloom, current, ping);
            Swap(ref ping, ref pong);
        }

        if (EnableVignette)
        {
            current = Apply(_fxVignette, current, ping);
            Swap(ref ping, ref pong);
        }

        if (EnableChromatic)
        {
            current = Apply(_fxChromatic, current, ping);
            Swap(ref ping, ref pong);
        }

        // === Final AA pass ===
        _fxAntialias.Parameters["screenSize"]?.SetValue(new Vector2(source.Width, source.Height));
        current = Apply(_fxAntialias, current, ping);
        Swap(ref ping, ref pong);

        return current;
    }

    private static void Swap(ref RenderTarget2D a, ref RenderTarget2D b) => (a, b) = (b, a);
}