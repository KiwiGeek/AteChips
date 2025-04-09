namespace AteChips.Core.Shared.Interfaces;
public class DisplayCharacteristics
{
    public float PixelAspectRatio { get; init; } = 1.0f;

    public DisplayCharacteristics() { }

    public DisplayCharacteristics(float pixelAspectRatio)
    {
        PixelAspectRatio = pixelAspectRatio;
    }
}
