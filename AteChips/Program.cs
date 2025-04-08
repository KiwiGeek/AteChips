using System;
using AteChips.Host.Runtime;

namespace AteChips;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Create the emulated machine (core CHIP-8 hardware)
        Chip8Machine emulatedMachine = Chip8Machine.Instance;
        emulatedMachine.Reset();

        // Create and run the emulator runtime (host layer + main loop), and pass it the machine we're emulating
        EmulatorRuntime chip8 = new (emulatedMachine);
        chip8.Run();
    }
}