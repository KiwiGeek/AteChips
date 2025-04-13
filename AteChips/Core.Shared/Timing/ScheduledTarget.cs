namespace AteChips.Core.Shared.Timing;

public readonly struct ScheduledTarget
{
    public IHertzDriven Target { get; }
    public double DeltaTime { get; }

    public ScheduledTarget(IHertzDriven target, double deltaTime)
    {
        Target = target;
        DeltaTime = deltaTime;
    }
}