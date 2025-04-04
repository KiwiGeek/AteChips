using System;
using AteChips.Interfaces;
using ImGuiNET;
using System.Numerics;

namespace AteChips;
public partial class Cpu : VisualizableHardware, IResettable, ICpu
{

    public enum CpuExecutionState
    {
        Running,
        Paused,
        Stepping
    }

    // CPU registers
    public char[] Registers { get; private set; } = null!;

    public ushort IndexRegister
    {
        get => _ram.GetUInt16(Ram.INDEX_REGISTER_ADDR);
        set => _ram.SetUInt16(Ram.INDEX_REGISTER_ADDR, value);
    }

    // the program counter is stored in RAM at 0x1FC
    public ushort ProgramCounter
    {
        get => _ram.GetUInt16(Ram.PROGRAM_COUNTER_ADDR);
        set => _ram.SetUInt16(Ram.PROGRAM_COUNTER_ADDR, value);
    }

    // the stack will be stored in ram from 0x00 to 0x4F. The stack pointer
    // will be stored at 0x1FB. This is the pointer, not the value.
    public byte StackPointer
    {
        get => _ram.GetByte(Ram.STACK_POINTER_ADDR);
        set => _ram.SetByte(Ram.STACK_POINTER_ADDR, value);
    }

    // the delay timer is stored in ram at 0x01FA. This is the pointer, not
    // the value.
    public byte DelayTimer
    {
        get => _ram.GetByte(Ram.DELAY_TIMER_ADDR);
        set => _ram.SetByte(Ram.DELAY_TIMER_ADDR, value);
    }

    // the sound timer is stored in ram at 0x01F9. This is the pointer, not 
    // the value.
    public byte SoundTimer
    {
        get => _ram.GetByte(Ram.SOUND_TIMER_ADDR);
        set => _ram.SetByte(Ram.SOUND_TIMER_ADDR, value);
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
        IndexRegister = 0x00;
        ProgramCounter = 0x0200;
        StackPointer = 0x00;
        DelayTimer = 0x00;
        SoundTimer = 0x00;
        ExecutionState = CpuExecutionState.Running;
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



}
