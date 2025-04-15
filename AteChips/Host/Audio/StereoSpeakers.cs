using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AteChips.Shared.Runtime;
using AteChips.Shared.Sound;
using PortAudioSharp;

namespace AteChips.Host.Audio;
internal class StereoSpeakers : IHostService, IAudioOutputDevice
{
    private IAudioOutputSignal? _connectedSignal;
    private int[]? _channelMap; // maps output buffer channels to source signal
    private const int _sampleRate = 44100;
    int _channels = 2;
    int _framesPerRequest;
    uint _samplesPerRequest;
    private Stream? _stream; // store as a field

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

            int outputChannels = 2;
            int sampleCount = (int)(frameCount * outputChannels);
            float[] outBuffer = new float[sampleCount];
            int sourceChannels = 2;

            // For now, just pull directly if formats match (no remap). We're assuming a stereo signal
            int requested = (int)(frameCount * sourceChannels);
            int written = _connectedSignal.GetAudioSamples(outBuffer, 0, requested);
            if (written < requested)
            {
                Array.Clear(outBuffer, written, requested - written);
            }

            Marshal.Copy(outBuffer, 0, output, sampleCount);
            return StreamCallbackResult.Continue;
        };

        _stream =
            new Stream(
                inParams: null,
                outParams: outputParameters,
                sampleRate: _sampleRate,
                framesPerBuffer: _samplesPerRequest,
                streamFlags: StreamFlags.NoFlag,
                callback: callback,
                userData: IntPtr.Zero);

        _stream.Start();
    }

    private static unsafe void ZeroBuffer(IntPtr output, uint totalSamples)
    {
        int byteCount = (int)(totalSamples * sizeof(float));
        Span<byte> span = new Span<byte>((void*)output, byteCount);
        span.Clear();
    }

    public IEnumerable<(int, string)> GetHardwareDevices()
    {
        for (int i = 0; i != PortAudio.DeviceCount; ++i)
        {
            DeviceInfo info = PortAudio.GetDeviceInfo(i);
            if (info.maxOutputChannels >= 2)
            {
                // skip extra windows API hosts; potentially need to do the same for Tux hosts, but I haven't tested yet.
                if (PortAudioHostInfoHelper.GetHostApiName(i).IsOneOf(["MME", "Windows WDM-KS", "Windows WASAPI"])) { continue; }

                yield return (i, $"{info.name}");
            }
        }
    }
}
