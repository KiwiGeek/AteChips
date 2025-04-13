using System.Linq;
using AteChips.Core;
using AteChips.Core.Shared.Interfaces;
using AteChips.Shared.Settings;
using ImGuiNET;
using Vector2 = System.Numerics.Vector2;

namespace AteChips.Host.UI.ImGui;
public class ImGuiFrontEnd
{

    private readonly ImGuiFileDialog _fileBrowser = new();
    private readonly IEmulatedMachine _machine;
    private readonly IVisualizable[] _visualizables;

    public ImGuiFrontEnd(IEmulatedMachine machine, IVisualizable[] hostVisualizables)
    {

        _machine = machine;
        _visualizables = _machine.Visualizables
            .Union(hostVisualizables)
            .OrderBy(visual => visual.GetType().Name)
            .ToArray();
    }

    public void RenderUi()
    {

        if (!Settings.ShowImGui) { return; }

        RenderConsoleMenu();

        foreach (IVisualizable status in _visualizables)
        {
            if (status.VisualShown) { status.Visualize(); }
        }

    }

    private void RenderConsoleMenu()
    {
        Vector2 topLeft = ImGuiNET.ImGui.GetMainViewport().Pos;

        ImGuiNET.ImGui.SetNextWindowPos(topLeft, ImGuiCond.Always);

        ImGuiWindowFlags flags =
             ImGuiWindowFlags.AlwaysAutoResize |
             ImGuiWindowFlags.NoTitleBar |
             ImGuiWindowFlags.NoResize |
             ImGuiWindowFlags.NoMove |
             ImGuiWindowFlags.NoScrollbar |
             ImGuiWindowFlags.NoSavedSettings |
             ImGuiWindowFlags.NoCollapse |
             ImGuiWindowFlags.NoDocking;

         ImGuiNET.ImGui.Begin("Debug Console", flags);
         // Top button bar
         foreach (IVisualizable visual in _visualizables)
         {
             if (ImGuiWidgets.ToggleButton(visual.GetType().Name, visual.VisualShown))
             {
                 visual.VisualShown ^= true;
             }

             ImGuiNET.ImGui.SameLine();
         }

         if (ImGuiNET.ImGui.Button("Reset"))
         {
             _machine.Reset();
         }
         ImGuiNET.ImGui.SameLine();

         if (ImGuiNET.ImGui.Button("Reboot"))
         {
             _machine.Get<Cpu>().Reset();
             _machine.Get<Cpu>().Run();
         }

         ImGuiNET.ImGui.SameLine();

         if (ImGuiNET.ImGui.Button("Open ROM"))
         {
             _fileBrowser.Open();
         }
         _fileBrowser.Render();
         ImGuiNET.ImGui.End();

         if (_fileBrowser.SelectedFile != null)
         {
             _machine.Reset();
             _machine.Get<Ram>().LoadRom(_fileBrowser.SelectedFile);
             _machine.Get<Cpu>().Reset();
             _machine.Get<Cpu>().Run();
            _fileBrowser.Reset(); // reset after loading
         }

    }

    

}