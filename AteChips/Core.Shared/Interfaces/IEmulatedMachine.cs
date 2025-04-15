using System.Collections.Generic;

namespace AteChips.Core.Shared.Interfaces;

public interface IEmulatedMachine
{
    DisplayCharacteristics DisplaySpec { get; }
    IReadOnlyList<IVisualizable> Visualizables { get; }
    IReadOnlyList<IResettable> Resettables { get; }
    IReadOnlyList<IUpdatable> Updatables { get; }

    T Get<T>() where T : IHardware;
    IEnumerable<T> GetAll<T>() where T : IHardware;

    

    void Reset();
}