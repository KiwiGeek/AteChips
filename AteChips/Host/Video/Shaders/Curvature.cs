using System;
using System.IO;
using AteChips.Host.Video.EffectSettings;
using OpenTK.Graphics.OpenGL4;

namespace AteChips.Host.Video.Shaders;

public class Curvature : IShaderEffect
{

    private readonly int _vao;
    private readonly CurvatureSettings _settings;
    private readonly int _shader;
    private int _curveXLocation;
    private int _curveYLocation;
    private int _warpXLocation;
    private int _warpYLocation;
    private int _fboOutput;
    private int _texOutput;

    public Curvature(int fullscreenQuadVao, CurvatureSettings settings)
    {
        _vao = fullscreenQuadVao;
        _settings = settings;
        _shader = CreateShaderProgram();
    }

    private int CreateShaderProgram()
    {
        int vertex = IShaderEffect.CreateShader("Basic.vert.glsl", ShaderType.VertexShader);
        int fragment = IShaderEffect.CreateShader("CrtCurvature.frag.glsl", ShaderType.FragmentShader);

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);
        GL.LinkProgram(program);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        _curveXLocation = GL.GetUniformLocation(program, "CurvatureX");
        _curveYLocation = GL.GetUniformLocation(program, "CurvatureY");
        _warpXLocation = GL.GetUniformLocation(program, "WarpX");
        _warpYLocation = GL.GetUniformLocation(program, "WarpY");

        return program;
    }

    public int Apply(int newFrameTexture, int width, int height)
    {
        if (!_settings.IsEnabled)
            return newFrameTexture;

        // Ensure output FBO/texture is sized correctly
        CreateOrResize(ref _fboOutput, ref _texOutput, width, height);

        // Bind output framebuffer
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fboOutput);
        GL.Viewport(0, 0, width, height);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        // Use curvature shader
        GL.UseProgram(_shader);

        // Bind input texture
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, newFrameTexture);

        // Set uniforms
        GL.Uniform1(_curveXLocation, _settings.CurvatureX);
        GL.Uniform1(_curveYLocation, _settings.CurvatureY);
        GL.Uniform1(_warpXLocation, _settings.WarpX);
        GL.Uniform1(_warpYLocation, _settings.WarpY);

        // Draw quad
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        return _texOutput;
    }

    private void CreateOrResize(ref int fbo, ref int tex, int width, int height)
    {
        if (tex != 0)
        {
            GL.DeleteFramebuffer(fbo);
            GL.DeleteTexture(tex);
        }

        tex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, tex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, tex, 0);
    }

}