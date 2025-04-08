using System.Linq;
using AteChips.Shared.Interfaces;
using AteChips.Shared.Settings;
using ImGuiNET;
using Vector2 = System.Numerics.Vector2;

namespace AteChips.Host.UI;
public class ImGuiController
{


    private readonly ImGuiFileBrowser _fileBrowser = new();

    public ImGuiController(int width, int height)
    {
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.Pos);
        ImGui.SetNextWindowSize(viewport.Size);
        ImGui.SetNextWindowViewport(viewport.ID);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        ImGuiWindowFlags hostWindowFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                           ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                                           ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus |
                                           ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration |
                                           ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoSavedSettings;

        ImGui.Begin("DockSpaceHost", hostWindowFlags);
        ImGui.PopStyleVar(3);

        // Create dockspace here
        ImGui.DockSpace(ImGui.GetID("MyDockSpace"), Vector2.Zero, ImGuiDockNodeFlags.None);
        ImGui.End();

        // RebuildFontAtlas();
    }

    public void RenderUi()
    {
        if (!Settings.ShowImGui) { return; }

        RenderConsoleMenu();

        foreach (IVisualizable status in Chip8Machine.Instance.Visualizables
                     .OrderBy(visual => visual.GetType().Name).ToList())
        {
            if (status.VisualShown) { status.RenderVisual(); }
        }

    }

    private void RenderConsoleMenu()
    {
        Vector2 topLeft = ImGui.GetMainViewport().Pos;

        ImGui.SetNextWindowPos(topLeft, ImGuiCond.Always);

         ImGuiWindowFlags flags =
             ImGuiWindowFlags.NoTitleBar |
             ImGuiWindowFlags.NoResize |
             ImGuiWindowFlags.NoMove |
             ImGuiWindowFlags.NoScrollbar |
             ImGuiWindowFlags.NoSavedSettings |
             ImGuiWindowFlags.NoCollapse |
             ImGuiWindowFlags.NoDocking;

        ImGui.Begin("Debug Console", flags);
         // Top button bar
         foreach (IVisualizable visual in Chip8Machine.Instance.Visualizables)
         {
             if (ToggleButton(visual.GetType().Name, visual.VisualShown))
             {
                 visual.VisualShown ^= true;
             }

            ImGui.SameLine();
         }

         if (ImGui.Button("Reset"))
         {
             Chip8Machine.Instance.Reset();
         }
        ImGui.SameLine();

         if (ImGui.Button("Reboot"))
         {
             Chip8Machine.Instance.Get<Cpu>().Reset();
             Chip8Machine.Instance.Get<Cpu>().Run();
         }

        ImGui.SameLine();

         if (ImGui.Button("Open ROM"))
         {
             _fileBrowser.Open();
         }
         _fileBrowser.Render();
        ImGui.End();

         if (_fileBrowser.SelectedFile != null)
         {
             Chip8Machine.Instance.Reset();
             Chip8Machine.Instance.Get<Ram>().LoadRom(_fileBrowser.SelectedFile);
             Chip8Machine.Instance.Get<Cpu>().Reset();
             Chip8Machine.Instance.Get<Cpu>().Run();
            _fileBrowser.Reset(); // reset after loading
         }

    }

    public static bool ToggleButton(string label, bool active)
    {
        ImGui.PushStyleColor(ImGuiCol.Button,
            active
                ? new System.Numerics.Vector4(0.3f, 0.7f, 0.3f, 1.0f)
                : new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1.0f));

        bool clicked = ImGui.Button(label);
        ImGui.PopStyleColor();

        return clicked;
    }

}