namespace AteChips.Host.Video.EffectSettings;

public class CurvatureSettings
{
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Horizontal curvature strength. Recommended range: 0.0 to 0.3
    /// </summary>
    public float CurvatureX { get; set; } = 0.1f;

    /// <summary>
    /// Vertical curvature strength. Recommended range: 0.0 to 0.3
    /// </summary>
    public float CurvatureY { get; set; } = 0.15f;
    public float WarpX { get; set; } = 0.0f;
    public float WarpY { get; set; } = 0.0f;
}