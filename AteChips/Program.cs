using AteChips.Core;
using AteChips.Host.Runtime;

namespace AteChips;

public static class Program
{
    private static Chip8Machine Chip8Machine = null!;
    private static EmulatorRuntime Chip8EmulatorRuntime = null!;

    public static void Main()
    {
        NativeResolver.Setup();
        Chip8Machine = new();
        Chip8EmulatorRuntime = new(Chip8Machine);
        Chip8EmulatorRuntime.Run();
    }
}