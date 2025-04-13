using AteChips.Core.Shared.Timing;

namespace AteChips.Host.Video;

public interface IDrawable : IHertzDriven
{
    bool Draw(double delta);

}