using AteChips.Interfaces;
using ImGuiNET;
using System;
using System.Numerics;

namespace AteChips;
public class Cpu : VisualizableHardware, IResettable, ICpu
{

    public enum CpuExecutionState
    {
        Running,
        Paused,
        Stepping
    }


    // CPU registers
    public char[] Registers { get; private set; } = null!;


    public ushort Index
    {
        get => (ushort)((_ram.Memory[0x1FE] << 8) + _ram.Memory[0x1FF]);
        set
        {
            _ram.Memory[0x1FE] = (byte)(value >> 8);
            _ram.Memory[0x1FF] = (byte)(value & 0x00FF);
        }
    }

    // the program counter is stored in RAM at 0x1FC
    public ushort ProgramCounter
    {
        get => (ushort)((_ram.Memory[0x1FC] << 8) + _ram.Memory[0x1FD]);
        set
        {
            _ram.Memory[0x1FC] = (byte)(value >> 8);
            _ram.Memory[0x1FD] = (byte)(value & 0x00FF);
        }
    }
    public ushort ProgramCounterValue => (ushort)((_ram.Memory[ProgramCounter] << 8) + _ram.Memory[ProgramCounter + 1]);

    // the stack will be stored in ram from 0x00 to 0x4F. The stack pointer
    // will be stored at 0x1FB. This is the pointer, not the value.
    public byte StackPointer
    {
        get => _ram.Memory[0x1FB];
        set => _ram.Memory[0x1FB] = value;
    }
    public ushort StackValue => (ushort)((_ram.Memory[StackPointer] << 8) + _ram.Memory[StackPointer + 1]);

    // the delay timer is stored in ram at 0x01FA. This is the pointer, not
    // the value.
    public byte DelayTimer
    {
        get => _ram.Memory[0x01FA];
        set => _ram.Memory[0x01FA] = value;
    }

    // the sound timer is stored in ram at 0x01F9. This is the pointer, not 
    // the value.
    public byte SoundTimer
    {
        get => _ram.Memory[0x01F9];
        set => _ram.Memory[0x01F9] = value;
    }

    // hardware we care about
    private readonly FrameBuffer _frameBuffer;
    private readonly Keyboard _keyboard;
    private readonly Ram _ram;

    // the CPU state
    public CpuExecutionState ExecutionState { get; private set; } = CpuExecutionState.Running;

    public Cpu(FrameBuffer frameBuffer, Keyboard keyboard, Ram ram)
    {
        _frameBuffer = frameBuffer;
        _keyboard = keyboard;
        _ram = ram;
        Reset();
    }

    public void Reset()
    {
        Registers = new char[16];
        Index = 0x00;
        ProgramCounter = 0x0200;
        StackPointer = 0x00;
        DelayTimer = 0x00;
        SoundTimer = 0x00;
        ExecutionState = CpuExecutionState.Running;
    }

    public override void RenderVisual()
    {
        ImGui.Begin("CPU State");
        ImGui.Text("General Purpose Registers:");
        for (int i = 0; i < Registers.Length; i++)
        {
            ImGui.Text($"V{i:X}: 0x{(byte)Registers[i]:X2}");

            if ((i + 1) % 4 != 0) { ImGui.SameLine(); }
        }
        ImGui.Separator();
        // Special registers
        ImGui.Text($"Index Register  (");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(1f, 0.0f, 1f, 1f),
            $"0x01FE");
        ImGui.SameLine();
        ImGui.Text(") :");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(1f, 0.0f, 1f, 1f),
            $"0x{Index:X4}");

        ImGui.Text("Program Counter (");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(0.3f, 0.3f, 1f, 1f),
            $"0x01FC");
        ImGui.SameLine();
        ImGui.Text(") :");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(0.3f, 0.3f, 1f, 1f),
            $"0x{ProgramCounter:X4}");
        ImGui.SameLine();
        ImGui.Text("(");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(1f, 1f, 0f, 1f),
            $"0x{ProgramCounterValue:X4}");
        ImGui.SameLine();
        ImGui.Text(")");

        ImGui.Text($"Stack Pointer   (");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(0f, 1f, 1f, 1f),
            $"0x01FB");
        ImGui.SameLine();
        ImGui.Text(") :");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(0f, 1f, 1f, 1f),
            $"0x{StackPointer:X2}");
        ImGui.SameLine();
        ImGui.Text("(");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(0f, 1f, 1f, 1f),
            $"0x{StackValue:X4}");
        ImGui.SameLine();
        ImGui.Text(")");

        ImGui.Text($"Delay Timer     (");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(0f, 1f, 0f, 1f),
            $"0x01FA");
        ImGui.SameLine();
        ImGui.Text(") :");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(0f, 1f, 0f, 1f),
            $"0x{DelayTimer:X2}");

        ImGui.Text($"Sound Timer     (");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f),
            $"0x01F9");
        ImGui.SameLine();
        ImGui.Text(") :");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f),
            $"0x{SoundTimer:X2}");

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

    private void Step()
    {
        ExecutionState = CpuExecutionState.Stepping;
    }

    private void Run()
    {
        ExecutionState = CpuExecutionState.Running;
    }

    private void Pause()
    {
        ExecutionState = CpuExecutionState.Paused;
    }


    public static string Disassemble(ushort opcode)
    {
        ushort nnn = (ushort)(opcode & 0x0FFF);
        byte n = (byte)(opcode & 0x000F);
        byte x = (byte)((opcode & 0x0F00) >> 8);
        byte y = (byte)((opcode & 0x00F0) >> 4);
        byte kk = (byte)(opcode & 0x00FF);

        switch (opcode & 0xF000)
        {
            case 0x0000:
                return opcode == 0x00E0 ? "CLS" :
                       opcode == 0x00EE ? "RET" : $"SYS {nnn:X3}";

            case 0x1000: return $"JP {nnn:X3}";
            case 0x2000: return $"CALL {nnn:X3}";
            case 0x3000: return $"SE V{x:X}, {kk:X2}";
            case 0x4000: return $"SNE V{x:X}, {kk:X2}";
            case 0x5000: return $"SE V{x:X}, V{y:X}";
            case 0x6000: return $"LD V{x:X}, {kk:X2}";
            case 0x7000: return $"ADD V{x:X}, {kk:X2}";
            case 0x8000:
                return (opcode & 0x000F) switch
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
                };

            case 0x9000: return $"SNE V{x:X}, V{y:X}";
            case 0xA000: return $"LD I, {nnn:X3}";
            case 0xB000: return $"JP V0, {nnn:X3}";
            case 0xC000: return $"RND V{x:X}, {kk:X2}";
            case 0xD000: return $"DRW V{x:X}, V{y:X}, {n}";
            case 0xE000:
                return kk switch
                {
                    0x9E => $"SKP V{x:X}",
                    0xA1 => $"SKNP V{x:X}",
                    _ => $"UNKNOWN 0x{opcode:X4}"
                };

            case 0xF000:
                return kk switch
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
                };

            default: return $"UNKNOWN 0x{opcode:X4}";
        }
    }

}
