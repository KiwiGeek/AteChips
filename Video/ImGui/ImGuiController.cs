using System.Linq;
using AteChips.Interfaces;
using ImGuiNET;
using Vector2 = System.Numerics.Vector2;

namespace AteChips.Video.ImGui;
public class ImGuiController
{


    private readonly ImGuiFileBrowser _fileBrowser = new();

    public ImGuiController(int width, int height)
    {
        ImGuiViewportPtr viewport = ImGuiNET.ImGui.GetMainViewport();
        ImGuiNET.ImGui.SetNextWindowPos(viewport.Pos);
        ImGuiNET.ImGui.SetNextWindowSize(viewport.Size);
        ImGuiNET.ImGui.SetNextWindowViewport(viewport.ID);

        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGuiNET.ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);

        ImGuiWindowFlags hostWindowFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                           ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                                           ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus |
                                           ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration |
                                           ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoSavedSettings;

        ImGuiNET.ImGui.Begin("DockSpaceHost", hostWindowFlags);
        ImGuiNET.ImGui.PopStyleVar(3);

        // Create dockspace here
        ImGuiNET.ImGui.DockSpace(ImGuiNET.ImGui.GetID("MyDockSpace"), System.Numerics.Vector2.Zero, ImGuiDockNodeFlags.None);
        ImGuiNET.ImGui.End();

        // RebuildFontAtlas();
    }

    public void RenderUi()
    {
        RenderConsoleMenu();

        foreach (IVisualizable status in Machine.Instance.Visualizables
                     .OrderBy(visual => visual.GetType().Name).ToList())
        {
            if (status.VisualShown) { status.RenderVisual(); }
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
         foreach (IVisualizable visual in Machine.Instance.Visualizables)
         {
             if (ToggleButton(visual.GetType().Name, visual.VisualShown))
             {
                 visual.VisualShown ^= true;
             }

             ImGuiNET.ImGui.SameLine();
         }

         if (ImGuiNET.ImGui.Button("Reset"))
         {
             Machine.Instance.Reset();
         }
         ImGuiNET.ImGui.SameLine();

         if (ImGuiNET.ImGui.Button("Reboot"))
         {
             Machine.Instance.Get<Cpu>().Reset();

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
             Machine.Instance.Reset();
             Machine.Instance.Get<Ram>().LoadRom(_fileBrowser.SelectedFile);
             _fileBrowser.Reset(); // reset after loading
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