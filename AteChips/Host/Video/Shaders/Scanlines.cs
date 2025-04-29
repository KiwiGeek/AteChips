using AteChips.Host.Video.EffectSettings;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Diagnostics;

namespace AteChips.Host.Video.Shaders
{
    public class Scanlines : IShaderEffect
    {
        // Uniform locations
        private readonly int _intensityLoc;
        private readonly int _sharpnessLoc;
        private readonly int _bleedLoc;
        private readonly int _flickerLoc;
        private readonly int _maskLoc;
        private readonly int _slotLoc;
        private readonly int _timeLoc;
        private readonly int _resLoc;
        private readonly int _texLoc;

        private readonly ScanlineSettings _settings;
        private readonly int _fullscreenQuadVao;
        private readonly int _shader;
        private readonly Stopwatch _clock = Stopwatch.StartNew();

        // Offscreen FBO + texture
        private int _fbo, _fboTex;
        private int _fboW, _fboH;

        public Scanlines(int fullscreenQuadVao, ScanlineSettings settings)
        {
            _fullscreenQuadVao = fullscreenQuadVao;
            _settings = settings;
            _shader = CreateShaderProgram();

            // Cache uniform locations once
            _intensityLoc = GL.GetUniformLocation(_shader, "Intensity");
            _sharpnessLoc = GL.GetUniformLocation(_shader, "Sharpness");
            _bleedLoc = GL.GetUniformLocation(_shader, "BleedAmount");
            _flickerLoc = GL.GetUniformLocation(_shader, "FlickerStrength");
            _maskLoc = GL.GetUniformLocation(_shader, "MaskStrength");
            _slotLoc = GL.GetUniformLocation(_shader, "SlotSharpness");
            _timeLoc = GL.GetUniformLocation(_shader, "Time");
            _resLoc = GL.GetUniformLocation(_shader, "Resolution");
            _texLoc = GL.GetUniformLocation(_shader, "Texture");
        }

        private int CreateShaderProgram()
        {
            var vert = IShaderEffect.CreateShader("Basic.vert.glsl", ShaderType.VertexShader);
            var frag = IShaderEffect.CreateShader("Scanlines.frag.glsl", ShaderType.FragmentShader);

            var prog = GL.CreateProgram();
            GL.AttachShader(prog, vert);
            GL.AttachShader(prog, frag);
            GL.LinkProgram(prog);

            GL.DeleteShader(vert);
            GL.DeleteShader(frag);

            return prog;
        }

        public int Apply(int sourceTex, int width, int height)
        {
            if (!_settings.IsEnabled)
                return sourceTex;

            // 1) (Re)allocate offscreen FBO + texture if size changed
            if (_fbo == 0 || _fboW != width || _fboH != height)
            {
                if (_fbo != 0)
                {
                    GL.DeleteFramebuffer(_fbo);
                    GL.DeleteTexture(_fboTex);
                }

                _fboW = width;
                _fboH = height;

                _fboTex = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, _fboTex);
                GL.TexImage2D(TextureTarget.Texture2D, 0,
                              PixelInternalFormat.Rgba,
                              width, height, 0,
                              PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                _fbo = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                                       FramebufferAttachment.ColorAttachment0,
                                       TextureTarget.Texture2D,
                                       _fboTex, 0);

                var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (status != FramebufferErrorCode.FramebufferComplete)
                    Debug.WriteLine($"[Scanlines] FBO incomplete: {status}");
            }

            // 2) Bind that offscreen FBO and set viewport to it
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            GL.Viewport(0, 0, width, height);

            // 3) Run the scanlines shader into the FBO
            GL.UseProgram(_shader);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, sourceTex);
            GL.Uniform1(_texLoc, 0);

            GL.Uniform1(_intensityLoc, _settings.Intensity);
            GL.Uniform1(_sharpnessLoc, _settings.Sharpness);
            GL.Uniform1(_bleedLoc, _settings.BleedAmount);
            GL.Uniform1(_flickerLoc, _settings.FlickerStrength);
            GL.Uniform1(_maskLoc, _settings.MaskStrength);
            GL.Uniform1(_slotLoc, _settings.SlotSharpness);

            float t = (float)_clock.Elapsed.TotalSeconds;
            GL.Uniform1(_timeLoc, t);

            // **Use the width/height passed in** for Resolution
            GL.Uniform2(_resLoc, new Vector2(width, height));

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.ScissorTest);

            GL.BindVertexArray(_fullscreenQuadVao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);

            // 4) Return the texture containing scanlines + original image
            return _fboTex;
        }
    }
}
