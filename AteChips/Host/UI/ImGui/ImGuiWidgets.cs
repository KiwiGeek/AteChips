using ImGuiNET;
using System;

namespace AteChips.Host.UI.ImGui;
public static class ImGuiWidgets
{
    public static void Checkbox(string label, Func<bool> getter, Action<bool> setter)
    {
        bool value = getter();
        if (ImGuiNET.ImGui.Checkbox(label, ref value))
        {
            setter(value);
        }
    }

    public static void SliderFloat(string label, Func<float> getter, Action<float> setter, float min = 0.0f, float max = 1.0f)
    {
        float value = getter();
        if (ImGuiNET.ImGui.SliderFloat(label, ref value, min, max))
        {
            setter(value);
        }
    }

    public static void SliderInt(string label, Func<int> getter, Action<int> setter, int min = 0, int max = 100)
    {
        int value = getter();
        if (ImGuiNET.ImGui.SliderInt(label, ref value, min, max))
        {
            setter(value);
        }
    }

    public static bool ToggleButton(string label, bool active)
    {
        ImGuiNET.ImGui.PushStyleColor(ImGuiCol.Button,
            active
                ? new System.Numerics.Vector4(0.3f, 0.7f, 0.3f, 1.0f)
                : new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1.0f));

        bool clicked = ImGuiNET.ImGui.Button(label);
        ImGuiNET.ImGui.PopStyleColor();

        return clicked;
    }
}