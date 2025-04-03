using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using AteChips.Interfaces;
using ImGuiNET;

namespace AteChips;
public class Ram : VisualizableHardware, IResettable, IRam
{
    public const int FontStartAddress = 0x50;
    private static readonly byte[] FONT_DATA =
    [
        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80  // F
    ];

    private Cpu? _cpu;

    public byte[] Memory { get; } = new byte[4096];

    // load the rom into memory, starting at 0x200
    public void LoadRom(string filePath)
    {
        byte[] rom = System.IO.File.ReadAllBytes(filePath);
        for (int i = 0; i < rom.Length; i++)
        {
            Memory[i + 0x200] = rom[i];
        }
    }

    public Ram()
    {
        Reset();
    }

    public override void RenderVisual()
    {
        _cpu ??= Machine.Instance.Get<Cpu>();

        ImGui.Begin("Ram", ImGuiWindowFlags.NoSavedSettings);

        const int BYTES_PER_ROW = 16;

        for (int address = 0; address < Memory.Length; address += BYTES_PER_ROW)
        {
            // Print address
            ImGui.Text($"0x{address:X4}: ");

            ImGui.SameLine();

            // Print hex values
            for (int i = 0; i < BYTES_PER_ROW; i++)
            {
                if (address + i < Memory.Length)
                {
                    ImGui.SameLine();
                    Vector4 color = new Vector4(0.8f, 0.8f, 0.8f, 1f);

                    if (address + i >= _cpu.ProgramCounter && address + i < _cpu.ProgramCounter + 2)
                    {
                        color = new Vector4(1f, 1f, 0f, 1f);
                    }

                    if (address + i == 0x1FC || address + i == 0x1FD)
                    {
                        color = new Vector4(0.3f, 0.3f, 1f, 1f);
                    }
                    if (address + i == 0x1FE || address + i == 0x1FF)
                    {
                        color = new Vector4(1f, 0f, 1f, 1f);
                    }

                    if (address + i == 0x1FB)
                    {
                        color = new Vector4(0f, 1f, 1f, 1f);
                    }

                    if (address + i >= _cpu.StackPointer && address + i < _cpu.StackPointer + 2)
                    {
                        color = new Vector4(0f, 1f, 1f, 1f);
                    }

                    if (address + i == 0x1FA)
                    {
                        color = new Vector4(0f, 1f, 0f, 1f);
                    }

                    if (address + i == 0x1F9)
                    {
                        color = new Vector4(1f, 0f, 0f, 1f);
                    }

                    ImGui.TextColored(color,
                        $"{Memory[address + i]:X2} ");
                    ImGui.SameLine();
                }
            }

            ImGui.NewLine();
        }

        ImGui.End();
    }

    public void Reset()
    {
        Memory.AsSpan().Clear();
        PopulateFont();
    }

    private void PopulateFont() =>
        Array.Copy(
            FONT_DATA,
            0,
            Memory,
            FontStartAddress,
            FONT_DATA.Length);
}
