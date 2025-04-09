using System.Linq;
using AteChips.Core.Shared;
using AteChips.Shared.Interfaces;
using AteChips.Shared.Settings;
using ImGuiNET;
using Vector2 = System.Numerics.Vector2;

namespace AteChips.Host.UI.ImGui;
public class ImGuiFrontEnd
{

    private readonly ImGuiFileDialog _fileBrowser = new();
    private readonly IEmulatedMachine _machine;

    public ImGuiFrontEnd(IEmulatedMachine machine, int width, int height)
    {

        _machine = machine;

        ImGuiViewportPtr viewport = ImGuiNET.ImGui.GetMainViewport();
        ImGuiNET.ImGui.SetNextWindowPos(viewport.Pos);
        ImGuiNET.ImGui.SetNextWindowSize(viewport.Size);
        ImGuiNET.ImGui.SetNextWindowViewport(viewport.ID);

        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        ImGuiWindowFlags hostWindowFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                           ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                                           ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus |
                                           ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration |
                                           ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoSavedSettings;

        ImGuiNET.ImGui.Begin("DockSpaceHost", hostWindowFlags);
        ImGuiNET.ImGui.PopStyleVar(3);

        // Create dock space here
        ImGuiNET.ImGui.DockSpace(ImGuiNET.ImGui.GetID("MyDockSpace"), Vector2.Zero, ImGuiDockNodeFlags.None);
        ImGuiNET.ImGui.End();

        // RebuildFontAtlas();
    }

    public void RenderUi()
    {
        if (!Settings.ShowImGui) { return; }

        RenderConsoleMenu();

        foreach (IVisualizable status in Program.Chip8EmulatorRuntime.Visuals
                     .OrderBy(visual => visual.GetType().Name).ToList())
        {
            if (status.VisualShown) { status.Visualize(); }
        }

    }

    private void RenderConsoleMenu()
    {
        Vector2 topLeft = ImGuiNET.ImGui.GetMainViewport().Pos;

        ImGuiNET.ImGui.SetNextWindowPos(topLeft, ImGuiCond.Always);

         ImGuiWindowFlags flags =
             ImGuiWindowFlags.NoTitleBar |
             ImGuiWindowFlags.NoResize |
             ImGuiWindowFlags.NoMove |
             ImGuiWindowFlags.NoScrollbar |
             ImGuiWindowFlags.NoSavedSettings |
             ImGuiWindowFlags.NoCollapse |
             ImGuiWindowFlags.NoDocking;

         ImGuiNET.ImGui.Begin("Debug Console", flags);
         // Top button bar
         foreach (IVisualizable visual in Program.Chip8EmulatorRuntime.Visuals)
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