using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using AteChips.Host.Video.EffectSettings;
using System;

namespace AteChips.Host.Video.Shaders
{
    public class Bloom : IShaderEffect
    {
        private readonly int _vao;
        private readonly BloomSettings _settings;

        private readonly int _shaderExtract;
        private readonly int _shaderBlur;
        private readonly int _shaderCombine;

        private int _fboExtract;
        private int _texExtract;
        private int _fboCombined;
        private int _fboBlur;
        private int _texBlur;
        private int _texCombined;

        public Bloom(int fullscreenQuadVao, BloomSettings settings)
        {
            _vao = fullscreenQuadVao;
            _settings = settings;

            _shaderExtract = Compile("BloomExtract.frag.glsl");
            _shaderBlur = Compile("BloomBlur.frag.glsl");
            _shaderCombine = Compile("BloomCombine.frag.glsl");
        }

        private int Compile(string fragFile)
        {
            int vert = IShaderEffect.CreateShader("Basic.vert.glsl", ShaderType.VertexShader);
            int frag = IShaderEffect.CreateShader(fragFile, ShaderType.FragmentShader);
            int prog = GL.CreateProgram();
            GL.AttachShader(prog, vert);
            GL.AttachShader(prog, frag);
            GL.LinkProgram(prog);
            GL.DeleteShader(vert);
            GL.DeleteShader(frag);
            return prog;
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

        public int Apply(int newFrameTex, int width, int height)
        {
            if (!_settings.IsEnabled) { return newFrameTex; }

            // Ensure FBOs and textures are the correct size
            CreateOrResize(ref _fboExtract, ref _texExtract, width, height);
            CreateOrResize(ref _fboBlur, ref _texBlur, width, height);
            CreateOrResize(ref _fboCombined, ref _texCombined, width, height);

            // === 1. Extract bright areas ===
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fboExtract);
            GL.Viewport(0, 0, width, height);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.UseProgram(_shaderExtract);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, newFrameTex);
            GL.Uniform1(GL.GetUniformLocation(_shaderExtract, "InputTexture"), 0);
            GL.Uniform1(GL.GetUniformLocation(_shaderExtract, "Threshold"), _settings.Threshold);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            // === 2. Blur extracted bloom texture ===
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fboBlur);
            GL.Viewport(0, 0, width, height);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.UseProgram(_shaderBlur);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texExtract);
            GL.Uniform1(GL.GetUniformLocation(_shaderBlur, "InputTexture"), 0);
            GL.Uniform2(GL.GetUniformLocation(_shaderBlur, "Resolution"), new Vector2(width, height));

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            // === 3. Combine original + blurred bloom ===
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fboCombined);
            GL.Viewport(0, 0, width, height);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.UseProgram(_shaderCombine);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, newFrameTex);
            GL.Uniform1(GL.GetUniformLocation(_shaderCombine, "OriginalTexture"), 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _texBlur);
            GL.Uniform1(GL.GetUniformLocation(_shaderCombine, "BloomTexture"), 1);

            GL.Uniform1(GL.GetUniformLocation(_shaderCombine, "BloomIntensity"), _settings.Intensity);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            // === Done ===
            return _texCombined;
        }

    }
}
