using System;
using Microsoft.Xna.Framework;
using AteChips.Interfaces;
using IDrawable = AteChips.Interfaces.IDrawable;

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
    private readonly Cpu _cpu;

    private bool firstTimeDebug = true;

    public Chip8(Machine machine)
    {
        _machine = machine;
        _keyboard = _machine.Get<Keyboard>();
        _frameBuffer = _machine.Get<FrameBuffer>();
        _display = _machine.Get<Display>();
        _ram = _machine.Get<Ram>();
        _cpu = _machine.Get<Cpu>();

    }

    public void LoadRom(string filePath) => _ram.LoadRom(filePath);

    // Called by MonoGame; this is the only part I couldn't move outside this method.
    protected override void LoadContent() => _display.LoadContent(GraphicsDevice, Content, this);

    // Update the game state
    protected override void Update(GameTime gameTime)
    {

        if (firstTimeDebug)
        {
            firstTimeDebug ^= true;
            // load the rom, and pause the CPU
            _cpu.Reset();
            _cpu.Pause();
            
            // maximize the window
            MonoGameManager.ToggleFullScreen();

            // show the ImGui windows we care about
            Settings.ShowImGui = true;
            _cpu.VisualShown = true;
            _ram.VisualShown = true;
        }

        // a frame has occured; we want to now emulate the next machine cycle.
        // todo: ExecutationState should be in the emulator settings, not the CPU.
        switch (_cpu.ExecutionState)
        {
            case Cpu.CpuExecutionState.Running:
                NextMachineCycle(gameTime);
                break;

            case Cpu.CpuExecutionState.Stepping:
                NextMachineCycle(gameTime);
                _cpu.Pause();
                break;

            case Cpu.CpuExecutionState.Paused:
                // Do nothing
                break;
        }

        /*Random r = new();
        _frameBuffer.TogglePixel(r.Next(0, 64), r.Next(0, 32));*/

    }

    private void NextMachineCycle(GameTime gameTime)
    {
        // the updatables list is already sorted by priority, so we can just
        // iterate through it and call each one.
        foreach (IUpdatable updatable in _machine.Updatables)
        {
            updatable.Update(gameTime);
        }
    }

    // Draw the game state
    protected override void Draw(GameTime gameTime)
    {

        // get all the drawables - in the chip8, this is just the display
        foreach (IDrawable drawable in _machine.Drawables)
        {
            drawable.Draw(gameTime);
        }
    }

}