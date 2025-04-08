namespace AteChips.Shared.Interfaces;

public interface IUpdatable
{
    bool Update(double gameTime);
    byte UpdatePriority { get; }
}