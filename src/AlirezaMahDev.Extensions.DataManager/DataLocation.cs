using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AlirezaMahDev.Extensions.DataManager;

public readonly struct DataLocation(IDataAccess access, long offset, Memory<byte> memory) : IDataLocation<DataLocation>
{
    public long Offset { get; } = offset;
    public int Length { get; } = memory.Length;

    public Memory<byte> Memory { get; } = memory;
    public IDataAccess Access { get; } = access;

    public void Save()
    {
        Write(Access, this);
    }

    public async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        await WriteAsync(Access, this, cancellationToken);
    }

    public static DataLocation Create(IDataAccess access, int length)
    {
        return Read(access, access.AllocateOffset(length), length);
    }

    public static async ValueTask<DataLocation> CreateAsync(IDataAccess access,
        int length,
        CancellationToken cancellationToken = default)
    {
        return await ReadAsync(access, access.AllocateOffset(length), length, cancellationToken);
    }

    public static DataLocation Read(IDataAccess access, long offset, int length) =>
        new(access, offset, access.ReadMemory(offset, length));

    public static async ValueTask<DataLocation> ReadAsync(IDataAccess access,
        long offset,
        int length,
        CancellationToken cancellationToken) =>
        new(access, offset, await access.ReadMemoryAsync(offset, length, cancellationToken));


    public static void Write(IDataAccess access, DataLocation location)
    {
        access.WriteMemory(location.Offset, location.Memory);
    }

    public static async ValueTask WriteAsync(IDataAccess access,
        DataLocation location,
        CancellationToken cancellationToken = default)
    {
        await access.WriteMemoryAsync(location.Offset, location.Memory, cancellationToken);
    }

    public static DataLocation<TValue> Create<TValue>(IDataAccess access)
        where TValue : unmanaged, IDataValue<TValue>, IDataValueDefault<TValue>
    {
        var location = new DataLocation<TValue>(Create(access, DataLocation<TValue>.Size));
        if (location.Value.Equals(default))
            location.Value = TValue.Default;
        return location;
    }

    public static async ValueTask<DataLocation<TValue>> CreateAsync<TValue>(IDataAccess access,
        CancellationToken cancellationToken = default)
        where TValue : unmanaged, IDataValue<TValue>, IDataValueDefault<TValue>
    {
        var location =
            new DataLocation<TValue>(await CreateAsync(access, DataLocation<TValue>.Size, cancellationToken));
        if (location.Value.Equals(default))
            location.Value = TValue.Default;
        return location;
    }

    public static DataLocation<TValue> Read<TValue>(IDataAccess access, long offset)
        where TValue : unmanaged, IDataValue<TValue>
    {
        return new(Read(access, offset, DataLocation<TValue>.Size));
    }

    public static async ValueTask<DataLocation<TValue>> ReadAsync<TValue>(IDataAccess access,
        long offset,
        CancellationToken cancellationToken)
        where TValue : unmanaged, IDataValue<TValue>
    {
        return new(await ReadAsync(access, offset, DataLocation<TValue>.Size, cancellationToken));
    }

    public static void Write<TValue>(IDataAccess access, DataLocation<TValue> location)
        where TValue : unmanaged, IDataValue<TValue>
    {
        Write(access, location.Base);
    }

    public static async ValueTask WriteAsync<TValue>(IDataAccess access,
        DataLocation<TValue> location,
        CancellationToken cancellationToken = default)
        where TValue : unmanaged, IDataValue<TValue>
    {
        await WriteAsync(access, location.Base, cancellationToken);
    }
}

public readonly struct DataLocation<TValue>(DataLocation @base) : IDataLocation<DataLocation<TValue>, TValue>
    where TValue : unmanaged, IDataValue<TValue>
{
    public static readonly int Size = Unsafe.SizeOf<TValue>();
    
    public long Offset { get; } = @base.Offset;
    public int Length { get; } = Size;
    
    private readonly Lock _lock = new();

    public DataLocation Base { get; } = @base;

    public ref TValue Value =>
        ref MemoryMarshal.AsRef<TValue>(Base.Memory.Span);

    public DataLocation<TValue> Update(Func<TValue, TValue> func)
    {
        using var scope = _lock.EnterScope();
        Value = func(Value);
        return this;
    }

    public async ValueTask<DataLocation<TValue>> UpdateAsync(
        Func<TValue, CancellationToken, ValueTask<TValue>> func,
        CancellationToken cancellationToken = default)
    {
        _lock.Enter();
        var temp = await func(Value, cancellationToken).ConfigureAwait(true);
        Value = temp;
        _lock.Exit();
        return this;
    }

    public IDataAccess Access => Base.Access;
    public Memory<byte> Memory => Base.Memory;

    public void Save()
    {
        Base.Save();
    }

    public async ValueTask SaveAsync(CancellationToken cancellationToken = default)
    {
        await Base.SaveAsync(cancellationToken);
    }
}