using System.Collections.Generic;
using AteChips.Shared.Sound;

namespace AteChips.Core.Shared.Interfaces;

public interface ISoundDevice : IVisualizable, IHardware, IResettable
{

    IEnumerable<IAudioOutputSignal> GetOutputs();

    IAudioOutputSignal GetPrimaryOutput();

    int SampleRate { get; }
    int Channels { get; }

    int GetSamples(float[] buffer, int offset, int count);
}