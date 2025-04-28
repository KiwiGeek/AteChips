namespace AteChips.Host.Video.Shaders;

public interface IShaderEffect
{
    bool Enabled { get; }
    int Apply(int newFrameTexture, int width, int height);
}