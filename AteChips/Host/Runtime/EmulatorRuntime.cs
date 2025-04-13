using AteChips.Core.Shared.Interfaces;
using AteChips.Core.Shared.Timing;
using AteChips.Host.Video;
using AteChips.Shared.Video;
using System;
using System.Diagnostics;
using AteChips.Core;
using AteChips.Host.Input;

namespace AteChips.Host.Runtime;
public class EmulatorRuntime
{
    private readonly TimingController _timing;

    public EmulatorRuntime(IEmulatedMachine emulatedMachine)
    {

        // Create the timing controller, and register all our hardware with it.
        _timing = new TimingController();
        foreach (IUpdatable updatable in emulatedMachine.Updatables)
        {
            _timing.Register(updatable);
        }

        // Create the display and connect it to the first video output. Manually register it
        // with the timing controller. For now, we just support one display.
        Display display = new(emulatedMachine);
        VideoOutputSignal signal = emulatedMachine.Get<IVideoCard>().GetPrimaryOutput();
        display.Connect(signal);
        _timing.Register(display);

        // Create the keyboard, and register it with the timing controller; it should happen
        // before the keypad, so that the keypad can use it.
        Keyboard keyboard = new (display, emulatedMachine.Get<Keypad>());
        _timing.Register(keyboard);

    }

    public void Run()
    {
        long previousTicks = Stopwatch.GetTimestamp();
        long frequency = Stopwatch.Frequency;

        bool done = false;

        while (!done)
        {
            // Calculate elapsed time since last frame
            long currentTicks = Stopwatch.GetTimestamp();
            double delta = (currentTicks - previousTicks) / (double)frequency;
            previousTicks = currentTicks;

            // Advance the global game time
            _timing.Tick(delta);

            // Get any IHertzDriven components that are due to run
            ReadOnlySpan<ScheduledTarget> dueNow = _timing.GetDueTargets();

            foreach (ScheduledTarget scheduledTarget in dueNow)
            {
                switch (scheduledTarget.Target)
                {
                    case IUpdatable updatable:
                        done |= updatable.Update(scheduledTarget.DeltaTime);
                        break;
                    case IDrawable drawable:
                        done |= drawable.Draw(_timing.GameTime);
                        break;
                }
            }
        }
    }
}
