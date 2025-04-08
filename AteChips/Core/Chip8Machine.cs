using System;
using System.Collections.Generic;
using System.Linq;
using AteChips.Core.Framebuffer;
using AteChips.Core.Keypad;
using AteChips.Host.Video;
using AteChips.Shared.Interfaces;

namespace AteChips;
public class Chip8Machine
{

    // todo: create an interface version of this (Core.Shared/IEmulatedMachine.cs) and the concrete implementation as Chip8Machine.cs in Core/Chip8Machine.
    // and then the Host.Runtime should receive an IEumlatedMachine rather than the concrete Chip8Machine.

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


    private void RebuildCacheIfNeeded()
    {
        _visualizableCache = _devices.OfType<IVisualizable>().OrderBy(v => v.GetType().Name).ToList();
        _resettableCache = _devices.OfType<IResettable>().ToList();
        _updatableCache = _devices.OfType<IUpdatable>().OrderBy(u => u.UpdatePriority).ToList();
        _drawableCache = _devices.OfType<IDrawable>().ToList();
    }

    private IReadOnlyList<IVisualizable>? _visualizableCache;
    private IReadOnlyList<IResettable>? _resettableCache;
    private IReadOnlyList<IUpdatable>? _updatableCache;
    private IReadOnlyList<IDrawable>? _drawableCache;
    private bool _deviceListDirty = true;


    private Chip8Machine()
    {
        FrameBuffer frameBuffer = Register<FrameBuffer>();
        Ram ram = Register<Ram>();
        Keyboard keyboard = Register<Keyboard>();
        Register(_ => new Buzzer(ram));
        Register(_ => new Cpu(frameBuffer, keyboard, ram));
    }

    public T Register<T>() where T : IHardware
    {
        T device = (T)Activator.CreateInstance(typeof(T))!;
        _devices.Add(device);
        _deviceListDirty = true;
        return device;
    }

    public T Register<T>(Func<Chip8Machine, T> factory) where T : IHardware
    {
        T instance = factory(this);
        _devices.Add(instance);
        _deviceListDirty = true;
        return instance;
    }
    public T Get<T>() where T : IHardware
    {
        return _devices.OfType<T>().Single();
    }

    public bool TryGet<T>(out T? result) where T : IHardware
    {
        result = _devices.OfType<T>().FirstOrDefault();
        return result != null;
    }

    public static Chip8Machine Instance { get; } = new ();


    public void Reset()
    {
        foreach (IResettable device in Resettables)
        {
            device.Reset();
        }
    }
}
