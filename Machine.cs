using System;
using System.Collections.Generic;
using System.Linq;
using AteChips.Interfaces;

namespace AteChips;
public class Machine
{
    private readonly List<IHardware> _devices = [];
    
    // ReSharper disable once UnusedMember.Global
    public IReadOnlyList<Hardware> Devices => [.. _devices.Cast<Hardware>()];

    public IReadOnlyList<IVisualizable> Visualizables => 
        [.. _devices.OfType<IVisualizable>().OrderBy(visual => visual.GetType().Name)];

    public IReadOnlyList<IResettable> Resettables =>
           [.. _devices.OfType<IResettable>()];

    private Machine()
    {
        _frameBuffer = Register<FrameBuffer>();
        _display = Register(_ => new Display(_frameBuffer));
        _ram = Register<Ram>();
        _keyboard = Register<Keyboard>();
        _cpu = Register(_ => new Cpu(_frameBuffer, _keyboard, _ram));
        Reset();
    }

    public T Register<T>() where T : Hardware
    {
        T device = (T)Activator.CreateInstance(typeof(T))!;
        _devices.Add(device);
        return device;
    }

    public T Register<T>(Func<Machine, T> factory) where T : Hardware
    {
        T instance = factory(this);
        _devices.Add(instance);
        return instance;
    }

    //public void Deregister<T>() where T : Hardware
    //{
    //    _devices.RemoveAll(f=> f.GetType() == typeof(T));
    //}

    public T Get<T>() where T : Hardware
    {
        return _devices.OfType<T>().Single();
    }

    //public bool TryGet<T>(out T? result) where T : Hardware
    //{
    //    result = _devices.OfType<T>().FirstOrDefault();
    //    return result != null;
    //}

    public static Machine Instance { get; } = new ();

    // this is just a static class so we can reference various bits of hardware from other bits
    // of hardware, without having to pass references around.
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly FrameBuffer _frameBuffer;
    // ReSharper disable NotAccessedField.Local
    private readonly Cpu _cpu;
    private readonly Ram _ram;
    private readonly Keyboard _keyboard;
    private readonly Display _display;
    // ReSharper restore NotAccessedField.Local

    public void Reset()
    {
        foreach (IResettable device in Resettables)
        {
            device.Reset();
        }
    }
}
