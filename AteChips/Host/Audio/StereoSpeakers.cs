using System;
using System.Linq;
using System.Runtime.InteropServices;
using AteChips.Shared.Sound;
using PortAudioSharp;

namespace AteChips.Host.Audio;
internal class StereoSpeakers
{
    private IAudioOutputSignal? _connectedSignal;
    private int[]? _channelMap; // maps output buffer channels to source signal
    private const int _sampleRate = 44100;
    int _channels = 2;
    int _framesPerRequest;
    uint _samplesPerRequest; 

    internal StereoSpeakers()
    {
        _framesPerRequest = (int)(_sampleRate / 60.0); // 735 frames
        _samplesPerRequest = (uint)(_framesPerRequest * _channels); // 735 frames * 2 channels
        Init();
    }

    /// <summary>
    /// Connects an audio signal source to the speaker.
    /// </summary>
    public void Connect(IAudioOutputSignal signal, int[] channelMap)
    {
        _connectedSignal = signal ?? throw new ArgumentNullException(nameof(signal));
        _channelMap = channelMap ?? throw new ArgumentNullException(nameof(channelMap));
    }

    /// <summary>
    /// Disconnects any current signal source.
    /// </summary>
    public void Disconnect()
    {
        _connectedSignal = null;
    }

    private void Init()
    {
        PortAudio.Initialize();
        int device = PortAudio.DefaultOutputDevice;
        DeviceInfo deviceInfo = PortAudio.GetDeviceInfo(device);
        StreamParameters outputParameters = new StreamParameters
        {
            device = device,
            channelCount = 2,
            sampleFormat = SampleFormat.Float32,
            suggestedLatency = deviceInfo.defaultLowOutputLatency,
            hostApiSpecificStreamInfo = IntPtr.Zero
        };

        // set up the callback - this goes to the connected audio signal, and requests a certain number of frames.
        Stream.Callback callback = (IntPtr input, IntPtr output, uint frameCount,
            ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, IntPtr userData) =>
        {
            if (_connectedSignal == null || _channelMap == null || output == IntPtr.Zero)
            {
                ZeroBuffer(output, frameCount * 2); // default to stereo
                return StreamCallbackResult.Continue;
            }

            int outputChannels = _channelMap?.Max() + 1 ?? _connectedSignal.Channels;
            int sampleCount = (int)(frameCount * outputChannels);
            float[] outBuffer = new float[sampleCount];
            int sourceChannels = _connectedSignal.Channels;

            if (sourceChannels == 1)
            {
                int monoSamples = (int)frameCount;
                float[] monoBuffer = new float[monoSamples];

                int written = _connectedSignal.GetAudioSamples(monoBuffer, 0, monoSamples);

                for (int i = 0; i < written; i++)
                {
                    float sample = monoBuffer[i];
                    foreach (int ch in _channelMap)
                    {
                        outBuffer[i * outputChannels + ch] = sample;
                    }
                }
            }
            else
            {
                // For now, just pull directly if formats match (no remap)
                int requested = (int)(frameCount * sourceChannels);
                int written = _connectedSignal.GetAudioSamples(outBuffer, 0, requested);
                if (written < requested)
                {
                    Array.Clear(outBuffer, written, requested - written);
                }
            }

            Marshal.Copy(outBuffer, 0, output, sampleCount);
            return StreamCallbackResult.Continue;
        };

        Stream stream =
            new Stream(
                inParams: null, 
                outParams: outputParameters,
                sampleRate: _sampleRate, 
                framesPerBuffer: _samplesPerRequest, 
                streamFlags: StreamFlags.NoFlag,
                callback: callback, 
                userData: IntPtr.Zero);

        stream.Start();
    }

    private static unsafe void ZeroBuffer(IntPtr output, uint totalSamples)
    {
        int byteCount = (int)(totalSamples * sizeof(float));
        Span<byte> span = new Span<byte>((void*)output, byteCount);
        span.Clear();
    }
}
