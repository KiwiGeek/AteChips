using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class DpiWatcher
{
    private float _lastScale = 1.0f;
    private float _pendingScale = 1.0f;
    private bool _dpiCheckQueued = false;

    private readonly Stopwatch _moveStopwatch = new();
    private const int DpiSettleTimeMs = 200;

    private readonly NativeWindow _window;
    private readonly Action<float> _onDpiChanged;

    public DpiWatcher(NativeWindow window, Action<float> onDpiChanged)
    {
        _window = window;
        _onDpiChanged = onDpiChanged;

        _window.Move += _ =>
        {
            _dpiCheckQueued = true;
            _moveStopwatch.Restart();
        };

        // Initial scale check
        _lastScale = GetCurrentDpiScale();
        _onDpiChanged?.Invoke(_lastScale);
    }

    public void Update()
    {
        if (!_dpiCheckQueued)
            return;

        if (_moveStopwatch.ElapsedMilliseconds > DpiSettleTimeMs)
        {
            _dpiCheckQueued = false;
            _pendingScale = GetCurrentDpiScale();

            if (Math.Abs(_pendingScale - _lastScale) > 0.01f)
            {
                _lastScale = _pendingScale;
                _onDpiChanged?.Invoke(_lastScale);
            }
        }
    }

    public float GetCurrentDpiScale()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                IntPtr hwnd = GetActiveWindow();
                uint dpi = GetDpiForWindow(hwnd);
                return dpi / 96f;
            }
            catch
            {
                // fallback below
            }
        }

        return EstimateScaleFromFramebuffer();
    }

    private float EstimateScaleFromFramebuffer()
    {
        int[] viewport = new int[4];
        GL.GetInteger(GetPName.Viewport, viewport);

        int framebufferWidth = viewport[2];
        int framebufferHeight = viewport[3];

        int clientWidth = _window.ClientSize.X;
        int clientHeight = _window.ClientSize.Y;

        if (clientWidth == 0 || clientHeight == 0)
            return 1.0f;

        float scaleX = (float)framebufferWidth / clientWidth;
        float scaleY = (float)framebufferHeight / clientHeight;

        return (scaleX + scaleY) / 2f;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hwnd);
}
