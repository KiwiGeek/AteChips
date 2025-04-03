using System;
using Microsoft.Xna.Framework;
using static AteChips.Cpu;

namespace AteChips;
public class Chip8 : Game
{
    // these are just helper methods for the various bits of hardware to access each other
    // they're pulled from the machine instance.
    private readonly Machine _machine;
    private readonly Keyboard _keyboard;
    private readonly FrameBuffer _frameBuffer;
    private readonly Display _display;
    private readonly Ram _ram;

    public Chip8(Machine machine)
    {
        _machine = machine;
        _keyboard = _machine.Get<Keyboard>();
        _frameBuffer = _machine.Get<FrameBuffer>();
        _display = _machine.Get<Display>();
        _ram = _machine.Get<Ram>();
    }

    public void LoadRom(string filePath) => _ram.LoadRom(filePath);

    // Called by MonoGame; this is the only part I couldn't move outside this method.
    protected override void LoadContent() => _display.LoadContent(GraphicsDevice, Content, this);

    // Update the game state
    protected override void Update(GameTime gameTime)
    {

        //switch (ExecutionState)
        //{
        //    case CpuExecutionState.Running:
        //        ExecuteNextInstruction();
        //        break;

        //    case CpuExecutionState.Stepping:
        //        ExecuteNextInstruction();
        //        ExecutionState = CpuExecutionState.Paused;
        //        break;

        //    case CpuExecutionState.Paused:
        //        // Do nothing
        //        break;
        //}



        _keyboard.Update();

        Random r = new();
        _frameBuffer.TogglePixel(r.Next(64), r.Next(32));
    }

    // Draw the game state
    protected override void Draw(GameTime gameTime)
    {
        _display.Draw(gameTime);
    }

}