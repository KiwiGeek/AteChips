using System;
using AteChips.Host.Runtime;

namespace AteChips;

public static class Program
{
    public static EmulatorRuntime Chip8EmulatorRuntime;
    public static Chip8Machine Chip8Machine;

    [STAThread]
    public static void Main(string[] args)
    {
        // Create the emulated machine (core CHIP-8 hardware)
        Chip8Machine = new Chip8Machine();
        Chip8Machine.Reset();

        // Create and run the emulator runtime (host layer + main loop), and pass it the machine we're emulating
        Chip8EmulatorRuntime = new (Chip8Machine);
        Chip8EmulatorRuntime.Run();
    }
}