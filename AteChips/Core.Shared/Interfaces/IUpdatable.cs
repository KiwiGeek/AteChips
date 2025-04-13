using AteChips.Core.Shared.Timing;

namespace AteChips.Core.Shared.Interfaces;

public interface IUpdatable : IHertzDriven
{
    bool Update(double gameTime);

}