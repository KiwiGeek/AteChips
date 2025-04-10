#if NOT_WINDOWS

namespace AteChips.Host.UI.ImGui;

public partial class DpiWatcher
{
    public partial float GetCurrentDpiScale()
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
}

#endif