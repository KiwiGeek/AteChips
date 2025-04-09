using System.Diagnostics;
using System.Linq;
using AteChips.Host.Video;
using System.Collections.Generic;
using AteChips.Core;
using AteChips.Core.Shared.Interfaces;

namespace AteChips.Host.Runtime;
public class EmulatorRuntime
{
    private readonly Display _display;
    private readonly IUpdatable[] _updatables;
    private readonly IDrawable[] _drawables;
    private readonly bool _singleDrawable;
    private readonly FrameBufferRenderer _gpu;
    private readonly List<IVisualizable> _visuals = [];
    public IEnumerable<IVisualizable> Visuals => _visuals;

    public EmulatorRuntime(IEmulatedMachine emulatedMachine)
    {

        // Extract the framebuffer from the emulated video memory
        FrameBuffer framebuffer = emulatedMachine.Get<FrameBuffer>();
        _gpu = new FrameBufferRenderer(framebuffer);      // Core-level GPU

        _display = new Display(emulatedMachine);
        _display.Connect(_gpu.GetOutputs().First()); // Hook up video output

        _drawables = [_display];
        _singleDrawable = _drawables.Length == 1;

        _updatables = emulatedMachine.Updatables.ToArray();

        // build the visuals list
        foreach (IVisualizable visualizable in emulatedMachine.Visualizables.ToList())
        {
            _visuals.Add(visualizable);
        }
        _visuals.Add(_display);
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
        _gpu.Update();
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
