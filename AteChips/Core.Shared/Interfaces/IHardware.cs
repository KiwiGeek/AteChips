using AteChips.Shared.Runtime;

namespace AteChips.Core.Shared.Interfaces;

public interface IHardware
{
    public string Name { get; }

    void SetHostBridge(IHostBridge hostBridge);
}