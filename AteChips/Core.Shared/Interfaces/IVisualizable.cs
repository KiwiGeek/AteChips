namespace AteChips.Core.Shared.Interfaces;

public interface IVisualizable
{
    void Visualize();

    bool VisualShown { get; set; }
}