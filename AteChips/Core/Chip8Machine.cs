using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AteChips.Core.Framebuffer;
using AteChips.Core.Keypad;
using AteChips.Core.Shared;
using AteChips.Core.Shared.Video;
using AteChips.Shared.Interfaces;

namespace AteChips;
public class Chip8Machine : IEmulatedMachine
{

    private readonly List<IHardware> _devices = [];

    public DisplayCharacteristics DisplaySpec { get; } = new(1.5f);

    public IReadOnlyList<IVisualizable> Visualizables { get; }
    public IReadOnlyList<IResettable> Resettables { get; }
    public IReadOnlyList<IUpdatable> Updatables { get; }

    private readonly FrameBuffer _frameBuffer;
    private readonly Ram _ram;
    private readonly Keyboard _keyboard;
    private readonly Buzzer _buzzer;
    private readonly Cpu _cpu;

    public Chip8Machine()
    {
        _frameBuffer = Register<FrameBuffer>();         // relies on nothing
        _ram = Register<Ram>();                         // relies on IEmulatedMachine
        _keyboard = Register<Keyboard>();               // relies on nothing
        _buzzer = Register<Buzzer>();                   // relies on IRam
        _cpu = Register<Cpu>();                         // relies on IFrameBuffer, IKeyboard and IRam

        // build the device lists.
        Resettables = _devices.OfType<IResettable>().ToList();
        Updatables = _devices.OfType<IUpdatable>().OrderBy(u => u.UpdatePriority).ToList();
        Visualizables = _devices.OfType<IVisualizable>().ToList();
    }

    private T Register<T>(params object[]? constructorArgs) where T : IHardware
    {

        Type type = typeof(T);
        ConstructorInfo? matchingConstructor = null;

        if (constructorArgs is { Length: > 0 })
        {
            // Try to find a constructor that exactly matches the argument types
            foreach (ConstructorInfo ctor in type.GetConstructors())
            {
                ParameterInfo[] parameters = ctor.GetParameters();
                if (parameters.Length != constructorArgs.Length) { continue; }

                bool match = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (constructorArgs[i] == null)
                    {
                        // Allow null if the parameter type is a reference type or nullable
                        if (parameters[i].ParameterType.IsValueType && Nullable.GetUnderlyingType(parameters[i].ParameterType) == null)
                        {
                            match = false;
                            break;
                        }
                    }
                    else if (!parameters[i].ParameterType.IsInstanceOfType(constructorArgs[i]))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    matchingConstructor = ctor;
                    break;
                }
            }

            if (matchingConstructor != null)
            {
                T instance = (T)matchingConstructor.Invoke(constructorArgs);
                _devices.Add(instance);
                return instance;
            }
        }

        // Fallback: auto-inject known dependencies
        ConstructorInfo fallbackConstructor = type.GetConstructors().First();
        ParameterInfo[] fallbackParams = fallbackConstructor.GetParameters();
        object?[] constructorParameters = new object?[fallbackParams.Length];

        for (int i = 0; i < fallbackParams.Length; i++)
        {
            Type paramType = fallbackParams[i].ParameterType;

            constructorParameters[i] = paramType.Name switch
            {
                nameof(IEmulatedMachine) => this,
                nameof(FrameBuffer) => _frameBuffer,
                nameof(Ram) => _ram,
                nameof(Keyboard) => _keyboard,
                nameof(Buzzer) => _buzzer,
                nameof(Cpu) => _cpu,
                _ => throw new InvalidOperationException("can't resolve dependency")
            };

        }

        T fallbackInstance = (T)Activator.CreateInstance(type, constructorParameters)!;
        _devices.Add(fallbackInstance);
        return fallbackInstance;
    }

    public T Get<T>() where T : IHardware => _devices.OfType<T>().Single();
    public IEnumerable<T> GetAll<T>() where T : IHardware => _devices.OfType<T>();

    public void Reset()
    {
        foreach (IResettable device in Resettables)
        {
            device.Reset();
        }
    }
}
