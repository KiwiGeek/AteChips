
namespace AteChips.Interfaces;

public abstract class Hardware : IHardware
{
    public virtual string Name => GetType().Name;
}