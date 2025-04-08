using System;
using System.Collections.Generic;
using System.Linq;
using AteChips.Interfaces;

namespace AteChips;
public class Machine
{
    private readonly List<IHardware> _devices = [];

    public IReadOnlyList<IVisualizable> Visualizables
    {
        get
        {
            if (_deviceListDirty) { RebuildCacheIfNeeded(); }
            return _visualizableCache!;
        }
    }

    public IReadOnlyList<IResettable> Resettables
    {
        get
        {
            if (_deviceListDirty) { RebuildCacheIfNeeded(); }
            return _resettableCache!;
        }
    }

    public IReadOnlyList<IUpdatable> Updatables
    {
        get
        {
            if (_deviceListDirty) { RebuildCacheIfNeeded(); }
            return _updatableCache!;
        }
    }

    public IReadOnlyList<IDrawable> Drawables
    {
        get
        {
            if (_deviceListDirty) { RebuildCacheIfNeeded(); }
            return _drawableCache!;
        }
    }

    private void RebuildCacheIfNeeded()
    {
        _visualizableCache = _devices.OfType<IVisualizable>().OrderBy(v => v.GetType().Name).ToList();
        _resettableCache = _devices.OfType<IResettable>().ToList();
        _updatableCache = _devices.OfType<IUpdatable>().OrderBy(u => u.UpdatePriority).ToList();
        _drawableCache = _devices.OfType<IDrawable>().ToList();
        _deviceListDirty = false;
    }

    private IReadOnlyList<IVisualizable>? _visualizableCache;
    private IReadOnlyList<IResettable>? _resettableCache;
    private IReadOnlyList<IUpdatable>? _updatableCache;
    private IReadOnlyList<IDrawable>? _drawableCache;
    private bool _deviceListDirty = true;


    private Machine()
    {
        FrameBuffer frameBuffer = Register<FrameBuffer>();
        Gpu gpu = Register(_ => new Gpu(frameBuffer));
        Register(_ => new Display(gpu, frameBuffer));
        Ram ram = Register<Ram>();
        Keyboard keyboard = Register<Keyboard>();
        Register(_ => new Buzzer(ram));
        Register(_ => new Cpu(frameBuffer, keyboard, ram));
    }

    public T Register<T>() where T : Hardware
    {
        T device = (T)Activator.CreateInstance(typeof(T))!;
        _devices.Add(device);
        _deviceListDirty = true;
        return device;
    }

    public T Register<T>(Func<Machine, T> factory) where T : Hardware
    {
        T instance = factory(this);
        _devices.Add(instance);
        _deviceListDirty = true;
        return instance;
    }
    public T Get<T>() where T : Hardware
    {
        return _devices.OfType<T>().Single();
    }

    public bool TryGet<T>(out T? result) where T : Hardware
    {
        result = _devices.OfType<T>().FirstOrDefault();
        return result != null;
    }

    public static Machine Instance { get; } = new ();


    public void Reset()
    {
        foreach (IResettable device in Resettables)
        {
            device.Reset();
        }
    }
}
