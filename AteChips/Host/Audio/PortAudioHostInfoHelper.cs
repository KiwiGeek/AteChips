using System;
using System.Runtime.InteropServices;

namespace AteChips.Host.Audio;
public static class PortAudioHostInfoHelper
{
    // Native bindings to PortAudio (assumes "libportaudio" resolves to your platform's PortAudio binary)
    private const string PortAudioLib =
#if WINDOWS
        "portaudio.dll";
#elif OSX
        "libportaudio.dylib";
#elif LINUX
        "libportaudio.so";
#endif

    [DllImport(PortAudioLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr Pa_GetDeviceInfo(int deviceIndex);

    [DllImport(PortAudioLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr Pa_GetHostApiInfo(int hostApiIndex);

    [StructLayout(LayoutKind.Sequential)]
    private struct PaDeviceInfo
    {
        public int structVersion;
        public IntPtr name;
        public int hostApi; // << this is what we want
        public int maxInputChannels;
        public int maxOutputChannels;
        public double defaultLowInputLatency;
        public double defaultLowOutputLatency;
        public double defaultHighInputLatency;
        public double defaultHighOutputLatency;
        public double defaultSampleRate;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PaHostApiInfo
    {
        public int structVersion;
        public int type;
        public IntPtr name;
        public int deviceCount;
        public int defaultInputDevice;
        public int defaultOutputDevice;
    }

    public static string? GetHostApiName(int deviceIndex)
    {
        IntPtr deviceInfoPtr = Pa_GetDeviceInfo(deviceIndex);
        if (deviceInfoPtr == IntPtr.Zero)
        {
            return null;
        }

        PaDeviceInfo deviceInfo = Marshal.PtrToStructure<PaDeviceInfo>(deviceInfoPtr);
        if (deviceInfo.hostApi < 0)
        {
            return null;
        }

        IntPtr hostApiInfoPtr = Pa_GetHostApiInfo(deviceInfo.hostApi);
        if (hostApiInfoPtr == IntPtr.Zero)
        {
            return null;
        }

        PaHostApiInfo hostApiInfo = Marshal.PtrToStructure<PaHostApiInfo>(hostApiInfoPtr);
        return Marshal.PtrToStringAnsi(hostApiInfo.name);
    }
}