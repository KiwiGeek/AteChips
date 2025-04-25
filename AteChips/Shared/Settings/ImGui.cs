namespace AteChips.Shared.Settings;
public class ImGuiSettings
{
    public uint Left { get; set; } = 100;   // Default starting X
    public uint Top { get; set; } = 100;    // Default starting Y
    public uint Width { get; set; } = 400;  // Reasonable width
    public uint Height { get; set; } = 300; // Reasonable height
    public bool IsCollapsed { get; set; } = false;
}
