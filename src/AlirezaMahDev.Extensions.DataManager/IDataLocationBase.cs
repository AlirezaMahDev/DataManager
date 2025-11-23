namespace AlirezaMahDev.Extensions.DataManager;

interface IDataLocationBase
{
    long Offset { get; }
    DataAccess Access { get; }
    Memory<byte> Memory { get; }
    void Save();
    ValueTask SaveAsync(CancellationToken cancellationToken = default);
}