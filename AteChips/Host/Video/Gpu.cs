using System;
using AteChips.Core.Framebuffer;
using AteChips.Shared.Interfaces;
using OpenTK.Graphics.OpenGL4;

namespace AteChips.Host.Video;
public class Gpu : Hardware
{
    private const int Width = 64;
    private const int Height = 32;
    private readonly FrameBuffer _frameBuffer;

    private int _texture;
    private int _vao, _vbo, _shader;

    public Gpu(FrameBuffer frameBuffer)
    {
        _frameBuffer = frameBuffer;
    }

    public void Init( int windowWidth, int windowHeight)
    {

        _texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _texture);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, Width, Height, 0, PixelFormat.Red, PixelType.UnsignedByte, nint.Zero);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        _shader = CreateShader();

        SetupFullscreenQuad();
    }

    public void Render(double delta, int x, int y, int width, int height)
    {
        ConvertFramebufferToBytes(_frameBuffer.Pixels, _textureBuffer);

        GL.BindTexture(TextureTarget.Texture2D, _texture);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Width, Height, PixelFormat.Red, PixelType.UnsignedByte, _textureBuffer);

        GL.Viewport(x, y, width, height); 

        DrawFullscreenQuad();
    }

    public void SetupFullscreenQuad()
    {
        float[] vertices = {
            //   X      Y       U     V
            -1f, -1f,   0f, 1f,  // bottom-left
            1f, -1f,   1f, 1f,  // bottom-right
            1f,  1f,   1f, 0f,  // top-right

            -1f, -1f,   0f, 1f,  // bottom-left
            1f,  1f,   1f, 0f,  // top-right
            -1f,  1f,   0f, 0f   // top-left
        };

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        // Position (location = 0)
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // TexCoord (location = 1)
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
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
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    private int CreateShader()
    {
        string vertexSource = @"
        #version 330 core

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;

void main()
{
    gl_Position = vec4(aPosition, 0.0, 1.0);
    vTexCoord = aTexCoord;
}";

        string fragmentSource = @"
        #version 330 core

in vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uTexture;

void main()
{
    float pixel = texture(uTexture, vTexCoord).r;
    FragColor = vec4(vec3(pixel), 1.0); // white for on-pixel, black for off
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
