using AteChips.Core.Shared.Interfaces;

namespace AteChips.Core.Shared.Base;
public abstract class VisualizableHardware : IHardware, IVisualizable
{
    public bool VisualShown { get; set; } = false;

    public abstract void Visualize();

    public virtual string Name => GetType().Name;
}
