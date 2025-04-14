using System;
using AteChips.Core.Shared.Interfaces;

namespace AteChips.Shared.Sound;

public sealed class AudioOutputSignal : IAudioOutputSignal
{
    private readonly ISoundDevice _soundDevice;

    public AudioOutputSignal(ISoundDevice soundDevice)
    {
        _soundDevice = soundDevice ?? throw new ArgumentNullException(nameof(soundDevice));
    }

    public int GetAudioSamples(float[] buffer, int offset, int count)
    {
        if (buffer == null) { throw new ArgumentNullException(nameof(buffer)); }
        if (offset < 0 || offset >= buffer.Length) { throw new ArgumentOutOfRangeException(nameof(offset)); }
        if (count < 0 || (offset + count) > buffer.Length) { throw new ArgumentOutOfRangeException(nameof(count)); }

        return _soundDevice.GetSamples(buffer, offset, count);

    }
}