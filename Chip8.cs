using System.Diagnostics;
using System.Linq;
using AteChips.Interfaces;
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery

namespace AteChips;
class Chip8
{
    private readonly Display _display;
    private readonly IUpdatable[] _updatables;
    private readonly IDrawable[] _drawables;
    private readonly bool _singleDrawable;

    public Chip8(Machine machine)
    {
        _display = machine.Get<Display>();

        _drawables = machine.Drawables.ToArray();
        _singleDrawable = _drawables.Length == 1;

        _updatables = machine.Updatables.ToArray();
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
