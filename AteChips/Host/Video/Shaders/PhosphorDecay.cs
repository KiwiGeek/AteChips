using AteChips.Host.Video.EffectSettings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace AteChips.Host.Video.Shaders
{
    public class PhosphorDecay : IShaderEffect
    {
        private int _framebuffer;
        private int _texture;          // The persistent decay texture
        private int _shaderProgram;
        private int _resolutionLocation;
        private int _decayRateLocation;
        private int _prevFrameLocation;
        private int _newFrameLocation;
        private int _vao;

        private int _width;
        private int _height;

        private readonly PhosphorDecaySettings _settings;

        public PhosphorDecay(int fullscreenQuadVao, PhosphorDecaySettings settings)
        {
            _vao = fullscreenQuadVao;
            _settings = settings;
            _shaderProgram = CreateShader();
        }

        private int CreateShader()
        {
            int vertex = IShaderEffect.CreateShader("Basic.vert.glsl", ShaderType.VertexShader);
            int fragment = IShaderEffect.CreateShader("PhosphorDecay.frag.glsl", ShaderType.FragmentShader);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertex);
            GL.AttachShader(program, fragment);
            GL.LinkProgram(program);

            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            _resolutionLocation = GL.GetUniformLocation(program, "Resolution");
            _decayRateLocation = GL.GetUniformLocation(program, "DecayRate");
            _prevFrameLocation = GL.GetUniformLocation(program, "PrevFrame");
            _newFrameLocation = GL.GetUniformLocation(program, "NewFrame");

            return program;
        }

        private void CreateFramebuffer(int width, int height)
        {
            if (_framebuffer != 0)
            {
                GL.DeleteFramebuffer(_framebuffer);
                GL.DeleteTexture(_texture);
            }

            _texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                          PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            _framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                                    TextureTarget.Texture2D, _texture, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            _width = width;
            _height = height;
        }

        public int Apply(int newFrameTex, int width, int height)
        {
            if (!_settings.IsEnabled)
            {
                return newFrameTex; // No effect applied
            }

            if (_framebuffer == 0 || width != _width || height != _height)
            {
                CreateFramebuffer(width, height);
            }

            // --- Blend new frame into decay texture ---
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
            GL.Viewport(0, 0, width, height);

            GL.UseProgram(_shaderProgram);

            // Bind decay texture to slot 0
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.Uniform1(_prevFrameLocation, 0);

            // Bind fresh frame to slot 1
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, newFrameTex);
            GL.Uniform1(_newFrameLocation, 1);

            // Set decay rate and resolution
            GL.Uniform1(_decayRateLocation, _settings.DecayRate);
            GL.Uniform2(_resolutionLocation, new Vector2(width, height));

            // Draw full quad
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // ⚡ Now the decay buffer (_texture) holds the blended frame
            return _texture;
        }
    }
}
