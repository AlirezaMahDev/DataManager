namespace AlirezaMahDev.Extensions.DataManager;

public interface IDataLocationBase
{
    long Offset { get; }
    int Length { get; }
    IDataAccess Access { get; }
    Memory<byte> Memory { get; }
    void Save();
    ValueTask SaveAsync(CancellationToken cancellationToken = default);
}