using System.Diagnostics;
using System.Linq;
using AteChips.Core.Framebuffer;
using AteChips.Host.Video;
using AteChips.Shared.Interfaces;

namespace AteChips.Host.Runtime;
class EmulatorRuntime
{
    private readonly Display _display;
    private readonly IUpdatable[] _updatables;
    private readonly IDrawable[] _drawables;
    private readonly bool _singleDrawable;

    public EmulatorRuntime(Chip8Machine emulatedMachine)
    {
        _display = new Display(emulatedMachine.Get<FrameBuffer>());

        _drawables = [_display];
        _singleDrawable = _drawables.Length == 1;

        _updatables = emulatedMachine.Updatables.ToArray();
    }

    public void Run()
    {
        long previousTicks = Stopwatch.GetTimestamp();
        long frequency = Stopwatch.Frequency;

        bool done = false;

        while (!done)
        {
            long currentTicks = Stopwatch.GetTimestamp();
            double delta = (currentTicks - previousTicks) / (double)frequency;
            previousTicks = currentTicks;

            done = Update(delta);
            Render(delta);
        }
    }

    private bool Update(double delta)
    {
        bool allDone = false;

        foreach (IUpdatable updatable in _updatables)
        {
            allDone |= updatable.Update(delta);
        }

        return allDone;
    }

    private void Render(double delta)
    {
        if (_singleDrawable)
        {
            _display.Draw(delta);
            return;
        }

        foreach (IDrawable drawable in _drawables)
        {
            drawable.Draw(delta);
        }
    }

}
