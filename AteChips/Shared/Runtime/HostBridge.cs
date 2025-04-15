using System;
using System.Collections.Generic;
using System.Linq;

namespace AteChips.Shared.Runtime;

public class HostBridge : IHostBridge
{
    private readonly Dictionary<Type, List<IHostService>> _services = new();
    private readonly object _lock = new();

    public void Register<T>(T service) where T : IHostService
    {
        ArgumentNullException.ThrowIfNull(service);

        Type type = typeof(T);

        lock (_lock)
        {
            if (!_services.TryGetValue(type, out List<IHostService>? list))
            {
                list = new List<IHostService>();
                _services[type] = list;
            }

            list.Add(service);
        }
    }

    public T? Get<T>() where T : IHostService
    {
        lock (_lock)
        {
            if (_services.TryGetValue(typeof(T), out List<IHostService>? list))
            {
                return list.OfType<T>().FirstOrDefault();
            }
        }

        return default;
    }

    public IEnumerable<T> GetAll<T>() where T : IHostService
    {
        lock (_lock)
        {
            if (_services.TryGetValue(typeof(T), out List<IHostService>? list))
            {
                return list.OfType<T>().ToList();
            }
        }

        return Enumerable.Empty<T>();
    }
}