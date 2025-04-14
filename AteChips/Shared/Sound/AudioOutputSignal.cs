using System;
using AteChips.Core.Shared.Interfaces;
using System.Buffers;

namespace AteChips.Shared.Sound;

public sealed class AudioOutputSignal : IAudioOutputSignal
{
    private readonly ISoundDevice _soundDevice;
    private int _channels;

    public AudioOutputSignal(ISoundDevice soundDevice)
    {
        _soundDevice = soundDevice ?? throw new ArgumentNullException(nameof(soundDevice));
    }

    /// <summary>
    /// Number of channels (e.g. mono = 1, stereo = 2).
    /// </summary>
    public int Channels { get; set; } = 2;

    /// <summary>
    /// Retrieves audio samples from the underlying buzzer.
    /// </summary>
    public int GetAudioSamples(float[] buffer, int offset, int count)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (offset < 0 || offset >= buffer.Length) throw new ArgumentOutOfRangeException(nameof(offset));
        if (count < 0 || (offset + count) > buffer.Length) throw new ArgumentOutOfRangeException(nameof(count));

        if (_soundDevice.Channels == 1 && _channels == 2)
        {
            // Convert mono to stereo
            int monoSamplesNeeded = count / 2;
            float[] tempMono = ArrayPool<float>.Shared.Rent(monoSamplesNeeded);

            int samplesWritten = _soundDevice.GetSamples(tempMono, 0, monoSamplesNeeded);

            for (int i = 0; i < samplesWritten; i++)
            {
                float sample = tempMono[i];
                buffer[offset + i * 2] = sample;     // Left
                buffer[offset + i * 2 + 1] = sample; // Right
            }

            ArrayPool<float>.Shared.Return(tempMono, clearArray: true);
            return samplesWritten * 2;
        }
        else
        {
            // No mapping needed, forward directly
            return _soundDevice.GetSamples(buffer, offset, count);
        }
    }
}