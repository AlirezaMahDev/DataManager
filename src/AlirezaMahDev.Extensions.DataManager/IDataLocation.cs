namespace AlirezaMahDev.Extensions.DataManager;

public interface IDataLocation<TDataLocation> : IDataLocationBase
    where TDataLocation : IDataLocation<TDataLocation>
{
    static abstract DataLocation Create(IDataAccess access, int length);

    static abstract ValueTask<DataLocation> CreateAsync(IDataAccess access,
        int length,
        CancellationToken cancellationToken = default);

    static abstract DataLocation Read(IDataAccess access, long offset, int length);

    static abstract ValueTask<DataLocation> ReadAsync(IDataAccess access,
        long offset,
        int length,
        CancellationToken cancellationToken = default);

    static abstract void Write(IDataAccess access, DataLocation location);

    static abstract ValueTask WriteAsync(IDataAccess access,
        DataLocation location,
        CancellationToken cancellationToken = default);

    static abstract DataLocation<TValue> Create<TValue>(IDataAccess access)
        where TValue : unmanaged, IDataValue<TValue>, IDataValueDefault<TValue>;

    static abstract ValueTask<DataLocation<TValue>> CreateAsync<TValue>(IDataAccess access,
        CancellationToken cancellationToken = default)
        where TValue : unmanaged, IDataValue<TValue>, IDataValueDefault<TValue>;

    static abstract DataLocation<TValue> Read<TValue>(IDataAccess access, long offset)
        where TValue : unmanaged, IDataValue<TValue>;

    static abstract ValueTask<DataLocation<TValue>> ReadAsync<TValue>(IDataAccess access,
        long offset,
        CancellationToken cancellationToken = default)
        where TValue : unmanaged, IDataValue<TValue>;

    static abstract void Write<TValue>(IDataAccess access, DataLocation<TValue> location)
        where TValue : unmanaged, IDataValue<TValue>;

    static abstract ValueTask WriteAsync<TValue>(IDataAccess access,
        DataLocation<TValue> location,
        CancellationToken cancellationToken = default)
        where TValue : unmanaged, IDataValue<TValue>;
}

public interface IDataLocation<TSelf, TValue> : IDataLocationBase
    where TSelf : IDataLocation<TSelf, TValue>
    where TValue : unmanaged, IDataValue<TValue>
{
    ref TValue Value { get; }
}