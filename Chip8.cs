using System.Diagnostics;
using System.Linq;
using AteChips.Interfaces;

namespace AteChips;
class Chip8
{

    private readonly Machine _machine;
    private bool done = false;

    public Chip8(Machine machine)
    {
        _machine = machine;
    }

    public void Start()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        double previousTime = stopwatch.Elapsed.TotalSeconds;

        while (!done)
        {
            double currentTime = stopwatch.Elapsed.TotalSeconds;
            double delta = currentTime - previousTime;
            previousTime = currentTime;

            done = Update(delta);
            Render(delta);
        }
    }

    private bool Update(double delta)
    {
        bool allDone = false;
        foreach (IUpdatable updatable in _machine.Updatables.OrderBy(f => f.UpdatePriority))
        {
            if (updatable.Update(delta)) { allDone = true; }
        }

        return allDone;
    }

    private void Render(double delta)
    {

        foreach (IDrawable drawable in _machine.Drawables)
        {
            drawable.Draw(delta);
        }

    }

}
