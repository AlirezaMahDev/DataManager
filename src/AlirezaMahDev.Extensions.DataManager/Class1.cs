using System.Buffers;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

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
            new(key), LazyThreadSafetyMode.ExecutionAndPublication), this);
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

class DataAccess(string path) : IDisposable
{
    private readonly Lock _lock = new();
    private bool _disposedValue;

    public SafeFileHandle SafeFileHandle { get; } = File.OpenHandle(
            Path.Combine(Environment.CurrentDirectory, path),
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None);

    public DataLocation<DataPath> GetRoot() =>
        DataLocation<DataPath>.Create(this, 0);
    public async ValueTask<DataLocation<DataPath>> GetRootAsync(CancellationToken cancellationToken = default) =>
        await DataLocation<DataPath>.CreateAsync(this, 0, cancellationToken);

    public void NeedLength(long length)
    {
        if (RandomAccess.GetLength(SafeFileHandle) >= length)
            return;
        using var scope = _lock.EnterScope();
        if (RandomAccess.GetLength(SafeFileHandle) >= length)
            return;
        RandomAccess.SetLength(SafeFileHandle, (long)BitOperations.RoundUpToPowerOf2((ulong)length));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                SafeFileHandle.Dispose();
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

interface IDataLocationBase
{
    DataAccess Access { get; }
    Memory<byte> Memory { get; }
    void Save();
    ValueTask SaveAsync(CancellationToken cancellationToken = default);
}

interface IDataLocation<TDataLocation> : IDataLocationBase
    where TDataLocation : IDataLocation<TDataLocation>
{
    static abstract DataLocation Create(DataAccess access, long offset, int length);
    static abstract ValueTask<DataLocation> CreateAsync(DataAccess access, long offset, int length, CancellationToken cancellationToken = default);
    static abstract DataLocation Read(DataAccess access, long offset, int length);
    static abstract ValueTask<DataLocation> ReadAsync(DataAccess access, long offset, int length, CancellationToken cancellationToken = default);
    static abstract void Write(DataAccess access, DataLocation location);
    static abstract ValueTask WriteAsync(DataAccess access, DataLocation location, CancellationToken cancellationToken = default);
}

interface IDataLocation<TDataLocation, TDataValue> : IDataLocationBase
    where TDataLocation : IDataLocation<TDataLocation, TDataValue>
    where TDataValue : unmanaged, IEquatable<TDataValue>
{
    static abstract DataLocation<TDataValue> Create(DataAccess access, long offset);
    static abstract ValueTask<DataLocation<TDataValue>> CreateAsync(DataAccess access, long offset, CancellationToken cancellationToken = default);
    static abstract DataLocation<TDataValue> Read(DataAccess access, long offset);
    static abstract ValueTask<DataLocation<TDataValue>> ReadAsync(DataAccess access, long offset, CancellationToken cancellationToken = default);
    static abstract void Write(DataAccess access, DataLocation<TDataValue> location);
    static abstract ValueTask WriteAsync(DataAccess access, DataLocation<TDataValue> location, CancellationToken cancellationToken = default);

    ref TDataValue Value { get; }
}

static class DataExtensions
{
    extension(DataLocation<DataPath> location)
    {

    }
}

record struct DataPath()
{
    public long Length = 0;
    public long Next = -1;
    public long Child = -1;
}

readonly struct DataLocation : IDisposable, IDataLocation<DataLocation>
{
    private readonly long _offset;
    private readonly IMemoryOwner<byte> _memoryOwner;

    public DataLocation(DataAccess access, long offset, int length)
    {
        _offset = offset;
        Access = access;
        _memoryOwner = MemoryPool<byte>.Shared.Rent(length);
        Memory = _memoryOwner.Memory[..length];
    }

    public Memory<byte> Memory { get; }

    public DataAccess Access { get; }

    public void Save()
    {
        Write(Access, this);
    }
    public async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        await WriteAsync(Access, this, cancellationToken);
    }

    public void Dispose()
    {
        _memoryOwner.Dispose();
    }

    public static DataLocation Create(DataAccess access, long offset, int length)
    {
        access.NeedLength(offset + length);
        return Read(access, offset, length);
    }

    public static async ValueTask<DataLocation> CreateAsync(DataAccess access, long offset, int length, CancellationToken cancellationToken = default)
    {
        return await ReadAsync(access, offset, length, cancellationToken);
    }

    public static DataLocation Read(DataAccess manager, long offset, int length)
    {
        var location = new DataLocation(manager, offset, length);
        RandomAccess.Read(manager.SafeFileHandle, location.Memory.Span, offset);
        return location;
    }

    public static async ValueTask<DataLocation> ReadAsync(DataAccess manager, long offset, int length, CancellationToken cancellationToken)
    {
        var location = new DataLocation(manager, offset, length);
        await RandomAccess.ReadAsync(manager.SafeFileHandle, location.Memory, offset, cancellationToken);
        return location;
    }

    public static void Write(DataAccess manager, DataLocation location)
    {
        RandomAccess.Write(manager.SafeFileHandle, location.Memory.Span, location._offset);
    }

    public static async ValueTask WriteAsync(DataAccess manager, DataLocation location, CancellationToken cancellationToken = default)
    {
        await RandomAccess.WriteAsync(manager.SafeFileHandle, location.Memory, location._offset, cancellationToken);
    }
}


readonly struct DataLocation<TDataValue>(DataLocation @base) : IDisposable, IDataLocation<DataLocation<TDataValue>, TDataValue>
    where TDataValue : unmanaged, IEquatable<TDataValue>
{
    private static readonly int Length = Unsafe.SizeOf<TDataValue>();
    private readonly DataLocation _base = @base;

    public ref TDataValue Value =>
        ref MemoryMarshal.AsRef<TDataValue>(_base.Memory.Span);

    public DataAccess Access => _base.Access;
    public Memory<byte> Memory => _base.Memory;

    public void Save()
    {
        _base.Save();
    }
    public async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        await _base.SaveAsync(cancellationToken);
    }

    public void Dispose()
    {
        _base.Dispose();
    }

    public static DataLocation<TDataValue> Create(DataAccess access, long offset)
    {
        var location = new DataLocation<TDataValue>(DataLocation.Create(access, offset, Length));
        if (location.Value.Equals(default))
            location.Value = new();
        return location;
    }
    public static async ValueTask<DataLocation<TDataValue>> CreateAsync(DataAccess access, long offset, CancellationToken cancellationToken = default)
    {
        var location = new DataLocation<TDataValue>(await DataLocation.CreateAsync(access, offset, Length, cancellationToken));
        if (location.Value.Equals(default))
            location.Value = new();
        return location;
    }

    public static DataLocation<TDataValue> Read(DataAccess access, long offset)
    {
        return new DataLocation<TDataValue>(DataLocation.Read(access, offset, Length));
    }

    public static async ValueTask<DataLocation<TDataValue>> ReadAsync(DataAccess access, long offset, CancellationToken cancellationToken)
    {
        return new DataLocation<TDataValue>(await DataLocation.ReadAsync(access, offset, Length, cancellationToken));
    }

    public static void Write(DataAccess access, DataLocation<TDataValue> location)
    {
        DataLocation.Write(access, location._base);
    }

    public static async ValueTask WriteAsync(DataAccess access, DataLocation<TDataValue> location, CancellationToken cancellationToken = default)
    {
        await DataLocation.WriteAsync(access, location._base, cancellationToken);
    }
}
