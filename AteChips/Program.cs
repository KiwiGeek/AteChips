using AteChips.Host.Runtime;

namespace AteChips;

public static class Program
{
    public static Chip8Machine Chip8Machine { get; } = new ();
    public static EmulatorRuntime Chip8EmulatorRuntime { get; } = new(Chip8Machine);

    public static void Main() => Chip8EmulatorRuntime.Run();

}