using AteChips.Core.Shared.Timing;
using AteChips.Shared.Runtime;

namespace AteChips.Core;

public class CrystalTimer : ICrystalTimer
{
    public byte DelayTimer { get; set; }
    public byte SoundTimer { get; set; }

    public double FrequencyHz => 60.0;
    public byte UpdatePriority => UpdatePriorities.CrystalTimer;

    public void Reset()
    {
        DelayTimer = 0;
        SoundTimer = 0;
    }

    public bool Update(double delta)
    {
        if (DelayTimer > 0) { DelayTimer--; }

        if (SoundTimer > 0) { SoundTimer--; }

        return false;
    }

    public string Name => nameof(CrystalTimer);

    private IHostBridge? _hostBridge;

    public virtual void SetHostBridge(IHostBridge hostBridge)
    {
        _hostBridge = hostBridge;
    }
}
