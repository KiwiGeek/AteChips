namespace AteChips.Interfaces;

public interface IUpdatable
{
    void Update(double gameTime);
    byte UpdatePriority { get; }
}