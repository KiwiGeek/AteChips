using Microsoft.Xna.Framework;

namespace AteChips.Interfaces;

public interface IUpdatable
{
    void Update(GameTime gameTime);
    byte UpdatePriority { get; }
}