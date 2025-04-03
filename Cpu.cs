using AteChips.Interfaces;

namespace AteChips;
public class Cpu : VisualizableHardware, IResettable, ICpu
{
    
    // CPU registers
    //public const int NUM_REGISTERS = 16;
    //public byte[] Registers { get; } = new byte[NUM_REGISTERS];
    //// program counter
    //public ushort ProgramCounter { get; set; } = 0x200;
    //// index register
    //public ushort IndexRegister { get; set; }
    //// stack
    //public Stack<ushort> Stack { get; } = new();
    //// delay timer
    //public byte DelayTimer { get; set; }
    //// sound timer
    //public byte SoundTimer { get; set; }
    // reset the CPU state
    public void Reset()
    {

    }

    // program counter
    // index register
    // stack 
    // delay timer
    // sound timer
    // registers

    public override void RenderVisual()
    {
        //throw new System.NotImplementedException();
    }

}
