using System.Collections.Generic;

namespace AteChips.Host.Video.Shaders;

public static class ShaderPipeline
{
    private static readonly List<IShaderEffect> Effects = [];

    public static void AddEffect(IShaderEffect effect)
    {
        Effects.Add(effect);
    }

    public static int Apply(int newFrameTexture, int width, int height)
    {
        int currentTexture = newFrameTexture;

        foreach (IShaderEffect effect in Effects)
        {
            currentTexture = effect.Apply(currentTexture, width, height);
        }

        return currentTexture;
    }
}