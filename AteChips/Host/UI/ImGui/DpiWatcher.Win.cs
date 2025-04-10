#if WINDOWS

using System;
using System.Runtime.InteropServices;
using OpenTK.Windowing.Desktop;

namespace AteChips.Host.UI.ImGui;

public class DpiWatcher
{
    [DllImport("user32.dll")] private static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] private static extern uint GetDpiForWindow(IntPtr hWnd);

    private float _lastScale;

    public DpiWatcher(NativeWindow window, Action<float> onDpiChanged)
    {
        window.Move += _ =>
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
        //onDpiChanged.Invoke(_lastScale);
    }

    public float GetCurrentDpiScale()
    {
        IntPtr hWnd = GetActiveWindow();
        uint dpi = GetDpiForWindow(hWnd);
        return dpi / 96f;
    }

}

#endif