namespace AteChips.Shared.Interfaces;
public abstract class VisualizableHardware : Hardware, IVisualizable
{
    public bool VisualShown { get; set; } = false;

    public abstract void RenderVisual();
}
