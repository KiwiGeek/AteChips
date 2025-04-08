using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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