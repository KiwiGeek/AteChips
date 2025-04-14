using AteChips.Core.Shared.Interfaces;

namespace AteChips.Core;

public interface ICrystalTimer : IHardware, IResettable, IUpdatable
{
    byte DelayTimer { get; set; }
    byte SoundTimer { get; set; }
}