#if WINDOWS

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AteChips.Host.UI.ImGui;

public partial class DpiWatcher
{
    [LibraryImport("user32.dll", SetLastError = true)] 
    private static partial IntPtr GetActiveWindow();

    [LibraryImport("user32.dll", SetLastError = true)] 
    private static partial uint GetDpiForWindow(IntPtr hWnd);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public partial float GetCurrentDpiScale()
    {
        IntPtr hWnd = GetActiveWindow();
        uint dpi = GetDpiForWindow(hWnd);
        return dpi / 96f;
    }

}

#endif