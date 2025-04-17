using AteChips.Core.Shared.Timing;
using AteChips.Host.Video;
using System;
using System.Collections.Generic;

namespace AteChips.Host.Runtime;

public sealed class TimingController
{
    private sealed class ScheduledEntry
    {
        public IHertzDriven Target { get; }
        public double Interval { get; } // In seconds
        public double LastRunTime { get; set; }

        public ScheduledEntry(IHertzDriven target, double interval)
        {
            Target = target;
            Interval = interval;
            LastRunTime = 0;
        }
    }

    private readonly List<ScheduledEntry> _scheduled = [];
    private ScheduledTarget[] _dueBuffer = new ScheduledTarget[32];
    private int _dueCount;
    private double _lastFrameTime;

    public double GameTime { get; private set; }
    public double DeltaTime => GameTime - _lastFrameTime;

    public void Register(IHertzDriven component)
    {
        _scheduled.Add(new ScheduledEntry(component, 1.0 / component.FrequencyHz));
    }

    public void Tick(double deltaTime)
    {
        _lastFrameTime = GameTime;
        GameTime += deltaTime;
    }

    public ReadOnlySpan<ScheduledTarget> GetDueTargets()
    {
        _dueCount = 0;

        foreach (ScheduledEntry entry in _scheduled)
        {
            double delta = GameTime - entry.LastRunTime;
            if (delta >= entry.Interval)
            {
                entry.LastRunTime = GameTime;

                if (_dueCount >= _dueBuffer.Length)
                {
                    Array.Resize(ref _dueBuffer, _dueBuffer.Length * 2);
                }

                _dueBuffer[_dueCount++] = new ScheduledTarget(entry.Target, delta);
            }
        }

        Array.Sort(_dueBuffer, 0, _dueCount, TimingComparer);
        return _dueBuffer.AsSpan(0, _dueCount);
    }

    private static IComparer<ScheduledTarget> TimingComparer { get; } = Comparer<ScheduledTarget>.Create((x,y) => (x.Target is IDrawable && y.Target is not IDrawable) ? 1 : (x.Target is not IDrawable && y.Target is IDrawable) ? -1 : x.Target.UpdatePriority.CompareTo(y.Target.UpdatePriority));
}
