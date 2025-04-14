using System.Collections.Generic;
using AteChips.Core.Shared.Timing;
using AteChips.Shared.Sound;

namespace AteChips.Core.Shared.Interfaces;

public interface ISoundDevice : IVisualizable, IHardware, IResettable, IHertzDriven
{

    IEnumerable<IAudioOutputSignal> GetOutputs();

    IAudioOutputSignal GetPrimaryOutput();

    int SampleRate { get; }
    int Channels { get; }

    int GetSamples(float[] buffer, int offset, int count);
}