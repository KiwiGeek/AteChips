using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenTK.Graphics.OpenGL4;

namespace AteChips.Host.Video.Shaders;

public static class Basic 
{

    public static int CreateShaderProgram()
    {
        // Create and compile the vertex shader
        int vertex = CreateShader("Basic.vert.glsl", ShaderType.VertexShader);
        int fragment = CreateShader("Basic.frag.glsl", ShaderType.FragmentShader);

        // Create new shader program and attach both shaders to it, and then link
        int program = GL.CreateProgram();
        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);
        GL.LinkProgram(program);

        int textureUniform = GL.GetUniformLocation(program, "Texture");
        GL.UseProgram(program);
        GL.Uniform1(textureUniform, 0); // Tell it to use texture unit 0
        GL.UseProgram(0);

        // Shaders are now part of the program, so we can delete the raw handles
        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        // Return the linked shader program ID
        return program;
    }

    private static string LoadEmbeddedShader(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string fullResourceName = assembly
            .GetManifestResourceNames()
            .First(f => f.Contains(resourceName, StringComparison.InvariantCultureIgnoreCase));

        using Stream stream = assembly.GetManifestResourceStream(fullResourceName)!;
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    private static int CreateShader(string resourceName, ShaderType shaderType)
    {
        int shaderIndex = GL.CreateShader(shaderType);
        GL.ShaderSource(shaderIndex, LoadEmbeddedShader(resourceName));
        GL.CompileShader(shaderIndex);
        return shaderIndex;
    }
}
