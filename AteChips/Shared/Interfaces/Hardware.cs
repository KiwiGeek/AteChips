namespace AteChips.Shared.Interfaces;

public abstract class Hardware : IHardware
{
    public virtual string Name => GetType().Name;
}