using System;
using OpenTK.Graphics.OpenGL4;

namespace AteChips.Host.Video.Shaders;

public class PhosphorDecay : IShaderEffect
{

    private readonly int _fullscreenQuadVao;
    private readonly Func<bool> _isEnabled;
    private readonly Func<float> _decayRate;

    private int _textureA, _textureB;                       // two history textures
    private int _framebufferObjectA, _framebufferObjectB;   // their FBOs
    private bool _useA = true;                              // which one will receive next frame

    public bool Enabled => _isEnabled();
    private readonly int _shader;

    public PhosphorDecay(int fullscreenQuadVao, Func<bool> isEnabled, Func<float> decayRate)
    {
        _shader = CreateShaderProgram();
        _fullscreenQuadVao = fullscreenQuadVao;
        _isEnabled = isEnabled;
        _decayRate = decayRate;
    }

    // --------------------------------------------------------------------
    // IShaderEffect
    // --------------------------------------------------------------------
    public int Apply(int newFrameTex, int width, int height)
    {
        if (!Enabled) { return newFrameTex; }

        EnsureHistoryTexture(width, height);

        int sourceTexture = _useA ? _textureA : _textureB;                                  // previous history
        int destFrameBufferObject = _useA ? _framebufferObjectB : _framebufferObjectA;      // where we write now
        int destTexture = _useA ? _textureB : _textureA;                                    // texture attached there
        _useA ^= true;                                                                      // toggle for next frame

        // --- render blend into dstTex -----------------------------------
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, destFrameBufferObject);
        GL.Viewport(0, 0, width, height);
        GL.UseProgram(_shader);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, sourceTexture);
        GL.Uniform1(GL.GetUniformLocation(_shader, "History"), 0);

        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, newFrameTex);
        GL.Uniform1(GL.GetUniformLocation(_shader, "NewFrame"), 1);

        GL.Uniform1(GL.GetUniformLocation(_shader, "DecayRate"), _decayRate());

        GL.BindVertexArray(_fullscreenQuadVao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        // clean up state for caller
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.ActiveTexture(TextureUnit.Texture0);

        return destTexture;   // new blended frame
    }

    // --------------------------------------------------------------------
    // helper: lazily create the two textures/FBOs
    // --------------------------------------------------------------------
    private void EnsureHistoryTexture(int w, int h)
    {
        if (_textureA != 0) { return; } // already created

        (_textureA, _framebufferObjectA) = CreatePair(w, h);
        (_textureB, _framebufferObjectB) = CreatePair(w, h);

        // clear both history textures once
        foreach (int fbo in new[] { _framebufferObjectA, _framebufferObjectB })
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private static (int tex, int fbo) CreatePair(int w, int h)
    {
        int tex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, tex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8,
            w, h, 0, PixelFormat.Red, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        int fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, tex, 0);

        return (tex, fbo);
    }

    private static int CreateShaderProgram()
    {
        int vertex = IShaderEffect.CreateShader("Basic.vert.glsl", ShaderType.VertexShader);
        int fragment = IShaderEffect.CreateShader("PhosphorDecay.frag.glsl", ShaderType.FragmentShader);

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);
        GL.LinkProgram(program);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        return program;
    }

}