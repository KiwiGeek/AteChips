using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenTK.Graphics.OpenGL4;

namespace AteChips.Host.Video.Shaders;

public interface IShaderEffect
{
    bool Enabled { get; }
    int Apply(int newFrameTexture, int width, int height);

    protected static string LoadEmbeddedShader(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string fullResourceName = assembly
            .GetManifestResourceNames()
            .First(f => f.Contains(resourceName, StringComparison.InvariantCultureIgnoreCase));

        using Stream stream = assembly.GetManifestResourceStream(fullResourceName)!;
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    protected static int CreateShader(string resourceName, ShaderType shaderType)
    {
        int shaderIndex = GL.CreateShader(shaderType);
        GL.ShaderSource(shaderIndex, LoadEmbeddedShader(resourceName));
        GL.CompileShader(shaderIndex);
        return shaderIndex;
    }
}