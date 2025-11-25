namespace AlirezaMahDev.Extensions.DataManager;

public interface IDataStorage<TSelf> : IDataValue<TSelf>
    where TSelf : unmanaged, IDataStorage<TSelf>
{
    long Data { get; set; }
    int Size { get; set; }
}