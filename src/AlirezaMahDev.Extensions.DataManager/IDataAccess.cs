namespace AlirezaMahDev.Extensions.DataManager;

public interface IDataAccess
{
    long AllocateOffset(int length);

    DataLocation<DataPath> GetRoot();
    ValueTask<DataLocation<DataPath>> GetRootAsync(CancellationToken cancellationToken = default);

    DataLocation<DataTrash> GetTrash();
    ValueTask<DataLocation<DataTrash>> GetTrashAsync(CancellationToken cancellationToken = default);

    Memory<byte> ReadMemory(long offset, int length);

    ValueTask<Memory<byte>> ReadMemoryAsync(long offset,
        int length,
        CancellationToken cancellationToken = default);

    void WriteMemory(long offset, Memory<byte> memory);
    ValueTask WriteMemoryAsync(long offset, Memory<byte> memory, CancellationToken cancellationToken = default);

    void Save();
    Task SaveAsync(CancellationToken cancellationToken = default);
}