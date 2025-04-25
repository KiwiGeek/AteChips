using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using AteChips.Core.Shared.Interfaces;
using AteChips.Shared.Runtime;
using AteChips.Shared.Sound;
using PortAudioSharp;

namespace AteChips.Host.Audio;
internal class StereoSpeakers : IHostService, IAudioOutputDevice, ISettingsChangedNotifier
{

    public event Action? SettingsChanged;

    private IAudioOutputSignal? _connectedSignal;
    private int[]? _channelMap; // maps output buffer channels to source signal
    private const int SAMPLE_RATE = 44100;
    private const int CHANNELS = 2;
    private readonly uint _samplesPerRequest;
    private Stream? _stream; // store as a field

    private readonly StereoSpeakersSetting _speakerSettings;

    internal StereoSpeakers(StereoSpeakersSetting settings)
    {
        _speakerSettings = settings ?? throw new ArgumentNullException(nameof(settings));

        int framesPerRequest = (int)(SAMPLE_RATE / 60.0); // 735 frames
        _samplesPerRequest = (uint)(framesPerRequest * CHANNELS); // 735 frames * 2 channels
        
        PortAudio.Initialize();

        // if we have a device name in settings, we try to find it. If we don't, or we can't 
        // find it, then we fall back to the default device.
        if (!string.IsNullOrEmpty(_speakerSettings.AudioDeviceName))
        {
            // try to find the device by name
            int deviceId = GetHardwareDevices()
                .FirstOrDefault(f => f.Item2 == _speakerSettings.AudioDeviceName).Item1;
            if (deviceId != 0)
            {
                ConnectToSoundDevice(deviceId);
                return;
            }
        }

        // get the default audio device. For Mac and Linux, this is the first device. For windows, we're
        // going with the lowest numbered device returned by GetHardwareDevices, because that's _probably_
        // the default DirectSound device. I've done zero testing to confirm this.
        int defaultDeviceId = GetHardwareDevices().OrderBy(f => f.Item1).FirstOrDefault().Item1;

        // pass that ID on to the init function, to set up the stream.
        ConnectToSoundDevice(defaultDeviceId);
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

    public void ConnectToSoundDevice(int deviceId)
    {
        // are we already hooked up to a sound card?
        if (_stream?.IsActive == true) { _stream.Stop(); }

        DeviceInfo deviceInfo = PortAudio.GetDeviceInfo(deviceId);
        StreamParameters outputParameters = new ()
        {
            device = deviceId,
            channelCount = 2,
            sampleFormat = SampleFormat.Float32,
            suggestedLatency = deviceInfo.defaultLowOutputLatency,
            hostApiSpecificStreamInfo = IntPtr.Zero
        };
        _speakerSettings.AudioDeviceName = deviceInfo.name;
        SettingsChanged?.Invoke();

        _stream =
            new Stream(
                inParams: null,
                outParams: outputParameters,
                sampleRate: SAMPLE_RATE,
                framesPerBuffer: _samplesPerRequest,
                streamFlags: StreamFlags.NoFlag,
                callback: HandleAudioCallback,
                userData: IntPtr.Zero);

            _stream.Start();
    }

    private StreamCallbackResult HandleAudioCallback(
        IntPtr input,
        IntPtr output,
        uint frameCount,
        ref StreamCallbackTimeInfo timeInfo,
        StreamCallbackFlags statusFlags,
        IntPtr userData)
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

        int requested = (int)(frameCount * sourceChannels);
        int written = _connectedSignal.GetAudioSamples(outBuffer, 0, requested);
        if (written < requested)
        {
            Array.Clear(outBuffer, written, requested - written);
        }

        Marshal.Copy(outBuffer, 0, output, sampleCount);
        return StreamCallbackResult.Continue;
    }

    private static unsafe void ZeroBuffer(IntPtr output, uint totalSamples)
    {
        int byteCount = (int)(totalSamples * sizeof(float));
        Span<byte> span = new ((void*)output, byteCount);
        span.Clear();
    }

    public static IEnumerable<(int, string)> GetHardwareDevices()
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
