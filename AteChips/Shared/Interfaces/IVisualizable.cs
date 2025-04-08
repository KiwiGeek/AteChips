namespace AteChips.Shared.Interfaces;

public interface IVisualizable
{
    void RenderVisual();

    bool VisualShown { get; set; }
}