using System.Collections.Concurrent;

namespace AlirezaMahDev.Extensions.DataManager;

class DataManager : IDisposable
{
    private readonly ConcurrentDictionary<string, Lazy<DataAccess>> _cache = [];
    private bool _disposedValue;

    DataAccess Open(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return OpenLazy(path).Value;
    }

    Lazy<DataAccess> OpenLazy(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return _cache.GetOrAdd(path,
            static (key, arg) =>
                new(() =>
                        new(key),
                    LazyThreadSafetyMode.ExecutionAndPublication),
            this);
    }

    bool Close(string path)
    {
        if (!_cache.TryRemove(path, out var dataAccess))
        {
            return false;
        }

        if (!dataAccess.IsValueCreated)
        {
            return true;
        }

        dataAccess.Value.Dispose();
        return true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                foreach (var dataAccess in _cache.Values)
                {
                    if (dataAccess.IsValueCreated)
                    {
                        dataAccess.Value.Dispose();
                    }
                }

                _cache.Clear();
            }

            _disposedValue = true;
        }
    }

    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}