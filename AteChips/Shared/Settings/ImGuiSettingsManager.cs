
using System.Numerics;
using ImGuiNET;
using Shared.Settings;

namespace AteChips.Shared.Settings;

public static class ImGuiWindowSettingsManager
{
    public static void Begin(string windowName, ImGuiSettings settings)
    {
        ImGui.SetNextWindowPos(new Vector2(settings.Left, settings.Top), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new Vector2(settings.Width, settings.Height), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowCollapsed(settings.IsCollapsed, ImGuiCond.FirstUseEver);

        ImGui.Begin(windowName, ImGuiWindowFlags.NoSavedSettings);
    }

    public static void Update(ImGuiSettings settings)
    {
        var pos = ImGui.GetWindowPos();
        var size = ImGui.GetWindowSize();
        bool collapsed = ImGui.IsWindowCollapsed();

        bool changed = false;

        if (settings.Left != (uint)pos.X || settings.Top != (uint)pos.Y)
        {
            settings.Left = (uint)pos.X;
            settings.Top = (uint)pos.Y;
            changed = true;
        }

        if (!collapsed)
        {
            if (settings.Width != (uint)size.X || settings.Height != (uint)size.Y)
            {
                settings.Width = (uint)size.X;
                settings.Height = (uint)size.Y;
                changed = true;
            }
        }

        if (settings.IsCollapsed != collapsed)
        {
            settings.IsCollapsed = collapsed;
            changed = true;
        }

        if (changed)
        {
            SettingsManager.SaveOnChangeDebounced();
        }
    }
}