using AteChips.Host.Video.Shaders;
using System.Collections.Generic;

public class ShaderPipeline
{
    private readonly List<IShaderEffect> _effects = new();
    private readonly int _fullscreenQuadVao;

    public ShaderPipeline(int width, int height, int fullscreenQuadVao)
    {
        _fullscreenQuadVao = fullscreenQuadVao;
    }

    public void AddEffect(IShaderEffect effect)
    {
        _effects.Add(effect);
    }

    public int Apply(int newFrameTexture, int width, int height)
    {
        int currentTexture = newFrameTexture;

        foreach (var effect in _effects)
        {
            if (effect.Enabled)
            {
                currentTexture = effect.Apply(currentTexture, width, height);
            }
        }

        return currentTexture;
    }
}