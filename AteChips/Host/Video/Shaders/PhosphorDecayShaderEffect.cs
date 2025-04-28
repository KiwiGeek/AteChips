using System;
using OpenTK.Graphics.OpenGL4;

namespace AteChips.Host.Video.Shaders
{
    public class PhosphorDecayShaderEffect : IShaderEffect
    {
        // --------------------------------------------------------------------
        // ctor-supplied
        // --------------------------------------------------------------------
        private readonly int _shader;
        private readonly int _fullscreenQuadVao;
        private readonly Func<bool> _isEnabled;
        private readonly Func<float> _getDecayRate;

        // --------------------------------------------------------------------
        // ping-pong state (created lazily in EnsureHistoryTexture)
        // --------------------------------------------------------------------
        private int _texA, _texB;   // two history textures
        private int _fboA, _fboB;   // their FBOs
        private bool _useA = true;  // which one will receive next frame

        public bool Enabled => _isEnabled();

        public PhosphorDecayShaderEffect(int shader,
                                         int fullscreenQuadVao,
                                         Func<bool> isEnabled,
                                         Func<float> getDecayRate)
        {
            _shader = shader;
            _fullscreenQuadVao = fullscreenQuadVao;
            _isEnabled = isEnabled;
            _getDecayRate = getDecayRate;
        }

        // --------------------------------------------------------------------
        // IShaderEffect
        // --------------------------------------------------------------------
        public int Apply(int newFrameTex, int width, int height)
        {
            if (!Enabled)
                return newFrameTex;          // bypass

            EnsureHistoryTexture(width, height);

            int srcTex = _useA ? _texA : _texB;   // previous history
            int dstFbo = _useA ? _fboB : _fboA;   // where we write now
            int dstTex = _useA ? _texB : _texA;   // texture attached there
            _useA = !_useA;                         // toggle for next frame

            // --- render blend into dstTex -----------------------------------
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, dstFbo);
            GL.Viewport(0, 0, width, height);
            GL.UseProgram(_shader);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, srcTex);
            GL.Uniform1(GL.GetUniformLocation(_shader, "u_History"), 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, newFrameTex);
            GL.Uniform1(GL.GetUniformLocation(_shader, "u_NewFrame"), 1);

            GL.Uniform1(GL.GetUniformLocation(_shader, "u_DecayRate"), _getDecayRate());

            GL.BindVertexArray(_fullscreenQuadVao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            // clean up state for caller
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ActiveTexture(TextureUnit.Texture0);

            return dstTex;   // new blended frame
        }

        // --------------------------------------------------------------------
        // helper: lazily create the two textures/FBOs
        // --------------------------------------------------------------------
        private void EnsureHistoryTexture(int w, int h)
        {
            if (_texA != 0) return;          // already created

            (_texA, _fboA) = CreatePair(w, h);
            (_texB, _fboB) = CreatePair(w, h);

            // clear both history textures once
            foreach (var fbo in new[] { _fboA, _fboB })
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
    }
}
