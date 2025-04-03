using System;
using System.Numerics;
using AteChips.Interfaces;
using ImGuiNET;

namespace AteChips;
public class Ram : VisualizableHardware, IResettable, IRam
{
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
                    ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.8f, 1f),
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
        // todo: reset the font
    }
}
