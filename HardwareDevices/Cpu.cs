using System;
using AteChips.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace AteChips;

public partial class Cpu : VisualizableHardware, ICpu
{

    // todo: adjustable hertz
    // todo: quirks mode

    private double _cpuAccumulator = 0;
    private double _timerAccumulator = 0;

    // frequencies (in Hz)
    private const double CPU_HZ = 1400;
    private const double TIMER_HZ = 60;

    public enum CpuExecutionState
    {
        Running,
        Paused,
        Stepping
    }

    private bool _waitingForKey = false;
    private byte _waitingRegister = 0;
    private byte? _pressedKey = null; // track which key was pressed


    // CPU registers
    public byte[] Registers { get; private set; } = null!;

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

    public ref byte GetSoundTimerRef() => ref _ram.Memory[Ram.SOUND_TIMER_ADDR];

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
        Registers = new byte[16];
        IndexRegister = 0x00;
        ProgramCounter = 0x0200;
        StackPointer = 0x00;
        DelayTimer = 0x00;
        SoundTimer = 0x00;
        ExecutionState = CpuExecutionState.Running;
    }

    public void Step()
    {
        ExecutionState = CpuExecutionState.Stepping;
    }

    public void Run()
    {
        ExecutionState = CpuExecutionState.Running;
    }

    public void Pause()
    {
        ExecutionState = CpuExecutionState.Paused;
    }


    public void Update(GameTime gameTime)
    {

        double elapsed = gameTime.ElapsedGameTime.TotalSeconds;

        _cpuAccumulator += elapsed;
        _timerAccumulator += elapsed;

        double cpuStep = 1.0 / CPU_HZ;
        double timerStep = 1.0 / TIMER_HZ;

        // Run CPU steps as often as needed
        while (_cpuAccumulator >= cpuStep)
        {
            Tick();
            _cpuAccumulator -= cpuStep;
        }

        while (_timerAccumulator >= timerStep)
        {
            if (DelayTimer > 0) { DelayTimer -= 1; }
            _timerAccumulator -= timerStep;
        }

    }

    void Tick()
    {

        if (_waitingForKey)
        {
            // Step 1: Look for new key press
            if (_pressedKey is null)
            {
                for (byte i = 0; i < 16; i++)
                {
                    if (_keyboard.Keypad[i] == Keyboard.KeyState.Pressed)
                    {
                        _pressedKey = i;
                        Registers[_waitingRegister] = i;
                        break;
                    }
                }

                return; // still waiting, don’t execute anything else
            }

            // Step 2: Wait for that specific key to be released
            if (_keyboard.Keypad[_pressedKey.Value] == Keyboard.KeyState.Up)
            {
                _waitingForKey = false;
                _pressedKey = null;
            }

            return; // still waiting
        }

        ushort instruction = Fetch();
        Action command = Decode(instruction);
        command.Invoke();
    }

    private ushort Fetch()
    {
        // fetch the next instruction
        ushort instruction = _ram.GetUInt16(ProgramCounter);
        ProgramCounter += 2;
        return instruction;
    }

    private Action Decode(ushort instruction)
    {
        if (instruction == 0x00E0) { return ClearDisplay(); }
        if (instruction == 0x00EE) { return Return(); }
        if ((instruction & 0xF000) == 0x0000) { return System(); }
        if ((instruction & 0xF000) == 0x1000) { return Jump((ushort)(instruction & 0x0FFF)); }
        if ((instruction & 0xF000) == 0x2000) { return Call((ushort)(instruction & 0x0FFF)); }
        if ((instruction & 0xF000) == 0x3000) { return SkipEqual(instruction); }
        if ((instruction & 0xF000) == 0x4000) { return SkipNotEqual(instruction); }
        if ((instruction & 0xF000) == 0x5000) { return SkipRegistersEqual(instruction); }
        if ((instruction & 0xF000) == 0x6000) { return LoadRegister((byte)((instruction & 0x0F00) >> 8), (byte)(instruction & 0x00FF)); }
        if ((instruction & 0xF000) == 0x7000) { return AddImmediate(instruction); }
        if ((instruction & 0xF00F) == 0x8000) { return LoadRegisterFromRegister(instruction); }
        if ((instruction & 0xF00F) == 0x8001) { return Or(instruction); }
        if ((instruction & 0xF00F) == 0x8002) { return And(instruction); }
        if ((instruction & 0xF00F) == 0x8003) { return Xor(instruction); }
        if ((instruction & 0xF00F) == 0x8004) { return Add(instruction); }
        if ((instruction & 0xF00F) == 0x8005) { return Sub(instruction); }
        if ((instruction & 0xF00F) == 0x8006) { return ShiftRight(instruction); }
        if ((instruction & 0xF00F) == 0x8007) { return SubNoBorrow(instruction); }
        if ((instruction & 0xF00F) == 0x800E) { return ShiftLeft(instruction); }
        if ((instruction & 0xF000) == 0x9000) { return SkipRegistersNotEqual((ushort)(instruction & 0x0FFF)); }
        if ((instruction & 0xF000) == 0xA000) { return LoadIndex((ushort)(instruction & 0x0FFF)); }
        if ((instruction & 0xF000) == 0xB000) { return JumpRegisterRelative(instruction); }
        if ((instruction & 0xF000) == 0xC000) { return RandomByte(instruction); }
        if ((instruction & 0xF000) == 0xD000) { return Draw(instruction); }
        if ((instruction & 0xF0FF) == 0xE09E) { return SkipOnKeypadPressed(instruction); }
        if ((instruction & 0xF0FF) == 0xE0A1) { return SkipOnKeypadNotPressed(instruction); }
        if ((instruction & 0xF0FF) == 0xF007) { return LoadRegisterFromDelayTimer(instruction); }
        if ((instruction & 0xF0FF) == 0xF00A) { return WaitForKeypress(instruction); }
        if ((instruction & 0xF0FF) == 0xF015) { return LoadDelayTimer(instruction); }
        if ((instruction & 0xF0FF) == 0xF018) { return LoadSoundTimer(instruction); }
        if ((instruction & 0xF0FF) == 0xF01E) { return AddRegisterToIndex(instruction); }
        if ((instruction & 0xF0FF) == 0xF029) { return IndexToFontSprite(instruction); }
        if ((instruction & 0xF0FF) == 0xF033) { return BinaryCodedDecimal(instruction); }
        if ((instruction & 0xF0FF) == 0xF055) { return StoreMultipleRegisters(instruction); }
        if ((instruction & 0xF0FF) == 0xF065) { return LoadMultipleRegisters(instruction); }

        throw new NotImplementedException($"Instruction {instruction:X4} not implemented.");
    }

    public byte UpdatePriority => 2;

    Action ClearDisplay() => () => _frameBuffer.Reset();

    Action System() => () => { /* no op */ };

    Action LoadIndex(ushort address) => () => IndexRegister = address;

    Action LoadRegister(byte registerIndex, byte value) => () => Registers[registerIndex] = value;

    Action Draw(ushort instruction)
    {
        return () =>
        {

            // the instruction is in the format 0xDXYN
            // the sprites are 8 bits wide and n bits tall.
            int vx = Registers[((instruction & 0x0F00) >> 8)];
            int vy = Registers[((instruction & 0x00F0) >> 4)];
            int height = (ushort)(instruction & 0x000F);

            // modulo wrap the x and y coordinates, and reset the erase register
            vx %= _frameBuffer.Width;
            vy %= _frameBuffer.Height;
            Registers[0xF] = 0x00;

            for (int row = 0; row < height; row++)
            {
                // get the sprite data from memory
                byte spriteByte = _ram.GetByte((ushort)(IndexRegister + row));
                for (int col = 0; col < 8; col++)
                {
                    // skip the pixel if it is not set
                    if ((spriteByte & (0x80 >> col)) == 0) { continue; }

                    if (vx + col >= _frameBuffer.Width) { continue; }
                    if (vy + row >= _frameBuffer.Height) { continue; }
                    Registers[0xF] |= (byte)(_frameBuffer.TogglePixel(vx + col, vy + row, out bool _) ? 1 : 0);
                }
            }
        };
    }

    Action AddImmediate(ushort instruction) => () => Registers[(instruction & 0x0F00) >> 8] += (byte)(instruction & 0x00FF);

    Action Jump(ushort address) => () => ProgramCounter = address;

    Action SkipEqual(ushort instruction) => () =>
    {
        // skip the next instruction if the register is equal to the value
        if (Registers[(instruction & 0x0F00) >> 8] == (byte)(instruction & 0x00FF))
        {
            ProgramCounter += 2;
        }
    };

    Action SkipNotEqual(ushort instruction) => () =>
    {
        // skip the next instruction if the register is not equal to the value
        if (Registers[(instruction & 0x0F00) >> 8] != (byte)(instruction & 0x00FF))
        {
            ProgramCounter += 2;
        }
    };

    Action SkipRegistersEqual(ushort instruction) => () =>
    {
        // skip the next instruction if the registers are equal
        if (Registers[(instruction & 0x0F00) >> 8] == Registers[(instruction & 0x00F0) >> 4])
        {
            ProgramCounter += 2;
        }
    };

    Action SkipRegistersNotEqual(ushort instruction) => () =>
    {
        // skip the next instruction if the registers are not equal
        if (Registers[(instruction & 0x0F00) >> 8] != Registers[(instruction & 0x00F0) >> 4])
        {
            ProgramCounter += 2;
        }
    };

    Action Call(ushort address) => () =>
    {
        // pop the current program counter onto the stack
        _ram.SetUInt16(StackPointer, ProgramCounter);
        StackPointer += 2;
        // jump to the address
        ProgramCounter = address;
    };

    Action Return() => () =>
    {
        // pop the program counter from the stack
        StackPointer -= 2;
        ProgramCounter = _ram.GetUInt16(StackPointer);
    };

    Action LoadRegisterFromRegister(ushort instruction) => () =>
    {
        // load the value of the register into the other register
        byte destination = (byte)((instruction & 0x0F00) >> 8);
        byte source = (byte)((instruction & 0x00F0) >> 4);
        Registers[destination] = Registers[source];
    };

    Action Or(ushort instruction) => () =>
    {
        // bitwise or the two registers
        byte destination = (byte)((instruction & 0x0F00) >> 8);
        byte source = (byte)((instruction & 0x00F0) >> 4);
        Registers[destination] |= Registers[source];
    };

    Action And(ushort instruction) => () =>
    {
        // bitwise or the two registers
        byte destination = (byte)((instruction & 0x0F00) >> 8);
        byte source = (byte)((instruction & 0x00F0) >> 4);

        Registers[destination] &= Registers[source];
    };

    Action Xor(ushort instruction) => () =>
    {
        // bitwise or the two registers
        byte destination = (byte)((instruction & 0x0F00) >> 8);
        byte source = (byte)((instruction & 0x00F0) >> 4);
        Registers[destination] ^= Registers[source];
    };

    Action Add(ushort instruction) => () =>
    {
        // add the two registers
        byte destination = (byte)((instruction & 0x0F00) >> 8);
        byte source = (byte)((instruction & 0x00F0) >> 4);

        ushort result = (ushort)(Registers[destination] + Registers[source]);

        // add the registers, and mod 256
        Registers[destination] = (byte)(result & 0xFF); // wrap naturally

        // set the carry flag if the result is greater than 255
        Registers[0xF] = (result > 255) ? (byte)1 : (byte)0;
    };

    Action Sub(ushort instruction) => () =>
    {
        // subtract the two registers
        byte x = (byte)((instruction & 0x0F00) >> 8);
        byte y = (byte)((instruction & 0x00F0) >> 4);

        byte vx = Registers[x];
        byte vy = Registers[y];

        byte carry = (vx >= vy) ? (byte)1 : (byte)0;

        Registers[x] = (byte)((vx - vy) & 0xFF);
        Registers[0x0F] = carry;
    };

    Action SubNoBorrow(ushort instruction) => () =>
    {
        byte x = (byte)((instruction & 0x0F00) >> 8);
        byte y = (byte)((instruction & 0x00F0) >> 4);

        byte vx = Registers[x];
        byte vy = Registers[y];

        byte carry = (vy >= vx) ? (byte)1 : (byte)0;

        Registers[x] = (byte)((vy - vx) & 0xFF);
        Registers[0xF] = carry;
    };

    Action ShiftRight(ushort instruction) => () =>
    {
        // shift the register right
        byte destination = (byte)((instruction & 0x0F00) >> 8);
        byte carry = (byte)(Registers[destination] & 0x01);
        Registers[destination] = (byte)(Registers[destination] >> 1);
        Registers[0x0F] = carry;
    };

    Action ShiftLeft(ushort instruction) => () =>
    {
        // shift the register left
        byte destination = (byte)((instruction & 0x0F00) >> 8);
        byte value = Registers[destination];
        byte carry = (byte)((value & 0x80) >> 7); // MSB becomes carry (1 if set)
        Registers[destination] = (byte)(value << 1);
        Registers[0x0F] = carry;
    };

    Action LoadMultipleRegisters(ushort instruction) => () =>
    {
        // load the registers from memory
        byte count = (byte)((instruction & 0x0F00) >> 8);
        for (int i = 0; i <= count; i++)
        {
            Registers[i] = _ram.GetByte((ushort)(IndexRegister + i));
        }
    };

    Action StoreMultipleRegisters(ushort instruction) => () =>
    {
        // store the registers to memory
        byte count = (byte)((instruction & 0x0F00) >> 8);
        for (int i = 0; i <= count; i++)
        {
            _ram.SetByte((ushort)(IndexRegister + i), Registers[i]);
        }
    };

    Action BinaryCodedDecimal(ushort instruction) => () =>
    {
        // store the BCD value of the register in memory
        byte register = (byte)((instruction & 0x0F00) >> 8);
        byte value = Registers[register];
        _ram.SetByte(IndexRegister, (byte)(value / 100));
        _ram.SetByte((ushort)(IndexRegister + 1), (byte)((value / 10) % 10));
        _ram.SetByte((ushort)(IndexRegister + 2), (byte)(value % 10));
    };

    Action AddRegisterToIndex(ushort instruction) => () =>
    {
        // add the register to the index
        byte register = (byte)((instruction & 0x0F00) >> 8);
        IndexRegister += Registers[register];
    };

    Action LoadDelayTimer(ushort instruction) => () =>
    {
        byte register = (byte)((instruction & 0x0F00) >> 8);
        DelayTimer = Registers[register];
    };

    Action LoadRegisterFromDelayTimer(ushort instruction) => () =>
    {
        byte register = (byte)((instruction & 0x0F00) >> 8);
        Registers[register] = DelayTimer;
    };

    Action SkipOnKeypadPressed(ushort instruction) => () =>
    {
        // skip the next instruction if the key is pressed
        byte register = (byte)((instruction & 0x0F00) >> 8);
        if (_keyboard.Keypad[Registers[register]] is Keyboard.KeyState.Down or Keyboard.KeyState.Pressed)
        {
            ProgramCounter += 2;
        }
    };

    Action SkipOnKeypadNotPressed(ushort instruction) => () =>
    {
        // skip the next instruction if the key is not pressed
        byte register = (byte)((instruction & 0x0F00) >> 8);
        if (_keyboard.Keypad[Registers[register]] is Keyboard.KeyState.Up or Keyboard.KeyState.Released)
        {
            ProgramCounter += 2;
        }
    };

    Action WaitForKeypress(ushort instruction) => () =>
    {
        byte x = (byte)((instruction & 0x0F00) >> 8);

        _waitingForKey = true;
        _waitingRegister = x;
    };

    Action JumpRegisterRelative(ushort instruction) => () =>
    {
        // jump to the address + the value of the register
        ushort address = (ushort)(instruction & 0x0FFF);
        ProgramCounter = (ushort)(address + Registers[0]);
    };

    Action RandomByte(ushort instruction) => () =>
    {
        // generate a random byte and AND it with the value
        byte register = (byte)((instruction & 0x0F00) >> 8);
        byte value = (byte)(instruction & 0x00FF);
        Random random = new Random();
        Registers[register] = (byte)(random.Next(0, 256) & value);
    };

    Action IndexToFontSprite(ushort instruction) => () =>
    {
        // set the index to the font sprite
        byte register = (byte)((instruction & 0x0F00) >> 8);
        IndexRegister = (ushort)(Ram.FontStartAddress + Registers[register] * 5);
    };

    Action LoadSoundTimer(ushort instruction) => () =>
    {
        // load the sound timer
        byte register = (byte)((instruction & 0x0F00) >> 8);
        SoundTimer = Registers[register];
    };
}

