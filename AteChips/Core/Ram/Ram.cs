using System;
using AteChips.Core.Shared.Base;
using AteChips.Core.Shared.Interfaces;

namespace AteChips.Core;

public partial class Ram : VisualizableHardware, IRam
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

    public const ushort STACK_POINTER_ADDR = 0x1FB;
    public const ushort PROGRAM_COUNTER_ADDR = 0x1FC;
    public const ushort PROGRAM_COUNTER_UPPER_ADDR = PROGRAM_COUNTER_ADDR;
    public const ushort PROGRAM_COUNTER_LOWER_ADDR = PROGRAM_COUNTER_ADDR + 1;
    public const ushort INDEX_REGISTER_ADDR = 0x1FE;
    public const ushort INDEX_REGISTER_UPPER_ADDR = INDEX_REGISTER_ADDR;
    public const ushort INDEX_REGISTER_LOWER_ADDR = INDEX_REGISTER_ADDR + 1;
    public const ushort DELAY_TIMER_ADDR = 0x01FA;
    public const ushort SOUND_TIMER_ADDR = 0x01F9;

    private readonly IEmulatedMachine _machine;
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

    public Ram(IEmulatedMachine machine)
    {
        _machine = machine;
        Reset();
    }

    public ushort GetUInt16(ushort address) => (ushort)((ushort)(Memory[address % 4096] << 8) + Memory[(address + 1) % 4096]);
    public void SetUInt16(ushort address, ushort value)
    {
        Memory[address] = (byte)(value >> 8);
        Memory[address + 1] = (byte)(value & 0x00FF);
    }

    public byte GetByte(ushort address) => Memory[address];
    public void SetByte(ushort address, byte value) => Memory[address] = value;
    public ref byte GetByteRef(ushort address) => ref Memory[address];

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
