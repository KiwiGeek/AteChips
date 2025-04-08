namespace AteChips.Shared.Interfaces;
public abstract class Visualizable : IVisualizable
{
    public abstract void RenderVisual();
    public bool VisualShown { get; set; } = false;
}