using System.Collections.Generic;

namespace AteChips.Shared.Runtime;

public interface IHostBridge
{
    T? Get<T>() where T : IHostService;
    IEnumerable<T> GetAll<T>() where T : IHostService;
    void Register<T>(T service) where T : IHostService;
}