namespace AteChips.Shared.Interfaces;

public interface IVisualizable
{
    void Visualize();

    bool VisualShown { get; set; }
}