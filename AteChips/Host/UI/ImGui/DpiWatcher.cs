using OpenTK.Windowing.Desktop;
using System;

namespace AteChips.Host.UI.ImGui;

public partial class DpiWatcher
{

    private float _lastScale;
    private readonly NativeWindow _window;

    public partial float GetCurrentDpiScale();

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