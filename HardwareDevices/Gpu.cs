using System;
using AteChips.Interfaces;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace AteChips;
class Gpu : Hardware
{
    private const int Width = 64;
    private const int Height = 32;
    private FrameBuffer _framebuffer;
    private int _projectionLocation;
    private int _windowWidth;
    private int _windowHeight;

    private int _texture;
    private int _vao, _vbo, _shader;

    public void Init(int windowWidth, int windowHeight)
    {
        _framebuffer = Machine.Instance.Get<FrameBuffer>();
        _texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _texture);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, Width, Height, 0, PixelFormat.Red, PixelType.UnsignedByte, IntPtr.Zero);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        _shader = CreateShader();
        _projectionLocation = GL.GetUniformLocation(_shader, "projection");

        _windowHeight = windowHeight;
        _windowWidth = windowWidth;

        SetupFullscreenQuad();
    }

    public void Tick()
    {
        // Future: cycle-based GPU logic (timing, scanlines, etc.)
    }

    public void Render(double delta, int windowWidth, int windowHeight)
    {
        ConvertFramebufferToBytes(_framebuffer.Pixels, _textureBuffer);

        GL.BindTexture(TextureTarget.Texture2D, _texture);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Width, Height, PixelFormat.Red, PixelType.UnsignedByte, _textureBuffer);

        GL.Viewport(0, 0, windowWidth, windowHeight);
        _windowWidth = windowWidth;
        _windowHeight = windowHeight;

        DrawFullscreenQuad();
    }

    public void SetupFullscreenQuad()
    {
        float[] verts = {
            // x, y, u, v (in pixel space)
            0f,   0f,  0f, 0f,
            0f,  32f,  0f, 1f,
           64f,  32f,  1f, 1f,

            0f,   0f,  0f, 0f,
           64f,  32f,  1f, 1f,
           64f,   0f,  1f, 0f,
        };

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
    }

    private byte[] _textureBuffer = new byte[Width * Height];

    private void ConvertFramebufferToBytes(bool[] source, byte[] target)
    {
        for (int i = 0; i < source.Length; i++)
            target[i] = source[i] ? (byte)255 : (byte)0;
    }

    private void DrawFullscreenQuad()
    {
        GL.UseProgram(_shader);

        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(
            0, 64,
            32, 0, // flip Y for top-left origin
            -1.0f, 1.0f
        );
        GL.UniformMatrix4(_projectionLocation, false, ref projection);

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    public void Clear()
    {
        _framebuffer.Reset();
    }

    public void SetPixel(int x, int y, byte value)
    {
        _framebuffer[x, y] = value == 1;
    }

    private int CreateShader()
    {
        string vertexSource = @"
        #version 330 core
        layout (location = 0) in vec2 in_pos;
        layout (location = 1) in vec2 in_uv;
        uniform mat4 projection;
        out vec2 uv;
        void main() {
            uv = in_uv;
            gl_Position = projection * vec4(in_pos, 0.0, 1.0);
        }";

        string fragmentSource = @"
        #version 330 core
        in vec2 uv;
        out vec4 color;
        uniform sampler2D tex;
        void main() {
            float val = texture(tex, uv).r;
            color = vec4(val, val, val, 1.0);
        }";

        int vertex = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertex, vertexSource);
        GL.CompileShader(vertex);
        Console.WriteLine(GL.GetShaderInfoLog(vertex));

        int fragment = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragment, fragmentSource);
        GL.CompileShader(fragment);
        Console.WriteLine(GL.GetShaderInfoLog(fragment));

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);
        GL.LinkProgram(program);
        Console.WriteLine(GL.GetProgramInfoLog(program));

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        return program;
    }
}
