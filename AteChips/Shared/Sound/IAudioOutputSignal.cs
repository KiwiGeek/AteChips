namespace AteChips.Shared.Sound;

public interface IAudioOutputSignal
{

    /// <summary>
    /// Gets the number of audio channels (1 = mono, 2 = stereo).
    /// </summary>
    int Channels { get; }

    /// <summary>
    /// Fills the provided buffer with audio samples.
    /// The buffer should be interleaved if stereo (L, R, L, R...).
    /// </summary>
    /// <param name="buffer">The buffer to fill with PCM float samples (-1.0 to 1.0).</param>
    /// <param name="offset">Start index in buffer.</param>
    /// <param name="count">Maximum number of floats to write (must be multiple of Channels).</param>
    /// <returns>The number of floats actually written.</returns>
    int GetAudioSamples(float[] buffer, int offset, int count);
}