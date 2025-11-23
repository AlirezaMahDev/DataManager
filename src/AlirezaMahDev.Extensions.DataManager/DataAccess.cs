using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Hashing;
using System.Runtime.CompilerServices;

using Microsoft.Win32.SafeHandles;

namespace AlirezaMahDev.Extensions.DataManager;

interface IDataAccess
{
    DataLocation<DataPath> GetRoot();
    ValueTask<DataLocation<DataPath>> GetRootAsync(CancellationToken cancellationToken = default);
}

class DataIndex
{
    
}

class DataMemory : IMemoryOwner<byte>
{
    private readonly IMemoryOwner<byte> _memoryOwner;
    private int _usedCount;
    private UInt128 _hash;

    public DataMemory(int length)
    {
        _memoryOwner = MemoryPool<byte>.Shared.Rent(length);
        Memory = _memoryOwner.Memory[..length];
    }

    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public int UsedCount => _usedCount;

    public bool CheckHash =>
        _hash == GenerateHash();

    public void CreateHash() =>
        _hash = GenerateHash();

    private UInt128 GenerateHash() =>
        XxHash128.HashToUInt128(Memory.Span);

    public Memory<byte> Memory
    {
        get
        {
            Interlocked.Increment(ref _usedCount);
            return field;
        }
    }

    public void Dispose() => _memoryOwner.Dispose();
}

sealed class DataAccess : IDisposable, IDataAccess
{
    private bool _disposedValue;

    private readonly ConcurrentDictionary<long, DataMemory> _cache = [];
    private long _length;

    private readonly SafeFileHandle _safeFileHandle;

    public DataAccess(string path)
    {
        _safeFileHandle = File.OpenHandle(
            Path.Combine(Environment.CurrentDirectory, path),
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
        DataLocation<DataPath>.Read(this, 0);

    public async ValueTask<DataLocation<DataPath>> GetRootAsync(CancellationToken cancellationToken = default) =>
        await DataLocation<DataPath>.ReadAsync(this, 0, cancellationToken);

    public long GenerateOffset(int length)
    {
        return Interlocked.Add(ref _length, length) - length;
    }

    private void Dispose(bool disposing)
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

    public Memory<byte> Read(long offset, int length)
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

    public async ValueTask<Memory<byte>> ReadAsync(long offset,
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

    public void Write(long offset, Span<byte> span)
    {
        RandomAccess.Write(_safeFileHandle, span, offset);
    }

    public async ValueTask WriteAsync(long offset, Memory<byte> memory, CancellationToken cancellationToken = default)
    {
        await RandomAccess.WriteAsync(_safeFileHandle, memory, offset, cancellationToken);
    }

    public void Save()
    {
        Parallel.ForEach(_cache.Where(x => !x.Value.CheckHash),
            (pair, _) =>
                Write(pair.Key, pair.Value.Memory.Span));
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await Parallel.ForEachAsync(_cache.Where(x => !x.Value.CheckHash),
            cancellationToken,
            async ValueTask (pair, token) =>
                await WriteAsync(pair.Key, pair.Value.Memory, token));
    }

    public void Dispose() =>
        Dispose(disposing: true);
}