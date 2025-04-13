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

        public bool IsDue(double gameTime) => (gameTime - LastRunTime) >= Interval;
    }

    private readonly List<ScheduledEntry> _scheduled = new();
    private ScheduledTarget[] _dueBuffer = new ScheduledTarget[32];
    private int _dueCount = 0;

    private double _gameTime;
    private double _lastFrameTime;

    public double GameTime => _gameTime;
    public double DeltaTime => _gameTime - _lastFrameTime;

    public void Register(IHertzDriven component)
    {
        _scheduled.Add(new ScheduledEntry(component, 1.0 / component.FrequencyHz));
    }

    public void Tick(double deltaTime)
    {
        _lastFrameTime = _gameTime;
        _gameTime += deltaTime;
    }

    public ReadOnlySpan<ScheduledTarget> GetDueTargets()
    {
        _dueCount = 0;

        foreach (var entry in _scheduled)
        {
            double delta = _gameTime - entry.LastRunTime;
            if (delta >= entry.Interval)
            {
                entry.LastRunTime = _gameTime;

                if (_dueCount >= _dueBuffer.Length)
                    Array.Resize(ref _dueBuffer, _dueBuffer.Length * 2);

                _dueBuffer[_dueCount++] = new ScheduledTarget(entry.Target, delta);
            }
        }

        Array.Sort(_dueBuffer, 0, _dueCount, CompositeComparer.Instance);
        return _dueBuffer.AsSpan(0, _dueCount);
    }

    private sealed class CompositeComparer : IComparer<ScheduledTarget>
    {
        public static readonly CompositeComparer Instance = new();

        public int Compare(ScheduledTarget x, ScheduledTarget y)
        {
            bool xIsDrawable = x.Target is IDrawable;
            bool yIsDrawable = y.Target is IDrawable;

            if (xIsDrawable && !yIsDrawable) return 1;
            if (!xIsDrawable && yIsDrawable) return -1;

            return x.Target.UpdatePriority.CompareTo(y.Target.UpdatePriority);
        }
    }
}
