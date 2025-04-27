namespace AteChips.Shared.Settings;

public class Chip8Settings
{
    public AudioSettings Audio { get; set; } = new();
    public DisplaySettings Display { get; set; } = new();
}