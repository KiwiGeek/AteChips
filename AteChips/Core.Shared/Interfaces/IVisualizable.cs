using AteChips.Shared.Runtime;
using System.Collections.Generic;

namespace AteChips.Core.Shared.Interfaces;

public interface IVisualizable
{

    static IHostBridge HostBridge = new HostBridge();

    private static readonly Dictionary<IVisualizable, bool> ShownStates = [];

    void Visualize();

    bool VisualShown
    {
        get => ShownStates.TryGetValue(this, out bool shown) && shown;
        set => ShownStates[this] = value;
    }

}