#if NOT_WINDOWS

using System;
using OpenTK.Windowing.Desktop;

namespace AteChips.Host.UI.ImGui;

public class DpiWatcher
{
    public float GetCurrentDpiScale()
    {
        int[] viewport = new int[4];
        OpenTK.Graphics.OpenGL4.GL.GetInteger(OpenTK.Graphics.OpenGL4.GetPName.Viewport, viewport);

        int framebufferWidth = viewport[2];
        int framebufferHeight = viewport[3];

        int clientWidth = _window.ClientSize.X;
        int clientHeight = _window.ClientSize.Y;

        if (clientWidth == 0 || clientHeight == 0) { return 1.0f; }

        float scaleX = (float)framebufferWidth / clientWidth;
        float scaleY = (float)framebufferHeight / clientHeight;

        return (scaleX + scaleY) / 2f;
    }

    private float _lastScale;
    private readonly NativeWindow _window;


    public DpiWatcher(NativeWindow window, Action<float> onDpiChanged)
    {
        _window = window;

        _window.Move += _ =>
        {
            float pendingScale = GetCurrentDpiScale();

            if (Math.Abs(pendingScale - _lastScale) > 0.01f)
            {
                _lastScale = pendingScale;
                onDpiChanged.Invoke(_lastScale);
            }
        };

        // Initial scale check
        _lastScale = GetCurrentDpiScale();
        onDpiChanged.Invoke(_lastScale);
    }

}

#endif