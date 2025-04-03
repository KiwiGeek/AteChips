namespace AteChips.Interfaces;
public abstract class VisualizableHardware : Hardware, IVisualizable
{
    public bool VisualShown { get; set; } = false;

    public abstract void RenderVisual();
}
