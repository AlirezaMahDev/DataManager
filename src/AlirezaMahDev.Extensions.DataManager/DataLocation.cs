using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AlirezaMahDev.Extensions.DataManager;

readonly struct DataLocation(DataAccess access, long offset, Memory<byte> memory) : IDataLocation<DataLocation>
{
    public long Offset { get; } = offset;

    public Memory<byte> Memory { get; } = memory;
    public DataAccess Access { get; } = access;

    public void Save()
    {
        Write(Access, this);
    }

    public async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        await WriteAsync(Access, this, cancellationToken);
    }

    public static DataLocation Create(DataAccess access, int length)
    {
        return Read(access, access.GenerateOffset(length), length);
    }

    public static async ValueTask<DataLocation> CreateAsync(DataAccess access,
        int length,
        CancellationToken cancellationToken = default)
    {
        return await ReadAsync(access, access.GenerateOffset(length), length, cancellationToken);
    }

    public static DataLocation Read(DataAccess access, long offset, int length) =>
        new(access, offset, access.Read(offset, length));

    public static async ValueTask<DataLocation> ReadAsync(DataAccess access,
        long offset,
        int length,
        CancellationToken cancellationToken) =>
        new(access, offset, await access.ReadAsync(offset, length, cancellationToken));


    public static void Write(DataAccess access, DataLocation location)
    {
        access.Write(location.Offset, location.Memory.Span);
    }

    public static async ValueTask WriteAsync(DataAccess access,
        DataLocation location,
        CancellationToken cancellationToken = default)
    {
        await access.WriteAsync(location.Offset, location.Memory, cancellationToken);
    }
}

readonly struct DataLocation<TDataValue>(DataLocation @base) : IDataLocation<DataLocation<TDataValue>, TDataValue>
    where TDataValue : unmanaged, IDataValue<TDataValue>
{
    public long Offset { get; } = @base.Offset;
    private static readonly int Length = Unsafe.SizeOf<TDataValue>();
    private readonly DataLocation _base = @base;
    private readonly Lock _lock = new();

    public ref TDataValue Value =>
        ref MemoryMarshal.AsRef<TDataValue>(_base.Memory.Span);

    public DataLocation<TDataValue> Update(Func<TDataValue, TDataValue> func)
    {
        using var scope = _lock.EnterScope();
        Value = func(Value);
        return this;
    }

    public async ValueTask<DataLocation<TDataValue>> UpdateAsync(
        Func<TDataValue, CancellationToken, ValueTask<TDataValue>> func,
        CancellationToken cancellationToken = default)
    {
        _lock.Enter();
        Value = await func(Value, cancellationToken).ConfigureAwait(true);
        _lock.Exit();
        return this;
    }

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

    public static DataLocation<TDataValue> Create(DataAccess access)
    {
        var location = new DataLocation<TDataValue>(DataLocation.Create(access, Length));
        if (location.Value.Equals(default))
            location.Value = TDataValue.Default;
        return location;
    }

    public static async ValueTask<DataLocation<TDataValue>> CreateAsync(DataAccess access,
        CancellationToken cancellationToken = default)
    {
        var location =
            new DataLocation<TDataValue>(await DataLocation.CreateAsync(access, Length, cancellationToken));
        if (location.Value.Equals(default))
            location.Value = TDataValue.Default;
        return location;
    }

    public static DataLocation<TDataValue> Read(DataAccess access, long offset)
    {
        return new(DataLocation.Read(access, offset, Length));
    }

    public static async ValueTask<DataLocation<TDataValue>> ReadAsync(DataAccess access,
        long offset,
        CancellationToken cancellationToken)
    {
        return new(await DataLocation.ReadAsync(access, offset, Length, cancellationToken));
    }

    public static void Write(DataAccess access, DataLocation<TDataValue> location)
    {
        DataLocation.Write(access, location._base);
    }

    public static async ValueTask WriteAsync(DataAccess access,
        DataLocation<TDataValue> location,
        CancellationToken cancellationToken = default)
    {
        await DataLocation.WriteAsync(access, location._base, cancellationToken);
    }
}