using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AteChips.Video.ImGui;
public static class ImGuiHelpers
{
    public static void Checkbox(string label, Func<bool> getter, Action<bool> setter)
    {
        bool value = getter();
        if (ImGuiNET.ImGui.Checkbox(label, ref value))
        {
            setter(value);
        }
    }
}