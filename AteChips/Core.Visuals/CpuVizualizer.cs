using ImGuiNET;
using System;
using System.Numerics;

namespace AteChips;
public partial class Cpu
{

    private readonly Vector4 MAGENTA = new(1f, 0.0f, 1f, 1f);
    private readonly Vector4 DARK_MAGENTA = new(0.6f, 0f, 0.6f, 1f);
    private readonly Vector4 INDIGO = new(0.3f, 0.3f, 1f, 1f);
    private readonly Vector4 YELLOW = new(1f, 1f, 0f, 1f);
    private readonly Vector4 CYAN = new(0f, 1f, 1f, 1f);
    private readonly Vector4 DARK_CYAN = new(0f, .6f, .6f, 1f);
    private readonly Vector4 GREEN = new(0f, 1f, 0f, 1f);
    private readonly Vector4 RED = new(1f, 0f, 0f, 1f);

    private bool firstOpen = true;

    public override void RenderVisual()
    {

        if (firstOpen)
        {
            firstOpen ^= true;
            ImGuiViewportPtr viewport = ImGui.GetMainViewport();
            Vector2 workPos = viewport.WorkPos;
            Vector2 workSize = viewport.WorkSize;

            Vector2 windowSize = new (350, 446);
            Vector2 topRightPos = new (workPos.X + workSize.X - windowSize.X-7, workPos.Y+45);

            ImGui.SetNextWindowPos(topRightPos, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.FirstUseEver);
        }

        ImGui.Begin("CPU State", ImGuiWindowFlags.NoSavedSettings);
        ImGui.Text("General Purpose Registers:");
        for (int i = 0; i < Registers.Length; i++)
        {
            ImGui.Text($"V{i:X}: 0x{(byte)Registers[i]:X2}");

            if ((i + 1) % 4 != 0) { ImGui.SameLine(); }
        }
        ImGui.Separator();
        // Special registers

        RenderSpecialPurposeRegister(
            description: "Index Register",
            address: Ram.INDEX_REGISTER_ADDR,
            addressColor: MAGENTA,
            dataType: typeof(ushort),
            isPointer: true,
            pointerColor: DARK_MAGENTA);

        RenderSpecialPurposeRegister(
            description: "Program Counter",
            address: Ram.PROGRAM_COUNTER_ADDR,
            addressColor: INDIGO,
            dataType: typeof(ushort),
            isPointer: true,
            pointerColor: YELLOW);

        RenderSpecialPurposeRegister(
            description: "Stack Pointer",
            address: Ram.STACK_POINTER_ADDR,
            addressColor: CYAN,
            dataType: typeof(byte),
            isPointer: true,
            pointerColor: DARK_CYAN);

        RenderSpecialPurposeRegister(
            description: "Delay Timer",
            address: Ram.DELAY_TIMER_ADDR,
            addressColor: GREEN,
            dataType: typeof(byte));

        RenderSpecialPurposeRegister(
            description: "Sound Timer",
            address: Ram.SOUND_TIMER_ADDR,
            addressColor: RED,
            dataType: typeof(byte));

        ImGui.Separator();

        // Running state toggle
        ImGui.Text("CPU State:");
        ImGui.SameLine();

        bool isRunning = ExecutionState == CpuExecutionState.Running;

        // Run / Pause toggle
        if (isRunning)
        {
            if (ImGui.Button("Pause")) { Pause(); }
        }
        else
        {
            if (ImGui.Button("Run")) { Run(); }

            ImGui.SameLine();
            if (ImGui.Button("Step")) { Step(); }
        }

        ImGui.Separator();

        // Running state toggle
        ImGui.Text("Disassembler:");
        {
            ushort pc = ProgramCounter;

            ImGui.Columns(4, "disasm_cols", true);
            ImGui.SetColumnWidth(0, 30);   // Arrow
            ImGui.SetColumnWidth(1, 57);   // Address
            ImGui.SetColumnWidth(2, 57);   // Opcode

            ImGui.SetColumnWidth(3, 220);  // Instruction

            ImGui.Text(""); ImGui.NextColumn();
            ImGui.Text("Addr"); ImGui.NextColumn();
            ImGui.Text("OpCode"); ImGui.NextColumn();
            ImGui.Text("Decoded Instruction"); ImGui.NextColumn();
            ImGui.Separator();

            for (int i = 0; i < 10; i++) // Show 10 instructions
            {
                ushort addr = (ushort)(pc + (i * 2));
                if (addr + 1 >= _ram.Memory.Length) { break; }

                ushort opcode = (ushort)((_ram.Memory[addr] << 8) | _ram.Memory[addr + 1]);
                string disasm = Disassemble(opcode);

                // Arrow for current instruction

                ImGui.Text(addr == pc ? "->" : "");
                ImGui.NextColumn();

                ImGui.Text($"0x{addr:X4}");
                ImGui.NextColumn();

                ImGui.Text($"0x{opcode:X4}");
                ImGui.NextColumn();

                ImGui.Text(disasm);
                ImGui.NextColumn();
            }

            ImGui.Columns(1);
        }

        ImGui.End();
    }

    public static string PadOrTruncate(string input, int length)
    {
        input ??= "";
        return input.Length > length
            ? input[..length]
            : input.PadRight(length);
    }


    private void RenderSpecialPurposeRegister(string description, ushort address, Vector4 addressColor, Type dataType,
        bool isPointer = false, Vector4? pointerColor = null)
    {
        if (dataType != typeof(ushort) && dataType != typeof(byte))
        {
            throw new ArgumentException("Only ushort and byte are supported", nameof(dataType));
        }

        if (isPointer && pointerColor is null)
        {
            throw new ArgumentNullException(nameof(pointerColor), "pointerColor required when isPointer");
        }
        ImGui.Text($"{PadOrTruncate(description, 16)}(");
        ImGui.SameLine();
        ImGui.TextColored(addressColor, $"0x{address:X4}");
        ImGui.SameLine();
        ImGui.Text(") :");
        ImGui.SameLine();
        if (dataType == typeof(ushort))
        {
            ImGui.TextColored(addressColor, $"0x{_ram.GetUInt16(address):X4}");
        }
        else if (dataType == typeof(byte))
        {
            ImGui.TextColored(addressColor, $"0x{_ram.GetByte(address):X2}");
        }

        if (isPointer)
        {
            ushort pointerAddress = dataType == typeof(ushort)
                ? _ram.GetUInt16(address)
                : _ram.GetByte(address);

            ImGui.SameLine();
            ImGui.Text("(");
            ImGui.SameLine();
            ImGui.TextColored(pointerColor!.Value,
                $"0x{_ram.GetUInt16(pointerAddress):X4}");
            ImGui.SameLine();
            ImGui.Text(")");
        }
    }


    public static string Disassemble(ushort opcode)
    {
        ushort nnn = (ushort)(opcode & 0x0FFF);
        byte n = (byte)(opcode & 0x000F);
        byte x = (byte)((opcode & 0x0F00) >> 8);
        byte y = (byte)((opcode & 0x00F0) >> 4);
        byte kk = (byte)(opcode & 0x00FF);

        return (opcode & 0xF000) switch
        {
            0x0000 => opcode == 0x00E0 ? "CLS" : opcode == 0x00EE ? "RET" : $"SYS {nnn:X3}",
            0x1000 => $"JP {nnn:X3}",
            0x2000 => $"CALL {nnn:X3}",
            0x3000 => $"SE V{x:X}, {kk:X2}",
            0x4000 => $"SNE V{x:X}, {kk:X2}",
            0x5000 => $"SE V{x:X}, V{y:X}",
            0x6000 => $"LD V{x:X}, {kk:X2}",
            0x7000 => $"ADD V{x:X}, {kk:X2}",
            0x8000 => (opcode & 0x000F) switch
            {
                0x0 => $"LD V{x:X}, V{y:X}",
                0x1 => $"OR V{x:X}, V{y:X}",
                0x2 => $"AND V{x:X}, V{y:X}",
                0x3 => $"XOR V{x:X}, V{y:X}",
                0x4 => $"ADD V{x:X}, V{y:X}",
                0x5 => $"SUB V{x:X}, V{y:X}",
                0x6 => $"SHR V{x:X}",
                0x7 => $"SUBN V{x:X}, V{y:X}",
                0xE => $"SHL V{x:X}",
                _ => $"UNKNOWN 0x{opcode:X4}"
            },
            0x9000 => $"SNE V{x:X}, V{y:X}",
            0xA000 => $"LD I, {nnn:X3}",
            0xB000 => $"JP V0, {nnn:X3}",
            0xC000 => $"RND V{x:X}, {kk:X2}",
            0xD000 => $"DRW V{x:X}, V{y:X}, {n}",
            0xE000 => kk switch
            {
                0x9E => $"SKP V{x:X}",
                0xA1 => $"SKNP V{x:X}",
                _ => $"UNKNOWN 0x{opcode:X4}"
            },
            0xF000 => kk switch
            {
                0x07 => $"LD V{x:X}, DT",
                0x0A => $"LD V{x:X}, K",
                0x15 => $"LD DT, V{x:X}",
                0x18 => $"LD ST, V{x:X}",
                0x1E => $"ADD I, V{x:X}",
                0x29 => $"LD F, V{x:X}",
                0x33 => $"LD B, V{x:X}",
                0x55 => $"LD [I], V0-V{x:X}",
                0x65 => $"LD V0-V{x:X}, [I]",
                _ => $"UNKNOWN 0x{opcode:X4}"
            },
            _ => $"UNKNOWN 0x{opcode:X4}"
        };
    }

}
