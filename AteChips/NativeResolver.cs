using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AteChips;

static class NativeResolver
{

    public static void Setup()
    {
        NativeLibrary.SetDllImportResolver(typeof(PortAudioSharp.PortAudio).Assembly, Resolve);
    }

    public static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == "portaudio")
        {
            string baseDir = AppContext.BaseDirectory;
            string runtimeId = GetRuntimeId();
            string libraryPath = GetLibraryName();
            string nativePath = Path.Combine(baseDir, "runtimes", runtimeId, "native", libraryPath);
            if (File.Exists(nativePath))
            {
                return NativeLibrary.Load(nativePath);
            }
        }

        return IntPtr.Zero;
    }

    private static string GetLibraryName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "portaudio.dll";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "libportaudio.dylib";
        }

        return "libportaudio.so";
    }
    
    private static string GetRuntimeId()
    {
        string os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : 
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" :
                    "linux";

        string arch = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException("Unknown architecture.")
        };
        
        return $"{os}-{arch}";
    }
}