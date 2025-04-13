namespace AteChips.Core.Shared.Timing;

public interface IHertzDriven
{
    double FrequencyHz { get; }
    byte UpdatePriority { get; }
}