using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using Microsoft.Win32.SafeHandles;

namespace AlirezaMahDev.Extensions.DataManager;

class DataAccess : IDisposable, IDataAccess
{
    public string Path { get; }
    private bool _disposedValue;

    private readonly ConcurrentDictionary<long, DataMemory> _cache = [];
    private long _length;

    private readonly SafeFileHandle _safeFileHandle;

    public DataAccess(string path)
    {
        Path = path;
        _safeFileHandle = File.OpenHandle(
            System.IO.Path.Combine(Environment.CurrentDirectory, path),
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None);
        _length = RandomAccess.GetLength(_safeFileHandle);
        if (_length == 0)
        {
            this.Create<DataPath>();
        }
    }

    public DataLocation<DataPath> GetRoot() =>
        this.Read<DataPath>(0);

    public async ValueTask<DataLocation<DataPath>> GetRootAsync(CancellationToken cancellationToken = default) =>
        await this.ReadAsync<DataPath>(0, cancellationToken);


    public DataLocation<DataTrash> GetTrash()
    {
        return GetRoot()
            .Wrap(x => x.TreeDictionary())
            .GetOrAdd(".trash")
            .Wrap(x => x.Storage())
            .GetOrCreateData<DataPath, DataTrash>();
    }

    public async ValueTask<DataLocation<DataTrash>> GetTrashAsync(CancellationToken cancellationToken = default)
    {
        var root = await GetRootAsync(cancellationToken);
        var trashPath = await root.Wrap(x => x.TreeDictionary())
            .GetOrAddAsync(".trash", cancellationToken);
        return await trashPath
            .Wrap(x => x.Storage())
            .GetOrCreateDataAsync<DataPath, DataTrash>(cancellationToken);
    }

    public long AllocateOffset(int length)
    {
        return Interlocked.Add(ref _length, length) - length;
    }

    public Memory<byte> ReadMemory(long offset, int length)
    {
        if (_cache.TryGetValue(offset, out var dataMemory))
        {
            return dataMemory.Memory;
        }

        _cache.TryAdd(offset, dataMemory = new(length));

        RandomAccess.Read(_safeFileHandle, dataMemory.Memory.Span, offset);

        dataMemory.CreateHash();
        return dataMemory.Memory;
    }

    public async ValueTask<Memory<byte>> ReadMemoryAsync(long offset,
        int length,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(offset, out var dataMemory))
        {
            return dataMemory.Memory;
        }

        _cache.TryAdd(offset, dataMemory = new(length));

        await RandomAccess.ReadAsync(_safeFileHandle, dataMemory.Memory, offset, cancellationToken);

        dataMemory.CreateHash();
        return dataMemory.Memory;
    }

    public void WriteMemory(long offset, Memory<byte> memory)
    {
        RandomAccess.Write(_safeFileHandle, memory.Span, offset);
    }

    public async ValueTask WriteMemoryAsync(long offset,
        Memory<byte> memory,
        CancellationToken cancellationToken = default)
    {
        await RandomAccess.WriteAsync(_safeFileHandle, memory, offset, cancellationToken);
    }

    public void Save()
    {
        Parallel.ForEach(_cache.Where(x => !x.Value.CheckHash),
            (pair, _) =>
                WriteMemory(pair.Key, pair.Value.Memory));
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await Parallel.ForEachAsync(_cache.Where(x => !x.Value.CheckHash),
            cancellationToken,
            async ValueTask (pair, token) =>
                await WriteMemoryAsync(pair.Key, pair.Value.Memory, token));
    }

    public void Flush()
    {
        Save();
        _cache.Clear();
    }
    
    
    public async ValueTask FlushAsync(CancellationToken cancellationToken = default)
    {
        await SaveAsync(cancellationToken);
        _cache.Clear();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _safeFileHandle.Dispose();
            }

            _disposedValue = true;
        }
    }
    
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

class TempDataAccess() : DataAccess(System.IO.Path.GetTempFileName())
{
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            File.Delete(Path);
        }
    }
}