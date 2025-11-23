namespace AlirezaMahDev.Extensions.DataManager;

interface IDataLocation<TDataLocation> : IDataLocationBase
    where TDataLocation : IDataLocation<TDataLocation>
{
    static abstract DataLocation Create(DataAccess access, int length);

    static abstract ValueTask<DataLocation> CreateAsync(DataAccess access,
        int length,
        CancellationToken cancellationToken = default);

    static abstract DataLocation Read(DataAccess access, long offset, int length);

    static abstract ValueTask<DataLocation> ReadAsync(DataAccess access,
        long offset,
        int length,
        CancellationToken cancellationToken = default);

    static abstract void Write(DataAccess access, DataLocation location);

    static abstract ValueTask WriteAsync(DataAccess access,
        DataLocation location,
        CancellationToken cancellationToken = default);
}

interface IDataLocation<TDataLocation, TDataValue> : IDataLocationBase
    where TDataLocation : IDataLocation<TDataLocation, TDataValue>
    where TDataValue : unmanaged, IDataValue<TDataValue>
{
    static abstract DataLocation<TDataValue> Create(DataAccess access);

    static abstract ValueTask<DataLocation<TDataValue>> CreateAsync(DataAccess access,
        CancellationToken cancellationToken = default);

    static abstract DataLocation<TDataValue> Read(DataAccess access, long offset);

    static abstract ValueTask<DataLocation<TDataValue>> ReadAsync(DataAccess access,
        long offset,
        CancellationToken cancellationToken = default);

    static abstract void Write(DataAccess access, DataLocation<TDataValue> location);

    static abstract ValueTask WriteAsync(DataAccess access,
        DataLocation<TDataValue> location,
        CancellationToken cancellationToken = default);

    ref TDataValue Value { get; }
}